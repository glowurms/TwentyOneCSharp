using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool running = true;

            GameService gameService = new();
            gameService.StartNewGame(3, 500, 6);
            // GameDisplayService gameDisplay = new(ref gameService);

            while (running)
            {
                // gameDisplay.RenderGame();
                Console.Clear();
                Console.WriteLine(gameService.GameStateInfo);
                ConsoleKeyInfo playerInput = Console.ReadKey();
                if (GameService.GameActionKeys[PlayerGameActions.Quit] == playerInput.Key)
                {
                    running = false;
                    // GameDisplayService.ClearGame();
                }
            }
        }
    }
}