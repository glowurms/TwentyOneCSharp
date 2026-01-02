using TwentyOne.Constants;
using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne.Tests.Services;
public class GameServiceTest
{
    [Theory]
    [InlineData(-1, 500, 6)]
    [InlineData(0, 300, 4)]
    [InlineData(1, 200, 8)]
    [InlineData(2, 500, 6)]
    [InlineData(6, 300, 4)]
    [InlineData(7, 200, 8)]
    [InlineData(10, 200, 8)]
    public void StartNewGame_InitializesGameStateCorrectly(int requestedPlayerCount, int bankRollAmount, int shoeCount)
    {
        GameService gameService = new();
        GameState gameState = gameService.StartNewGame(requestedPlayerCount, bankRollAmount, shoeCount);

        int expectedPlayerCount = Math.Min(GameConstants.MaxPlayers, Math.Max(GameConstants.MinPlayers, requestedPlayerCount));

        Assert.Equal(shoeCount, gameState.Shoe.DeckCount);
        Assert.Equal(expectedPlayerCount, gameState.Players.Count);
        Assert.Equal(GamePhase.Betting, gameState.CurrentGamePhase);

        foreach(Player player in gameState.Players)
        {
            Assert.Equal(bankRollAmount, player.Bankroll);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(10)]
    public void DealInitialCards_WorksCorrectly(int requestedPlayerCount)
    {
        GameService gameService = new();
        GameState gameState = gameService.StartNewGame(requestedPlayerCount, 500, 6);

        gameService.DealInitialCards();

        int cardsInPlayCount = (gameState.Players.Count * 2) + 2; // 2 Cards to each player and the dealer

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
}
