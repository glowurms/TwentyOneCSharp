namespace TwentyOne.Models
{
    public class Player(string name, decimal startingBankroll)
    {
        public string Name { get; set; } = name;
        public decimal Bankroll { get; set; } = startingBankroll;
        public List<Hand> HandsInPlay { get; set; } = [];
        public bool SittingOut { get; set; } = false;

        public string PlayerInfo()
        {
            return $"Player: Name[{Name}], Bankroll[{Bankroll}], HandsInPlay[{HandsInPlay.Count}]";
        }
    }
}