using TwentyOne.Constants;
using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne
{
    internal class Program
    {
        public static bool DisplaySizeIsTooSmall()
        {
            return Console.WindowWidth < GameConstants.PreferredTerminalWidth || Console.WindowHeight < GameConstants.PreferredTerminalHeight;
        }

        static void Main(string[] args)
        {
            bool running = true;
            
            // Allows display of suits ♣ ♦ ♥ ♠
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            int windowMaxWidth = Console.LargestWindowWidth;
            int windowMaxHeight = Console.LargestWindowHeight;

            string displaySizeWarning = $"Use minimum dimensions of {GameConstants.PreferredTerminalWidth} columns by {GameConstants.PreferredTerminalHeight} rows to prevent wrapping and scrolling.";

            GameService gameService = new();
            gameService.StateChanged = () =>
            {
                // Redraw
                Console.Clear();
                Console.WriteLine(TextService.GameStateSummary(gameService.GameState));
                if (DisplaySizeIsTooSmall())
                {
                    Console.WriteLine(displaySizeWarning);
                }
            };

            gameService.StartNewGame(3, 500, 6);

            while (running)
            {
                ConsoleKeyInfo playerInput = Console.ReadKey(true);
                // Game Specific keypress captured
                if (Keybinds.KeyToGameAction.TryGetValue(playerInput.Key, out GameActions gameAction))
                {
                    if(gameAction == GameActions.Cancel || gameAction == GameActions.Quit)
                    {
                        running = false;
                    }
                    else if(gameAction == GameActions.Continue)
                    {
                        gameService.ContinueGame();
                    }
                }
                // Player specific keypress captured
                else if (Keybinds.KeyToPlayerAction.TryGetValue(playerInput.Key, out PlayerActions playerHandAction))
                {
                    gameService.SelectPlayerAction(playerHandAction);
                }
            }
            Console.Clear();
        }
    }
}