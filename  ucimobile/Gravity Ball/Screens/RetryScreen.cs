using System;
using Microsoft.Xna.Framework;

namespace GameState
{
    /// <summary>
    /// The retry menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class RetryScreen : MenuScreen
    {
        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public RetryScreen()
            : base("Paused")
        {
            // Create our menu entries.
            MenuEntry retryGameMenuEntry = new MenuEntry("Replay Level");
            MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");

            // Hook up menu event handlers.
            retryGameMenuEntry.Selected += retrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(retryGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to quit this game?";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }

        /// <summary>
        /// Event handler for when the Replay menu entry is selected.
        /// </summary>
        void retrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(, ControllingPlayer);
            GameplayScreen.getInstance().reload();
            this.OnCancel(sender, e);
        }
        #endregion

    }
}
