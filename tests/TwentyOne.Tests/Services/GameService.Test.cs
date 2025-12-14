using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne.Tests.Services
{
    public class GameServiceTest
    {
        [Theory]
        [InlineData(500, 6, 1)]
        [InlineData(300, 4, 2)]
        [InlineData(200, 8, 6)]
        public void StartNewGame_InitializesGameStateCorrectly(int bankRollAmount, int shoeCount, int playerCount)
        {
            GameService gameService = new();
            GameState gameState = gameService.StartNewGame(bankRollAmount, shoeCount, playerCount);

            Assert.Equal(shoeCount, gameState.Shoe.DeckCount);
            Assert.Equal(playerCount, gameState.Players.Count);
            Assert.Equal(GamePhase.Betting, gameState.CurrentGamePhase);

            foreach(Player player in gameState.Players)
            {
                Assert.Equal(bankRollAmount, player.Bankroll);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(6)]
        public void DealInitialCards_WorksCorrectly(int playerCount)
        {
            GameService gameService = new();
            GameState gameState = gameService.StartNewGame(500, 6, playerCount);

            gameService.DealInitialCards();

            int cardsInPlayCount = (playerCount * 2) + 2; // 2 Cards to each player and the dealer

            Assert.Equal(2, gameState.DealerHand.TotalCardCount);

            foreach(Player player in gameState.Players)
            {
                foreach(Hand hand in player.HandsInPlay)
                {
                    Assert.Equal(2, hand.TotalCardCount);
                }
            }
            // Each player and the dealer has 2 cards, so total 4 cards dealt
            Assert.Equal(gameState.Shoe.TotalCardCount - cardsInPlayCount, gameState.Shoe.UndealtCardCount);
        }

        [Theory]
        // Bet should be only option in Betting phase. Cards shouldn't matter or even be dealt in scenario
        [InlineData(
            new Rank[] {},
            GamePhase.Betting, 
            new PlayerHandActions[] {
                PlayerHandActions.Bet
            })]
        // Excludes: Bet
        [InlineData(
            new Rank[] { Rank.Eight, Rank.Eight },
            GamePhase.PlayerTurns, 
            new PlayerHandActions[] {
                PlayerHandActions.Hit, PlayerHandActions.Stand, PlayerHandActions.DoubleDown, PlayerHandActions.Split
            })] 
        // Excludes: Split, Bet
        [InlineData(
            new Rank[] { Rank.Five, Rank.Ten },
            GamePhase.PlayerTurns, 
            new PlayerHandActions[] {
                PlayerHandActions.Hit, PlayerHandActions.Stand, PlayerHandActions.DoubleDown
            })]
        // Excludes: Split, DoubleDown, Bet
        [InlineData(
            new Rank[] { Rank.Two, Rank.Ten, Rank.Eight },
            GamePhase.PlayerTurns, 
            new PlayerHandActions[] {
                PlayerHandActions.Hit, PlayerHandActions.Stand
            })]
        // Hand is Natural. No actions available
        [InlineData(
            new Rank[] { Rank.Ace, Rank.Jack },
            GamePhase.PlayerTurns, 
            new PlayerHandActions[] {
            })]
        // Hand is bust. No actions available
        [InlineData(
            new Rank[] { Rank.Two, Rank.Ten, Rank.Eight, Rank.Nine },
            GamePhase.PlayerTurns, 
            new PlayerHandActions[] {
            })]
        public void AvailablePlayerActions_ReturnsCorrectActions(Rank[] ranks, GamePhase gamePhase, PlayerHandActions[] expectedActions)
        {
            // Split setup
            Hand hand = new();
            foreach (Rank rank in ranks)
            {
                hand.AddCard(new Card(rank, Suit.Hearts)); // Suit doesn't matter here
            }

            Player player = new("Player", 500);
            player.HandsInPlay.Add(hand);

            // Simulate game state player turn
            GameState gamestate = new();
            gamestate.Players.Add(player);
            gamestate.CurrentGamePhase = gamePhase;

            GameService gameService = new(ref gamestate);

            // Get actions for each case
            List<PlayerHandActions> actualactions = gameService.AvailablePlayerActions();


            Assert.Equal(actualactions.OrderDescending(), expectedActions.OrderDescending());
        }

        [Theory]
        // General card value scenarios
        [InlineData(new Rank[] { Rank.Two, Rank.Three }, 5)]
        [InlineData(new Rank[] { Rank.Two, Rank.Three, Rank.Four }, 9)]
        [InlineData(new Rank[] { Rank.Two, Rank.Three, Rank.Four, Rank.Five }, 14)]
        [InlineData(new Rank[] { Rank.Five, Rank.Six, Rank.Ten }, 21)]

        // Ace scenarios
        [InlineData(new Rank[] { Rank.Ace, Rank.Five }, 16)]
        [InlineData(new Rank[] { Rank.Ace, Rank.Ten }, 21)]
        [InlineData(new Rank[] { Rank.Ace, Rank.Jack }, 21)]
        [InlineData(new Rank[] { Rank.Ace, Rank.Queen }, 21)]
        [InlineData(new Rank[] { Rank.Ace, Rank.King }, 21)]

        // Mutiple Ace scenarios
        [InlineData(new Rank[] { Rank.Ace, Rank.Ace, Rank.Nine }, 21)]
        [InlineData(new Rank[] { Rank.Ace, Rank.Ace, Rank.Ace }, 13)]

        // 7 Ace scenario (unlikely but possible in testing)
        [InlineData(new Rank[] { 
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
            Rank.Ace, Rank.Ace }, 17)]

        // 11 Ace scenario (unlikely but possible in testing)
        [InlineData(new Rank[] { 
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
            Rank.Ace }, 21)]

        // 12 Ace scenario (unlikely but possible in testing)
        [InlineData(new Rank[] { 
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
            Rank.Ace, Rank.Ace }, 12)]

        public void HandValue_CalculatesCorrectly(Rank[] ranks, int expectedValue)
        {
            Hand hand = new();
            foreach (var rank in ranks)
            {
                hand.AddCard(new Card(rank, Suit.Hearts));
            }

            int value = GameService.HandValue(hand);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData(new Rank[] { Rank.Ten, Rank.King, Rank.Two }, true)]
        [InlineData(new Rank[] { Rank.Ten, Rank.King }, false)]
        public void HandIsBust_WorksCorrectly(Rank[] ranks, bool expectedIsBust)
        {
            Hand hand = new();
            foreach (var rank in ranks)
            {
                hand.AddCard(new Card(rank, Suit.Hearts));
            }

            bool isBust = GameService.HandIsBust(hand);

            Assert.Equal(expectedIsBust, isBust);
        }

        [Theory]
        [InlineData(new Rank[] { Rank.Ten, Rank.King, Rank.Ace }, true)]
        [InlineData(new Rank[] { Rank.Ten, Rank.King }, false)]
        // 11 Ace scenario (unlikely but possible in testing)
        [InlineData(new Rank[] { 
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
            Rank.Ace }, true)]
        public void HandIsTwentyOne_WorksCorrectly(Rank[] ranks, bool expectedIsTwentyOne)
        {
            Hand hand = new();
            foreach (var rank in ranks)
            {
                hand.AddCard(new Card(rank, Suit.Hearts));
            }

            bool isTwentyOne = GameService.HandIsTwentyOne(hand);

            Assert.Equal(expectedIsTwentyOne, isTwentyOne);
        }

        [Theory]
        [InlineData(new Rank[] { Rank.Ace, Rank.King }, true)]  // Natural 21
        [InlineData(new Rank[] { Rank.Ten, Rank.King }, false)] // Not 21
        [InlineData(new Rank[] { Rank.Ace, Rank.Nine, Rank.Ace }, false)] // 21 but not natural
        public void HandIsNatural_WorksCorrectly(Rank[] ranks, bool expectedIsNatural)
        {
            Hand hand = new();
            foreach (var rank in ranks)
            {
                hand.AddCard(new Card(rank, Suit.Hearts));
            }

            bool isNatural = GameService.HandIsNatural(hand);

            Assert.Equal(expectedIsNatural, isNatural);
        }

        // [Theory]
        // [InlineData(new Rank[] { Rank.Two, Rank.Three }, true)] // 5 < 17
        // [InlineData(new Rank[] { Rank.Ten, Rank.Seven }, false)] // 17 == 17
        // [InlineData(new Rank[] { Rank.King, Rank.Nine }, false)] // 19 > 17
        // public void DealerShouldDraw_WorksCorrectly(Rank[] ranks, bool expectedShouldDraw)
        // {
        //     Hand dealerHand = new();
        //     foreach (var rank in ranks)
        //     {
        //         dealerHand.AddCard(new Card(rank, Suit.Hearts));
        //     }

        //     GameState gameState = new()
        //     {
        //         Shoe = new Shoe(3),
        //         DealerHand = new Hand(),
        //         Players = [new Player("Player1", 100m)]
        //     };

        //     gameState.DealerHand = dealerHand;

        //     GameService gameService = new(ref gameState);

        //     bool shouldDraw = gameService.DealerShouldDraw();

        //     Assert.Equal(expectedShouldDraw, shouldDraw);
        // }
    }
}