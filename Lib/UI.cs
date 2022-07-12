using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Text;

namespace Sokoban;

using static Utils;

//Ui
internal abstract class UI
{
    //Static
    static List<UI> elements = new();
    static bool clicking;
    public static bool Clicking => clicking;
    public static GameWindow window;

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
        Font = Assets.Load<SpriteFont>("bahnschrift")!;
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
            if (locked)
            {
                mainColor = mainLockedColor;
                bgColor = bgLockedColor;
            };
        }
    }

    public abstract void Activate();

    protected virtual void Update(KeyboardState keys, MouseState mouse)
    {
        mainColor = mainDefaultColor;
        bgColor = bgDefaultColor;

        if (rect.Contains(mouse.Position) && !Clicking)
        {
            Mouse.SetCursor(MouseCursor.Hand);

            mainColor = mainSelectedColor;
            bgColor = bgSelectedColor;

            if (mouse.LeftButton == ButtonState.Pressed)
                Activate();
        }
    }

    protected abstract void Draw(SpriteBatch spriteBatch);

    protected readonly int layer = 0;
    public static int CurrentLayer { get; set; }

    protected UI(Rectangle rect, string text, int layer)
    {
        this.rect = rect;
        this.text = text;
        this.layer = layer;
    }

    static public void UpdateElements(KeyboardState keys, MouseState mouse)
    {
        Mouse.SetCursor(MouseCursor.Arrow);

        foreach (UI element in elements)
            if (element.layer == CurrentLayer && !element.locked)
                element.Update(keys, mouse);

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

//Button
internal class Button : UI
{
    event Action func;

    public Button(Rectangle rect, Action func, string text, int layer)
        : base(rect, text, layer)
    {
        this.func = func;
        allowHold = false;
    }
    public override void Activate() => func.Invoke();

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bgColor);
        spriteBatch.DrawRectangle(rect, mainColor, 4);
        
        float scale = 1f;
        
        Vector2 measure = Font.MeasureString(text) * scale;
        Vector2 position = new Vector2(center(rect).X - measure.X / 2, center(rect).Y - measure.Y / 2);
        
        spriteBatch.DrawString(Font, text, position, mainColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
    }
}

internal class TextureButton : Button
{
    static int instance = 0;
    Texture2D texture;

    public TextureButton(Texture2D texture, Rectangle rect, Action func, int layer)
        : base(rect, func, "TEXTURE_BUTTON" + (instance++), layer)
    {
        this.texture = texture;
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, rect, Color.White);
    }
}

internal class CheckBox : UI
{
    static readonly Point size = new Point(24, 24);
    const int textOffset = 5;

    bool isChecked = false;
    bool IsChecked => isChecked;
    event Action act1;
    event Action act2;

    public CheckBox(Point position, Action act1, Action act2, string text, int layer)
        : base( new Rectangle(position, size), text, layer)
    {
        this.act1 = act1;
        this.act2 = act2;
        allowHold = false;
        rect.Width = (int)Font.MeasureString(text).X;
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

    protected override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle box = new(rect.Location, size);
        spriteBatch.FillRectangle(box, bgColor);
        spriteBatch.DrawRectangle(box, mainColor, 3);

        float scale = 0.7f;

        Vector2 measure = Font.MeasureString(text) * scale;
        Vector2 position = new Vector2(box.Right + textOffset, center(box.Y, box.Bottom, measure.Y));

        spriteBatch.DrawString(Font, text, position, mainColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);

        if(isChecked)
        {
            spriteBatch.DrawCircle(new CircleF(center(box).ToPoint(), 7f), 32, mainColor, 7);
        }
    }
}

internal class Slider : UI
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

        Rectangle bar = new Rectangle(rect.X, center(rect.Y, rect.Y + size.Y, barSizeY), size.X, barSizeY);
        spriteBatch.FillRectangle(bar, Color.Gray);

        Rectangle slider = new Rectangle(sliderX, rect.Y, sliderSize.X, sliderSize.Y);
        spriteBatch.FillRectangle(slider, Color.White);
        spriteBatch.DrawRectangle(slider, Color.Black, 3);
    }
}

class TextBox : UI
{
    const string avaliableCharaters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-=!@#$%^&*()_+[]{};':|\\\",./<>?`~ ";

    public string Text => writtenText.ToString();
    StringBuilder writtenText = new();

    bool focus = false;

    public TextBox(Rectangle rect, string text, int layer)
        : base(rect, text, layer)
    {
        mainColor = Color.Black;
        bgColor = Color.White;
    }

    public override void Activate()
    {
        if (focus) return;

        Console.WriteLine("TB Activated"); 
        window.TextInput += TextInput;
        focus = true;        
    }

    public void Deactivate()
    {
        Console.WriteLine("TB Deactivated");
        window.TextInput -= TextInput;
        focus = false;
    }


    private void TextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Character == '\b')
        {
            if (writtenText.Length > 0)
                writtenText.Remove(writtenText.Length - 1, 1);
        }

        if(avaliableCharaters.Contains(e.Character))
            writtenText.Append(e.Character);
    }

    protected override void Update(KeyboardState keys, MouseState mouse)
    {
        if (rect.Contains(mouse.Position))
        {
            Mouse.SetCursor(MouseCursor.IBeam);

            if (mouse.LeftButton == ButtonState.Pressed && !Clicking)
            {
                Activate();
                return;
            }
        }
        
        bool unfocusInput = (
            keys.IsKeyDown(Keys.Escape) ||
            (mouse.LeftButton == ButtonState.Pressed && !Clicking)
        ); 
        
        if (focus && unfocusInput)
            Deactivate();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bgColor);

        if(writtenText.Length == 0 && !focus)
            spriteBatch.DrawString(Font, text, rect.Location.ToVector2() + new Vector2(10, 10), Color.Gray);
        else
            spriteBatch.DrawString(Font, writtenText, rect.Location.ToVector2() + new Vector2(10, 10), mainColor);

        spriteBatch.DrawRectangle(rect, focus ? Color.Blue : mainColor, 3);
    }
}