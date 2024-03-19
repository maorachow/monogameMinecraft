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
using System.Net.Security;
//using System.Numerics;

namespace monogameMinecraft
{
    public enum GameStatus
    {
        Menu,
        Settings,
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
        public Effect ssaoEffect;
        public Effect gBufferEffect;
        public Effect skyboxEffect;
        public Effect lightShaftEffect;
        public Effect ssrEffect;
        public AlphaTestEffect chunkNSEffect;
        public GamePlayer gamePlayer;
        public ChunkRenderer chunkRenderer;
        public Thread updateWorldThread;
        public Thread tryRemoveChunksThread;
        public int renderDistance=512;
        public GameStatus status=GameStatus.Menu;
         public EntityRenderer entityRenderer;
       
        public ShadowRenderer shadowRenderer;
        public SSAORenderer ssaoRenderer;
        public SkyboxRenderer skyboxRenderer;
        public GBufferRenderer gBufferRenderer;
        public SSRRenderer ssrRenderer;
        public TextureCube skyboxTex;
        public VolumetricLightRenderer volumetricLightRenderer;
        public GameTimeManager gameTimeManager;
        public PointLightUpdater pointLightUpdater;
        public bool renderDebug=true;
        public MinecraftGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        //    IsMouseVisible = false;
            Window.AllowUserResizing = true;

            Window.ClientSizeChanged += OnResize;
            _graphics.PreparingDeviceSettings += PrepareGraphicsDevice;
            IsMouseVisible = true;
            this.IsFixedTimeStep = false;
        //         TargetElapsedTime = System.TimeSpan.FromMilliseconds(33);
            //this.OnExiting += OnExit;
        }
        
