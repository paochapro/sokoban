using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Text;

namespace Sokoban;

using static Utils;

static class Utils
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

        if(a > b)  
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
        foreach(int chance in chances)
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
    static public float center(float x, float size) => x / 2 - size / 2;
    static public int center(int x, int x2, int size) => (x + x2) / 2 - size / 2;
    static public int center(int x, int size) => x / 2 - size / 2;
    static public Vector2 center(Rectangle rect) => new Vector2( rect.X + (float)rect.Width / 2, rect.Y + (float)rect.Height / 2);

    static public double avg(params double[] values) => values.Average();
    static public int percent(double value, double percent) => (int)Math.Round(value / 100 * percent);
    static public int Round(double value) => (int)Math.Round(value);
    static public int Round(float value) => (int)Math.Round(value);
}
static class MonoGame
{
    static public Microsoft.Xna.Framework.Content.ContentManager Content;

    static private void ContentAvaliable()
    {
        if (Content == null)
            throw new Exception("content is null in MonoGame");
    }

    static public T? Load<T>(string asset)
    {
        ContentAvaliable();

        if (!AssetExists(asset))
        {
            print("No asset \"" + asset + "\" was found in MonoGame:Load");
            return default(T);
        }
        return Content.Load<T>(asset);
    }
    static public Texture2D LoadTexture(string asset)
    {
        ContentAvaliable();

        if (!AssetExists(asset))
        {
            print("No texture \"" + asset + "\" was found in MonoGame:Load");
            asset = "error";
        }

        return Content.Load<Texture2D>(asset);
    }
    static private bool AssetExists(string asset)
    {
        ContentAvaliable();
        return File.Exists(Content.RootDirectory + @"\" + asset + ".xnb");
    }
}

class Entities : Group<Entity>
{
    static new public Entity Add(Entity ent)
    {
        ent.entityID = lastGroupID++;
        group.Add(ent);
        return ent;
    }
    static new public void Destroy(int id)
    {
        if (group.Count == 0) return;

        group.RemoveAt(id);
        lastGroupID--;

        //UpdateIDs
        foreach (Entity ent in group)
        {
            if (ent.entityID > id)
                ent.entityID--;
        }
    }
    static new public void Destroy(Entity entity)
    {
        Destroy(entity.entityID);
    }
}

//Entity
public abstract class Entity
{
    public int entityID { get; set; }
    public int groupID { get; set; }

    protected RectangleF rectangle;
    protected Texture2D texture;
    public RectangleF Rect => rectangle;
    public Texture2D Texture => texture;

    public Entity(RectangleF rectangle, Texture2D texture)
    {
        this.rectangle = rectangle;
        this.texture = texture;
    }

    public Entity() : this(new RectangleF(0,0,0,0), null) {}

    public virtual void Destroy() => Entities.Destroy(entityID);
    public virtual void OnTouch() {}

    public abstract void Update(GameTime gameTime);

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)rectangle;
        final.Location -= MyGame.Camera.ToPoint();

        spriteBatch.Draw(texture, final, Color.White);
    }
}


class DebugLines : Group<DebugLine>
{}

//Debug
class DebugLine : Entity
{
    public Vector2 p1;
    public Vector2 p2;
    public Color color;
    bool showCounter;
    string? customText;

    public DebugLine(Vector2 p1, Vector2 p2, Color color, bool showCounter = true, string? customText = null)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.color = color;
        this.showCounter = showCounter;
        this.customText = customText;
    }
    public DebugLine(Vector2 p1, Vector2 p2) 
        : this(p1,p2,Color.Red)
    {}
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!MyGame.Debug) return;

        Vector2 p1 = this.p1 - MyGame.Camera;
        Vector2 p2 = this.p2 - MyGame.Camera;

        //Line
        spriteBatch.DrawLine(p1,p2, color, 1);

        //Text
        if (!showCounter) return;
            
        float length = p1.Y - p2.Y;
        float textHeight = UI.Font.MeasureString(length.ToString()).Y;
        Vector2 textPos = new(p1.X + 15, center(p1.Y, p2.Y, textHeight));
        spriteBatch.DrawString(UI.Font, customText ?? length.ToString(), textPos, color);

        //Endings
        Vector2 endingP1 = new(p1.X - 4, p1.Y);
        Vector2 endingP2 = new(p1.X + 4, p1.Y);
        spriteBatch.DrawLine(endingP1, endingP2, color, 1);
        endingP1 = new(p2.X - 4, p2.Y);
        endingP2 = new(p2.X + 4, p2.Y);
        spriteBatch.DrawLine(endingP1, endingP2, color, 1);
    }
    public override void Destroy() => DebugLines.Destroy(groupID);
    public override void Update(GameTime gameTime) {}
}

