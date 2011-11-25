using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameState
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            String filename = null;
            if (args.Length > 0)
                filename = args[0];

            if (filename == null)
            {
                using (GameStateManagementGame game = new GameStateManagementGame())
                {
                    game.Run();
                }
            }
            else
            {
                using (GameStateManagementGame game = new GameStateManagementGame(filename))
                {
                    game.Run();
                }
            }
        }
    }
#endif
}
