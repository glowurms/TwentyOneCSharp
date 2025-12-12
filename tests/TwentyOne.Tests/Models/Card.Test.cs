using TwentyOne.Models;

namespace TwentyOne.Tests.Models
{
    public class CardTest
    {
        [Fact]
        public void Card_Initialized_Correctly()
        {
            Card card = new(Rank.Ace, Suit.Hearts);
            Assert.Equal(Rank.Ace, card.Rank);
            Assert.Equal(Suit.Hearts, card.Suit);
        }

        [Fact]
        public void Card_ToString_Correctly()
        {
            Card card = new(Rank.King, Suit.Spades);
            Assert.Equal("King of Spades", card.ToString());
        }
    }
}