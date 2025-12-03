using TwentyOne.Models.Enums;

namespace TwentyOne.Models
{
    public class Shoe : Deck
    {
        public readonly int DeckCount = 1;
        public Shoe(int ShoeDeckCount = 1)
        {
            DeckCount = ShoeDeckCount;
            // First set of cards already added to AllCards and UndealtCards in base Deck constructor
            for (int count = 0; count < DeckCount - 1; count++)
            {
                foreach (Suit suit in Enum.GetValues<Suit>())
                {
                    foreach (Rank rank in Enum.GetValues<Rank>())
                    {
                        Card card = new(rank, suit);
                        AllCards.Add(card);
                        UndealtCards.Add(card);
                    }
                }
            }
        }
    }
}