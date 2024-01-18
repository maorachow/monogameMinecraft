using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net.Mime;
using System.Reflection;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using monogameMinecraft;
using System.Threading;
namespace monogameMinecraft
{
    public enum GameStatus
    {
        Started,
        Quiting
    }
    public class MinecraftGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
       
        public BasicEffect chunkEffect;
        public GamePlayer gamePlayer;
        public ChunkRenderer chunkRenderer;
        public Thread updateWorldThread;
        public Thread tryRemoveChunksThread;
        public int renderDistance=128;
        public GameStatus status;
        public MinecraftGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            Window.ClientSizeChanged += OnResize;
           
            //   this.IsFixedTimeStep = false;
            //      TargetElapsedTime = System.TimeSpan.FromMilliseconds(0.1);
            //this.OnExiting += OnExit;
        }
    
        void OnResize(Object sender, EventArgs e)
        {
            gamePlayer.cam.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 1000f);
            Debug.WriteLine(GraphicsDevice.Viewport.Width + " " + GraphicsDevice.Viewport.Height);
        }
        protected override void Initialize()
        {
            gamePlayer = new GamePlayer(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 1.8f, 0.5f), this);
            Chunk.biomeNoiseGenerator.SetFrequency(0.002f);

            Task t = new Task(() => ChunkManager.ReadJson());
            t.RunSynchronously();
            chunkEffect = new BasicEffect(GraphicsDevice);
            updateWorldThread = new Thread(() => ChunkManager.UpdateWorldThread(renderDistance, gamePlayer));
            updateWorldThread.Start();
            tryRemoveChunksThread = new Thread(() => ChunkManager.TryDeleteChunksThread(renderDistance, gamePlayer));
            tryRemoveChunksThread.Start();
            // Chunk c = new Chunk(new Vector2Int(0,0));
           
          //  chunkEffect = Content.Load<Effect>("blockeffect");
            chunkRenderer = new ChunkRenderer(this, chunkEffect, GraphicsDevice);
            base.Initialize();
        }
        Texture2D terrainTex;
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            terrainTex = Content.Load<Texture2D>("terrain");
            
           // terrainTex.
            Debug.WriteLine(terrainTex.Width + " " + terrainTex.Height);
            
            chunkRenderer.atlas = terrainTex;
           
            // TODO: use this.Content to load your game content here
        }
        int lastMouseX;
        int lastMouseY;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
           //     status = GameStatus.Quiting;

              //  Exit();
                Environment.Exit(0);
            }


            // TODO: Add your update logic here


            ProcessPlayerKeyboardInput(gameTime);
            //     Debug.WriteLine(gamePlayer.playerPos);
            //     Debug.WriteLine(gamePlayer.cam.Pitch+" "+ gamePlayer.cam.Yaw);
            //    Debug.WriteLine(gamePlayer.cam.position + " " + gamePlayer.cam.front+" "+gamePlayer.cam.up);
            base.Update(gameTime);

        }
        void ProcessPlayerMouseInput()
        {
        var mState = Mouse.GetState();
            gamePlayer.cam.ProcessMouseMovement(mState.X - lastMouseX, lastMouseY - mState.Y);
            lastMouseY = mState.Y;
            lastMouseX = mState.X;
        }
        void ProcessPlayerKeyboardInput(GameTime gameTime)
        {
            var kState = Keyboard.GetState();
            Vector3 playerVec = new Vector3(0f, 0f,0f);
            if (kState.IsKeyDown(Keys.W))
            {
                playerVec.Z = 1f;
            }

            if (kState.IsKeyDown(Keys.S))
            {
                playerVec.Z = -1f;
            }

            if (kState.IsKeyDown(Keys.A))
            {
                playerVec.X = -1f;
            }

            if (kState.IsKeyDown(Keys.D))
            {
                playerVec.X = 1f;
            }
            if (kState.IsKeyDown(Keys.Space))
            {
                playerVec.Y = 1f;
            }
            if (kState.IsKeyDown(Keys.LeftShift))
            {
                playerVec.Y =- 1f;
            }
            gamePlayer.ProcessPlayerInputs(playerVec, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }
        protected override void Draw(GameTime gameTime)
        {


            
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // Debug.WriteLine(ChunkManager.chunks.Count);
            ProcessPlayerMouseInput();
            chunkRenderer.RenderAllChunks(ChunkManager.chunks,gamePlayer);
            base.Draw(gameTime);
        }
    }
}