namespace TwentyOne.Models
{
    public class Player(string name, decimal startingBankroll)
    {
        public string Name { get; set; } = name;
        public decimal Bankroll { get; set; } = startingBankroll;
        public List<Hand> HandsInPlay { get; set; } = [];
    }
}