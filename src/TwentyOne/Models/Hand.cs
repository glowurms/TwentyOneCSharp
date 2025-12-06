using TwentyOne.Models.Enums;

namespace TwentyOne.Models
{
    public class Hand
    {
        public int TotalCardCount { get { return Cards.Count; } }
        public List<Card> CardsInHand { get { return Cards; } }

        private readonly List<Card> Cards = [];

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public bool HasCard(Card card)
        {
            return Cards.Contains(card);
        }

        public bool RemoveCard(Card card)
        {
            return Cards.Remove(card);
        }

        public void ClearHand()
        {
            Cards.Clear();
        }
    }
}