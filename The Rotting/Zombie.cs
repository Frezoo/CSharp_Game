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
        public float Speed; // Скорость 
        public float Damage = 0.1f; // Урон, наносимый игроку
        public float Health = 1f; // Здоровье врага
        public bool IsAlive = true; // Проверка, жив ли враг
        private Player _player; // Ссылка на игрока
                            
        private float _frameTime = 0.03f; // Время между кадрами (в секундах)
        private float _timeSinceLastFrame = 0f; // Время с последнего кадра
        const float distanceToUpdateRotation = 30;
        private float distanceToPlayer;

        private float fadeAlpha = 1f; // Уровень прозрачности (1f = полностью видимый, 0f = полностью прозрачный)
        private bool isFading = false; // Флаг для отслеживания состояния затухания
        private float fadeSpeed = 3f; // Скорость затухания (чем больше, тем быстрее)

        private bool isTakingDamage = false; // Флаг для отслеживания состояния "моргания"
        private float damageBlinkTimer = 0f; // Таймер для эффекта моргания
        private const float damageBlinkDuration = 0.05f; // Продолжительность моргания (в секундах)
        private Color damageColor = Color.Red; // Цвет для моргания

        const float radiusOfSoftRepulsion = 100;
        const float minimumDistanceBetweenZombies = 40;
        const float distanceToAttack = 3;


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
            distanceToPlayer = direction.Length();

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
                    if (distanceToOtherZombie < radiusOfSoftRepulsion) 
                    {
                        Vector2 repulsionDirection = Position - otherZombie.Position;
                        repulsionDirection.Normalize();
                        direction += repulsionDirection * (100f - distanceToOtherZombie) / 100f; // Чем ближе, тем сильнее отталкивание
                    }

                    // Сильное отталкивание при близком контакте
                    if (distanceToOtherZombie < minimumDistanceBetweenZombies) 
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


            

        }

        public void Attack()
        {
            if (distanceToPlayer <= distanceToAttack)
            {
                _player.ChangeHealth(Damage);
            }
        }

        public void Update(float deltaTime, List<Zombie> zombies)
        {
            if (Health <= 0  && !isFading)
            {
                isFading = true; // Начинаем затухание
            }

            if (isTakingDamage)
            {
                // Уменьшаем таймер моргания
                damageBlinkTimer -= deltaTime;

                // Если таймер закончился, выключаем моргание
                if (damageBlinkTimer <= 0)
                {
                    isTakingDamage = false;
                    damageBlinkTimer = 0;
                }
            }


            if (isFading)
            {
                fadeAlpha -= fadeSpeed * deltaTime; // Уменьшаем прозрачность
                fadeAlpha = Math.Max(fadeAlpha, 0); // Не позволяем прозрачности стать меньше 0

                if (fadeAlpha <= 0)
                {
                    IsAlive = false;
                }
            }
            else
            {
                // Если зомби еще жив, обновляем его движение и анимацию
                _timeSinceLastFrame += deltaTime;
                if (_timeSinceLastFrame >= _frameTime)
                {
                    frame = (frame + 1) % 16;
                    _timeSinceLastFrame = 0f;
                }

                MoveToPlayer(deltaTime, zombies);
                Attack();
            }
        }

        public void DrawZombie(SpriteBatch sb)
        {
            // Создаем цвет с учетом прозрачности
            Color drawColor =  isTakingDamage ? damageColor : Color.White * fadeAlpha;

            // Отрисовываем зомби
            sb.Draw(
                ZombieWalkTexture,
                Position,
                new Rectangle(frame * 246, 0, 234, 311),
                drawColor,
                Rotation,
                Origin,
                Scale,
                SpriteEffects.None,
                0f
            );
        }

        public void GetDamage(float damage)
        {
            if (Health >= 0)
            {
                Health -= damage;
                isTakingDamage = true;
                damageBlinkTimer = damageBlinkDuration;
            }
     
        }
    }
}
