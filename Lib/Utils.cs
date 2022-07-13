using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Sokoban;

internal static class Utils
{
    static public void print(params object[] args)
    {
        foreach (object var in args)
            Console.Write(var + " ");
        Console.WriteLine();
    }
    static public int clamp(int value, int min, int max)
    {
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }
    static public float clamp(float value, float min, float max)
    {
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }
    static public string ReadUntil(this StreamReader reader, char value, bool skipLast = false)
    {
        string result = "";
        int peek = reader.Peek();

        while (Convert.ToChar(peek) != value && peek != -1)
        {
            result += Convert.ToChar(reader.Read());
            peek = reader.Peek();
        }

        if (skipLast) reader.Read();

        return result;
    }
    
    //Randomness
    static public int RandomBetween(int a, int b)
    {
        int seed = (int)DateTime.Now.Ticks;

        if (a > b)
        {
            (a, b) = (b, a);
        }

        return new Random(seed).Next(a, b);
    }
    static public int Random(int min, int max)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).Next(min, max);
    }

    static public float RandomFloat(float min, float max)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).NextSingle(min, max);
    }

    static public bool Chance(int percent)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).Next(100) < percent;
    }
    static public int Chance(params int[] chances)
    {
        if (chances.Sum() != 100)
            return -1;

        int seed = (int)DateTime.Now.Ticks;
        int randomNumber = new Random(seed).Next(100) + 1;

        int previousSum = 0;
        int index = 0;
        foreach (int chance in chances)
        {
            if (randomNumber <= previousSum + chance &&
                randomNumber > previousSum)
            {
                return index;
            }

            index++;
            previousSum += chance;
        }

        //Error, impossible
        return -2;
    }

    //Math
    static public float center(float x, float x2, float size) => (x + x2) / 2 - size / 2;
    static public int center(int x, int x2, int size) => (x + x2) / 2 - size / 2;
    static public float center(float x, float size) => x / 2 - size / 2;
    static public int center(int x, int size) => x / 2 - size / 2;
    static public Vector2 center(Rectangle rect) => new Vector2(rect.X + (float)rect.Width / 2, rect.Y + (float)rect.Height / 2);

    static public int percent(double value, double percent) => (int)Math.Round(value / 100 * percent);
    static public double avg(params double[] values) => values.Average();
    static public int Round(double value) => (int)Math.Round(value);
    static public int Round(float value) => (int)Math.Round(value);
}
