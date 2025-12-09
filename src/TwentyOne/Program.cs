using TwentyOne.Models;
using TwentyOne.Models.Enums;
using Spectre.Console;
using TwentyOne.Services;

namespace TwentyOne
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool running = true;

            GameState gameState = new()
            {
                Shoe = new Shoe(3),
                DealerHand = new Hand(),
                Players = [new Player("Player1", 100m)]
            };

            GameService GameService = new(ref gameState);
            GameDisplayService gameDisplay = new(ref gameState);

            GameService.DealInitialCards();

            while (running)
            {
                gameDisplay.RenderGame();
                ConsoleKeyInfo playerInput = Console.ReadKey();
                if (GameService.GameActionKeys[PlayerGameActions.Quit] == playerInput.Key)
                {
                    running = false;
                }
            }
        }
    }
}