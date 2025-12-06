using TwentyOne.Models.Enums;

namespace TwentyOne.Models
{
    public class Shoe : Deck
    {
        public readonly int DeckCount = 1;
        public int CutCardPosition { get { return _cutCardPosition; } }
        public bool CutCardReached { get { return UndealtCardCount <= CutCardPosition; } }

        private int _cutCardPosition = 0;

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

        public override void Shuffle()
        {
            base.Shuffle();
            Random rng = new();
            // Cut card position between 15% and 25% of total cards in shoe
            _cutCardPosition = rng.Next((int)(TotalCardCount * 0.15), (int)(TotalCardCount * 0.25));
        }
    }
}