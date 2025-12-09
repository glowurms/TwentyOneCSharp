using TwentyOne.Models;
using TwentyOne.Models.Enums;

namespace TwentyOne.Services
{
    public class GameService
    {
        public static readonly Dictionary<Rank, int> RankValues = new()
        {
            { Rank.Ace, 11 },
            { Rank.Two, 2 },
            { Rank.Three, 3 },
            { Rank.Four, 4 },
            { Rank.Five, 5 },
            { Rank.Six, 6 },
            { Rank.Seven, 7 },
            { Rank.Eight, 8 },
            { Rank.Nine, 9 },
            { Rank.Ten, 10 },
            { Rank.Jack, 10 },
            { Rank.Queen, 10 },
            { Rank.King, 10 }
        };

        public static readonly Dictionary<PlayerHandActions, string> ActionDescriptions = new()
        {
            { PlayerHandActions.Hit, "Take another card" },
            { PlayerHandActions.Stand, "Keep your current hand" },
            { PlayerHandActions.DoubleDown, "Double your bet and take one more card" },
            { PlayerHandActions.Split, "Split your hand into two hands" }
        };

        public static readonly Dictionary<PlayerHandActions, ConsoleKey> ActionKeys = new()
        {
            { PlayerHandActions.Hit, ConsoleKey.Spacebar },
            { PlayerHandActions.Stand, ConsoleKey.S },
            { PlayerHandActions.DoubleDown, ConsoleKey.D },
            { PlayerHandActions.Split, ConsoleKey.F }
        };

        public static readonly Dictionary<PlayerGameActions, ConsoleKey> GameActionKeys = new()
        {
            { PlayerGameActions.Instructions, ConsoleKey.I },
            { PlayerGameActions.Quit, ConsoleKey.Q }
        };

        private GameState _gameState;

        public GameService(ref GameState gameState)
        {
            _gameState = gameState;
        }

        public void DealInitialCards()
        {
            if (_gameState.Shoe.CutCardReached || _gameState.Shoe.CutCardPosition == 0)
            {
                _gameState.Shoe.Shuffle();
            }

            _gameState.DealerHand = new Hand();

            _gameState.Players[0].HandsInPlay.Clear();
            _gameState.Players[0].HandsInPlay.Add(new Hand());

            _gameState.Players[0].HandsInPlay[0].AddCard(DealCardFromShoe());
            _gameState.DealerHand.AddCard(DealCardFromShoe());
            _gameState.Players[0].HandsInPlay[0].AddCard(DealCardFromShoe());
            _gameState.DealerHand.AddCard(DealCardFromShoe());
        }

        public static List<PlayerHandActions> AvailablePlayerActions(Hand hand)
        {
            var actions = new List<PlayerHandActions> { PlayerHandActions.Stand };

            if (hand.TotalCardCount == 2)
            {
                actions.Add(PlayerHandActions.DoubleDown);

                if (CanSplitHand(hand))
                {
                    actions.Add(PlayerHandActions.Split);
                }
            }

            actions.Add(PlayerHandActions.Hit);

            return actions;
        }

        public static int HandValue(Hand hand)
        {
            int value = 0;
            int aceCount = hand.CardsInHand.Count(c => c.Rank == Rank.Ace);
            foreach (var card in hand.CardsInHand)
            {
                if (card.Rank != Rank.Ace)
                {
                    value += RankValues[card.Rank];
                }
            }
            value += aceCount; // Count all aces as 1 initially
            while (value <= 11 && aceCount > 0)
            {
                value += 10; // Upgrade an ace from 1 to 11
                aceCount--;
            }
            return value;
        }

        public static bool HandIsBust(Hand hand)
        {
            if (HandValue(hand) > 21)
            {
                return true;
            }
            return false;
        }

        public static bool HandIsTwentyOne(Hand hand)
        {
            if (HandValue(hand) == 21)
            {
                return true;
            }
            return false;
        }

        public bool DealerShouldDraw()
        {
            return HandValue(_gameState.DealerHand) < 17;
        }

        private static bool CanSplitHand(Hand hand)
        {
            if(hand.CardsInHand[0].Rank == hand.CardsInHand[1].Rank)
            {
                return true;
            }
            return false;
        }

        private Card DealCardFromShoe()
        {
            Card? card = _gameState.Shoe.DealCard();
            return card ?? throw new Exception("Not enough cards in the shoe.");
        }
    }
}