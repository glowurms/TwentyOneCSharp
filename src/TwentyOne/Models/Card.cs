using TwentyOne.Models.Enums;

namespace TwentyOne.Models
{
    public class Card(Rank CardRank, Suit CardSuit)
    {
        public Rank Rank { get; } = CardRank;
        public Suit Suit { get; } = CardSuit;

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}