using TwentyOne.Models;
using TwentyOne.Models.Enums;

public class HandTest
{
    [Fact]
    public void AddCard_IncreasesTotalCardCount()
    {
        Hand hand = new Hand();
        Card card = new Card(Rank.Ten, Suit.Hearts);

        hand.AddCard(card);

        Assert.Equal(1, hand.TotalCardCount);
        Assert.Contains(card, hand.CardsInHand);
    }

    [Fact]
    public void HasCard()
    {
        Hand hand = new Hand();
        Card card = new Card(Rank.Ten, Suit.Hearts);
        Card cardNotInHand = new Card(Rank.Queen, Suit.Diamonds);

        hand.AddCard(card);
    
        Assert.True(hand.HasCard(card));
        Assert.False(hand.HasCard(cardNotInHand));
    }

    [Fact]
    public void RemoveCard()
    {
        Hand hand = new Hand();
        Card card = new Card(Rank.Five, Suit.Spades);
        Card cardNotInHand = new Card(Rank.Queen, Suit.Diamonds);
        hand.AddCard(card);

        bool removedNotInHand = hand.RemoveCard(cardNotInHand);
        bool removed = hand.RemoveCard(card);
        bool removeFromEmptyHand = hand.RemoveCard(card);

        Assert.False(removedNotInHand);
        Assert.True(removed);
        Assert.Equal(0, hand.TotalCardCount);
        Assert.DoesNotContain(card, hand.CardsInHand);
        Assert.False(removeFromEmptyHand);
    }

    [Fact]
    public void ClearHand()
    {
        Hand hand = new Hand();
        hand.AddCard(new Card(Rank.Three, Suit.Diamonds));
        hand.AddCard(new Card(Rank.Seven, Suit.Clubs));

        hand.ClearHand();

        Assert.Equal(0, hand.TotalCardCount);
    }
}