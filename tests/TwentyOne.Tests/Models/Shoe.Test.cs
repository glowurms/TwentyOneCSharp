using TwentyOne.Models;
using TwentyOne.Models.Enums;

namespace TwentyOne.Tests.Models
{
    public class ShoeTest
    {
        [Fact]
        public void Shoe_Initialized_Correctly()
        {
            int rankCount = Enum.GetValues<Rank>().Length;
            int suitCount = Enum.GetValues<Suit>().Length;
            
            // Test default constructor (1 deck)
            int totalCardCount = rankCount * suitCount;

            Shoe shoe = new();

            Assert.Equal(1, shoe.DeckCount);
            Assert.Equal(totalCardCount, shoe.TotalCardCount);
            Assert.Equal(totalCardCount, shoe.UndealtCardCount);

            // Test constructor with specified deck count (1 deck)
            int shoeDeckCount = 1;
            totalCardCount = rankCount * suitCount * shoeDeckCount;

            shoe = new(shoeDeckCount);

            Assert.Equal(shoeDeckCount, shoe.DeckCount);
            Assert.Equal(totalCardCount, shoe.TotalCardCount);
            Assert.Equal(totalCardCount, shoe.UndealtCardCount);


            // Test constructor with multiple deck count (3 decks)
            shoeDeckCount = 3;
            totalCardCount = rankCount * suitCount * shoeDeckCount;

            shoe = new(shoeDeckCount);

            Assert.Equal(shoeDeckCount, shoe.DeckCount);
            Assert.Equal(totalCardCount, shoe.TotalCardCount);
            Assert.Equal(totalCardCount, shoe.UndealtCardCount);
        }

        [Fact]
        public void Shuffle_SetsCutCardPosition_Correctly()
        {
            Shoe shoe = new(4); // 4 decks
            int totalCardCount = shoe.TotalCardCount;

            shoe.Shuffle();

            int cutCardPosition = shoe.CutCardPosition;

            Assert.InRange(cutCardPosition, (int)(totalCardCount * 0.15), (int)(totalCardCount * 0.25));
        }

        [Fact]
        public void CutCardReached_ValueUpdates_Correctly()
        {
            Shoe shoe = new(2); // 2 decks
            shoe.Shuffle();

            int cutCardPosition = shoe.CutCardPosition;

            // Deal cards until just above the cut card position
            while (shoe.UndealtCardCount > cutCardPosition + 1)
            {
                shoe.DealCard();
            }

            Assert.False(shoe.CutCardReached);

            // Deal one more card to reach the cut card position
            shoe.DealCard();

            Assert.True(shoe.CutCardReached);
        }
    }
}