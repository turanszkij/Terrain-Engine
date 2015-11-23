//Call every sub-class into this one
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace namespace_default
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public GraphicsDevice device;
        public SpriteBatch spriteBatch;

        public Effect texturedEffect;
        public Effect billboardGrassEffect;
        public Effect grassEffect;
        public Effect skyEffect;
        public Effect pointSpriteEffect;
        public Effect waterEffect;
        public Effect shadowMapEffect;

        public Random rand = new Random();

        public TextureTerrain terrain;
        Water water;
        public BBGrass bbgrass;
        public Grass grass;
        public Enviroment enviro;
        public Camera cam;
        FrameRate fps;
        TextWriter text;
        public SkyBox sky;
        public ModelLoader car;
        public ModelLoader human;
        Controls controls;
        public PointSprites_Single moon;
        public PointSprites_Multi snow, rain;
        public PointSprites_Single[] clouds;
            Texture2D[] cloudTextures = new Texture2D[5];


        public RenderTarget2D shadowMap;

        #region BLOOM
        BloomComponent bloom;
        int bloomSettingsIndex = 0;
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            bloom = new BloomComponent(this);
            Components.Add(bloom);
            bloom.Visible = false;
        }

        protected override void Initialize()
        {
            //graphics.PreferredBackBufferWidth = 1280;
            //graphics.PreferredBackBufferHeight = 720;
            //graphics.IsFullScreen = true;
            //graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();
            base.Initialize();

            Window.Title = "Terrain Engine";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;
            texturedEffect = Content.Load<Effect>("effect");
            billboardGrassEffect = Content.Load<Effect>("billboardgrass");
            grassEffect = Content.Load<Effect>("nonbillboardgrass");
            skyEffect = Content.Load<Effect>("sky");
            pointSpriteEffect = Content.Load<Effect>("PointSprite");
            waterEffect = Content.Load<Effect>("watereffect");
            shadowMapEffect = Content.Load<Effect>("shadowmap");

            enviro = new Enviroment();
            terrain = new TextureTerrain(this, enviro.map, enviro.texture);
            cam = new Camera(this,new Vector3(terrain.terrainWidth/2,15,terrain.terrainLength/2));
            controls = new Controls(this);
            if (enviro.billboardedGrass)
                bbgrass = new BBGrass(this, terrain, enviro.grassTexture);
            else
                grass = new Grass(this, terrain, enviro.grassTexture);
            fps = new FrameRate();
            text = new TextWriter(this, "font");
            sky = new SkyBox(this, Vector3.Zero, "sky/dome2/dome");
            car = new ModelLoader(this, "car/car", new Vector3(250, terrain.heightData[250, 230], 230), 1, 7);
            human = new ModelLoader(this, "human", new Vector3(terrain.terrainLength / 2, terrain.heightData[terrain.terrainLength / 2, terrain.terrainWidth / 2], terrain.terrainWidth / 2), 0.2f);
            moon = new PointSprites_Single(this, new Vector3(-10000, 20000, -10000), Content.Load<Texture2D>("moon"), 5000);
            snow = new PointSprites_Multi(this, cam.cameraPosition, Content.Load<Texture2D>("snow"), enviro.weatherParticles, 0.2f, rand);
            rain = new PointSprites_Multi(this, cam.cameraPosition, Content.Load<Texture2D>("rain"), enviro.weatherParticles, 0.2f, rand);
            water = new Water(this, terrain.terrainWidth / 10, terrain.terrainLength / 10, 10, Content.Load<Texture2D>("water"), Content.Load<Texture2D>("waterbump"));


            for (int i = 0; i < cloudTextures.Length; i++)
            {
                cloudTextures[i] = Content.Load<Texture2D>("clouds/cloud" + i);
            }
            clouds = new PointSprites_Single[500];
            for (int i = 0; i < clouds.Length; i++)
            {
                clouds[i] = new PointSprites_Single(this, new Vector3(terrain.terrainLength / 2f + (float)(rand.NextDouble() * 2 - 1) * 10000, rand.Next(400, 600), terrain.terrainWidth / 2f + (float)(rand.NextDouble() * 2 - 1) * 10000), cloudTextures[rand.Next(0,5)], 1000);
            }



            shadowMap = new RenderTarget2D(device, 2048, 2048);
        }
        
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (cam.fpsState)
            {
                cam.ProcessInput((float)gameTime.ElapsedGameTime.TotalMilliseconds / 500f);
            }
            else
            {
                cam.ProcessInput((float)gameTime.ElapsedGameTime.TotalMilliseconds / 20f, ref human.position, ref human.rotation);
                
            }
            
            fps.Update(gameTime);

            if (!controls.show)
            {
                PointLight();
            }

            if (!enviro.FlyMode)
            {
                StandOnTerrain(ref cam.cameraPosition,1.5f);
                StandOnTerrain(ref human.position,0);
            }
            moon.Update(new Vector3(-2, -2, 0) * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50f);

            controls.Update();
            enviro.Update(gameTime);


            if (enviro.enableSnow)
                snow.Fall(cam.cameraPosition, enviro.WindDirection.Length() * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f);
            if (enviro.enableRain)
                rain.Fall(cam.cameraPosition, enviro.WindDirection.Length() * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 500f);
            foreach (PointSprites_Single x in clouds)
            {
                if (x!=null)
                {
                    x.Update(enviro.WindDirection * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f);
                }
            }

            if (cam.cameraPosition.Y < enviro.waterLevel)
            {
                enviro.underWater = true;
            }
            else
                enviro.underWater = false;

            if(Keyboard.GetState().IsKeyDown(Keys.Up))
                enviro.depthbias+=0.0001f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                enviro.depthbias -= 0.0001f;


            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                UnloadContent();
                Exit();
            }

            HandleInput();

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            device.DepthStencilState = DepthStencilState.Default;


            DrawShadow();
            if (enviro.detailWater)
            {
                if (!enviro.underWater)
                {
                    water.DrawRefractionMap(2);
                    water.DrawReflectionMap(1);
                }
                else
                {
                    water.DrawRefractionMap(1);
                    water.DrawReflectionMap(2);
                }
            }


            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            bloom.BeginDraw();

            sky.Draw(cam.cameraPosition + new Vector3(0, -100, 0), cam.viewMatrix, 0);


            device.BlendState = BlendState.NonPremultiplied;
            moon.Draw(cam.viewMatrix);

            device.DepthStencilState = DepthStencilState.DepthRead;
            foreach (PointSprites_Single x in clouds)
            {
                x.Draw(cam.viewMatrix);
            }

            terrain.Draw(cam.viewMatrix,0);
            water.Draw();

            if (enviro.billboardedGrass)
                bbgrass.Draw(0, cam.viewMatrix);
            else
                grass.Draw(0, cam.viewMatrix);


            car.Draw(cam.viewMatrix, 0);
            human.Draw(cam.viewMatrix, 0);



            if (enviro.enableSnow)
                snow.Draw(cam.viewMatrix);
            if (enviro.enableRain)
                rain.Draw(cam.viewMatrix);


            base.Draw(gameTime);
            spriteBatch.Begin();
            text.Write(fps.frameRate + " FPS\nCampos: " + cam.cameraPosition
                +"\nDepth Bias: "+enviro.depthbias
                +"\nPress ENTER to bring up menu"
                +"\nHold SHIFT for faster camera"
                , new Vector2(), Color.Yellow);
            
            controls.Draw(spriteBatch);
            spriteBatch.End();

            fps.Count();

        }

        private void DrawShadow()
        {
            device.SetRenderTarget(shadowMap);
            //device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);


            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullClockwiseFace;
            device.RasterizerState = rs;

            device.DepthStencilState = DepthStencilState.Default;
            device.BlendState = BlendState.Opaque;

            shadowMapEffect.CurrentTechnique = shadowMapEffect.Techniques["ShadowMap"];

            shadowMapEffect.Parameters["xWorld"].SetValue(terrain.worldPosition);
            shadowMapEffect.Parameters["xView"].SetValue(cam.shadowViewMatrix);
            shadowMapEffect.Parameters["xProjection"].SetValue(cam.shadowProjectionMatrix);

            shadowMapEffect.CurrentTechnique.Passes["Pass0"].Apply();

            terrain.DrawForShadowMap();

            device.SetRenderTarget(null);

            if (Keyboard.GetState().IsKeyDown(Keys.Back))
            {
                System.IO.Stream s = System.IO.File.OpenWrite("shadowmap.jpg");
                shadowMap.SaveAsJpeg(s, shadowMap.Width, shadowMap.Height);
                s.Close();
            }
        }
        private void PointLight()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                enviro.pointLightPos = cam.cameraPosition;
            }
        }
        private void StandOnTerrain(ref Vector3 target, float floatamount)
        {
            Vector3 yvec = target;
            if ((int)Math.Round((double)cam.cameraPosition.X) < terrain.terrainLength
                && (int)Math.Round((double)cam.cameraPosition.X) > 0
                && (int)Math.Round((double)cam.cameraPosition.Z) < terrain.terrainWidth
                && (int)Math.Round((double)cam.cameraPosition.Z) > 0)
            {
                yvec.Y = terrain.heightData[(int)Math.Round((double)target.X), (int)Math.Round((double)target.Z)] + floatamount;
                target = Vector3.Lerp(target, yvec, 0.1f);
            }
        }

        #region FOR BLOOM
        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        private void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Switch to the next bloom settings preset?
            if ((currentGamePadState.Buttons.A == ButtonState.Pressed &&
                 lastGamePadState.Buttons.A != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.Y) &&
                 lastKeyboardState.IsKeyUp(Keys.Y)))
            {
                bloomSettingsIndex = (bloomSettingsIndex + 1) %
                                     BloomSettings.PresetSettings.Length;

                bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                bloom.Visible = true;
            }

            // Toggle bloom on or off?
            if ((currentGamePadState.Buttons.B == ButtonState.Pressed &&
                 lastGamePadState.Buttons.B != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.B) &&
                 lastKeyboardState.IsKeyUp(Keys.B)))
            {
                bloom.Visible = !bloom.Visible;
            }

            // Cycle through the intermediate buffer debug display modes?
            if ((currentGamePadState.Buttons.X == ButtonState.Pressed &&
                 lastGamePadState.Buttons.X != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.X) &&
                 lastKeyboardState.IsKeyUp(Keys.X)))
            {
                bloom.Visible = true;
                bloom.ShowBuffer++;

                if (bloom.ShowBuffer > BloomComponent.IntermediateBuffer.FinalResult)
                    bloom.ShowBuffer = 0;
            }
        }
        #endregion
    }
}
