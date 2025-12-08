using Spectre.Console;
using TwentyOne.Models;

namespace TwentyOne.Services
{
    public class GameDisplayService
    {
        private Text _headerText;
        private Panel _dealerPanel;
        private Panel _playerPanel;
        private Panel _shoePanel;
        private Panel _bankrollPanel;

        private Columns _dealerSpace;
        private Columns _playerSpace;
        private Columns _actionsSpace;

        private GameState _gameState;

        public GameDisplayService(ref GameState gameState)
        {
            _gameState = gameState;

            _headerText = new Text("Twenty-One Game", new Style(Color.Aqua)).Centered();

            _dealerPanel = new(new Text("Dealer's Hand"));
            _dealerPanel.Expand = true;

            _playerPanel = new(new Text("Player's Hand"));
            _playerPanel.Expand = true;

            _shoePanel = new(new Text("Shoe Status"));
            _bankrollPanel = new(new Text("Player's Bankroll"));

            _dealerSpace = new(_dealerPanel, _shoePanel);
            _playerSpace = new(_playerPanel, _bankrollPanel);
            _actionsSpace = new();
        }

        public void RenderGame()
        {
            AnsiConsole.Clear();
            Grid twentyOneGameGrid = new();
            twentyOneGameGrid.AddColumn();
            twentyOneGameGrid.AddRow(_headerText);
            twentyOneGameGrid.AddRow(_dealerSpace);
            twentyOneGameGrid.AddRow(_actionsSpace);
            twentyOneGameGrid.AddRow(_playerSpace);
            twentyOneGameGrid.AddRow(StatusMessageText());
            AnsiConsole.Write(twentyOneGameGrid);
        }

        public void RenderInstructions()
        {
            AnsiConsole.Clear();
            Grid twentyOneGameGrid = new();
            twentyOneGameGrid.AddColumn();
            twentyOneGameGrid.AddRow(_headerText);
            twentyOneGameGrid.AddRow(CreateInstructions());
            twentyOneGameGrid.AddRow(StatusMessageText());
            AnsiConsole.Write(twentyOneGameGrid);
        }

        private Text StatusMessageText()
        {
            return new Text(_gameState.StatusMessage, new Style(Color.Yellow)).Centered();
        }

        private Table CreateInstructions()
        {
            Table instructionsTable = new();
            instructionsTable.HideHeaders();
            instructionsTable.Border(TableBorder.Rounded);
            instructionsTable.AddColumn("Action");
            instructionsTable.Columns[0].Padding = new Padding(1, 1, 1, 1);

            foreach (var action in GameService.ActionKeys)
            {
                instructionsTable.AddRow(new Markup($"[bold]{action.Value}[/] - {action.Key}"));
            }

            instructionsTable.AddRow(new Text("----").Centered());

            foreach (var action in GameService.GameActionKeys)
            {
                instructionsTable.AddRow(new Markup($"[bold]{action.Value}[/] - {action.Key}"));
            }

            return instructionsTable;
        }
    }
}