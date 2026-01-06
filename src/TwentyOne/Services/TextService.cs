using TwentyOne.Constants;
using TwentyOne.Models;

namespace TwentyOne.Services;

public static class TextService
{
    public static readonly Dictionary<GamePhase, string> GamePhaseDescriptions = new()
    {
        { GamePhase.Betting, "Taking Bets" },
        { GamePhase.Dealing, "Dealing Cards" },
        { GamePhase.Naturals, "Handling player Naturals (21)" },
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
        if (gameState.CurrentPlayerIntent == PlayerActions.None)
        {
            return $"{currentPlayer.Name}'s turn to {string.Join(", ", gameState.CurrentPlayerOptions)}";
        }
        else
        {
            return $"{currentPlayer.Name} {PlayerActionTakenText[gameState.CurrentPlayerIntent]}";
        }
    }

    public static string CardText(Card card, bool shortHand = false)
    {
        if (shortHand)
        {
            return $"{CardConstants.RankAbbreviations[card.Rank]}{CardConstants.SuitAbbreviations[card.Suit]}";
        }
        return $"{card.Rank} of {card.Suit}";
    }

    public static List<string> CardTexts(Hand hand, bool shortHand = false)
    {
        List<string> CardTexts = [];
        foreach(Card card in hand.Cards)
        {
            CardTexts.Add(TextService.CardText(card, shortHand));
        }
        return CardTexts;
    }

    public static string CardsTextInline(Hand hand, bool shortHand = false)
    {
        return string.Join(", ", CardTexts(hand, shortHand));
    }

    public static string GameStateSummary(GameState gameState)
    {

        string rule = "========================== ♣ ♦ ♥ ♠ ==========================";
        string thinRule = "-------------------------- ♣ ♦ ♥ ♠ --------------------------";
        List<string> gameInfo = [];
        gameInfo.Add(rule);

        // Basic game state info
        gameInfo.Add($"GamePhase: {gameState.GamePhase}, Players: {gameState.Players.Count}, CardsLeft: {gameState.Shoe.UndealtCardCount}");

        // Dealer info
        gameInfo.Add(rule);
        gameInfo.Add($"DEALER (TableWinnings: ${gameState.TableWinnings})");
        List<string> dealerHandInfo = [];
        if (gameState.DealerHand.Cards.Count > 0)
        {
            dealerHandInfo.Add($"Hand ({RulesService.HandValue(gameState.DealerHand)}) Cards: {CardsTextInline(gameState.DealerHand, true)}");
        }
        if(dealerHandInfo.Count > 0)
        {
            gameInfo.Add(string.Join(" ", dealerHandInfo));
        }
        gameInfo.Add(rule);

        // Player info
        foreach (Player player in gameState.Players)
        {
            gameInfo.Add($"{player.Name} (Bankroll: ${player.Bankroll})");

            // Hands info
            foreach (Hand currentHand in player.Hands)
            {
                List<string> currentHandInfo = [];
                Bet? betForCurrentHand = gameState.ActiveBets.Find(bet => bet.Hand == currentHand);
                if(betForCurrentHand != null)
                {
                    currentHandInfo.Add($"Bet ${betForCurrentHand.Amount}");
                }
                if(currentHand.Cards.Count > 0)
                {
                    currentHandInfo.Add($"Hand ({RulesService.HandValue(currentHand)}) Cards: {CardsTextInline(currentHand, true)}");
                }
                if(currentHandInfo.Count > 0)
                {
                    gameInfo.Add(string.Join(" ", currentHandInfo));
                }
            }
            gameInfo.Add(thinRule);
        }

        gameInfo.Add(rule);
        gameInfo.Add(GamePhaseDescriptions[gameState.GamePhase]);

        switch (gameState.GamePhase)
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