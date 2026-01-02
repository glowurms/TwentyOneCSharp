namespace TwentyOne.Models
{
    public class GameState ()
    {
        public Shoe Shoe { get; set; } = new();
        public Hand DealerHand { get; set; } = new();
        public DealerActions DealerAction { get; set; } = DealerActions.None;
        public List<Player> Players { get; set; } = [];
        public GamePhase CurrentGamePhase { get; set; } = GamePhase.Betting;
        public int CurrentPlayerIndex { get; set; } = 0;
        public int CurrentHandIndex { get; set; } = 0;
        public List<PlayerActions> CurrentPlayerOptions { get; set; } = [PlayerActions.None];
    }
}