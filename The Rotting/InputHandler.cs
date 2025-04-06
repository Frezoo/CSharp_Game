using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace The_Rotting
{
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
