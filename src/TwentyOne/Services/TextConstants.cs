using TwentyOne.Constants;
using TwentyOne.Models;

namespace TwentyOne.Services;

public static class TextService
{
    public static readonly Dictionary<GamePhase, string> GamePhaseDescriptions = new()
    {
        { GamePhase.Betting, "Taking Bets" },
        { GamePhase.Dealing, "Dealing Cards" },
        { GamePhase.PlayerTurns, "Player Turn" },
        { GamePhase.DealerTurn, "Dealer Turn" },
        { GamePhase.RoundEnd, "Round End" }
    };
  
    public static readonly Dictionary<PlayerActions, string> ActionDescriptions = new()
    {
        { PlayerActions.Bet, "Place a bet" },
        { PlayerActions.Hit, "Take another card" },
        { PlayerActions.Stand, "Keep your current hand" },
        { PlayerActions.DoubleDown, "Double your bet and take one more card" },
        { PlayerActions.Split, "Split your hand into two hands" }
    };

    public static readonly Dictionary<PlayerActions, string> PlayerActionTakenText = new()
    {
        { PlayerActions.Bet, "places a bet" },
        { PlayerActions.Hit, "hits and gets another card" },
        { PlayerActions.Stand, "stands" },
        { PlayerActions.DoubleDown, "doubles down and gets one more card" },
        { PlayerActions.Split, "splits" },
        { PlayerActions.None, "has no action to take" }
    };

    public static readonly Dictionary<DealerActions, string> DealerActionTakenText = new()
    {
        { DealerActions.ShowFaceDown, "Dealer reveals down card" },
        { DealerActions.Draw, "Dealer draws a card" },
        { DealerActions.Stand, "Dealer stands" },
        { DealerActions.None, "Dealer has no actions to take" }
    };

    public static string PlayerActionSummary(GameState gameState)
    {
        Player currentPlayer = gameState.Players[gameState.CurrentPlayerIndex];
        if (currentPlayer.SelectedAction == PlayerActions.None)
        {
            return $"{currentPlayer.Name}'s turn to {string.Join(", ", gameState.CurrentPlayerOptions)}";
        }
        else
        {
            return $"{currentPlayer.Name} {PlayerActionTakenText[currentPlayer.SelectedAction]}";
        }
    }

    public static string GameStateSummary(GameState gameState)
    {
        string rule = "====================================================";
        string thinRule = "----------------------------------------------------";
        List<string> gameInfo = [];
        gameInfo.Add(rule);

        // Basic game state info
        gameInfo.Add($"GamePhase: {gameState.CurrentGamePhase}, Players: {gameState.Players.Count}, CardsLeft: {gameState.Shoe.UndealtCardCount}");

        // Dealer info
        gameInfo.Add(thinRule);
        gameInfo.Add("Dealer:");
        gameInfo.Add($"Value: {RulesService.HandValue(gameState.DealerHand)}; {gameState.DealerHand.HandInfo()}");

        // Player info
        foreach (Player player in gameState.Players)
        {
            gameInfo.Add(thinRule);
            gameInfo.Add(player.PlayerInfo());

            // Hands info
            foreach (Hand hand in player.HandsInPlay)
            {
                gameInfo.Add($"Value: {RulesService.HandValue(hand)}; {hand.HandInfo()}");
            }
        }

        gameInfo.Add(thinRule);
        gameInfo.Add(GamePhaseDescriptions[gameState.CurrentGamePhase]);

        switch (gameState.CurrentGamePhase)
        {
            case GamePhase.Betting:
                gameInfo.Add(thinRule);
                gameInfo.Add(PlayerActionSummary(gameState));
                break;
            case GamePhase.Dealing:
                break;
            case GamePhase.PlayerTurns:
                gameInfo.Add(thinRule);
                gameInfo.Add(PlayerActionSummary(gameState));
                break;
            case GamePhase.DealerTurn:
                gameInfo.Add(thinRule);
                gameInfo.Add(DealerActionTakenText[gameState.DealerAction]);
                break;
            case GamePhase.RoundEnd:
                gameInfo.Add(thinRule);
                break;
        }

        gameInfo.Add(rule);
        return string.Join("\n", gameInfo);
    }
}