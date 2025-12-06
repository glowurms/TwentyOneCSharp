using TwentyOne.Models;
using TwentyOne.Models.Enums;

namespace TwentyOne.Tests.Models
{
    public class DeckTest
    {
        [Fact]
        public void DeckInitialized()
        {
            int rankCount = Enum.GetValues<Rank>().Length;
            int suitCount = Enum.GetValues<Suit>().Length;
            int totalCardCount = rankCount * suitCount;

            Deck deck = new();
        
            Assert.Equal(totalCardCount, deck.TotalCardCount);
            Assert.Equal(totalCardCount, deck.UndealtCardCount);
        }

        [Fact]
        public void DeckDealCards()
        {
            int rankCount = Enum.GetValues<Rank>().Length;
            int suitCount = Enum.GetValues<Suit>().Length;
            int totalCardCount = rankCount * suitCount;

            Deck deck = new();
            Card? card;
            
            for (int i = 1; i <= totalCardCount; i++)
            {
                card = deck.DealCard();
                Assert.NotNull(card);
                Assert.Equal(deck.TotalCardCount - i, deck.UndealtCardCount);
            }

            card = deck.DealCard();
            Assert.Null(card);
        }

        [Fact]
        public void DeckShuffle()
        {
            List<string> unshuffledHand = [];
            List<string> shuffledHand = [];

            Deck deck = new();
            Card? card;

            for (int i = 0; i < 10; i++)
            {
                card = deck.DealCard();
                if (card != null)
                {
                    unshuffledHand.Add(card.ToString());
                }
            }

            deck.Shuffle();

            for (int i = 0; i < 10; i++)
            {
                card = deck.DealCard();
                if (card != null)
                {
                    shuffledHand.Add(card.ToString());
                }
            }

            Assert.NotEqual(unshuffledHand, shuffledHand);
        }
    }
}