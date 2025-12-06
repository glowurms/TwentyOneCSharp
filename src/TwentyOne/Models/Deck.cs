using TwentyOne.Models.Enums;

namespace TwentyOne.Models
{
    public class Deck
    {
        public int TotalCardCount { get { return AllCards.Count; } }
        public int UndealtCardCount { get { return UndealtCards.Count; } }

        protected readonly List<Card> AllCards = [];
        protected readonly List<Card> UndealtCards = [];

        public Deck()
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

        // Sets UndealtCards to the full set of AllCards in a random order
        // Shuffle using Fisher-Yates algorithm
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        // Modified to start at index 1 since index 0 does not need to be swapped
        public virtual void Shuffle()
        {
            UndealtCards.Clear();

            foreach (Card card in AllCards)
            {
                UndealtCards.Add(card);
            }

            Random rng = new();
            for (int cardIndex = 1; cardIndex < UndealtCards.Count; cardIndex++)
            {
                int randomCardIndex = rng.Next(cardIndex);
                (UndealtCards[cardIndex], UndealtCards[randomCardIndex]) = (UndealtCards[randomCardIndex], UndealtCards[cardIndex]);
            }
        }

        public Card? DealCard()
        {
            if (UndealtCards.Count > 0)
            {
                Card cardToDeal = UndealtCards[0];
                UndealtCards.Remove(cardToDeal);
                return cardToDeal;
            }
            return null;
        }
    }
}