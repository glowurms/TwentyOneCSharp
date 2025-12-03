using TwentyOne.Models;

namespace TwentyOne
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Deck deck = new();
            deck.Shuffle();

            Card? card;
            for (int i = 0; i < 10; i++)
            {
                card = deck.DealCard();
                if (card != null)
                {
                    Console.WriteLine(card.ToString());
                }
            }
            
        }
    }
}