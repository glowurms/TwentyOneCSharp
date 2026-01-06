namespace TwentyOne.Models
{
    public class GameState ()
    {
        public Shoe Shoe { get; set; } = new();
        public GamePhase GamePhase { get; set; } = GamePhase.Betting;
        public decimal TableWinnings = 0m; // Unimportant, but interesting stat

        public Hand DealerHand { get; set; } = new();
        public DealerActions DealerAction { get; set; } = DealerActions.None;

        public List<Player> Players { get; set; } = [];
        public int CurrentPlayerIndex { get; set; } = 0;
        public int CurrentHandIndex { get; set; } = 0;

        public PlayerActions CurrentPlayerIntent { get; set; } = PlayerActions.None;
        public List<PlayerActions> CurrentPlayerOptions { get; set; } = [PlayerActions.None];

        public List<Bet> ActiveBets = [];

        public DealerActions? LastDealerAction = null;
        public Player? LastPlayer = null;
        public PlayerActions? LastPlayerIntent = null;
        public Hand? LastHand = null;
        public Card? LastDrawnCard = null;
        public Bet? LastResolvedBet = null;
    }
}