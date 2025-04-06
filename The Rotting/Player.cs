using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;



namespace The_Rotting
{
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
        public float AmmoTriggerDistation = 50f;


        private float _shootCooldown = 0.05f;
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
            SB.Draw(Texture, Position, null, Color, Rotation, Origin, Scale, SpriteEffects.None, 0);
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
}
