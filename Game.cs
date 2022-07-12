using System.Globalization;
using System.Net.Security;
using System.Security.AccessControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Sokoban;
using static Utils;

//////////////////////////////// Starting point
class MyGame : Game
{
    //More important stuff
    public static readonly Point screenSize = new(Map.BlockUnit * 12, Map.BlockUnit * 12);
    const string gameName = "Sokoban";
    const float defaultVolume = 0.3f;
    private const bool resizable = false;

    static public Point playerSpawn = new(0, 0);

    //General stuff
    static public GraphicsDeviceManager Graphics => graphics;
    static GraphicsDeviceManager graphics;
    static SpriteBatch spriteBatch;
    public static MouseState mouse { get => Mouse.GetState(); }
    public static KeyboardState keys { get => Keyboard.GetState(); }
    public static KeyboardState previousKeys;

    static private Vector2 camera;
    static public Vector2 Camera => camera;
    static public bool Debug { get; private set; } = true;

    public enum GameState { Menu, Game }

    private static GameState gameState; //never use this
    
    public static void SetGameState(GameState gameState)
    {
        MyGame.gameState = gameState;
        UI.CurrentLayer = Convert.ToInt32(gameState);
    }

    static readonly Dictionary<GameState, Action> drawMethods = new()
    {
        [GameState.Menu] = DrawMenu,
        [GameState.Game] = DrawGame,
    };

    //Game
    public static Player Player => player;
    static Player player;
    static int currentMap = 1;
    static int currentTime;
    const int totalMaps = 5;
    private const string mapExtension = ".bin";
    
    //Initialization
    static bool mapCompleted = false;
    static public void MapCompleted() => mapCompleted = true;
    
    static private void StartMap(int map)
    {
        currentMap = map;
        StartMap("map" + map);
    }
    
    static private void StartMap(string map)
    {
        SetGameState(GameState.Game);

        Console.WriteLine("starting map: " + map);
        
        Box.Boxes.Clear();
        Map.Goals.Clear();
        player.ResetTime();

        Map.LoadMap(map + mapExtension);
        player.SetPosition(playerSpawn);

        Reset();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        Assets.Content = Content;
        UI.Font = Content.Load<SpriteFont>("bahnschrift");
        UI.window = Window;
        CreateUi();

        player = new Player(playerSpawn);

        SetGameState(GameState.Menu);
    }

    protected override void Initialize()
    {
        Window.AllowUserResizing = resizable;
        Window.Title = gameName;
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = screenSize.X;
        graphics.PreferredBackBufferHeight = screenSize.Y;
        graphics.ApplyChanges();
        
        SoundEffect.MasterVolume = defaultVolume;

        Window.TextInput += Undo;

        base.Initialize();
    }

    //Time
    static public void TimeAdd()
    {
        currentTime++;

        Box.Boxes.ForEach(b => b.NewTime(currentTime));
        player.NewTime(currentTime);
    }

    static private void TimeShift(int time)
    {
        Box.Boxes.ForEach(b => b.ShiftTo(time));
        player.ShiftTo(time);
        Box.UpdateGoals();
    }

    static private void PreviousTime()
    {
        --currentTime;
        if (currentTime < 0) currentTime = 0;
        TimeShift(currentTime);
    }
        
    static private void Reset()
    {
        Box.UpdateGoals();
        Box.Boxes.ForEach(b => b.ResetTime());
        player.ResetTime();
        currentTime = -1;
        TimeAdd();
    }

    //Main
    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (keys.IsKeyDown(Keys.Escape)) Exit();
        
        UI.UpdateElements(keys, mouse);
        Event.ExecuteEvents(gameTime);

        if (gameState == GameState.Game)
        {
            Controls();
            
            if (player.PlayerMove())
                TimeAdd();
        }

        if (mapCompleted)
        {
            StartMap(currentMap + 1);
            mapCompleted = false;
        }

        previousKeys = keys;

