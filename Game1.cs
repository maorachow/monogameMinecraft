using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net.Mime;
using System.Reflection;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;
using System.Diagnostics;
namespace monogameMinecraft
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D faceTex;
        public Model model;
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        //  private Matrix view = Matrix.CreateLookAt(new Vector3(3, 0, 1), new Vector3(0, 0, 0), -Vector3.UnitY);
        //    private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 800f / 480f, 0.1f, 100f);
        public GamePlayer gamePlayer;
       
        public BasicEffect basicEffect;
        public VertexPositionTexture[] triangleVertices;
        public VertexBuffer vertexBuffer;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            Window.ClientSizeChanged += OnResize;
            //   Window.KeyDown += OnResize;
            TargetElapsedTime = System.TimeSpan.FromMilliseconds(33);
        }

        private void OnResize(object sender, InputKeyEventArgs e)
        {
            gamePlayer.cam.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 800f / 480f, 0.1f, 100f);
            Debug.WriteLine(Window.ClientBounds.Size.X + " " + Window.ClientBounds.Size.Y);
        }

        void OnResize(Object sender, EventArgs e)
        {
            gamePlayer.cam.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 100f);
            Debug.WriteLine(GraphicsDevice.Viewport.Width + " " + GraphicsDevice.Viewport.Height);
        }
        protected override void Initialize()
        { 
            gamePlayer=new GamePlayer(new Vector3(10,0,0),new Vector3(11,2,1),this);
            // TODO: Add your initialization logic here
            triangleVertices = new VertexPositionTexture[3];
            triangleVertices[0] = new VertexPositionTexture(new Vector3(0, 20, 0), new Vector2(0.5f,1f));
            triangleVertices[1] = new VertexPositionTexture(new Vector3(-20, -20, 0), new Vector2(0f, 0f));
            triangleVertices[2] = new VertexPositionTexture(new Vector3(20, -20, 0), new Vector2(1f, 0f));

            //Vert buffer
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 3, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionTexture>(triangleVertices);

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.Alpha = 1f;

            // Want to see the colors of the vertices, this needs to 
         //   be on
           
            //Lighting requires normal information which 
          //  VertexPositionColor does not have
            //If you want to use lighting and VPC you need to create a 
           // custom def
            basicEffect.LightingEnabled = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            faceTex = this.Content.Load<Texture2D>("awesomeface");
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = faceTex;
                model = Content.Load<Model>("BeachBall");
            // TODO: use this.Content to load your game content here
        }
        int lastMouseX;
        int lastMouseY;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            var kState=Keyboard.GetState();
            Vector2 playerVec=new Vector2(0f,0f);
            if (kState.IsKeyDown(Keys.W))
            {
                playerVec.Y = -10f;
            }

            if (kState.IsKeyDown(Keys.S))
            {
                playerVec.Y = 10f;
            }

            if (kState.IsKeyDown(Keys.A))
            {
               playerVec.X = -1f;
            }

            if (kState.IsKeyDown(Keys.D))
            {
                playerVec.X = 1f;
            }
            gamePlayer.cam.position +=new Vector3( playerVec.X,0,playerVec.Y )*(float) gameTime.ElapsedGameTime.TotalSeconds;
            gamePlayer.cam.updateCameraVectors();
            var mState=Mouse.GetState();
            gamePlayer.cam.ProcessMouseMovement( mState.X-lastMouseX ,lastMouseY-mState.Y);
            lastMouseY = mState.Y;
            lastMouseX= mState.X;
           
       //     Debug.WriteLine(gamePlayer.playerPos);
       //     Debug.WriteLine(gamePlayer.cam.Pitch+" "+ gamePlayer.cam.Yaw);
        //    Debug.WriteLine(gamePlayer.cam.position + " " + gamePlayer.cam.front+" "+gamePlayer.cam.up);
            base.Update(gameTime);
           
        }
        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            gamePlayer.cam.updateCameraVectors();
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
        
            // TODO: Add your drawing code here
            Matrix view = gamePlayer.cam.viewMatrix;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            basicEffect.Projection = gamePlayer.cam.projectionMatrix;
            basicEffect.View = view;
            basicEffect.World = world;
            
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                 
            {
                pass.Apply();
               GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 3);
                                              
            }
            DrawModel(model, world, view, gamePlayer.cam.projectionMatrix);
            base.Draw(gameTime);
        }
    }
}