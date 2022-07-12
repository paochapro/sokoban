using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Sokoban;

using static Utils;

class Box : ITimeShiftable
{
    static List<Box> boxes = new();
    static public List<Box> Boxes => boxes;

    static readonly Color defaultFill       = new Color(214, 26, 70);
    static readonly Color defaultOutline    = new Color(194, 6, 50);
    static readonly Color goalFill          = new Color(214, 66, 40);
    static readonly Color goalOutline       = new Color(194, 46, 20);

    Color fill = defaultFill;
    Color outline = defaultOutline;

    static public void UpdateGoals()
    {
        int completedGoals = 0;

        foreach (Box box in boxes)
        {
            box.fill = defaultFill;
            box.outline = defaultOutline;

            foreach (Point goal in Map.Goals)
            {
                if (box.position == goal)
                {
                    completedGoals++;
                    box.fill = goalFill;
                    box.outline = goalOutline;
                }
            }
        }

        if (completedGoals == Map.Goals.Count)
            MyGame.MapCompleted();
    }

    public Point position { get; private set; }
    List<Point> timePosition = new();

    public void ResetTime() => timePosition.Clear();
    

    public void ShiftTo(int time)
    {
        position = timePosition[time];
    }

    public void NewTime(int time)
    {
        if (time >= timePosition.Count)
            timePosition.Add(Point.Zero);

        timePosition.Insert(time, position);
    }

    bool moving = false;

    public bool TryMove(int x, int y)
    {
        if (x != 0 && y != 0)
            return false;

        Point move = new Point(x, y);
        Point finalPos = position + move;

        if (Map.Walls[finalPos.Y, finalPos.X] || 
            boxes.Select(b => b.position).Contains(finalPos))
        {
            return false;
        }

        /*moving = true;
        foreach (Box box in boxes)
        {
            if (box == this || box.moving)
                continue;

            if (box.position == finalPos)
            {
                if (!box.TryMove(x, y))
                {
                    moving = false;
                    return false;
                }
            }
        }
        moving = false;*/

        position += move;

        UpdateGoals();
        return true;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = new Rectangle(position * new Point(Map.BlockUnit), new Point(Map.BlockUnit));

        spriteBatch.FillRectangle(final, fill);
        spriteBatch.DrawRectangle(final, outline, 5);
    }

    public Box(int x, int y) => position = new Point(x, y);
}