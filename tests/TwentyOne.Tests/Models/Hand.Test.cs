using TwentyOne.Models;

namespace TwentyOne.Tests.Models
{
    public class HandTest
    {
        [Fact]
        public void AddCard_IncreasesTotalCardCount_Correctly()
        {
            Hand hand = new();
            Card card = new(Rank.Ten, Suit.Hearts);
            Card card2 = new(Rank.Ace, Suit.Hearts);

            hand.AddCard(card);
            Assert.Single(hand.Cards);

            hand.AddCard(card2);
            Assert.Equal(2, hand.Cards.Count);

            Assert.Contains(card, hand.Cards);
            Assert.Contains(card2, hand.Cards);
        }

        [Fact]
        public void HasCard_WorksCorrectly()
        {
            Hand hand = new();
            Card card = new(Rank.Ten, Suit.Hearts);
            Card cardNotInHand = new(Rank.Queen, Suit.Diamonds);

            hand.AddCard(card);
        
            Assert.True(hand.HasCard(card));
            Assert.False(hand.HasCard(cardNotInHand));
        }

        [Fact]
        public void HasRank_WorksCorrectly()
        {
            Hand hand = new();
            hand.AddCard(new Card(Rank.Ten, Suit.Hearts));
            hand.AddCard(new Card(Rank.Five, Suit.Clubs));
        
            Assert.True(hand.HasRank(Rank.Ten));
            Assert.False(hand.HasRank(Rank.Queen));
        }

        [Fact]
        public void HasSuit_WorksCorrectly()
        {
            Hand hand = new();
            hand.AddCard(new Card(Rank.Ten, Suit.Hearts));
            hand.AddCard(new Card(Rank.Five, Suit.Clubs));
        
            Assert.True(hand.HasSuit(Suit.Hearts));
            Assert.False(hand.HasSuit(Suit.Diamonds));
        }

        [Fact]
        public void RemoveCard_WorksCorrectly()
        {
            Hand hand = new();
            Card card = new(Rank.Five, Suit.Spades);
            Card cardNotInHand = new(Rank.Queen, Suit.Diamonds);
            hand.AddCard(card);

            bool removedNotInHand = hand.RemoveCard(cardNotInHand);
            bool removed = hand.RemoveCard(card);
            bool removeFromEmptyHand = hand.RemoveCard(card);

            Assert.False(removedNotInHand);
            Assert.True(removed);
            Assert.Empty(hand.Cards);
            Assert.DoesNotContain(card, hand.Cards);
            Assert.False(removeFromEmptyHand);
        }

        [Fact]
        public void ClearHand_WorksCorrectly()
        {
            Hand hand = new();
            hand.AddCard(new Card(Rank.Three, Suit.Diamonds));
            hand.AddCard(new Card(Rank.Seven, Suit.Clubs));

            hand.ClearHand();

            Assert.Empty(hand.Cards);
        }
    }
}