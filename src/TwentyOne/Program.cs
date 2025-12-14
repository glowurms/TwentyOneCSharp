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
            gameService.StartNewGame(500, 6, 3);
            GameDisplayService gameDisplay = new(ref gameService);

            gameService.DealInitialCards();

            while (running)
            {
                gameDisplay.RenderGame();
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