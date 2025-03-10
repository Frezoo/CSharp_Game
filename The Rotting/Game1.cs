using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace The_Rotting
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _crosshair;
        private Texture2D _background;
        private Texture2D _bulletTexture;
        private Texture2D _ammoBoxTexture; // Текстура коробки с патронами
        public SpriteFont _font;
        private Matrix _transformMatrix = Matrix.Identity;

        Player player;
        HealthBar heathBar;
        InputHandler inputHandler;
        private List<AmmoBox> _ammoBoxes = new List<AmmoBox>(); // Список коробок с патронами
        private float _spawnCooldown = 10f; // Интервал появления коробки (в секундах)
        private float _timeSinceLastSpawn = 0f; // Время с момента последнего появления коробки

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

            _font = Content.Load<SpriteFont>("font");

            player.Origin = new Vector2(player.Texture.Width / 2, player.Texture.Height / 2);
            heathBar.Origin = new Vector2(heathBar.BackTexture.Width / 2, heathBar.BackTexture.Height / 2);

            inputHandler = new InputHandler(player);
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

            player.MovePlayer(deltaTime);
            player.UpdateRotation(inputHandler.GetMouseWorldPosition(_transformMatrix));
            player.TimeSinceLastShot += deltaTime;
            player.TimeSinceLastReload += deltaTime;

            // Обновляем таймер появления коробок
            _timeSinceLastSpawn += deltaTime;
            if (_timeSinceLastSpawn >= _spawnCooldown)
            {
                SpawnAmmoBox();
                _timeSinceLastSpawn = 0f;
            }

            // Проверяем столкновение игрока с коробками
            for (int i = _ammoBoxes.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(player.Position, _ammoBoxes[i].Position) < 50f) // 50f — радиус подбора
                {
                    player.AddAmmo(_ammoBoxes[i].AmmoAmount); // Добавляем патроны игроку
                    _ammoBoxes.RemoveAt(i); // Удаляем коробку
                }
            }

            // Обновляем пули
            for (int i = player.Bullets.Count - 1; i >= 0; i--)
            {
                player.Bullets[i].UpdateBullet(deltaTime);

                if (player.Bullets[i].IsOffScreen(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height))
                {
                    player.Bullets.RemoveAt(i);
                }
            }

            base.Update(gameTime);
        }
        private void SpawnAmmoBox()
        {
            Random random = new Random();
            Vector2 position = new Vector2(random.Next(50, 1230), random.Next(50, 670)); // Случайные координаты
            int ammoAmount = random.Next(25, 45); // Количество патронов в коробке
            _ammoBoxes.Add(new AmmoBox(position, _ammoBoxTexture, ammoAmount));
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

            // Отрисовываем игрока, интерфейс и прицел
            player.DrawPlayer();
            player.DrawStatistic(_font);
            _spriteBatch.Draw(_crosshair, inputHandler.GetMouseWorldPosition(_transformMatrix), null, Color.White, 0, new Vector2(_crosshair.Width / 2, _crosshair.Height / 2), 0.03f, SpriteEffects.None, 0);

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

    public class Bullet
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Rotation;
        private float speed = 2800f;

        public Bullet(Vector2 position, Vector2 direction, float rotation)
        {
            Position = position;
            Direction = direction;
            Rotation = rotation;
        }

        public void UpdateBullet(float deltaTime)
        {
            Position += Direction * speed * deltaTime;
        }

        public void DrawBullet(SpriteBatch sb, Texture2D texture)
        {
            sb.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
        }

        public bool IsOffScreen(int screenWidth, int screenHeight)
        {
            return Position.X < 0 || Position.X > screenWidth || Position.Y < 0 || Position.Y > screenHeight;
        }
    }

    public class Player
    {
        public Texture2D Texture;
        public SpriteBatch SB;

        public Vector2 Position;
        public Vector2 Origin;
        public Vector2 Direction;

        public List<Bullet> Bullets = new List<Bullet>();

        public float Rotation;
        public float Scale;

        private float _speed;
        private float _health = 1f;
        private int _ammo = 30;
        private int _maxAmmo = 90;

        private float _shootCooldown = 0.1f;
        public float TimeSinceLastShot = 0f;

        private float _reloadCooldown = 1f;
        public float TimeSinceLastReload = 1f;

        private HealthBar hpBar;

        public Color Color;

        public Player(SpriteBatch sB, GraphicsDevice gd, HealthBar healthBar)
        {
            Position = new Vector2(gd.Viewport.Width / 2, gd.Viewport.Height / 2);
            Direction = Vector2.Zero;
            Rotation = 0f;
            Color = Color.White;
            Scale = 0.14f;
            SB = sB;
            _speed = 750;
            hpBar = healthBar;
        }

        public void DrawPlayer()
        {
            SB.Draw(Texture, Position, null, Color, Rotation, Origin, Scale, SpriteEffects.None, 0f);
        }

        public void DrawStatistic(SpriteFont font)
        {
            SB.DrawString(font, $"{_ammo}/{_maxAmmo}", new Vector2(1170, 25), Color.Black);
            if (TimeSinceLastReload < _reloadCooldown)
            {
                SB.DrawString(font, $"Reload: {_reloadCooldown - TimeSinceLastReload:F1}s", new Vector2(1100, 50), Color.Blue);
            }
            hpBar.DrawHealthBar(_health);
        }

        public void MovePlayer(float deltaTime)
        {
            Vector2 newPosition = Position + (Direction * _speed * deltaTime);
            if (Direction != Vector2.Zero)
            {
                Direction.Normalize();
                if (newPosition.X >= 40 && newPosition.X <= 1240 && newPosition.Y >= 40 && newPosition.Y <= 680)
                {
                    Position = newPosition;
                }
            }
            Direction = Vector2.Zero;
        }

        public void UpdateRotation(Vector2 mousePosition)
        {
            float deltaX = mousePosition.X - Position.X;
            float deltaY = mousePosition.Y - Position.Y;
            Rotation = MathF.Atan2(deltaY, deltaX);
        }

        public void Shoot()
        {
            if ((TimeSinceLastShot >= _shootCooldown && _ammo - 1 >= 0) && (TimeSinceLastReload >= _reloadCooldown))
            {
                Vector2 direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
                Bullet newBullet = new Bullet(Position, direction, Rotation);
                Bullets.Add(newBullet);
                TimeSinceLastShot = 0f;
                _ammo--;
            }
        }

        public void Reload()
        {
            if (TimeSinceLastReload >= _reloadCooldown)
            {
                int ammoToAdd = Math.Min(30 - _ammo, _maxAmmo);
                if (ammoToAdd > 0)
                {
                    _ammo += ammoToAdd;
                    _maxAmmo -= ammoToAdd;
                    TimeSinceLastReload = 0f;
                }
            }
        }

        public void AddAmmo(int amount)
        {
            _maxAmmo += amount;
        }

        public void ChangeHealth(float delta)
        {
            _health -= delta;
        }
    }

    public class Enemy
    {
        public Vector2 Position;
        public Texture2D Texture;
        public float Speed  = 100f; // Скорость врага
        public float Damage  = 0.1f; // Урон, наносимый игроку
        public float Health = 1f; // Здоровье врага
        public bool IsAlive => Health > 0; // Проверка, жив ли враг

        private Player _player; // Ссылка на игрока

        public Enemy(Vector2 position, Texture2D texture, Player player)
        {
            Position = position;
            Texture = texture;
            _player = player;
        }

        public void MoveToPlayer(float deltaTime)
        {
            Vector2 direction = _player.Position - Position;
            direction.Normalize();
            Position += direction * Speed * deltaTime;

            // Проверяем столкновение с игроком
            if (Vector2.Distance(Position, _player.Position) < 50f) // 50f — радиус столкновения
            {
                _player.ChangeHealth(Damage); // Наносим урон игроку
                Health = 0; // Враг умирает после столкновения
            }
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

    public class InputHandler
    {
        private Player player;

        public InputHandler(Player player)
        {
            this.player = player;
        }

        public void HandleKeyboardInput(KeyboardState keyboardState)
        {
            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {
                if (key == Keys.W)
                {
                    player.Direction.Y -= 1;
                }
                else if (key == Keys.S)
                {
                    player.Direction.Y += 1;
                }
                else if (key == Keys.A)
                {
                    player.Direction.X -= 1;
                }
                else if (key == Keys.D)
                {
                    player.Direction.X += 1;
                }
            }
            if (keyboardState.IsKeyDown(Keys.R))
            {
                player.Reload();
            }
        }

        public void HandleMouseInput(MouseState mouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                player.Shoot();
            }
        }

        public Vector2 GetMouseWorldPosition(Matrix _transformMatrix)
        {
            MouseState mouseState = Mouse.GetState();
            Point mousePosition = mouseState.Position;
            Vector2 worldPosition = Vector2.Transform(mousePosition.ToVector2(), Matrix.Invert(_transformMatrix));
            return worldPosition;
        }
    }

    
}