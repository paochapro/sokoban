using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Text;

namespace Sokoban;

using static Utils;

static class Map
{
    public static readonly int BlockUnit = 48;
    const string mapFolder = @"Content\maps\";

    public static bool[,] Walls => walls;
    public static List<Point> Goals => goals;
    static bool[,] walls;
    static List<Point> goals = new List<Point>();

    public static bool LoadMap(string mapfile)
    {
        if (!File.Exists(mapFolder + mapfile))
        {
            Console.WriteLine(mapfile + " wasnt found!");
            return false;
        }

        using (BinaryReader reader = new BinaryReader(File.OpenRead(mapFolder + mapfile)))
        {
            Point mapSize = Point.Zero;
            mapSize.X = reader.ReadByte();
            mapSize.Y = reader.ReadByte();

            walls = new bool[mapSize.Y, mapSize.X];

            Console.WriteLine($"Map size: {mapSize.X}, {mapSize.Y}");

            for (int y = 0; y < mapSize.Y; ++y)
            {
                for (int x = 0; x < mapSize.X; ++x)
                {
                    BlockType blockType = (BlockType)reader.ReadByte();
                    Point pos = new Point(x, y);

                    if (blockType == BlockType.None) continue;
                    if (blockType == BlockType.Wall) walls[y, x] = true;
                    if (blockType == BlockType.Box) Box.Boxes.Add(new Box(x, y));
                    if (blockType == BlockType.Goal) goals.Add(pos);
                    if (blockType == BlockType.Player) MyGame.playerSpawn = pos;
                }
            }
        }

        CreateOutline();

        return true;
    }

    public static void ConvertToBinary(string mapfile, string dest)
    {
        string destFile = mapFolder + dest;

        StreamReader reader = new StreamReader(mapFolder + mapfile);
        BinaryWriter writer = new BinaryWriter(File.Open(destFile, FileMode.OpenOrCreate));

        string sizeX = reader.ReadUntil(' ', true);
        string sizeY = reader.ReadUntil('\n', true);

        Point mapSize = new Point(int.Parse(sizeX), int.Parse(sizeY));

        writer.Seek(0, SeekOrigin.Begin);
        writer.Write((byte)mapSize.X);
        writer.Write((byte)mapSize.Y);

        for (int y = 0; y < mapSize.Y; ++y)
        {
            for (int x = 0; x < mapSize.X; ++x)
            {
                if(reader.Peek() == '\n' || reader.Peek() == 13)
                {
                    --x;
                    reader.Read();
                    continue;
                }

                bool tileAvaliable = false;
                for(char numb = '0'; numb < ('9'+1); ++numb)
                {
                    if (reader.Peek() != numb)
                        continue;

                    tileAvaliable = true;
                }

                if(!tileAvaliable)
                {
                    throw new Exception("Wrong tile type in ConvertToBinary");
                }

                writer.Write((byte)(reader.Read() - 48));
            }
        }

        reader.Close();
        writer.Close();
    }

    public enum BlockType { None, Wall, Box, Goal, Player }

    static List<Rectangle> outline = new();
    static public List<Rectangle> Outline => outline;
    const int thickness = 5;

    static void CreateOutline()
    {
        outline.Clear();
        int bu = BlockUnit;

        for (int y = 0; y < walls.GetLength(0); ++y)
        {
            for (int x = 0; x < walls.GetLength(1); ++x)
            {
                if (!walls[y, x]) continue;

                var Check = (Point wall, Point pos, Point size) =>
                {
                    wall = new Point(wall.Y, wall.X);

                    if (wall.X >= walls.GetLength(1) || wall.X < 0) return;
                    if (wall.Y >= walls.GetLength(0) || wall.Y < 0) return;

                    if (!walls[wall.Y, wall.X])
                        outline.Add(new Rectangle(pos, size));
                };

                Check(new Point(y, x-1),    new Point(x * bu, y * bu),  new Point(thickness, bu));          //Left
                Check(new Point(y-1, x-1),  new Point(x * bu, y * bu),  new Point(thickness, thickness));   //Left top
                Check(new Point(y-1, x),    new Point(x * bu, y * bu),  new Point(bu, thickness));          //Top

                Check(new Point(y-1, x+1),  new Point(x * bu + bu - thickness, y * bu), new Point(thickness, thickness));   //Right top
                Check(new Point(y, x+1),    new Point(x * bu + bu - thickness, y * bu), new Point(thickness, bu));          //Right
                Check(new Point(y+1, x-1),  new Point(x * bu, y * bu + bu - thickness), new Point(thickness, thickness));   //Left bottom
                Check(new Point(y+1, x),    new Point(x * bu, y * bu + bu - thickness), new Point(bu, thickness));          //Bottom

                Check(new Point(y+1, x+1),  new Point(x * bu + bu - thickness, y * bu + bu - thickness),    new Point(thickness, thickness)); //Right bottom
            }
        }
    }
}
