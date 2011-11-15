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
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System.Collections;

using GameStateManagement;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.DebugViews;

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

        //Level Variables
        public static Player _hero;
        public static Level _currentLevel;
        public static World _world;
        String[] _levels = { "test.xml", "lvl1.xml", "sandbox.xml" };
        int currentLevel = 0;

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
            _world.Clear();
            loadLevel((currentLevel++) % _levels.Length);

        }

        public void initializeLevel()
        {
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64);
            if (_currentLevel == null)
                _currentLevel = new Level();
            if (_world == null)
                _world = new World(new Vector2(0f, 20f));
            if (_hero == null)
                _hero = new Player(ref _world);
            

            if (_camera == null)
            {
                _camera = new Camera2D(ScreenManager.GraphicsDevice);
                _camera.EnablePositionTracking = true;
                _camera.TrackingBody = _hero._body;
            }
            
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

#if WINDOWS_PHONE
            if (Microsoft.Phone.Shell.PhoneApplicationService.Current.State.ContainsKey("PlayerPosition"))
            {
                playerPosition = (Vector2)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["PlayerPosition"];
                enemyPosition = (Vector2)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["EnemyPosition"];
            }
#endif
        }


        public override void Deactivate()
        {
#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["PlayerPosition"] = playerPosition;
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["EnemyPosition"] = enemyPosition;
#endif

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
                    _hero.update();
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
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    _hero.moveLeft();
                    _camera.EnablePositionTracking = true;
                    if (_debugMode)
                        Console.WriteLine("Hero moving left.");
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    _hero.moveRight();
                    _camera.EnablePositionTracking = true;
                    if (_debugMode)
                        Console.WriteLine("Hero moving right.");
                }

                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    //Not yet implemented.
                }

                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    //Not yet implemented.
                }

                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    _hero.jump();
                    _camera.EnablePositionTracking = true;
                    if (_debugMode)
                        Console.WriteLine("Hero jumping.");
                }

                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    if (keyboardState.IsKeyDown(Keys.A))
                    {
                        _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(-50f, 0f)));
                    }
                    if (keyboardState.IsKeyDown(Keys.D))
                    {
                        _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(50f, 0f)));
                    }
                    if (keyboardState.IsKeyDown(Keys.W))
                    {
                        _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(0f, -50f)));
                    }
                    if (keyboardState.IsKeyDown(Keys.S))
                    {
                        _camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(0f, 50f)));
                    }
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
                        toggleDebugViewXNA();
                        _debugMode = !_debugMode;
                    }

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
                                               Color.CornflowerBlue, 0, 0);

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
            if (_debugView == null)
            {
                _debugView = new DebugViewXNA(_world);
                _debugView.AppendFlags(DebugViewFlags.DebugPanel);
                _debugView.DefaultShapeColor = Color.White;
                _debugView.SleepingShapeColor = Color.LightGray;
                _debugView.LoadContent(ScreenManager.GraphicsDevice, content);
            }
            _showDebugView = !_showDebugView;
        }


        #endregion
    }
}
