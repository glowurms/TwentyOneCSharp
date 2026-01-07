using TwentyOne.Constants;
using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool running = true;
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Allows display of suits ♣ ♦ ♥ ♠

            GameService gameService = new();
            gameService.StateChanged = () =>
            {
                // Redraw
                Console.Clear();
                Console.WriteLine(TextService.GameStateSummary(gameService.GameState));
            };

            gameService.StartNewGame(3, 500, 6);

            while (running)
            {
                ConsoleKeyInfo playerInput = Console.ReadKey(true);
                // Game Specific keypress captured
                if (Keybinds.GameAction.TryGetValue(playerInput.Key, out GameActions gameAction))
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
                else if (Keybinds.PlayerAction.TryGetValue(playerInput.Key, out PlayerActions playerHandAction))
                {
                    gameService.SelectPlayerAction(playerHandAction);
                }
            }
            Console.Clear();
        }
    }
}