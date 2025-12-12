namespace TwentyOne.Models
{
    public class GameState ()
    {
        public Shoe Shoe { get; set; } = new();
        public Hand DealerHand { get; set; } = new();
        public List<Player> Players { get; set; } = [];
        public List<PlayerHandActions> AvailableActions { get; set; } = [];
        public string StatusMessage { get; set; } = "Status Message Here";
        public GamePhase CurrentGamePhase { get; set; } = GamePhase.Betting;
        public int CurrentPlayerIndex { get; set; } = 0;
        public int CurrentHandIndex { get; set; } = 0;
    }
}