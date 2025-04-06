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


        public List<Zombie> Zombies = new List<Zombie>();
        int enemyCount => Zombies.Count();
        int maxEnemiesCount;


        public ZombieSpawner(Texture2D enemyWalkTexture, int maxEnemiesCount, Player player)
        {
            EnemyWalkTexture = enemyWalkTexture;
            this.maxEnemiesCount = maxEnemiesCount;
            this.player = player;
        }

        public void SpawnEnemies()
        {
            while (enemyCount < maxEnemiesCount)
            {
                Zombies.Add(new Zombie(new Vector2(rnd.Next(50, 1230), rnd.Next(50, 670)), EnemyWalkTexture, player));
            }
        }
    }
}
