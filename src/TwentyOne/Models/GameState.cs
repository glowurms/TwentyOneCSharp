namespace TwentyOne.Models
{
    public class GameState ()
    {
        public Shoe Shoe { get; set; } = new();
        public Hand DealerHand { get; set; } = new();
        public List<Player> Players { get; set; } = [];
        public List<PlayerHandActions> AvailableActions { get; set; } = [];
        public string StatusMessage { get; set; } = "Status Message Here";
        public string InfoMessage { get; set; } = "Info Message Here";
        public GamePhase CurrentGamePhase { get; set; } = GamePhase.Betting;
        public int CurrentPlayerIndex { get; set; } = 0;
        public int CurrentHandIndex { get; set; } = 0;

        public string GameStateInfo {
            get
            {
                string rule = "====================================================";
                string thinRule = "----------------------------------------------------";
                List<string> gameInfo = [];
                gameInfo.Add(rule);

                // Basic game state info
                gameInfo.Add($"GamePhase: {CurrentGamePhase}, Players: {Players.Count}, CardsLeft: {Shoe.UndealtCardCount}");

                // Dealer info
                gameInfo.Add(thinRule);
                gameInfo.Add("Dealer:");
                gameInfo.Add(DealerHand.HandInfo());

                // Player info
                foreach (Player player in Players)
                {
                    gameInfo.Add(thinRule);
                    gameInfo.Add(player.PlayerInfo());

                    // Hands info
                    foreach (Hand hand in player.HandsInPlay)
                    {
                        gameInfo.Add(hand.HandInfo());
                    }
                }

                gameInfo.Add(thinRule);
                gameInfo.Add(StatusMessage);
                gameInfo.Add(thinRule);
                gameInfo.Add(InfoMessage);

                gameInfo.Add(rule);
                return string.Join("\n", gameInfo);
            }
        }
    }
}