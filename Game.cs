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

    static public Point playerSpawn = new(0, 0);

    //General stuff
    static public GraphicsDeviceManager Graphics => graphics;
    static GraphicsDeviceManager graphics;
    static SpriteBatch spriteBatch;
    public static MouseState mouse { get => Mouse.GetState(); }
    public static KeyboardState keys { get => Keyboard.GetState(); }

    static private Vector2 camera;
    static public Vector2 Camera => camera;

    static public bool Debug { get; private set; } = true;

    public enum GameState { Menu, Game }

    private static GameState gameState; //never use this
    
    public static void SetGameState(GameState gameState)
    {
        MyGame.gameState = gameState;
        UI.CurrentLayer = Convert.ToInt32(gameState);
        Reset();
    }

    static readonly Dictionary<GameState, Action> drawMethods = new()
    {
        [GameState.Menu] = DrawMenu,
        [GameState.Game] = DrawGame,
    };

    //Game
    static Player player;
    public static Player Player => player;
    static int currentMap = 1;
    static int currentTime;
    static bool pressingZ = false;

    //Initialization
    static private void Reset()
    {
        Box.Boxes.Clear();
        Map.Goals.Clear();
        player.ResetTime();

        currentTime = -1;
    }

    static bool mapCompleted = false;
    static public void MapCompleted() => mapCompleted = true;

    static private void NextMap()
    {
        Reset();
        Map.LoadMap("map" + (currentMap++) + ".bin");
        player.SetPosition(playerSpawn);

        TimeChange();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        MonoGame.Content = Content;
        UI.Font = Content.Load<SpriteFont>("bahnschrift");
        CreateUi();

        Map.ConvertToBinary("convertme.txt", "map1.bin");
        Map.ConvertToBinary("convert2.txt", "map2.bin");
        Map.ConvertToBinary("convert3.txt", "map3.bin");
        Map.ConvertToBinary("convert4.txt", "map4.bin");

        player = new Player(playerSpawn);

        SetGameState(GameState.Game);
        NextMap();
    }

    protected override void Initialize()
    {
        Window.AllowUserResizing = false;
        Window.Title = gameName;
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = screenSize.X;
        graphics.PreferredBackBufferHeight = screenSize.Y;
        graphics.ApplyChanges();

        SoundEffect.MasterVolume = defaultVolume;

        base.Initialize();
    }

    static public void TimeChange()
    {
        currentTime++;

        foreach (Box box in Box.Boxes) box.NewTime(currentTime);
        player.NewTime(currentTime);
    }

    static private void PreviousTime()
    {
        currentTime--;
        if (currentTime < 0) currentTime = 0;

        foreach (Box box in Box.Boxes) box.ShiftTo(currentTime);
        player.ShiftTo(currentTime);
    }

    //Main
    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (keys.IsKeyDown(Keys.Escape)) Exit();

        Controls();

        UI.UpdateElements(mouse);
        Event.ExecuteEvents(gameTime);

        if (gameState == GameState.Game)
        {
            if(player.PlayerMove())
                TimeChange();
        }

        if (mapCompleted)
        {
            NextMap();
            mapCompleted = false;
        }

        base.Update(gameTime);
    }

    static private void Controls()
    {
        if(keys.IsKeyDown(Keys.Z) && !pressingZ)
            PreviousTime();
        
        pressingZ = keys.IsKeyDown(Keys.Z);
    }

    //Draw
    static private void DrawGame()
    {
        for(int y = 0; y < Map.Walls.GetLength(0); ++y)
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
    }

    static private void DrawMenu()
    {
        
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(new Color(247,242,212));

        spriteBatch.Begin();
        {
            UI.DrawElements(spriteBatch);
            drawMethods[gameState].Invoke();
            DebugLines.Draw(spriteBatch);
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }

    static private void CreateUi()
    {

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
