using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Sokoban;

using static Utils;

class Player : ITimeShiftable
{
    List<Point> timePosition = new();
    Point position = Point.Zero;

    public void ResetTime()
    {
        timePosition.Clear();
        NewTime(0);
    }

    public void ShiftTo(int time)
    {
        position = timePosition[time];
    }

    public void NewTime(int time)
    {
        if(time >= timePosition.Count)
            timePosition.Add(Point.Zero);

        timePosition.Insert(time, position);
    }

    public void SetPosition(Point pos) => position = pos;

    private bool TryMove(int x, int y)
    {
        if (x == 0 && y == 0) return false;
        
        Point move = new Point(x, y);
        Point finalPos = position + new Point(x, y);
        
        if (x != 0 && y != 0)
        {
            bool verticalMove = TryMove(0, y);
            bool horizontalMove = TryMove(x, 0);

            if(verticalMove && !horizontalMove) TryMove(x, 0);
            if(!verticalMove && horizontalMove) TryMove(0, y);
                
            return verticalMove && horizontalMove;
        }

        
        if (Map.Walls[finalPos.Y, finalPos.X]) return false;
        if (!TryPush(move, finalPos)) return false;

        position += move;

        MyGame.TimeAdd();

        return true;
    }

    private bool TryPush(Point move, Point final)
    {
        foreach(Box box in Box.Boxes)
        {
            if(box.position == final)
                return box.TryMove(move.X, move.Y);
        }

        return true;
    }


    bool pressingUp, pressingDown, pressingLeft, pressingRight;
    public bool PlayerMove()
    {
        int horizontalMove = 0;
        int verticalMove = 0;

        if(MyGame.IsKeyPressed(Keys.Left))      horizontalMove = -1;
        if(MyGame.IsKeyPressed(Keys.Right))     horizontalMove = 1;
        if(MyGame.IsKeyPressed(Keys.Up))        verticalMove = -1;
        if(MyGame.IsKeyPressed(Keys.Down))      verticalMove = 1;

        pressingUp      = MyGame.keys.IsKeyDown(Keys.Up);
        pressingDown    = MyGame.keys.IsKeyDown(Keys.Down);
        pressingLeft    = MyGame.keys.IsKeyDown(Keys.Left);
        pressingRight   = MyGame.keys.IsKeyDown(Keys.Right);

        return TryMove(horizontalMove, verticalMove);
    }

    private readonly Color fill = new(24, 44, 67);
    private readonly Color outline = new Color(9, 29, 52);
    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = new Rectangle(position * new Point(Map.BlockUnit), new Point(Map.BlockUnit));

        spriteBatch.FillRectangle(final, fill);
        spriteBatch.DrawRectangle(final, outline, 5);
    }

    public Player(Point pos) => position = pos;
    
}
