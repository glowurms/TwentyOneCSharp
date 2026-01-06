using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Allows display of suits ♣ ♦ ♥ ♠

            bool running = true;
            bool updateConsole = true;

            // Key bindings for game actions
            Dictionary<ConsoleKey, GameActions> GameActionKeys = new()
            {
                { ConsoleKey.Spacebar, GameActions.Continue },
                { ConsoleKey.Escape, GameActions.Cancel },
                { ConsoleKey.I, GameActions.Instructions },
                { ConsoleKey.Q, GameActions.Quit }
            };

            // Key bindings for player actions
            Dictionary<ConsoleKey, PlayerActions> PlayerActionKeys = new()
            {
                { ConsoleKey.B, PlayerActions.Bet },
                { ConsoleKey.F, PlayerActions.Hit },
                { ConsoleKey.S, PlayerActions.Stand },
                { ConsoleKey.D, PlayerActions.DoubleDown },
                { ConsoleKey.A, PlayerActions.Split }
            };

            GameService gameService = new();
            gameService.StartNewGame(3, 500, 6);
            // GameDisplayService gameDisplay = new(ref gameService);

            while (running)
            {
                // gameDisplay.RenderGame();
                if (updateConsole)
                {
                    Console.Clear();
                    Console.WriteLine(TextService.GameStateSummary(gameService.GameState));
                    updateConsole = false;
                }

                ConsoleKeyInfo playerInput = Console.ReadKey(true);

                if (GameActionKeys.TryGetValue(playerInput.Key, out GameActions gameAction))
                {
                    switch (gameAction)
                    {
                        case GameActions.Cancel:
                            running = false;
                            break;
                        case GameActions.Quit:
                            running = false;
                            break;
                        case GameActions.Continue:
                            gameService.ContinueGame();
                            updateConsole = true;
                            break;
                        default:
                            break;
                    }
                }
                else if (PlayerActionKeys.TryGetValue(playerInput.Key, out PlayerActions playerHandAction))
                {
                    gameService.SelectPlayerAction(playerHandAction);
                    updateConsole = true;
                }
            }
            Console.Clear();
        }
    }
}