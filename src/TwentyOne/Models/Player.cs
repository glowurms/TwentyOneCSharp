namespace TwentyOne.Models
{
    public class Player(string name, decimal startingBankroll)
    {
        public string Name { get; set; } = name;
        public decimal Bankroll { get; set; } = startingBankroll;
        public List<Hand> HandsInPlay { get; set; } = [];
        public decimal Bet { get; set; } = 0m;
        public bool SittingOut { get; set; } = false;
        public PlayerActions SelectedAction { get; set; } = PlayerActions.None;

        public string PlayerInfo()
        {
            return $"Player: Name[{Name}], Bankroll[{Bankroll}], HandsInPlay[{HandsInPlay.Count}]";
        }
    }
}