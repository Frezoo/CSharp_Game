using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace The_Rotting
{
    public class ZombieSpawner
    {
        public Texture2D EnemyWalkTexture;
        private Random rnd = new Random();
        private Player player;
        public int Wave { get; private set; } = 1; // Текущая волна, начинается с 1
        private bool waveStarted = false;

        public List<Zombie> Zombies = new List<Zombie>();
        int enemyCount => Zombies.Count();

        public ZombieSpawner(Texture2D enemyWalkTexture, Player player)
        {
            EnemyWalkTexture = enemyWalkTexture;
            this.player = player;
        }

        public void Update()
        {
            if (!waveStarted)
            {
                StartWave();
                waveStarted = true;
            }

            // Удаление мертвых зомби - предполагается, что IsAlive работает
            Zombies.RemoveAll(zombie => !zombie.IsAlive);

            // Проверка на окончание волны (все зомби убиты)
            if (enemyCount == 0 && waveStarted)
            {
                Wave++;
                waveStarted = false; // Сбрасываем, чтобы можно было начать новую волну
            }
        }

        private void StartWave()
        {
            int numberOfEnemies = CalculateEnemiesForWave(Wave);
            SpawnEnemies(numberOfEnemies);
        }

        private int CalculateEnemiesForWave(int waveNumber)
        {
            int baseEnemies = 5;
            float increasePercentage = 0.2f; // 20% увеличение за волну

            return (int)(baseEnemies * Math.Pow(1 + increasePercentage, waveNumber - 1));
        }

        private void SpawnEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Zombies.Add(new Zombie(GetSpawnPosition(), EnemyWalkTexture, player));
            }
        }

        private Vector2 GetSpawnPosition()
        {
            return new Vector2(rnd.Next(50, 1230), rnd.Next(50, 670));
        }

    }
}