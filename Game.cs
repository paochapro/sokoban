using System.Net.Mail;
using System.Runtime.InteropServices;
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
    private static Point screen = defaultScreenSize;
    public static Point screenSize => screen;
    
    private static readonly Point defaultScreenSize = new(576, 576);
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
    public static bool IsKeyPressed(Keys key) => MyGame.keys.IsKeyDown(key) && !MyGame.previousKeys.IsKeyDown(key);
    
    static private Vector2 camera;
    static public Vector2 Camera => camera;
    static public bool Debug { get; private set; } = true;

    public enum GameState { Menu, Game }

    private static GameState gameState;
    
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
    private const string mapExtension = ".bin";
    private const int totalMaps = 5;
    public static Player Player => player;
    private static Player player;
    private static int nextMap = 1;
    private static int currentTime;

    //Initialization
    static bool mapCompleted = false;
    static public void MapCompleted() => mapCompleted = true;
    
    private static void ChangeScreenSize(Point size)
    {
        screen = size;
        graphics.PreferredBackBufferWidth = size.X;
        graphics.PreferredBackBufferHeight = size.Y;
        graphics.ApplyChanges();
    }

    private void StartMap(int map)
    {
        nextMap = map + 1;
        StartMap("map" + map, false);
    }
    private void StartMap(string map, bool leaveOnWin)
    {
        if (!Map.Exists(map + mapExtension))
        {
            Console.WriteLine("Map " + map + "wasnt found, returning to menu");
            StartMenu();
            return;
        }
        
        if (gameState != GameState.Game)
        {
            SetGameState(GameState.Game);
            Window.TextInput += Undo;
        }
        
        Console.WriteLine("starting map: " + map);

        if (leaveOnWin) nextMap = -1;
        
        Box.Boxes.Clear();
        Map.Goals.Clear();
        player.ResetTime();

        if (!Map.LoadMap(map + mapExtension))
            throw new Exception("Map in LoadMap somehow wasnt found");

        player.SetPosition(playerSpawn);

        ChangeScreenSize(Map.Size * new Point(Map.BlockUnit));
        menuButton.Position = new(screenSize.X - menuButtonSize.X - percent(screenSize.X, 3), screenSize.Y - menuButtonSize.Y - percent(screenSize.Y, 2));

        UpdateMovesLabel(0);

        Reset();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        Assets.Content = Content;
        UI.Font = Content.Load<SpriteFont>("bahnschrift");
        UI.window = Window;
        CreateUi();
        
        StartMenu();

        player = new Player(playerSpawn);
    }

    protected override void Initialize()
    {
        Window.AllowUserResizing = resizable;
        Window.Title = gameName;
        IsMouseVisible = true;
        ChangeScreenSize(defaultScreenSize);

        SoundEffect.MasterVolume = defaultVolume;
        
        base.Initialize();
    }

    private static void UpdateMovesLabel(int time)
    {
        movesLabel.text = movesLabelText + time;
        Point measure = UI.Font.MeasureString(movesLabel.text).ToPoint();
        movesLabel.Position = new Point(center(screenSize.X, measure.X), screenSize.Y - measure.Y);
    }

    //Time
    static public void TimeAdd()
    {
        currentTime++;

        Box.Boxes.ForEach(b => b.NewTime(currentTime));
        player.NewTime(currentTime);

        UpdateMovesLabel(currentTime);
    }

    static private void TimeShift(int time)
    {
        Box.Boxes.ForEach(b => b.ShiftTo(time));
        player.ShiftTo(time);
        Box.UpdateGoals();
        
        UpdateMovesLabel(time);
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
            player.PlayerMove();
            
            if (mapCompleted)
            {
                mapCompleted = false;
                
                if (nextMap < 1 || nextMap > totalMaps)
                    StartMenu();
                else
                    StartMap(nextMap);
            }
        }

        previousKeys = keys;

        base.Update(gameTime);
    }

    static private void Controls()
    {
        if (IsKeyPressed(Keys.R))
        {
            TimeShift(0);
            Reset();
        }
        if(IsKeyPressed(Keys.O)) MapCompleted();
    }

    static private void Undo(object? sender, TextInputEventArgs args)
    {
        if (char.ToLower(args.Character) == 'z')
            PreviousTime();
    }
    
    //Draw
    static private void DrawGame()
    {
        //Bg, walls, goals
        Map.DrawMap(spriteBatch);
        
        //Boxes and player
        Box.Boxes.ForEach(b => b.Draw(spriteBatch));
        player.Draw(spriteBatch);
    }

    static private void DrawMenu()
    {
    }

    private void StartMenu()
    {
        if (gameState == GameState.Menu) return;

        Window.TextInput -= Undo;
        ChangeScreenSize(defaultScreenSize);
        SetGameState(GameState.Menu);
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
    private static readonly Point mapMenuStart = new(center(defaultScreenSize.X, mapButtonsLength), center(defaultScreenSize.Y, mapButtonSize.Y));

    private static Label movesLabel;
    private static Button menuButton;
    private readonly static Point startButtonSize = new(150, 150/3);
    private readonly static Point menuButtonSize = new(125, 125/3 + 5);
    private const string movesLabelText = "Moves: ";

    private void CreateUi()
    {        
        //Choose map
        var createMapButton = (int m) =>
        {
            Point pos = new(mapMenuStart.X + (mapButtonSize.X + mapButtonOffset) * m, mapMenuStart.Y);
            UI.Add(new Button(new Rectangle(pos, mapButtonSize), () => StartMap(m+1), (m+1).ToString(), 0));
        };
        
        for (int m = 0; m < totalMaps; ++m)
            createMapButton(m);

        //Enter map through textbox
        Point tbSize = new(200, 50);
        Point tbPos = new(center(defaultScreenSize.X, tbSize.X), 400);
        TextBox tbCustomMap = UI.Add(new TextBox(new Rectangle(tbPos, tbSize), "enter map", 0));

        Point buttonPos = new(center(defaultScreenSize.X, startButtonSize.X), tbPos.Y + tbSize.Y + 10);
        
        UI.Add(new Button(new Rectangle(buttonPos, startButtonSize), () => StartMap(tbCustomMap.Text, true), "Start", 0));

        //Map list
        int lastMapIndex = 0;
        int x = 10;
        int offset = 2;
        
        string text = "Avaliable maps:";
        Point measure = UI.Font.MeasureString(text).ToPoint();
        UI.Add(new Label(new Point(x, offset), text, Color.Black, 0));
        
        var addMapToList = (string map) =>
        {
            string text = (++lastMapIndex) + ". " + map;
            Point measure = UI.Font.MeasureString(text).ToPoint();
            Button mapButton = UI.Add(new Button(new Rectangle(x, lastMapIndex * measure.Y + offset, measure.X, measure.Y), () => StartMap(map, true), text, 0));
            mapButton.OnlyText = true;
        };

        string[] mapFiles = Directory.GetFiles(Map.mapFolder, mapExtension.Remove(0));

        foreach (string mapfile in mapFiles)
        {
            string map = mapfile.Remove(mapfile.IndexOf("."), mapExtension.Length);
            map = map.Substring(mapfile.LastIndexOf("\\")+1);
            
            addMapToList(map);
        }
        
        //Game
        menuButton = UI.Add(new Button(new Rectangle(Point.Zero, menuButtonSize), StartMenu, "Menu", 1));
        movesLabel = UI.Add(new Label(Point.Zero, movesLabelText + currentTime, Color.Black, 1));
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
