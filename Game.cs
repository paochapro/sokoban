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

    static public bool Debug { get; private set; } = false;

    public enum GameState { Menu, Game }

    private static GameState gameStateVariable; //never use this
    public static GameState gameState
    {
        get => gameStateVariable;
        set
        {
            gameStateVariable = value;
            UI.CurrentLayer = Convert.ToInt32(gameState);
            Reset();
        }
    }

    static readonly Dictionary<GameState, Action> drawMethods = new()
    {
        [GameState.Menu] = DrawMenu,
        [GameState.Game] = DrawGame,
    };

    static Player player;

    //Initialization
    static private void Reset()
    {
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        MonoGame.Content = Content;
        UI.Font = Content.Load<SpriteFont>("bahnschrift");
        CreateUi();

        Map.ConvertToBinary("convertme.txt");
        Map.LoadMap("converted_convertme.bin");

        player = new Player(playerSpawn);
        Entities.Add(player);

        gameState = GameState.Game;
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

    //Main
    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (keys.IsKeyDown(Keys.Escape)) Exit();

        Controls();

        UI.UpdateElements(mouse);
        Event.ExecuteEvents(gameTime);

        if (gameState == GameState.Game)
            Entities.Update(gameTime);

        if (gameState != GameState.Game)
        {
            base.Update(gameTime);
            return;
        }

        base.Update(gameTime);
    }

    static private void Controls()
    {

    }

    //Draw
    static private void DrawGame()
    {
        foreach (Point wall in Map.Walls)
        {
            spriteBatch.FillRectangle(new Rectangle(wall * new Point(Map.BlockUnit), new Point(Map.BlockUnit)), Color.Black);
        }

        player.Draw(spriteBatch);
    }

    static private void DrawMenu()
    {
        
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.LightBlue);

        spriteBatch.Begin();
        {
            UI.DrawElements(spriteBatch);
            drawMethods[gameState].Invoke();
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }

    //Initialize game states
    static private void StartGame()
    {
        gameState = GameState.Game;
    }

    static private void StartMenu()
    {   
        gameState = GameState.Menu;
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
