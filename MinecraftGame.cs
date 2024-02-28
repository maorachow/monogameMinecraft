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
//using System.Numerics;

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
        public Effect chunkShadowEffect;
        public Effect entityEffect;
        public AlphaTestEffect chunkNSEffect;
        public GamePlayer gamePlayer;
        public ChunkRenderer chunkRenderer;
        public Thread updateWorldThread;
        public Thread tryRemoveChunksThread;
        public int renderDistance=512;
        public GameStatus status;
         public EntityRenderer entityRenderer;
       
        public ShadowRenderer shadowRenderer;
        public MinecraftGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        //    IsMouseVisible = false;
            Window.AllowUserResizing = true;

            Window.ClientSizeChanged += OnResize;
            
               this.IsFixedTimeStep = false;
          //       TargetElapsedTime = System.TimeSpan.FromMilliseconds(5);
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
            IsMouseVisible = false;
            ChunkManager.chunks = new System.Collections.Concurrent.ConcurrentDictionary<Vector2Int, Chunk>();
            ChunkManager.chunkDataReadFromDisk = new Dictionary<Vector2Int, ChunkData>();
            Chunk.biomeNoiseGenerator.SetFrequency(0.002f);
            ChunkManager.ReadJson();
            status = GameStatus.Started;
            gamePlayer = new GamePlayer(new Vector3(-0.3f, 100, -0.3f), new Vector3(0.3f, 101.8f, 0.3f), this);
          
            updateWorldThread = new Thread(() => ChunkManager.UpdateWorldThread(renderDistance, gamePlayer,this));
            updateWorldThread.IsBackground = true;
            updateWorldThread.Start();
            tryRemoveChunksThread = new Thread(() => ChunkManager.TryDeleteChunksThread(renderDistance, gamePlayer, this));
            tryRemoveChunksThread.IsBackground = true;
            tryRemoveChunksThread.Start();
            chunkSolidEffect = Content.Load<Effect>("blockeffect");
            chunkShadowEffect = Content.Load<Effect>("createshadowmapeffect");
            entityEffect = Content.Load<Effect>("entityeffect");
            //  chunkEffect = Content.Load<Effect>("blockeffect");

            chunkRenderer = new ChunkRenderer(this, GraphicsDevice, chunkSolidEffect,null);
            chunkRenderer.SetTexture(terrainTex);
           
            entityRenderer = new EntityRenderer(this, GraphicsDevice, gamePlayer, entityEffect, Content.Load<Model>("zombie.geo"),Content.Load<Texture2D>("zombie"),Content.Load<Model>("zombiemodelref"), chunkShadowEffect, null);
            shadowRenderer = new ShadowRenderer(this, GraphicsDevice,chunkShadowEffect, chunkRenderer, entityRenderer);
            chunkRenderer.shadowRenderer = shadowRenderer;
            entityRenderer.shadowRenderer = shadowRenderer;
            GamePlayer.ReadPlayerData(gamePlayer,this);
            EntityBeh.InitEntityList();

            // EntityBeh.SpawnNewEntity(new Vector3(0, 100, 0), 0f, 0f, 0f, 0, this);
            EntityManager.ReadEntityData();
            Debug.WriteLine(EntityBeh.entityDataReadFromDisk.Count);
            EntityBeh.SpawnEntityFromData(this);
             
        }

        void QuitGameplay()
        {
            IsMouseVisible = true;
            ChunkManager.SaveWorldData();
            GamePlayer.SavePlayerData(gamePlayer);
       /*     foreach(var c in ChunkManager.chunks)
            {
            c.Value.Dispose();
            }*/
            EntityManager.SaveWorldEntityData();
            ChunkManager.isJsonReadFromDisk=false;
            ChunkManager.chunks .Clear();
            ChunkManager.chunkDataReadFromDisk.Clear();
            GC.Collect();
           
            

            status = GameStatus.Menu;
           // updateWorldThread.Abort();
         //   tryRemoveChunksThread.Abort();
        }
        SpriteFont sf;
        protected override void Initialize()
        {
            

             _spriteBatch = new SpriteBatch(GraphicsDevice);
          //  InitGameplay();
        //    sf = Content.Load<SpriteFont>("defaultfont");
            UIUtility.InitGameUI(this);
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
            if(!IsActive) return;
          //  Draw1(gameTime);
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
             //   Environment.Exit(0);
            }
                    ProcessPlayerKeyboardInput(gameTime);

                    ProcessPlayerMouseInput();
                    gamePlayer.UpdatePlayer((float)gameTime.ElapsedGameTime.TotalSeconds);
                    //    _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

                    foreach (var el in UIElement.inGameUIs)
                    {
                        el.Update();
                    }
                //    _spriteBatch.End();
                    // TODO: Add your update logic here
                    EntityManager.UpdateAllEntity((float)gameTime.ElapsedGameTime.TotalSeconds);
                    EntityManager.TrySpawnNewZombie(this);
                
                   
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
        MouseState lastMouseState;
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
            gamePlayer.ProcessPlayerInputs(playerVec, (float)gameTime.ElapsedGameTime.TotalSeconds, kState,mState,lastMouseState);
            lastMouseState = mState;


        }
        public void RenderWorld()
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
           shadowRenderer.UpdateLightMatrices(gamePlayer);
            shadowRenderer.RenderShadow(gamePlayer);
            chunkRenderer.RenderAllChunksOpq(ChunkManager.chunks, gamePlayer);
            entityRenderer.Draw();
            chunkRenderer.RenderAllChunksTransparent(ChunkManager.chunks, gamePlayer);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
        }
        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) return;

            switch (status)
            {
                case GameStatus.Started:
                    //            Debug.WriteLine("started");
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    // Debug.WriteLine(ChunkManager.chunks.Count);
                  
                    RenderWorld();
                    _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

                    foreach (var el in UIElement.inGameUIs)
                    {
                        el.DrawString(el.text);
                    }
                    _spriteBatch.End();
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(shadowRenderer.shadowMapTarget, new Rectangle(200, 0, 200, 200), Color.White);
                    _spriteBatch.End();
                    break;
                case GameStatus.Menu:
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

                    foreach (var el in UIElement.menuUIs)
                    {
                        el.DrawString(el.text);
                    }
                    _spriteBatch.End();
                    break;

            }
            base.Draw(gameTime);
            //    _
            
        }
         
    }
}