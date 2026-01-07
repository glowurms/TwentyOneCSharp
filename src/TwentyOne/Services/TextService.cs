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

    public static readonly string HorizontalRule =     "================================== ♣ ♦ ♥ ♠ ==================================";
    public static readonly string HorizontalRuleThin = "-----------------------------------------------------------------------------";

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
            if(card.FaceUp == false)
            {
                return $"XX";
            }
            return $"{CardConstants.RankAbbreviations[card.Rank]}{CardConstants.SuitAbbreviations[card.Suit]}";
        }
        if(card.FaceUp == false)
        {
            return $"Face down";
        }
        return $"{card.Rank} of {card.Suit}";
    }

    public static List<string> CardsTexts(Hand hand, bool shortHand = false)
    {
        List<string> CardTexts = [];
        foreach(Card card in hand.Cards)
        {
            CardTexts.Add(CardText(card, shortHand));
        }
        return CardTexts;
    }

    public static string CardsTextInline(Hand hand, bool shortHand = false)
    {
        return string.Join(", ", CardsTexts(hand, shortHand));
    }

    private static string HandInfoText(GameState gameState, Hand hand)
    {
        List<string> handInfo = [];
        Bet? betForHand = gameState.ActiveBets.Find(bet => bet.Hand == hand);
        if (betForHand != null)
        {
            if (betForHand.Type == BetType.DoubleDown)
            {
                handInfo.Add($"Double down ${betForHand.Amount}");
            }
            else if (betForHand.Type == BetType.DoubleDown)
            {
                handInfo.Add($"Split ${betForHand.Amount}");
            }
            else
            {
                handInfo.Add($"Bet ${betForHand.Amount}");
            }
        }
        if (hand.Cards.Count > 0)
        {
            handInfo.Add($"Hand ({RulesService.HandValue(hand)}) Cards: {CardsTextInline(hand, true)}");
        }
        return string.Join(" -- ", handInfo);
    }

    private static string PlayerInfoText(GameState gameState, Player player)
    {
        List<string> playerInfo = [];
        playerInfo.Add($"${player.Bankroll} -- {player.Name}");

        List<string> HandInfos = [];
        // Hands info
        foreach (Hand currentHand in player.Hands)
        {
            string currentHandInfo = HandInfoText(gameState, currentHand);
            if (currentHandInfo.Length > 0)
            {
                HandInfos.Add(currentHandInfo);
            }
        }
        if (HandInfos.Count > 0)
        {
            playerInfo.Add(string.Join("\n", HandInfos));
        }
        return string.Join("\n", playerInfo);
    }

    private static string PlayersInfoText(GameState gameState)
    {
        List<string> playerInfoTexts = [];
        foreach (Player player in gameState.Players)
        {
            string playerInfo = PlayerInfoText(gameState, player);
            if(playerInfo.Length > 0)
            {
                playerInfoTexts.Add(playerInfo);
            }
        }
        return string.Join("\n" + HorizontalRuleThin + "\n", playerInfoTexts);
    }

    public static string GameStateSummary(GameState gameState)
    {
        List<string> gameInfo = [];

        // ==========================
        gameInfo.Add(HorizontalRule); 
        // ==========================

        // Basic game state info block
        List<string> basicStateInfo = [];
        basicStateInfo.Add($"GamePhase: {gameState.GamePhase}");
        basicStateInfo.Add($"ShoeCards: {gameState.Shoe.UndealtCardCount}");
        basicStateInfo.Add($"CutCard: {gameState.Shoe.CutCardPosition}");
        basicStateInfo.Add($"Players: {gameState.Players.Count}");
        basicStateInfo.Add($"TableTake: {gameState.TableWinnings}");
        gameInfo.Add(string.Join(", ", basicStateInfo));

        // ==========================
        gameInfo.Add(HorizontalRule); 
        // ==========================

        // Dealer info block
        gameInfo.Add($"DEALER");
        List<string> dealerHandInfo = [];
        if (gameState.DealerHand.Cards.Count > 0)
        {
            dealerHandInfo.Add($"Hand ({RulesService.HandValue(gameState.DealerHand)}) Cards: {CardsTextInline(gameState.DealerHand, true)}");
        }
        if (dealerHandInfo.Count > 0)
        {
            gameInfo.Add(string.Join(" ", dealerHandInfo));
        }

        // ------------------------------
        gameInfo.Add(HorizontalRuleThin);
        // ------------------------------

        // Players info block
        string playerInfoText = PlayersInfoText(gameState);
        if (playerInfoText.Length > 0)
        {
            gameInfo.Add(playerInfoText);
        }

        // ==========================
        gameInfo.Add(HorizontalRule); 
        // ==========================

        // Game Phase info block
        gameInfo.Add(GamePhaseDescriptions[gameState.GamePhase]);

        // Previous step info block
        List<string> previousStepInfo = [];
        switch (gameState.GamePhase)
        {
            case GamePhase.Betting:
                if (gameState.LastPlayerIntent == PlayerActions.Bet && gameState.LastHand != null)
                {
                    List<string> handInfo = [];
                    Bet? betForHand = gameState.ActiveBets.Find(bet => bet.Hand == gameState.LastHand);
                    if (betForHand != null)
                    {
                        previousStepInfo.Add($"{betForHand.Player.Name} bets ${betForHand.Amount}");
                    }
                }
                break;
            case GamePhase.Dealing:
                if (gameState.LastDrawnCard != null)
                {
                    if(gameState.DealerHand.Cards.Count > 0 && gameState.LastDrawnCard == gameState.DealerHand.Cards.Last())
                    {
                        previousStepInfo.Add($"{CardText(gameState.LastDrawnCard)} dealt to dealer");
                    }
                    else if(gameState.LastPlayer != null)
                    {
                        previousStepInfo.Add($"{CardText(gameState.LastDrawnCard)} dealt to {gameState.LastPlayer.Name}");
                    }
                }
                break;
            case GamePhase.PlayerTurns:
                if (gameState.LastPlayerIntent != null && gameState.LastHand != null)
                {
                    List<string> playerTurnInfo = [];
                    Bet? betForHand = gameState.ActiveBets.Find(bet => bet.Hand == gameState.LastHand);
                    if (betForHand != null)
                    {
                        playerTurnInfo.Add($"{betForHand.Player.Name}");
                        playerTurnInfo.Add($"{PlayerActionTakenText[(PlayerActions)gameState.LastPlayerIntent]}");
                        playerTurnInfo.Add($"${betForHand.Amount}");
                        previousStepInfo.Add(string.Join(" ", playerTurnInfo));
                    }
                }
                break;
            case GamePhase.DealerTurn:
                if (gameState.LastDealerAction != null)
                {
                    previousStepInfo.Add(DealerActionTakenText[(DealerActions)gameState.LastDealerAction]);
                }
                break;
            case GamePhase.RoundEnd:
                if (gameState.LastResolvedBet != null)
                {
                    previousStepInfo.Add($"{gameState.LastResolvedBet.Player.Name} {gameState.LastResolvedBet.Resolution}");
                }
                break;
        }
        if(previousStepInfo.Count > 0)
        {
            // ------------------------------
            gameInfo.Add(HorizontalRuleThin);
            // ------------------------------

            gameInfo.Add(string.Join("\n" + HorizontalRuleThin + "\n", previousStepInfo));
        }

        // Current step info block
        List<string> currentStepInfo = [];
        switch (gameState.GamePhase)
        {
            case GamePhase.Betting:
                currentStepInfo.Add(PlayerActionSummary(gameState));
                break;
            case GamePhase.Dealing:
                break;
            case GamePhase.PlayerTurns:
                currentStepInfo.Add(PlayerActionSummary(gameState));
                break;
            case GamePhase.DealerTurn:
                if (gameState.LastDealerAction != null)
                {
                    currentStepInfo.Add(DealerActionTakenText[(DealerActions)gameState.LastDealerAction]);
                }
                break;
            case GamePhase.RoundEnd:
                break;
        }
        if(currentStepInfo.Count > 0)
        {
            // ------------------------------
            gameInfo.Add(HorizontalRuleThin);
            // ------------------------------

            gameInfo.Add(string.Join("\n" + HorizontalRuleThin + "\n", currentStepInfo));
        }

        // ==========================
        gameInfo.Add(HorizontalRule); 
        // ==========================

        if(gameState.LastDealerAction != null){
            gameInfo.Add($"LastDealerAction: {gameState.LastDealerAction}");
        }
        if(gameState.LastPlayer != null){
            gameInfo.Add($"LastPlayer: {gameState.LastPlayer}");
        }
        if(gameState.LastPlayerIntent != null){
            gameInfo.Add($"LastPlayerIntent: {gameState.LastPlayerIntent}");
        }
        if(gameState.LastHand != null){
            gameInfo.Add($"LastHand: {gameState.LastHand}");
        }
        if(gameState.LastDrawnCard != null){
            gameInfo.Add($"LastDrawnCard: {gameState.LastDrawnCard}");
        }
        if(gameState.LastResolvedBet != null){
            gameInfo.Add($"LastResolvedBet: {gameState.LastResolvedBet}");
        }

        return string.Join("\n", gameInfo);
    }
}