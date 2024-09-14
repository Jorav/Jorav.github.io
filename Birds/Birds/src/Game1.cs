using Birds.src.BVH;
using Birds.src;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Birds.src
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        private AABBTree controllerTree;
        private Camera camera;
        //private PerformanceMeasurer performanceMeasurer;
        //private MeanSquareError meanSquareError;
        public static int ScreenWidth;
        public static int ScreenHeight;
        public static float GRAVITY = 10;
        public static SpriteFont font;
        public static float timeStep = (1f / 60f);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ScreenWidth = _graphics.PreferredBackBufferWidth;
            ScreenHeight = _graphics.PreferredBackBufferHeight;
            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _graphics.ApplyChanges();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D textureParticle = Content.Load<Texture2D>("RotatingHull");
            font = Content.Load<SpriteFont>("font");
            //Sprite spriteParticle = new Sprite(textureParticle);
            controllerTree = new AABBTree();
            List<WorldEntity> returnedList = new List<WorldEntity>();
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(1134, 245)));
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(1124, 15)));
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(1124, 120)));
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(0, 0)));
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(3, 300)));
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(500, 5)));
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(110, 0)));
            returnedList.Add(new WorldEntity(textureParticle, new Vector2(232, 300)));

            //string[] ConfigVar = EntityFactory.ReadConfig();
            //GRAVITY = float.Parse(ConfigVar[2]);
            //List<WorldEntity> returnedList = EntityFactory.EntFacImplementation(ConfigVar[0], ConfigVar[1], textureParticle);
            GRAVITY = 10;

            controllerTree.root = controllerTree.CreateTreeTopDown_Median(null, returnedList);
            /*foreach(WorldEntity w in returnedList)
            {
                controllerTree.Add(w);
            }*/
            camera = new Camera(controllerTree) { AutoAdjustZoom = true };
            //performanceMeasurer = new PerformanceMeasurer();

            // TODO: Use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = default;
            try { gamePadState = GamePad.GetState(PlayerIndex.One); }
            catch (NotImplementedException) { /* ignore gamePadState */ }

            if (keyboardState.IsKeyDown(Keys.Escape) ||
                keyboardState.IsKeyDown(Keys.Back) ||
                gamePadState.Buttons.Back == ButtonState.Pressed)
            {
                try { Exit(); }
                catch (PlatformNotSupportedException) { /* ignore */ }
            }

            // TODO: Add your update logic here
            ScreenWidth = _graphics.PreferredBackBufferWidth;
            ScreenHeight = _graphics.PreferredBackBufferHeight;
            controllerTree.Update();
            camera.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(transformMatrix: camera.Transform);
            controllerTree.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
