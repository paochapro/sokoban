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

    public static List<Point> Walls => walls;
    public static List<Point> Goals => goals;
    static List<Point> walls = new();
    static List<Point> goals = new();

    public static void LoadMap(string mapfile)
    {
        using (BinaryReader reader = new BinaryReader(File.OpenRead(mapFolder + mapfile)))
        {
            Point mapSize = Point.Zero;
            mapSize.X = reader.ReadByte();
            mapSize.Y = reader.ReadByte();

            Console.WriteLine($"Map size: {mapSize.X}, {mapSize.Y}");

            for (int y = 0; y < mapSize.Y; ++y)
            {
                for (int x = 0; x < mapSize.X; ++x)
                {
                    BlockType blockType = (BlockType)reader.ReadByte();
                    Point pos = new Point(x, y);

                    if (blockType == BlockType.None) continue;
                    if (blockType == BlockType.Wall) walls.Add(pos);
                    if (blockType == BlockType.Box) Boxes.Add(new Box(x, y));
                    if (blockType == BlockType.Goal) goals.Add(pos);
                    if (blockType == BlockType.Player) MyGame.playerSpawn = pos;
                }
                Console.WriteLine();
            }
        }
    }

    public static void ConvertToBinary(string mapfile)
    {
        string convertedMapFile = mapFolder + "converted_" + mapfile.Replace(".txt", ".bin");

        StreamReader reader = new StreamReader(mapFolder + mapfile);
        BinaryWriter writer = new BinaryWriter(File.Open(convertedMapFile, FileMode.OpenOrCreate));

        string sizeX = reader.ReadUntil(' ', true);
        string sizeY = reader.ReadUntil('\n', true);

        Point mapSize = new Point(int.Parse(sizeX), int.Parse(sizeY));

        Console.WriteLine($"bin mapSize: {mapSize.X}, {mapSize.Y}");

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

                Console.Write((byte)reader.Peek() - 48);
                Console.Write(" ");

                writer.Write((byte)(reader.Read() - 48));
            }
            Console.WriteLine();
        }

        reader.Close();
        writer.Close();
    }

    public enum BlockType { None, Wall, Box, Goal, Player }
}

class Boxes : Group<Box> {}

class Box : Entity
{
    //static readonly Texture2D boxTexture = MonoGame.LoadTexture("box");

    public override void Update(GameTime gameTime)
    {
    }

    public Box(int x, int y) 
        : base(new RectangleF( new Point(x * Map.BlockUnit, y * Map.BlockUnit), new Point(Map.BlockUnit)), null)
    {
    }
}