using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne.Tests.Services
{
    public class GameServiceTest
    {
        [Fact]
        public void DealInitialCards_WorksCorrectly()
        {
            GameState gameState = new()
            {
                Shoe = new Shoe(4),
                DealerHand = new Hand(),
                Players = [new Player("Player1", 100m)]
            };

            GameService gameService = new(ref gameState);

            gameService.DealInitialCards();

            Assert.Equal(2, gameState.DealerHand.TotalCardCount);
            Assert.Equal(2, gameState.Players[0].HandsInPlay[0].TotalCardCount);
            Assert.Equal(gameState.Shoe.TotalCardCount - 4, gameState.Shoe.UndealtCardCount);
        }

        [Fact]
        public void StartNewGame_InitializesGameStateCorrectly()
        {
            GameService gameService = new();
            GameState gameStateOnePlayer = gameService.StartNewGame(500, 6, 1);
            GameState gameStateTwoPlayers = gameService.StartNewGame(500, 6, 2);

            Assert.Single(gameStateOnePlayer.Players);
            Assert.Equal(500, gameStateOnePlayer.Players[0].Bankroll);
            Assert.Equal(2, gameStateOnePlayer.DealerHand.TotalCardCount);
            Assert.Equal(2, gameStateOnePlayer.Players[0].HandsInPlay[0].TotalCardCount);
            // 2 cards each to dealer and 1 player = 4 cards dealt
            Assert.Equal(gameStateOnePlayer.Shoe.TotalCardCount - 4, gameStateOnePlayer.Shoe.UndealtCardCount);

            Assert.Equal(2, gameStateTwoPlayers.Players.Count);
            Assert.Equal(500, gameStateTwoPlayers.Players[0].Bankroll);
            Assert.Equal(500, gameStateTwoPlayers.Players[1].Bankroll);
            Assert.Equal(2, gameStateTwoPlayers.DealerHand.TotalCardCount);
            Assert.Equal(2, gameStateTwoPlayers.Players[0].HandsInPlay[0].TotalCardCount);
            Assert.Equal(2, gameStateTwoPlayers.Players[1].HandsInPlay[0].TotalCardCount);
            // 2 cards each to dealer and 2 players = 6 cards dealt
            Assert.Equal(gameStateTwoPlayers.Shoe.TotalCardCount - 6, gameStateTwoPlayers.Shoe.UndealtCardCount);
        }

        [Fact]
        public void AvailablePlayerActions_ReturnsCorrectActions()
        {
            Hand hand = new();
            hand.AddCard(new Card(Rank.Eight, Suit.Hearts));
            hand.AddCard(new Card(Rank.Eight, Suit.Spades));

            Hand handNoSplit = new();
            handNoSplit.AddCard(new Card(Rank.Eight, Suit.Hearts));
            handNoSplit.AddCard(new Card(Rank.Five, Suit.Spades));

            var actions = GameService.AvailablePlayerActions(hand);
            var actionsNoSplit = GameService.AvailablePlayerActions(handNoSplit);

            Assert.Contains(PlayerHandActions.Hit, actions);
            Assert.Contains(PlayerHandActions.Stand, actions);
            Assert.Contains(PlayerHandActions.DoubleDown, actions);
            Assert.Contains(PlayerHandActions.Split, actions);

            Assert.Contains(PlayerHandActions.Hit, actionsNoSplit);
            Assert.Contains(PlayerHandActions.Stand, actionsNoSplit);
            Assert.Contains(PlayerHandActions.DoubleDown, actionsNoSplit);
            Assert.DoesNotContain(PlayerHandActions.Split, actionsNoSplit);
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

        [Theory]
        [InlineData(new Rank[] { Rank.Two, Rank.Three }, true)] // 5 < 17
        [InlineData(new Rank[] { Rank.Ten, Rank.Seven }, false)] // 17 == 17
        [InlineData(new Rank[] { Rank.King, Rank.Nine }, false)] // 19 > 17
        public void DealerShouldDraw_WorksCorrectly(Rank[] ranks, bool expectedShouldDraw)
        {
            Hand dealerHand = new();
            foreach (var rank in ranks)
            {
                dealerHand.AddCard(new Card(rank, Suit.Hearts));
            }

            GameState gameState = new()
            {
                Shoe = new Shoe(3),
                DealerHand = new Hand(),
                Players = [new Player("Player1", 100m)]
            };

            gameState.DealerHand = dealerHand;

            GameService gameService = new(ref gameState);

            bool shouldDraw = gameService.DealerShouldDraw();

            Assert.Equal(expectedShouldDraw, shouldDraw);
        }
    }
}