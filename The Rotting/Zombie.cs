using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;



namespace The_Rotting
{
    public class Zombie
    {
        public Vector2 Position;
        public Texture2D ZombieWalkTexture;
        public float Rotation;
        public float Scale = 0.372f;
        public Vector2 Origin;
        private int frame;
        public float Speed; // Скорость теперь не константа
        public float Damage = 0.1f; // Урон, наносимый игроку
        public float Health = 1f; // Здоровье врага
        public bool IsAlive => Health > 0; // Проверка, жив ли враг
        private Player _player; // Ссылка на игрока
                                //
        private float _frameTime = 0.03f; // Время между кадрами (в секундах)
        private float _timeSinceLastFrame = 0f; // Время с последнего кадра
        const float distanceToUpdateRotation = 30;

        public Zombie(Vector2 position, Texture2D walkTexture, Player player)
        {
            Position = position;
            ZombieWalkTexture = walkTexture;
            _player = player;
            // Размеры одного кадра анимации зомби
            int frameWidth = 246; // Ширина одного кадра
            int frameHeight = 311; // Высота одного кадра
                                   // Устанавливаем Origin как центр текущего кадра анимации
            Origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            // Генерация случайной скорости в пределах 300 ± 20
            Random random = new Random();
            Speed = 300f + (float)(random.NextDouble() * 40f - 20f); // Диапазон: 280–320
        }

        public void MoveToPlayer(float deltaTime, List<Zombie> allZombies)
        {
            // Вычисляем направление к игроку
            Vector2 direction = _player.Position - Position;
            float distanceToPlayer = direction.Length();

            // Нормализуем вектор направления
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            // Обновляем угол поворота, только если зомби далеко от игрока
            if (distanceToPlayer >= distanceToUpdateRotation)
            {
                Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }

            // Добавляем логику избегания других зомби
            foreach (var otherZombie in allZombies)
            {
                if (otherZombie != this) // Исключаем текущего зомби
                {
                    float distanceToOtherZombie = Vector2.Distance(Position, otherZombie.Position);

                    // Мягкое отталкивание на большом расстоянии
                    if (distanceToOtherZombie < 100f) // 150f — радиус мягкого отталкивания
                    {
                        Vector2 repulsionDirection = Position - otherZombie.Position;
                        repulsionDirection.Normalize();
                        direction += repulsionDirection * (100f - distanceToOtherZombie) / 100f; // Чем ближе, тем сильнее отталкивание
                    }

                    // Сильное отталкивание при близком контакте
                    if (distanceToOtherZombie < 40f) // 70f — минимальное расстояние между зомби
                    {
                        Vector2 strongRepulsionDirection = Position - otherZombie.Position;
                        strongRepulsionDirection.Normalize();
                        direction += strongRepulsionDirection * 0.5f; // Сильная сила отталкивания
                    }
                }
            }

            // Нормализуем направление после всех изменений
            direction.Normalize();

            // Обновляем позицию зомби
            Position += direction * Speed * deltaTime;

            // Проверяем столкновение с игроком

        }

        public void Update(float deltaTime, List<Zombie> zombies)
        {
            // Обновляем анимацию
            _timeSinceLastFrame += deltaTime;
            if (_timeSinceLastFrame >= _frameTime)
            {
                frame = (frame + 1) % 16; // Переключаемся на следующий кадр
                _timeSinceLastFrame = 0f;
            }
            if (Vector2.Distance(Position, _player.Position) < 50f) // 50f — радиус столкновения
            {
                _player.ChangeHealth(Damage); // Наносим урон игроку

            }
            // Обновляем движение
            MoveToPlayer(deltaTime, zombies);
        }

        public void DrawZombie(SpriteBatch sb)
        {
            sb.Draw(ZombieWalkTexture, Position, new Rectangle(frame * 246, 0, 234, 311), Color.White, Rotation, Origin, Scale, SpriteEffects.None, 1f);

        }
    }
}
