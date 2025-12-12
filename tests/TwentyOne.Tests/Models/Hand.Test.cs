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

            hand.AddCard(card);

            Assert.Equal(1, hand.TotalCardCount);
            Assert.Contains(card, hand.CardsInHand);
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
            Assert.Equal(0, hand.TotalCardCount);
            Assert.DoesNotContain(card, hand.CardsInHand);
            Assert.False(removeFromEmptyHand);
        }

        [Fact]
        public void ClearHand_WorksCorrectly()
        {
            Hand hand = new();
            hand.AddCard(new Card(Rank.Three, Suit.Diamonds));
            hand.AddCard(new Card(Rank.Seven, Suit.Clubs));

            hand.ClearHand();

            Assert.Equal(0, hand.TotalCardCount);
        }
    }
}