//Events
class Event
{
    double delay;
    double startTime;
    static double globalTime;
    Action function;

    public Event(Action function, double delay)
    {
        this.delay = delay;
        this.function = function;
        startTime = globalTime;
    }

    static List<Event> events = new();

    static public void Add(Action func, double delay) => events.Add(new Event(func, delay));

    static public void ExecuteEvents(GameTime gameTime)
    {
        for (int i = 0; i < events.Count; ++i)
        {
            Event ev = events[i];
            if ((globalTime - ev.startTime) > ev.delay)
            {
                ev.function.Invoke();
                events.Remove(ev);
                --i;
            }
        }

        globalTime += gameTime.ElapsedGameTime.TotalSeconds;
    }
    
    static public void ClearEvents() => events.Clear();
}

//Ui
abstract class UI
{
    //Static
    static List<UI> elements = new();
    static bool clicking;
    public static bool Clicking => clicking;

    //Element
    protected Rectangle rect = Rectangle.Empty;
    protected Color mainColor = Color.Purple;
    protected Color bgColor = Color.Purple;

    //Main colors
    static protected Color mainDefaultColor = Color.Black;
    static protected Color mainSelectedColor = Color.Gold;
    static protected Color mainLockedColor = Color.Gray;

    //Bg colors
    static protected Color bgDefaultColor = Color.White;
    static protected Color bgSelectedColor = new Color(Color.Yellow, 50);
    static protected Color bgLockedColor = Color.DarkGray;

    public static SpriteFont Font { get; set; }

    public Point Position { get => rect.Location; set => rect.Location = value; }

    static UI()
    {
        Font = MonoGame.Load<SpriteFont>("bahnschrift")!;
    }

    protected string text;

    bool locked = false;
    protected bool allowHold;

    public bool Locked 
    {
        get => locked;
        set
        {
            locked = value;
            if(locked)
            {
                mainColor = mainLockedColor;
                bgColor = bgLockedColor;
            };
        }
    }

    public abstract void Activate();

    protected virtual void Update(MouseState mouse)
    {
        mainColor = mainDefaultColor;
        bgColor = bgDefaultColor;

        if (rect.Contains(mouse.Position))
        {
            if(!allowHold && clicking)
                return;
            
            mainColor = mainSelectedColor;
            bgColor = bgSelectedColor;

            Mouse.SetCursor(MouseCursor.Hand);

            if (mouse.LeftButton == ButtonState.Pressed)
                Activate();
        }
    }

    protected virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bgColor);
        spriteBatch.DrawRectangle(rect, mainColor, 4);

        float scale = 1f;

        Vector2 measure = Font.MeasureString(text) * scale;
        Vector2 position = new Vector2(center(rect).X - measure.X / 2, center(rect).Y - measure.Y / 2);

        spriteBatch.DrawString(Font, text, position, mainColor, 0, new Vector2(0,0), scale, SpriteEffects.None, 0);
    }

    protected readonly int layer = 0;
    public static int CurrentLayer { get; set; }

    protected UI(Rectangle rect, string text, int layer)
    {
        this.rect = rect;
        this.text = text;
        this.layer = layer;
    }

    static public void UpdateElements(MouseState mouse)
    {
        Mouse.SetCursor(MouseCursor.Arrow);

        foreach (UI element in elements)
            if (element.layer == CurrentLayer && !element.locked)
                element.Update(mouse);

        clicking = (mouse.LeftButton == ButtonState.Pressed);
    }
    static public void DrawElements(SpriteBatch spriteBatch)
    {
        foreach (UI element in elements)
            if (element.layer == CurrentLayer)
                element.Draw(spriteBatch);
    }
    static public T Add<T>(T elem) where T : UI
    {
        elements.Add(elem);
        return elem;
    }
}

