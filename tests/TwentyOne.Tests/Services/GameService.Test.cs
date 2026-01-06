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
        Assert.Equal(GamePhase.Betting, gameState.GamePhase);

        foreach(Player player in gameState.Players)
        {
            Assert.Equal(bankRollAmount, player.Bankroll);
        }
    }
}
