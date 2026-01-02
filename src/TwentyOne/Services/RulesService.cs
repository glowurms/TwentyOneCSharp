using TwentyOne.Constants;
using TwentyOne.Models;

namespace TwentyOne.Services;

public static class RulesService
{
    public static int HandValue(Hand hand)
    {
        int value = 0;
        int aceCount = hand.CardsInHand.Count(card => card.Rank == Rank.Ace);
        foreach (var card in hand.CardsInHand)
        {
            if (card.Rank != Rank.Ace)
            {
                value += CardConstants.RankValues[card.Rank];
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

    public static bool HandIsBust(Hand hand) { return HandValue(hand) > 21; }

    public static bool HandIsTwentyOne(Hand hand) { return HandValue(hand) == 21; }

    public static bool HandIsNatural(Hand hand)
    {
        return hand.TotalCardCount == 2 && HandValue(hand) == 21; 
    }

    public static bool CanDoubleDown(Hand hand)
    {
        bool handIsOnlyTwoCards = hand.CardsInHand.Count == 2;
        int handValue = HandValue(hand);
        return handIsOnlyTwoCards && handValue >= 9 && handValue <= 11; 
    }

    public static bool CanSplitHand(Hand hand, int playerHandCount)
    {
        bool resplitAllowed = playerHandCount < GameConstants.MaxResplitCount;
        bool handIsOnlyTwoCards = hand.CardsInHand.Count == 2;
        bool cardsMatch = hand.CardsInHand[0].Rank == hand.CardsInHand[1].Rank;
        return resplitAllowed && handIsOnlyTwoCards && cardsMatch; 
    }

    public static bool DealerShouldDraw(Hand dealerHand)
    {
        return HandValue(dealerHand) < GameConstants.DealerStandThreshold;
    }
}