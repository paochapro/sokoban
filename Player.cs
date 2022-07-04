using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Sokoban;

using static Utils;

class Player : Entity
{
    bool pressingUp;
    bool pressingDown;
    bool pressingLeft;
    bool pressingRight;

    private void TryMove(int x, int y)
    {
        Point move = new Point(x, y);
        Point finalPos = ((Rectangle)rectangle).Location + new Point(x, y);
        if (Map.Walls[finalPos.Y, finalPos.X]) return;
        if (!TryPush(move, finalPos)) return;

        rectangle.Position += move;
    }

    private bool TryPush(Point move, Point final)
    {
        for(int i=0; i < Boxes.Count; ++i)
        {
            Box box = Boxes.Get(i);

            if(box.Rect.Position == final)
                return box.TryMove(move.X, move.Y);
        }

        return true;
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
        Rectangle final = (Rectangle)rectangle;

        final.Location *= new Point(Map.BlockUnit);
        final.Size *= new Point(Map.BlockUnit);

        spriteBatch.FillRectangle(final, Color.Blue);
        spriteBatch.DrawRectangle(final, Color.DarkBlue, 5);
    }

    public Player(Point pos)
        : base( new RectangleF(pos, new Vector2(1)), null)
    {
    }
}
