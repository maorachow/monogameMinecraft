using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net.Mime;
using System.Reflection;
using MonoGame.Extended;
using MonoGame.Extended.Screens.Transitions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using monogameMinecraft;
using System.Threading;
using System.Collections.Generic;

namespace monogameMinecraft
{
    public enum GameStatus
    {
        Menu,
        Started,
        Quiting
    }
    public class MinecraftGame : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;
        public Effect chunkSolidEffect;
        public AlphaTestEffect chunkNSEffect;
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
            
               this.IsFixedTimeStep = false;
               //   TargetElapsedTime = System.TimeSpan.FromMilliseconds(16);
            //this.OnExiting += OnExit;
        }
    
        void OnResize(Object sender, EventArgs e)
        {
            UIElement.ScreenRect = this.Window.ClientBounds;
            foreach (UIElement element in UIElement.menuUIs )
            {
                element.OnResize();
            }
            switch (status)
            {
                case GameStatus.Started:
                    float aspectRatio = GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
                    gamePlayer.cam.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), aspectRatio, 0.1f, 1000f);
                    break;
            }
          //  button.OnResize();
            
            
            
            Debug.WriteLine(GraphicsDevice.Viewport.Width + " " + GraphicsDevice.Viewport.Height);
        }
        public void InitGameplay()
        {
            gamePlayer = new GamePlayer(new Vector3(-0.3f, 100, -0.3f), new Vector3(0.3f, 101.8f, 0.3f), this);
            
            Chunk.biomeNoiseGenerator.SetFrequency(0.002f);
          ChunkManager.ReadJson();
      
            chunkNSEffect=new AlphaTestEffect(GraphicsDevice);
            updateWorldThread = new Thread(() => ChunkManager.UpdateWorldThread(renderDistance, gamePlayer,this));
            updateWorldThread.Start();
            tryRemoveChunksThread = new Thread(() => ChunkManager.TryDeleteChunksThread(renderDistance, gamePlayer, this));
            tryRemoveChunksThread.Start();
            chunkSolidEffect = Content.Load<Effect>("blockeffect");
            //  chunkEffect = Content.Load<Effect>("blockeffect");
            chunkRenderer = new ChunkRenderer(this, GraphicsDevice, chunkNSEffect,chunkSolidEffect);
            chunkRenderer.SetTexture(terrainTex);
            GamePlayer.ReadPlayerData(gamePlayer,this);
            status = GameStatus.Started;
        }
        void QuitGameplay()
        {
            status = GameStatus.Quiting;
            ChunkManager.SaveWorldData();
            GamePlayer.SavePlayerData(gamePlayer);
            ChunkManager.chunks.Clear();
            ChunkManager.chunkDataReadFromDisk.Clear();

            
           // updateWorldThread.Abort();
         //   tryRemoveChunksThread.Abort();
        }
        SpriteFont sf;
        protected override void Initialize()
        {
            

             _spriteBatch = new SpriteBatch(GraphicsDevice);
          //  InitGameplay();
        //    sf = Content.Load<SpriteFont>("defaultfont");
            UIUtility.InitMenuUI(this);
            // Chunk c = new Chunk(new Vector2Int(0,0));
            //  chunkSolidEffect = new Effect();

            base.Initialize();
        }
        Texture2D terrainTex;
        protected override void LoadContent()
        {
           
            
            terrainTex = Content.Load<Texture2D>("terrain");
            
            chunkSolidEffect = Content.Load<Effect>("blockeffect");
           // terrainTex.
    //       Debug.WriteLine(terrainTex.Width + " " + terrainTex.Height);
         //   button = new UIButton(new Vector2(0.1f, 0.1f), 0.3f, 0.2f, terrainTex, new Vector2(0.15f, 0.15f), sf, _spriteBatch,this.Window);
           // chunkRenderer.atlas = terrainTex;
           
            // TODO: use this.Content to load your game content here
        }
        int lastMouseX;
        int lastMouseY;
        protected override void Update(GameTime gameTime)
        {
            switch (status)
            {
                case GameStatus.Menu:
                    foreach(var el in UIElement.menuUIs)
                    {
                        el.Update();
                    }
                    break;
                case GameStatus.Started:

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //     status = GameStatus.Quiting;
                QuitGameplay();
              //  Exit();
                Environment.Exit(0);
            }


            // TODO: Add your update logic here
           
            gamePlayer.UpdatePlayer((float)gameTime.ElapsedGameTime.TotalSeconds);
            ProcessPlayerKeyboardInput(gameTime);


                    break;
            }
          
            
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
            var mState = Mouse.GetState();
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
            gamePlayer.ProcessPlayerInputs(playerVec, (float)gameTime.ElapsedGameTime.TotalSeconds, kState,mState); 
            ProcessPlayerMouseInput();
        }
        protected override void Draw(GameTime gameTime)
        {


            switch (status)
            {
                case GameStatus.Started:
                    Debug.WriteLine("started");
                GraphicsDevice.Clear(Color.CornflowerBlue);
            // Debug.WriteLine(ChunkManager.chunks.Count);
         
                GraphicsDevice.DepthStencilState= DepthStencilState.Default;
                chunkRenderer.RenderAllChunks(ChunkManager.chunks,gamePlayer);

                    break;
                    case GameStatus.Menu:
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    _spriteBatch.Begin(samplerState:SamplerState.PointWrap);

                    foreach (var el in UIElement.menuUIs)
                    {
                        el.DrawString(el.text);
                    }
                         _spriteBatch.End();
                    break;

            }
           
        //    _
            base.Draw(gameTime);
        }
    }
}