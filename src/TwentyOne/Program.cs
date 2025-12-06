using TwentyOne.Models;
using Spectre.Console;

namespace TwentyOne
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[bold yellow]Welcome to Twenty-One![/]");
                AnsiConsole.MarkupLine("1. Start New Game");
                AnsiConsole.MarkupLine("2. Exit");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an option:")
                        .AddChoices(new[] { "Start New Game", "Exit" }));

                switch (choice)
                {
                    case "Start New Game":
                        running = false;
                        break;
                    case "Exit":
                        running = false;
                        break;
                }
            }
        }
    }
}