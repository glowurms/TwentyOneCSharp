using TwentyOne.Models;
using TwentyOne.Models.Enums;

namespace TwentyOne.Tests
{
    public class CardTest
    {
        [Fact]
        public void CardInitialized()
        {
            Card card = new(Rank.Ace, Suit.Hearts);
            Assert.Equal(Rank.Ace, card.Rank);
            Assert.Equal(Suit.Hearts, card.Suit);
        }

        [Fact]
        public void CardToString()
        {
            Card card = new(Rank.King, Suit.Spades);
            Assert.Equal("King of Spades", card.ToString());
        }
    }
}