        base.Update(gameTime);
    }

    static private void Controls()
    {
        if (keys.IsKeyDown(Keys.R) && !previousKeys.IsKeyDown(Keys.R))
        {
            TimeShift(0);
            Reset();
        }
    }

    static private void Undo(object? sender, TextInputEventArgs args)
    {
        if(gameState != GameState.Game) return;
        
        if (char.ToLower(args.Character) == 'z')
            PreviousTime();
    }

    //Draw
    static private void DrawGame()
    {
        bool light = false;
        for (int y = 0; y < Map.Size.Y; ++y)
        {
            for(int x = 0; x < Map.Size.X; ++x)
            {
                Color color = light ? new Color(247, 242, 212) : new Color(242, 237, 207);
                spriteBatch.FillRectangle(new Rectangle(new Point(x, y) * new Point(Map.BlockUnit), new Point(Map.BlockUnit)), color);
                light = !light;
            }

            if(Map.Size.X % 2 == 0)
                light = !light;
        }

        for (int y = 0; y < Map.Walls.GetLength(0); ++y)
            for (int x = 0; x < Map.Walls.GetLength(1); ++x)
                if (Map.Walls[y,x])
                    spriteBatch.FillRectangle(new Rectangle(new Point(x,y) * new Point(Map.BlockUnit), new Point(Map.BlockUnit)), new Color(35,106,185));

        foreach (Rectangle outline in Map.Outline)
            spriteBatch.FillRectangle(outline, new Color(15, 86, 165));

        foreach (Point goal in Map.Goals)
            spriteBatch.FillRectangle(new Rectangle(goal * new Point(Map.BlockUnit), new Point(Map.BlockUnit)), new Color(207, 202, 172));

        foreach(Box box in Box.Boxes)
            box.Draw(spriteBatch);
        
        player.Draw(spriteBatch);

        string text = "Moves: " + currentTime;
        Vector2 measure = UI.Font.MeasureString(text);
        spriteBatch.DrawString(UI.Font, text, new Vector2(center(screenSize.X, measure.X), screenSize.Y - measure.Y), Color.Black);
    }

    static private void DrawMenu()
    {
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(new Color(247,242,212));

        spriteBatch.Begin();
        {
            drawMethods[gameState].Invoke();
            UI.DrawElements(spriteBatch);
            DebugLines.Draw(spriteBatch);
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private static readonly Point mapButtonSize = new(40,40);
    private static readonly int mapButtonOffset = 5;
    
    private static readonly int mapButtonsLength = (mapButtonSize.X + mapButtonOffset) * totalMaps;
    private static readonly Point mapMenuStart = new(center(screenSize.X, mapButtonsLength), center(screenSize.Y, mapButtonSize.Y));

    private static void CreateUi()
    {        
        var createMapButton = (int m) =>
        {
            Point pos = new(mapMenuStart.X + (mapButtonSize.X + mapButtonOffset) * m, mapMenuStart.Y);
            UI.Add(new Button(new Rectangle(pos, mapButtonSize), () => StartMap(m+1), (m+1).ToString(), 0));
        };

        for (int m = 0; m < totalMaps; ++m)
            createMapButton(m);

        Point tbSize = new(200, 50);
        Point tbPos = new(center(screenSize.X, tbSize.X), 400);
        TextBox tbCustomMap = UI.Add(new TextBox(new Rectangle(tbPos, tbSize), "enter map", 0));

        Point buttonSize = new(150, 50);
        Point buttonPos = new(center(screenSize.X, buttonSize.X), tbPos.Y + tbSize.Y + 10);
        UI.Add(new Button(new Rectangle(buttonPos, buttonSize), () => StartMap(tbCustomMap.Text), "Start", 0));
        
        //Game
        buttonPos = new(screenSize.X - buttonSize.X - percent(screenSize.X, 3),
            screenSize.Y - buttonSize.Y - percent(screenSize.Y, 2));
        UI.Add(new Button(new Rectangle(buttonPos, buttonSize), () => SetGameState(GameState.Menu), "Menu", 1));
    }
    
    public MyGame() : base()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }
}

class Program
{
    public static void Main()
    {
        using (MyGame game = new MyGame())
            game.Run();
    }
}
