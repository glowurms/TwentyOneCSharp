using Spectre.Console;
using TwentyOne.Models;

namespace TwentyOne.Services
{
    public class GameDisplayService
    {
        private Text _headerText;
        private GameState _gameState;

        public GameDisplayService(ref GameState gameState)
        {
            _gameState = gameState;
            _headerText = new Text("TwentyOne Game", new Style(Color.Aqua)).LeftJustified();
        }

        public void RenderGame()
        {
            AnsiConsole.Clear();
            Table dealerShoeInstructionsTable = new();
            dealerShoeInstructionsTable.Border(TableBorder.Rounded);
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

            Table playerBankrollTable = new();
            playerBankrollTable.Border(TableBorder.Rounded);
            playerBankrollTable.AddColumn("PlayerHand");
            playerBankrollTable.AddColumn("Bankroll");
            playerBankrollTable.Columns[0].Width = 53;
            playerBankrollTable.Columns[1].Width = 20;
            // Total adds up to 80 with padding and borders
            playerBankrollTable.AddRow(
                CreatePlayerHands(),
                new Text($"${_gameState.Players[0].Bankroll}"));

            Rows rowsToDisplay = new(
                _headerText,
                dealerShoeInstructionsTable,
                new Text("Available actions will go here...", new Style(Color.Green)).LeftJustified(),
                playerBankrollTable,
                StatusMessageText());

            AnsiConsole.Write(rowsToDisplay);
        }

        private Text StatusMessageText()
        {
            return new Text(_gameState.StatusMessage, new Style(Color.Yellow)).LeftJustified();
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
            handPanel.Header = new PanelHeader($" Hand [[{GameService.HandValue(hand)}]] ");
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