class Button : UI
{
    event Action func;

    public Button(Rectangle rect, Action func, string text, int layer)
        : base(rect,text,layer)
    {
        this.func = func;
        allowHold = false;
    }
    public override void Activate() => func.Invoke();
}

class CheckBox : UI
{
    bool isChecked = false;
    bool IsChecked => isChecked;

    event Action act1;
    event Action act2;

    public CheckBox(Rectangle rect, Action act1, Action act2, string text, int layer)
        : base(rect, text, layer)
    {
        this.act1 = act1;
        this.act2 = act2;
        allowHold = false;
    }
    public override void Activate()
    {
        isChecked = !isChecked;

        if (isChecked)
        {
            act1.Invoke();
        }
        else
        {
            act2.Invoke();
        }
    }
}

class Slider : UI
{
    public const int sizeY = 50;
    const int sliderSizeX = 20;
    const int sliderOffset = 15;

    static readonly Point size = new(300 + sliderSizeX, sizeY);
    static readonly Point sliderSize = new(sliderSizeX, size.Y);
    static readonly int barSizeY = 10;

    int min, max;
    int sliderX;
    event Action<int> func;

    Point textSize;
    Point textPos;

    public Slider(Point pos, string text, Action<int> func, int defaultValue, int layer)
        : base(new Rectangle(pos, size), text, layer)
    {
        textSize = Font.MeasureString(text).ToPoint();
        textPos = new Point(pos.X, center(pos.Y, pos.Y + size.Y, textSize.Y) - 3);

        int offset = textSize.X + sliderOffset;

        rect.X += offset;
        min = rect.X;
        max = rect.X + size.X - sliderSize.X;
        sliderX = rect.X + defaultValue;
        allowHold = true;

        this.func = func;
    }

    public override void Activate()
    {
        sliderX = MyGame.mouse.Position.X;
        sliderX = clamp(sliderX, min, max);

        //Getting range from 0 to 100
        int denominator = (size.X - sliderSizeX) / 100;
        float value = (float)(sliderX - rect.X) / denominator;

        func.Invoke((int)Math.Round(value));
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(Font, text, textPos.ToVector2(), Color.Black);

        Rectangle bar = new Rectangle(rect.X, center(rect.Y, rect.Y+size.Y,barSizeY) , size.X, barSizeY);
        spriteBatch.FillRectangle(bar, Color.Gray);

        Rectangle slider = new Rectangle(sliderX, rect.Y, sliderSize.X, sliderSize.Y);
        spriteBatch.FillRectangle(slider, Color.White);
        spriteBatch.DrawRectangle(slider, Color.Black, 3);
    }
}

public class Group<T> where T : Entity
{
    static protected List<T> group = new List<T>();
    static protected int lastGroupID = 0;

    static public int Count => group.Count;
    static public List<T> All => group;

    static public T Get(int i) => group[i];

    //General
    static public void Update(GameTime gameTime)
    {
        for (int i = 0; i < group.Count; ++i)
            group[i].Update(gameTime);
    }
    static public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < group.Count; ++i)
            group[i].Draw(spriteBatch);
    }
    static public void Add(T ent)
    {
        ent.groupID = lastGroupID++;
        group.Add(ent);
        Entities.Add(ent);
    }

    protected Group() { }

    //Destroy
    static public void Destroy(int id)
    {
        if (group.Count == 0) return;
        Entities.Destroy(group[id].entityID);

        group.RemoveAt(id);
        lastGroupID--;

        //UpdateIDs
        foreach (T ent in group)
        {
            if (ent.groupID > id)
                ent.groupID--;
        }
    }
    static public void Destroy(Entity entity)
    {
        Destroy(entity.groupID);
    }
    //Clear
    static public void Clear()
    {
        group.ForEach(e => Entities.Destroy(e));
        group.Clear();
        lastGroupID = 0;
    }
}