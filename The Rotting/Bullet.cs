using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace The_Rotting
{
    public class Bullet
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float BulletDamage = 0.3f;
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
}
