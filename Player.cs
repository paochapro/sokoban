using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Sokoban;

using static Utils;

class Player : Entity
{
    public static readonly int sizeUnit = Map.BlockUnit;
    Point position;

    bool pressingUp;
    bool pressingDown;
    bool pressingLeft;
    bool pressingRight;

    private void TryMove(int x, int y)
    {
        foreach (Point wall in Map.Walls)
        {
            if (position + new Point(x,y) == wall)
                return;
        }

        position += new Point(x,y);
        rectangle.Position = new(position.X * sizeUnit, position.Y * sizeUnit);
    }

    private void Controls()
    {
        int horizontalMove = 0;
        int verticalMove = 0;
        if(MyGame.keys.IsKeyDown(Keys.Left)     && !pressingLeft)   horizontalMove = -1;
        if(MyGame.keys.IsKeyDown(Keys.Right)    && !pressingRight)  horizontalMove = 1;
        if(MyGame.keys.IsKeyDown(Keys.Up)       && !pressingUp)     verticalMove = -1;
        if(MyGame.keys.IsKeyDown(Keys.Down)     && !pressingDown)   verticalMove = 1;

        TryMove(horizontalMove, verticalMove);

        pressingUp      = MyGame.keys.IsKeyDown(Keys.Up);
        pressingDown    = MyGame.keys.IsKeyDown(Keys.Down);
        pressingLeft    = MyGame.keys.IsKeyDown(Keys.Left);
        pressingRight   = MyGame.keys.IsKeyDown(Keys.Right);
    }

    public override void Update(GameTime gameTime)
    {
        Controls();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rectangle, Color.Blue);
        spriteBatch.DrawRectangle(rectangle, Color.DarkBlue, 5);
    }

    public Player(Point pos)
        : base( new RectangleF(pos * new Point(sizeUnit), new Vector2(sizeUnit, sizeUnit)), null)
    {
        position = pos;
    }
}
