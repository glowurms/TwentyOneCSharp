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

        public static void DealInitialCards(ref Shoe shoe, ref Hand dealerHand, ref Hand playerHand)
        {
            if (shoe.CutCardReached || shoe.CutCardPosition == 0)
            {
                shoe.Shuffle();
            }

            playerHand.AddCard(DealCardFromShoe(ref shoe));
            dealerHand.AddCard(DealCardFromShoe(ref shoe));
            playerHand.AddCard(DealCardFromShoe(ref shoe));
            dealerHand.AddCard(DealCardFromShoe(ref shoe));
        }

        public static List<PlayerActions> AvailablePlayerActions(Hand hand)
        {
            var actions = new List<PlayerActions> { PlayerActions.Stand };

            if (hand.TotalCardCount == 2)
            {
                actions.Add(PlayerActions.DoubleDown);

                if (CanSplitHand(hand))
                {
                    actions.Add(PlayerActions.Split);
                }
            }

            actions.Add(PlayerActions.Hit);

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

        public static bool DealerShouldDraw(Hand dealerHand)
        {
            return HandValue(dealerHand) < 17;
        }

        private static bool CanSplitHand(Hand hand)
        {
            if(hand.CardsInHand[0].Rank == hand.CardsInHand[1].Rank)
            {
                return true;
            }
            return false;
        }

        private static Card DealCardFromShoe(ref Shoe shoe)
        {
            Card? card = shoe.DealCard();
            return card ?? throw new Exception("Not enough cards in the shoe.");
        }
    }
}