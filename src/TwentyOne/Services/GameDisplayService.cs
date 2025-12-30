using Spectre.Console;
using TwentyOne.Models;

namespace TwentyOne.Services
{
    public class GameDisplayService
    {
        private Text _headerText;
        private GameService _gameService;
        private GameState _gameState { get { return _gameService.GameState; } }

        public GameDisplayService(ref GameService gameService)
        {
            _gameService = gameService;
            _headerText = new Text("TwentyOne Game", new Style(Color.Aqua)).LeftJustified();
        }

        public static void ClearGame()
        {
            AnsiConsole.Clear();
        }
        public void RenderGame()
        {
            AnsiConsole.Clear();
            Table dealerShoeInstructionsTable = new();
            dealerShoeInstructionsTable.Border(TableBorder.Heavy);
            dealerShoeInstructionsTable.BorderColor(Color.DarkGreen);
            dealerShoeInstructionsTable.AddColumn("Dealer");
            dealerShoeInstructionsTable.AddColumn("Shoe");
            dealerShoeInstructionsTable.AddColumn("Keys");
            dealerShoeInstructionsTable.Columns[0].Width = 30;
            dealerShoeInstructionsTable.Columns[1].Width = 20;
            dealerShoeInstructionsTable.Columns[2].Width = 20;
            // Total adds up to 80 with padding and borders
            dealerShoeInstructionsTable.AddRow(
                CreateHandPanel(_gameState.DealerHand),
                CreateShoeInfo(),
                KeymapInfo());

            List<Table> playerTables = [];
            foreach(Player player in _gameState.Players)
            {
                playerTables.Add(CreatePlayerTable(player));
            }
            Columns playerColumns = new(playerTables);

            Text infoText = new Text(_gameService.GameStateInfo, new Style(Color.Grey)).LeftJustified();

            Rows rowsToDisplay = new(
                _headerText,
                dealerShoeInstructionsTable,
                new Text("Available actions will go here...", new Style(Color.Green)).LeftJustified(),
                playerColumns,
                StatusMessageText(),
                infoText);

            AnsiConsole.Write(rowsToDisplay);
        }

        private static Table CreatePlayerTable(Player player)
        {
            Table playerTable = new();
            playerTable.Border(TableBorder.Heavy);
            playerTable.BorderColor(Color.DarkCyan);
            playerTable.AddColumn(player.Name);
            playerTable.Columns[0].Width = 26;

            Panel bankrollPanel = new($"{player.Bankroll}");
            bankrollPanel.BorderColor(Color.Gold1);
            bankrollPanel.Padding = new Padding(2, 2, 2, 2);
            bankrollPanel.Header = new PanelHeader($"Bankroll");
            bankrollPanel.Expand = true;

            playerTable.AddRow(bankrollPanel);

            foreach (var hand in player.HandsInPlay)
            {
                playerTable.AddRow(CreateHandPanel(hand));
            }

            return playerTable;
        }

        private Text StatusMessageText()
        {
            return new Text(_gameService.StatusMessage, new Style(Color.Yellow)).LeftJustified();
        }

        private static Rows KeymapInfo()
        {
            List<Markup> instructionMarkups = [];

            foreach (var action in GameService.ActionKeys)
            {
                instructionMarkups.Add(new Markup($"[bold][[{action.Value}]][/] - {action.Key}"));
            }

            instructionMarkups.Add(new Markup("----").Centered());

            foreach (var action in GameService.GameActionKeys)
            {
                instructionMarkups.Add(new Markup($"[bold][[{action.Value}]][/] - {action.Key}"));
            }

            return new Rows(instructionMarkups);
        }

        private static Panel CreateHandPanel(Hand hand)
        {
            List<string> cardStrings = [];
            foreach (Card card in hand.CardsInHand)
            {
                cardStrings.Add(card.ToString());
            }

            Panel handPanel = new(string.Join("\n", cardStrings));
            handPanel.Expand = true;
            handPanel.Border = BoxBorder.Rounded;
            handPanel.BorderColor(Color.Silver);
            handPanel.Padding = new Padding(2, 2, 2, 2);
            handPanel.Header = new PanelHeader($" Hand [[ Value {GameService.HandValue(hand)} ]] ");
            return handPanel;
        }

        private Columns CreatePlayerHands()
        {
            List<Panel> handPanels = [];
            foreach (var hand in _gameState.Players[0].HandsInPlay)
            {
                handPanels.Add(CreateHandPanel(hand));
            }

            Columns playerHands = new(handPanels);
            return playerHands;
        }

        private Rows CreateShoeInfo()
        {
            Panel undealtCardsPanel = new($"{_gameState.Shoe.UndealtCardCount}");
            undealtCardsPanel.Border = BoxBorder.Rounded;
            undealtCardsPanel.Header = new PanelHeader("Undealt Cards");
            undealtCardsPanel.Expand = true;
            undealtCardsPanel.Padding = new Padding(2, 2, 2, 2);

            Panel decksPanel = new($"{_gameState.Shoe.DeckCount}");
            decksPanel.Border = BoxBorder.Rounded;
            decksPanel.Header = new PanelHeader("Decks in Shoe");
            decksPanel.Expand = true;
            decksPanel.Padding = new Padding(2, 2, 2, 2);

            return new Rows(undealtCardsPanel, decksPanel);
        }
    }
}