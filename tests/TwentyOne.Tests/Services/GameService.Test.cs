using TwentyOne.Models;
using TwentyOne.Models.Enums;
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