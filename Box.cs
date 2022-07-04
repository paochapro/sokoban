using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Sokoban;

using static Utils;

class Boxes : Group<Box> { }

class Box : Entity
{
    //static readonly Texture2D boxTexture = MonoGame.LoadTexture("box");

    bool moving = false;

    static private void UpdateGoals()
    {
        int completedGoals = 0;
        for (int b = 0; b < Boxes.Count; ++b)
        {
            Box box = Boxes.Get(b);

            for (int g = 0; g < Map.Goals.Count; ++g)
            {
                if(box.Rect.Position == Map.Goals[g])
                    completedGoals++;
            }
        }

        if(completedGoals == Map.Goals.Count)
        {
            print("Win");
        }
    }

    public bool TryMove(int x, int y)
    {
        if (x != 0 && y != 0)
            return false;

        Point move = new Point(x, y);
        Point finalPos = ((Rectangle)rectangle).Location + move;

        if (Map.Walls[finalPos.Y, finalPos.X])
            return false;

        moving = true;
        for (int i = 0; i < Boxes.Count; ++i)
        {
            Box box = Boxes.Get(i);

            if (box == this || box.moving) 
                continue;

            if (box.Rect.Position == finalPos)
            {
                if (!box.TryMove(x, y))
                {
                    moving = false;
                    return false;
                }
            }
                
        }
        moving = false;

        rectangle.Position += move;

        UpdateGoals();
        return true;
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)rectangle;

        final.Location *= new Point(Map.BlockUnit);
        final.Size *= new Point(Map.BlockUnit);

        spriteBatch.FillRectangle(final, Color.Green);
        spriteBatch.DrawRectangle(final, Color.DarkGreen, 5);
    }

    public Box(int x, int y)
        : base(new RectangleF(new Point(x, y), new Point(1)), null)
    {
    }
}