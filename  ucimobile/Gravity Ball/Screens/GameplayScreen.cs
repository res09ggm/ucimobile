#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using FarseerPhysics;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.SamplesFramework;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

#endregion

namespace GameState
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        //XNA Variables
        public static ContentManager content;
        SpriteFont gameFont;
        float pauseAlpha;
        InputAction pauseAction;
        static GameplayScreen currentGame;
        KeyboardState lastKeyboardState;

        //Level Variables
        public static Player _hero;
        public static Level _currentLevel;
        public static World _world;
        String[] _levels = { "lvl1.xml", "test.xml" };
        int currentLevelNum = 0;
        DateTime timer;
        public TimerManager gameTimers;
        private GameTime privateGameTime;

        public bool completedLevel { get; set; }

        //Camera Controls
        private Vector2 _screenCenter;
        public static Camera2D _camera;

        //Debug Controls
        private bool _debugMode = false;
        private bool _showDebugView = false;
        DebugViewXNA _debugView;

        //Video
        Video video;
        bool IsIntro = true;
        VideoPlayer player;
        Texture2D videoTexture;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);

            currentGame = this;
        }

        public void loadLevel(int levelNumber)
        {
            if (levelNumber >= 0 && levelNumber < _levels.Length)
            {
                String contentdir = content.RootDirectory;
                contentdir += "/Levels/" + _levels[levelNumber];
                initializeLevel();
                _currentLevel = Level.FromFile(contentdir, content);
            }
        }

        public void loadNextLevel()
        {
            GameplayScreen g = GameplayScreen.getInstance();
            _hero = null;
            _world = null;
            _camera = null;
            _currentLevel = null;
            g._debugView = null;

            loadLevel((++currentLevelNum) % _levels.Length);

        }

        public void initializeLevel()
        {
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64);
            if (_currentLevel == null)
                _currentLevel = new Level();
            if (_world == null)
            {
                _world = new World(new Vector2(0f, 12f));
                //_world.Clear();
            }
            if (_hero == null)
                _hero = new Player(_world);

            if (_camera == null)
            {
                _camera = new Camera2D(ScreenManager.GraphicsDevice);
                _camera.EnablePositionTracking = true;
                _camera.TrackingBody = _hero._body;
            }
            if (gameTimers == null)
            {
                gameTimers = new TimerManager(ref _world);
            }

            if (_debugView == null)
            {
                _debugView = new DebugViewXNA(_world);
                _debugView.AppendFlags(DebugViewFlags.DebugPanel);
                _debugView.DefaultShapeColor = Color.White;
                _debugView.SleepingShapeColor = Color.LightGray;
                _debugView.LoadContent(ScreenManager.GraphicsDevice, content);
            }
            completedLevel = false;
            
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                {
                    content = new ContentManager(ScreenManager.Game.Services, "Content");
                }

                gameFont = content.Load<SpriteFont>("gamefont");
                video = content.Load<Video>("intro");
                player = new VideoPlayer();

                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                //Thread.Sleep(1000);
                
                loadLevel(0);

                if (_debugMode)
                {
                    System.Collections.Generic.List<Body> blist = _world.BodyList;

                    foreach (Body b in blist)
                    {
                        Console.WriteLine(b.Position.ToString());
                    }
                }

                //_world.AddController(new FarseerPhysics.Controllers.GravityController(10f, 300f, 0f));
                //_world.Enabled = true;

                //_view = Matrix.Identity;
                //_cameraPosition = Vector2.Zero;
                _screenCenter = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2f,
                                                ScreenManager.GraphicsDevice.Viewport.Height / 2f);
                
                
                


                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }


        public override void Deactivate()
        {

            base.Deactivate();
        }

        public static World getWorld()
        {
            return _world;
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            content.Unload();

#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("PlayerPosition");
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("EnemyPosition");
#endif
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            // hack so we can access the gameTime from outside of update() method.
            // the passed in gameTime is generated every so often, so we must
            // check if our reference to gameTime is the same as the one passed in
            // Perhaps consider using System.DateTime but that creates overhead for
            // locale conversion and checking.
            if (this.privateGameTime == null || !this.privateGameTime.Equals(gameTime))
                this.privateGameTime = gameTime;
            base.Update(gameTime, otherScreenHasFocus, false);
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);
            if(IsIntro)
            {
                player.IsLooped = false;
                //player.Play(video);
                IsIntro = false;
            }

            if(player.State == MediaState.Stopped)
            {
                if(IsActive)
                {                                   
                    // TODO: this game isn't very fun! You could probably improve
                    // it by inserting something more interesting in this space :-)
                    gameTimers.update(gameTime);
                    _hero.update(gameTime);
                    _camera.Update(gameTime);
                }
            }
            
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];
            MouseState mouseState = input.CurrentMouseState;
            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
            {
#if WINDOWS_PHONE
                ScreenManager.AddScreen(new PhonePauseScreen(), ControllingPlayer);
#else
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
#endif
            }
            else
            {
                //Handle Cursor

                //

                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    _hero.moveLeft();
                    _camera.EnablePositionTracking = true;
                    if (_debugMode)
                        Console.WriteLine("Hero moving left.");
                }

                if (keyboardState.IsKeyDown(Keys.S))
                {
                    //Not yet implemented.
                }

                if (keyboardState.IsKeyDown(Keys.D))
                {
                    _hero.moveRight();
                    _camera.EnablePositionTracking = true;
                    if (_debugMode)
                        Console.WriteLine("Hero moving right.");
                }

                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    if (_debugMode)
                        Console.WriteLine("Hero jumping.");
                    _hero.jump();
                    _camera.EnablePositionTracking = true;
                }

                if ((keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift)) &&
                        keyboardState.IsKeyDown(Keys.Space))
                {
                    _hero.action();
                }

                if (keyboardState.IsKeyDown(Keys.NumPad1) || keyboardState.IsKeyDown(Keys.D1))
                {
                    _hero.selectAbility((int)AbilityType.GRAVITY_BALL);
                }

                if (keyboardState.IsKeyDown(Keys.NumPad2) || keyboardState.IsKeyDown(Keys.D2))
                {
                    _hero.selectAbility((int)AbilityType.GRAVITY_SPHERE);
                }

                if (keyboardState.IsKeyDown(Keys.NumPad3) || keyboardState.IsKeyDown(Keys.D3))
                {
                    _hero.selectAbility((int)AbilityType.GRAVITY_HOLE);
                }

                if (keyboardState.IsKeyDown(Keys.NumPad4) || keyboardState.IsKeyDown(Keys.D4))
                {
                    _hero.selectAbility((int)AbilityType.GRAVITY_FLIP);
                }

                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(-50f, 0f)));
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(50f, 0f)));
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(0f, -50f)));
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(0f, 50f)));
                }
                

                if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl))
                {
                    if (keyboardState.IsKeyDown(Keys.PageUp))
                    {
                        _camera.Zoom += .05f;
                    }

                    if (keyboardState.IsKeyDown(Keys.PageDown))
                    {
                        _camera.Zoom -= .05f;
                    }

                    if (keyboardState.IsKeyDown(Keys.D0) || keyboardState.IsKeyDown(Keys.NumPad0))
                    {
                        _camera.Zoom = 1f;
                        _camera.Position = _hero.getSimPosition();
                    }

                    if (keyboardState.IsKeyDown(Keys.OemTilde))
                    {
                        //if (!lastKeyboardState.IsKeyDown(Keys.OemTilde))
                        {
                            toggleDebugViewXNA();
                            _debugMode = !_debugMode;
                        }
                        lastKeyboardState = keyboardState;
                    }
                    if (keyboardState.IsKeyDown(Keys.OemPlus))
                    {
                        if (!lastKeyboardState.IsKeyDown(Keys.OemPlus))
                            loadNextLevel();
                        lastKeyboardState = keyboardState;
                    }
                }

                if(input.IsNewMouseButtonPress(MouseButtons.LeftButton))
                {
                    Vector2 position = _camera.ConvertScreenToWorld(input.Cursor);
                    //Console.WriteLine("World Position: " +position);
                    //Console.WriteLine("Cursor Pos: "+input.Cursor);
                    _hero.setSimPosition(position);

                }
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            if(player.State != MediaState.Stopped)
                videoTexture = player.GetTexture();
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            
            Rectangle screen = new Rectangle(ScreenManager.GraphicsDevice.Viewport.X,
                ScreenManager.GraphicsDevice.Viewport.Y,
                ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _camera.View);
            
            if(player.State == MediaState.Stopped)
                videoTexture = null;
            if(videoTexture != null)
            {
                
                spriteBatch.Draw(videoTexture, screen, Color.White);
                
            }
            else
            {
                //Have each object in the Map draw itself.
                _currentLevel.draw(spriteBatch);
                _hero.draw(spriteBatch);
            }

            spriteBatch.End();

            if (_showDebugView)
            {
                // calculate the projection and view adjustments for the debug view
                Matrix projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(ScreenManager.GraphicsDevice.Viewport.Width),
                                                                 ConvertUnits.ToSimUnits(ScreenManager.GraphicsDevice.Viewport.Height), 0f, 0f, 1f);
                Matrix simProjection = _camera.SimProjection;
                Matrix simView = _camera.SimView;
                // draw the debug view
                _debugView.RenderDebugData(ref simProjection, ref simView);
            }

           
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);
                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        public void toggleDebugViewXNA()
        {
            _showDebugView = !_showDebugView;
        }


        #endregion

        public void showRetryScreen()
        {
            ScreenManager.AddScreen(new RetryScreen(), ControllingPlayer);
        }
            
        public void reload()
        {
            GameplayScreen g = GameplayScreen.getInstance();
            _hero = null;
            _world = null;
            _camera = null;
            _currentLevel = null;
            g._debugView = null;
            completedLevel = false;

            g.loadLevel(g.currentLevelNum);
        }

        public static GameplayScreen getInstance()
        {
            return currentGame;
        }

        public bool loadNextLevelHandler(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (timer == System.DateTime.MinValue)
            {
                completedLevel = true;
                timer = System.DateTime.Now;
                timer = timer.AddSeconds(1.5);
                return false;
            }
            else if (System.DateTime.Now >= timer)
            {
                //TODO create congratulations screen
                // GameplayScreen.getInstance().showNewLevelScreen();
                completedLevel = false;
                timer = DateTime.MinValue;
                loadNextLevel();

                return true;
            }
            else return true;
        }

        /*public GameTime getGameTime()
        {
            return this.privateGameTime;
        }*/

    }
}