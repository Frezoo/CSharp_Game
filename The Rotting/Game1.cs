using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace The_Rotting
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _crosshair;
        private Texture2D _background;
        private Texture2D _bulletTexture;
        private Texture2D _ammoBoxTexture;
        private Texture2D _zombieWalkTexture;

        public SpriteFont _font;
        private Matrix _transformMatrix = Matrix.Identity;

        Player player;
        ZombieSpawner zombieSpawner;

        HealthBar heathBar;
        InputHandler inputHandler;

        private List<AmmoBox> _ammoBoxes = new List<AmmoBox>();


        private float _spawnCooldown = 10f;
        private float _timeSinceLastSpawn = 0f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        public void SetResolution(int width, int height)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            SetResolution(1280, 720);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            heathBar = new HealthBar(new Vector2(161.5f, 39), _spriteBatch, Content.Load<Texture2D>("HP_FRONT"), Content.Load<Texture2D>("HP_BG"));
            player = new Player(_spriteBatch, GraphicsDevice, heathBar);


            _background = Content.Load<Texture2D>("background");
            player.Texture = Content.Load<Texture2D>("Solider_AK");
            _crosshair = Content.Load<Texture2D>("crosshair");
            _bulletTexture = Content.Load<Texture2D>("bullet");
            _ammoBoxTexture = Content.Load<Texture2D>("ammo_box_texture");
            _zombieWalkTexture = Content.Load<Texture2D>("ZombieWalk");



            zombieSpawner = new ZombieSpawner(_zombieWalkTexture, 1, player);

            LoadFonts();
            SetOrigins();

            inputHandler = new InputHandler(player);
        }

        private void LoadFonts()
        {
            _font = Content.Load<SpriteFont>("font");
        }

        private void SetOrigins()
        {
            player.Origin = new Vector2(player.Texture.Width / 2, player.Texture.Height / 2);
            heathBar.Origin = new Vector2(heathBar.BackTexture.Width / 2, heathBar.BackTexture.Height / 2);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            inputHandler.HandleKeyboardInput(keyboardState);
            inputHandler.HandleMouseInput(mouseState);

            zombieSpawner.SpawnEnemies();

            UpdatePlayerStatus(deltaTime);
            SpawnAmmoBox(deltaTime);
            HandleAmmoCollection();
            ProcessBullets(deltaTime);

            foreach (var zombie in zombieSpawner.Zombies)
            {
                zombie.Update(deltaTime, zombieSpawner.Zombies);
            }
            zombieSpawner.Zombies = zombieSpawner.Zombies.Where(z => z.IsAlive).ToList();
            

            base.Update(gameTime);
        }



        private void UpdatePlayerStatus(float deltaTime)
        {
            player.MovePlayer(deltaTime);
            player.UpdateRotation(inputHandler.GetMouseWorldPosition(_transformMatrix));
            player.TimeSinceLastShot += deltaTime;
            player.TimeSinceLastReload += deltaTime;
        }

        private void ProcessBullets(float deltaTime)
        {
            for (int i = player.Bullets.Count - 1; i >= 0; i--)
            {
                var bullet = player.Bullets[i];
                bullet.UpdateBullet(deltaTime);

                // Создаем прямоугольник для пули
                Rectangle bulletBounds = new Rectangle(
                    (int)bullet.Position.X,
                    (int)bullet.Position.Y,
                    _bulletTexture.Width,
                    _bulletTexture.Height
                );

                // Проверяем столкновение с каждым зомби
                foreach (var zombie in zombieSpawner.Zombies)
                {
                    // Создаем прямоугольник для зомби
                    Rectangle zombieBounds = new Rectangle(
                        (int)zombie.Position.X - (int)zombie.Origin.X,
                        (int)zombie.Position.Y - (int)zombie.Origin.Y,
                        zombie.ZombieWalkTexture.Width / 16, // Ширина одного кадра
                        zombie.ZombieWalkTexture.Height
                    );

                    // Проверяем столкновение
                    if (CheckCollision(bulletBounds, zombieBounds))
                    {
                        // Наносим урон зомби
                        zombie.Health -= bullet.BulletDamage;

                        // Удаляем пулю
                        player.Bullets.RemoveAt(i);

                        // Прерываем цикл, так как пуля уже уничтожена
                        break;
                    }
                }

                // Удаляем пулю, если она вышла за пределы экрана
                if (bullet.IsOffScreen(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height))
                {
                    player.Bullets.RemoveAt(i);
                }
            }

        }

        public bool CheckCollision(Rectangle fristRectangle, Rectangle secondRectangle)
        {
            return fristRectangle.Intersects(secondRectangle);
        }

        private void HandleAmmoCollection()
        {
            for (int i = _ammoBoxes.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(player.Position, _ammoBoxes[i].Position) < player.AmmoTriggerDistation)
                {
                    player.AddAmmo(_ammoBoxes[i].AmmoAmount);
                    _ammoBoxes.RemoveAt(i);
                }
            }
        }

        private void SpawnAmmoBox(float deltaTime)
        {
            Random random = new Random();
            _timeSinceLastSpawn = _timeSinceLastSpawn + deltaTime;
            if (_timeSinceLastSpawn >= _spawnCooldown)
            {
                Vector2 position = new Vector2(random.Next(50, 1230), random.Next(50, 670));
                int ammoAmount = random.Next(25, 45);
                _ammoBoxes.Add(new AmmoBox(position, _ammoBoxTexture, ammoAmount));
                _timeSinceLastSpawn = 0f;
            }

        }
        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_background, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1f);

            // Отрисовываем коробки с патронами
            foreach (var ammoBox in _ammoBoxes)
            {
                ammoBox.Draw(_spriteBatch);
            }

            // Отрисовываем пули
            foreach (var bullet in player.Bullets)
            {
                bullet.DrawBullet(_spriteBatch, _bulletTexture);
            }

            //Отрисовывание зомби


            // Отрисовываем игрока, интерфейс и прицел
            player.DrawPlayer();
            player.DrawStatistic(_font);
            _spriteBatch.Draw(_crosshair, inputHandler.GetMouseWorldPosition(_transformMatrix), null, Color.White, 0, new Vector2(_crosshair.Width / 2, _crosshair.Height / 2), 0.03f, SpriteEffects.None, 0);

            foreach (var zombie in zombieSpawner.Zombies)
            {
                zombie.DrawZombie(_spriteBatch);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }

    public class AmmoBox
    {
        public Vector2 Position;
        public Texture2D Texture;
        public int AmmoAmount;

        public AmmoBox(Vector2 position, Texture2D texture, int ammoAmount)
        {
            Position = position;
            Texture = texture;
            AmmoAmount = ammoAmount;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0, Vector2.Zero, 0.05f, SpriteEffects.None, 0f);

        }
    }

    public class HealthBar
    {
        public Vector2 Position;
        public Vector2 Origin;

        private SpriteBatch sb;

        public Texture2D FrontTexture;
        public Texture2D BackTexture;

        private float maxHealth = 1f;
        private float hpCondition = 1f;

        public HealthBar(Vector2 position, SpriteBatch sb, Texture2D frontTexture, Texture2D backTexture)
        {
            Position = position;
            this.sb = sb;
            FrontTexture = frontTexture;
            BackTexture = backTexture;
        }

        public void DrawHealthBar(float currentHealth)
        {
            hpCondition = currentHealth / maxHealth;
            sb.Draw(BackTexture, Position, null, Color.White, 0, Origin, 1f, SpriteEffects.None, 1f);
            sb.Draw(FrontTexture, Position, new Rectangle(0, 0, (int)(303 * hpCondition), 49), Color.White, 0, Origin, 1f, SpriteEffects.None, 1f);
        }
    }

    


}