        public void PrepareGraphicsDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            _graphics.PreferMultiSampling = true;
          
        }
        void OnResize(Object sender, EventArgs e)
        {
            UIElement.ScreenRect = this.Window.ClientBounds;
            foreach (UIElement element in UIElement.menuUIs )
            {
                element.OnResize();
            }
            foreach(UIElement element1 in UIElement.settingsUIs)
            {
                element1.OnResize();
            }
            switch (status)
            {
                case GameStatus.Started:


                    int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
                    int height = GraphicsDevice.PresentationParameters.BackBufferHeight;
                    Debug.WriteLine(width);
                    Debug.WriteLine(height);
                    gBufferRenderer.Resize(width,height,GraphicsDevice);
              
                       

                           ssaoRenderer.ssaoTarget = new RenderTarget2D(ssaoRenderer.graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
                           volumetricLightRenderer.blendVolumetricMap = new RenderTarget2D(volumetricLightRenderer.device, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
                           volumetricLightRenderer.renderTargetLum = new RenderTarget2D(volumetricLightRenderer.device, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
                           ssrRenderer.renderTargetSSR=new RenderTarget2D(ssrRenderer.graphicsDevice, width/2,height/2,false,SurfaceFormat.Vector4, DepthFormat.Depth24);
                           volumetricLightRenderer.lightShaftTarget = new RenderTarget2D(GraphicsDevice, (int)((float)width), (int)((float)height), false, SurfaceFormat.Vector4, DepthFormat.Depth24);
                    float aspectRatio = GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
                    gamePlayer.cam.aspectRatio= aspectRatio;
                    gamePlayer.cam.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), aspectRatio, 0.1f, 1000f);
                    break;
            }
          //  button.OnResize();
            
            
            
            Debug.WriteLine(GraphicsDevice.Viewport.Width + " " + GraphicsDevice.Viewport.Height);
        }
        public void InitGameplay(object obj)
        {
            GraphicsDevice.PresentationParameters.MultiSampleCount =2;
            IsMouseVisible = false;
            ChunkManager.chunks = new System.Collections.Concurrent.ConcurrentDictionary<Vector2Int, Chunk>();
            ChunkManager.chunkDataReadFromDisk = new Dictionary<Vector2Int, ChunkData>();
            Chunk.biomeNoiseGenerator.SetFrequency(0.002f);
            ChunkManager.ReadJson();
            GameOptions.ReadOptionsJson();
            status = GameStatus.Started;
            gamePlayer = new GamePlayer(new Vector3(-0.3f, 100, -0.3f), new Vector3(0.3f, 101.8f, 0.3f), this);
          
            updateWorldThread = new Thread(() => ChunkManager.UpdateWorldThread( gamePlayer,this));
            updateWorldThread.IsBackground = true;
            updateWorldThread.Start();
            tryRemoveChunksThread = new Thread(() => ChunkManager.TryDeleteChunksThread( gamePlayer, this));
            tryRemoveChunksThread.IsBackground = true;
            tryRemoveChunksThread.Start();
            chunkSolidEffect = Content.Load<Effect>("blockeffect");
            chunkShadowEffect = Content.Load<Effect>("createshadowmapeffect");
            entityEffect = Content.Load<Effect>("entityeffect");
            //  chunkEffect = Content.Load<Effect>("blockeffect");
            gBufferEffect = Content.Load<Effect>("gbuffereffect");
            ssaoEffect = Content.Load<Effect>("ssaoeffect");
            lightShaftEffect = Content.Load<Effect>("lightshafteffect");
            skyboxEffect = Content.Load<Effect>("skyboxeffect");
            ssrEffect = Content.Load<Effect>("ssreffect");
            gameTimeManager = new GameTimeManager(gamePlayer);
            chunkRenderer = new ChunkRenderer(this, GraphicsDevice, chunkSolidEffect,null, gameTimeManager);
            pointLightUpdater = new PointLightUpdater(chunkSolidEffect, gamePlayer);
            chunkRenderer.SetTexture(terrainTex,terrainNormal, terrainDepth);
            gBufferRenderer=new GBufferRenderer(this.GraphicsDevice, gBufferEffect,gamePlayer,chunkRenderer);
            ssaoRenderer = new SSAORenderer(ssaoEffect, gBufferRenderer, chunkRenderer, this.GraphicsDevice, gamePlayer,Content.Load<Texture2D>("randomnormal"));
            ssrRenderer = new SSRRenderer(GraphicsDevice, gamePlayer, gBufferRenderer, ssrEffect);
            entityRenderer = new EntityRenderer(this, GraphicsDevice, gamePlayer, entityEffect, Content.Load<Model>("zombie.geo"),Content.Load<Texture2D>("zombie"),Content.Load<Model>("zombiemodelref"), chunkShadowEffect, null,gameTimeManager);
            shadowRenderer = new ShadowRenderer(this, GraphicsDevice,chunkShadowEffect, chunkRenderer, entityRenderer,gameTimeManager);
            chunkRenderer.shadowRenderer = shadowRenderer;
            chunkRenderer.SSAORenderer = ssaoRenderer;
            entityRenderer.shadowRenderer = shadowRenderer;
            chunkRenderer.lightUpdater = pointLightUpdater;
            shadowRenderer.zombieModel = Content.Load<Model>("zombiemodelref");
            skyboxRenderer = new SkyboxRenderer(GraphicsDevice, skyboxEffect, null, gamePlayer,Content.Load<Texture2D>("skybox/skybox"), Content.Load<Texture2D>("skybox/skyboxup"), Content.Load<Texture2D>("skybox/skybox"), Content.Load<Texture2D>("skybox/skybox"), Content.Load<Texture2D>("skybox/skyboxdown"), Content.Load<Texture2D>("skybox/skybox"),
                Content.Load<Texture2D>("skybox/skyboxnight"), Content.Load<Texture2D>("skybox/skyboxnightup"), Content.Load<Texture2D>("skybox/skyboxnight"), Content.Load<Texture2D>("skybox/skyboxnight"), Content.Load<Texture2D>("skybox/skyboxnightdown"), Content.Load<Texture2D>("skybox/skyboxnight"),gameTimeManager
                );
            volumetricLightRenderer = new VolumetricLightRenderer(GraphicsDevice, gBufferRenderer, _spriteBatch,Content.Load<Effect>("volumetricmaskblend"), lightShaftEffect,gamePlayer, gameTimeManager);
            chunkRenderer.SSRRenderer = ssrRenderer;
            volumetricLightRenderer.entityRenderer = entityRenderer;
            GamePlayer.ReadPlayerData(gamePlayer,this);
            EntityBeh.InitEntityList();
          //  rasterizerState.CullMode = CullMode.None;
         //   rasterizerState1.CullMode = CullMode.CullCounterClockwiseFace;
            // EntityBeh.SpawnNewEntity(new Vector3(0, 100, 0), 0f, 0f, 0f, 0, this);
            EntityManager.ReadEntityData();
            Debug.WriteLine(EntityBeh.entityDataReadFromDisk.Count);
            EntityBeh.SpawnEntityFromData(this);
          
            
        }

        void QuitGameplay()
        {
            IsMouseVisible = true;
            GameOptions.SaveOptions(null);
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
            EntityBeh.InitEntityList();
            GC.Collect();
           
            

            status = GameStatus.Menu;
           // updateWorldThread.Abort();
         //   tryRemoveChunksThread.Abort();
        }

        public void GoToSettings(object obj)
        {
            GameOptions.ReadOptionsJson();
            
            this.status = GameStatus.Settings;
        }

      public void GoToMenuFromSettings(object obj)
       {
            GameOptions.SaveOptions(null);
           this.status = GameStatus.Menu;
                  
         }
            SpriteFont sf;
        protected override void Initialize()
        {
            

             _spriteBatch = new SpriteBatch(GraphicsDevice);
          //  InitGameplay();
        //    sf = Content.Load<SpriteFont>("defaultfont");
            UIUtility.InitGameUI(this);
            GameOptions.ReadOptionsJson();
            // Chunk c = new Chunk(new Vector2Int(0,0));
            //  chunkSolidEffect = new Effect();

            base.Initialize();
        }
        Texture2D terrainTex;
        Texture2D terrainNormal;
        Texture2D terrainDepth;
        protected override void LoadContent()
        {
           
            
            terrainTex = Content.Load<Texture2D>("terrain");
            terrainNormal = Content.Load<Texture2D>("terrainnormal");
            chunkSolidEffect = Content.Load<Effect>("blockeffect");
            terrainDepth = Content.Load<Texture2D>("terrainheight");
       //     skyboxTex = Content.Load<TextureCube>("skybox");
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


                case GameStatus.Settings:
                    foreach (var el in UIElement.settingsUIs)
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
                    gameTimeManager.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                //    _spriteBatch.End();
                    // TODO: Add your update logic here
                    EntityManager.UpdateAllEntity((float)gameTime.ElapsedGameTime.TotalSeconds);
                    EntityManager.TrySpawnNewZombie(this, (float)gameTime.ElapsedGameTime.TotalSeconds);
                
                   
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
        RasterizerState rasterizerState = new RasterizerState();
        RasterizerState rasterizerState1 = new RasterizerState();
        public void RenderWorld()
        {  
            shadowRenderer.UpdateLightMatrices(gamePlayer);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
          //  GraphicsDevice.RasterizerState = rasterizerState;
            shadowRenderer.RenderShadow(gamePlayer);
            gBufferRenderer.Draw();
             ssaoRenderer.Draw();
            volumetricLightRenderer.Draw();
            ssrRenderer.Draw();
            skyboxRenderer.Draw();

            //   GraphicsDevice.RasterizerState = rasterizerState1;
            pointLightUpdater.UpdatePointLight();
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
                    gamePlayer.cam.updateCameraVectors();
                   
                    RenderWorld();
                    _spriteBatch.Begin(blendState:BlendState.Additive);
                    _spriteBatch.Draw(volumetricLightRenderer.lightShaftTarget, new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth , GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                    _spriteBatch.End();
                   
                    _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

                    foreach (var el in UIElement.inGameUIs)
                    {
                        el.DrawString(el.text);
                    }
                   _spriteBatch.End();
                     _spriteBatch.Begin();
                    if (renderDebug)
                    {
                        _spriteBatch.Draw(shadowRenderer.shadowMapTarget, new Rectangle(200, 0, 200, 200), Color.White);
                        _spriteBatch.Draw(shadowRenderer.shadowMapTargetFar, new Rectangle(200, 200, 200, 200), Color.White);

                        _spriteBatch.Draw(ssaoRenderer.ssaoTarget, new Rectangle(400, 0, 200, 200), Color.White);
                        _spriteBatch.Draw(gBufferRenderer.renderTargetProjectionDepth, new Rectangle(400, 200, 200, 200), Color.White);
                        _spriteBatch.Draw(gBufferRenderer.renderTargetNormalWS, new Rectangle(600, 200, 200, 200), Color.White);
                        _spriteBatch.Draw(gBufferRenderer.renderTargetAlbedo, new Rectangle(200, 600, 200, 200), Color.White);
                        _spriteBatch.Draw(gBufferRenderer.renderTargetRoughness, new Rectangle(400, 800, 200, 200), Color.White);
                         _spriteBatch.Draw(volumetricLightRenderer.blendVolumetricMap, new Rectangle(800, 200, 200, 200), Color.White);
                         _spriteBatch.Draw(volumetricLightRenderer.lightShaftTarget, new Rectangle(800, 400, 200, 200), Color.White);

                        _spriteBatch.Draw(ssrRenderer.renderTargetSSR, new Rectangle(200, 800, 200, 200), Color.White);
                    }


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



                case GameStatus.Settings:
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

                    foreach (var el in UIElement.settingsUIs)
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