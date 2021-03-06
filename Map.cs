using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Text;

namespace Sokoban;
using static Utils;

static class Map
{
    public static readonly int BlockUnit = 48;
    public const string mapFolder = @"Content\maps\";

    public static Point Size { get; private set; }
    public static bool[,] Walls => walls;
    private static bool[,] walls;
    public static List<Point> Goals => goals;
    private static List<Point> goals = new List<Point>();
    public enum BlockType { None, Wall, Box, Goal, Player, BoxGoal, PlayerGoal }

    public static bool Exists(string mapfile) => File.Exists(mapFolder + mapfile);
    
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
            Size = mapSize;

            walls = new bool[mapSize.Y, mapSize.X];

            Console.WriteLine($"Map size: {mapSize.X}, {mapSize.Y}");

            for (int y = 0; y < mapSize.Y; ++y)
            {
                for (int x = 0; x < mapSize.X; ++x)
                {
                    BlockType blockType = (BlockType)reader.ReadByte();
                    Point pos = new Point(x, y);

                    if (blockType == BlockType.None)    continue;
                    if (blockType == BlockType.Wall)    walls[y, x] = true;
                    if (blockType == BlockType.Box      || blockType == BlockType.BoxGoal)     Box.Boxes.Add(new Box(x, y));
                    if (blockType == BlockType.Player   || blockType == BlockType.PlayerGoal)  MyGame.playerSpawn = pos;

                    if (blockType == BlockType.Goal ||
                        blockType == BlockType.BoxGoal ||
                        blockType == BlockType.PlayerGoal)
                    {
                        goals.Add(pos);
                    }
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
    
    //Colors
    private static readonly Color bgGridLight = new Color(247, 242, 212);
    private static readonly Color bgGridDark = new Color(242, 237, 207);
    private static readonly Color wallColor = new Color(35,106,185);
    private static readonly Color goalColor = new Color(207, 202, 172);
    private static readonly Color outlineColor =  new Color(15, 86, 165);

    public static void DrawMap(SpriteBatch spriteBatch)
    {
        //Bg
        bool light = false;
        for (int y = 0; y < Size.Y; ++y)
        {
            for(int x = 0; x < Size.X; ++x)
            {
                Color color = light ? bgGridLight : bgGridDark;
                spriteBatch.FillRectangle(new Rectangle(new Point(x,y) * new Point(BlockUnit), new Point(BlockUnit)), color);
                light = !light;
            }

            if(Size.X % 2 == 0)
                light = !light;
        }

        //Walls
        for (int y = 0; y < walls.GetLength(0); ++y)
        for (int x = 0; x < walls.GetLength(1); ++x)
            if (walls[y,x])
                spriteBatch.FillRectangle(new Rectangle(new Point(x,y) * new Point(BlockUnit), new Point(BlockUnit)), wallColor);

        //Wall outlines
        foreach (Rectangle outline in outline)
            spriteBatch.FillRectangle(outline, outlineColor);

        //Goals
        foreach (Point goal in goals)
            spriteBatch.FillRectangle(new Rectangle(goal * new Point(BlockUnit), new Point(BlockUnit)), goalColor);
    }
}
