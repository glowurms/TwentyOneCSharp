using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool running = true;
            bool updateConsole = true;

            // Key bindings for game actions
            Dictionary<ConsoleKey, PlayerGameActions> GameActionKeys = new()
            {
                { ConsoleKey.Spacebar, PlayerGameActions.Continue },
                { ConsoleKey.Escape, PlayerGameActions.Cancel },
                { ConsoleKey.I, PlayerGameActions.Instructions },
                { ConsoleKey.Q, PlayerGameActions.Quit }
            };

            // Key bindings for player actions
            Dictionary<ConsoleKey, PlayerHandActions> PlayerActionKeys = new()
            {
                { ConsoleKey.B, PlayerHandActions.Bet },
                { ConsoleKey.F, PlayerHandActions.Hit },
                { ConsoleKey.S, PlayerHandActions.Stand },
                { ConsoleKey.D, PlayerHandActions.DoubleDown },
                { ConsoleKey.A, PlayerHandActions.Split }
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
                    Console.WriteLine(gameService.GameStateInfo);
                    updateConsole = false;
                }

                ConsoleKeyInfo playerInput = Console.ReadKey(true);

                if (GameActionKeys.TryGetValue(playerInput.Key, out PlayerGameActions gameAction))
                {
                    switch (gameAction)
                    {
                        case PlayerGameActions.Cancel:
                            running = false;
                            break;
                        case PlayerGameActions.Quit:
                            running = false;
                            break;
                        case PlayerGameActions.Continue:
                            gameService.ContinueGame();
                            updateConsole = true;
                            break;
                        default:
                            break;
                    }
                    // GameDisplayService.ClearGame();
                }
                else if (PlayerActionKeys.TryGetValue(playerInput.Key, out PlayerHandActions playerHandAction))
                {
                    gameService.HandlePlayerAction(playerHandAction);
                    updateConsole = true;
                }
            }
            Console.Clear();
        }
    }
}