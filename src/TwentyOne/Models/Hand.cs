namespace TwentyOne.Models
{
    public class Hand
    {
        public readonly List<Card> Cards = [];

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public bool HasCard(Card card)
        {
            return Cards.Contains(card);
        }

        public bool HasRank(Rank rank)
        {
            foreach(Card card in Cards)
            {
                if(card.Rank == rank)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasSuit(Suit suit)
        {
            foreach(Card card in Cards)
            {
                if(card.Suit == suit)
                {
                    return true;
                }
            }
            return false;
        }

        public bool RemoveCard(Card card)
        {
            return Cards.Remove(card);
        }

        public void ClearHand()
        {
            Cards.Clear();
        }

        public string HandInfo()
        {
            List<string> cardStrings = [.. Cards.Select(c => c.ToString())];
            return $"Hand {{ Cards.Count: {Cards.Count}; Cards:[{string.Join(", ", cardStrings)}]";
        }
    }
}