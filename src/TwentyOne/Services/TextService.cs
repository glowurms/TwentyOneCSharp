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
    public static readonly string ContinueOptionText = $"[{Keybinds.GameActionToKey[GameActions.Continue]}] Continue";

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

    public static string HandTotalAndCardsText(Hand hand, bool shortHand = false)
    {
        return $"Total ({RulesService.HandValue(hand)}) Cards: {CardsTextInline(hand, shortHand)}";
    }

    public static string PlayerActionText(PlayerActions action, bool showKeybinds = false)
    {
        if (showKeybinds)
        {
            return $"[{Keybinds.ToPlayerActionToKey[action]}] {action}";
        }
        return action.ToString();
        
    }

    public static List<string> PlayerOptionsTexts(GameState gameState, bool showKeybinds = false)
    {
        List<string> playerOptions = [];
        foreach(PlayerActions action in gameState.CurrentPlayerOptions)
        {
            playerOptions.Add(PlayerActionText(action, showKeybinds));
        }
        return playerOptions;
    }

    public static string PlayerOptionsInline(GameState gameState, bool showKeybinds = false)
    {
        return string.Join(" ", PlayerOptionsTexts(gameState, showKeybinds));
    }
 
    public static string PlayerActionDescription(GameState gameState)
    {
        Player currentPlayer = gameState.Players[gameState.CurrentPlayerIndex];
        if (gameState.CurrentPlayerIntent == PlayerActions.None)
        {
            return $"{currentPlayer.Name}'s turn";
        }
        return $"{currentPlayer.Name} {PlayerActionTakenText[gameState.CurrentPlayerIntent]}";
    }

    public static string PlayerOptionsDescription(GameState gameState)
    {
        if (gameState.CurrentPlayerIntent == PlayerActions.None)
        {
            return $"{PlayerOptionsInline(gameState, true)}";
        }
        return ContinueOptionText;
    }

    public static string GameStateSummary(GameState gameState)
    {
        List<string> gameInfo = [];
        gameInfo.Add(HorizontalRule); 

        List<string> gameInfoBlocks = [];
        gameInfoBlocks.Add(DealerInfoBlock(gameState));
        gameInfoBlocks.Add(PlayersInfoBlock(gameState));
        gameInfoBlocks.Add(PreviousGamePhaseInfoBlock(gameState));
        gameInfoBlocks.Add(GamePhaseInfoBlock(gameState));

        gameInfo.Add(string.Join($"\n{HorizontalRule}\n",gameInfoBlocks));
        gameInfo.Add(HorizontalRule); 

        // TODO: debug toggle
        gameInfo.Add("\n\n");
        gameInfo.Add(DebugGameStateBlock(gameState));

        return string.Join($"\n", gameInfo);
    }

    private static string HandInfoText(GameState gameState, Hand hand)
    {
        List<string> handInfo = [];
        Bet? betForHand = gameState.ActiveBets.Find(bet => bet.Hand == hand);
        if (betForHand is not null)
        {
            if (betForHand.Type == BetType.DoubleDown)
            {
                handInfo.Add($"${betForHand.Amount} Double down");
            }
            else if (betForHand.Type == BetType.DoubleDown)
            {
                handInfo.Add($"${betForHand.Amount} Split");
            }
            else
            {
                handInfo.Add($"${betForHand.Amount}");
            }
        }
        if (hand.Cards.Count > 0)
        {
            handInfo.Add(HandTotalAndCardsText(hand, true));
        }
        return string.Join(" | ", handInfo);
    }

    private static string DealerInfoBlock(GameState gameState)
    {
        List<string> dealerInfo = [];
        List<string> dealerHeader = [];
        dealerHeader.Add($"Shoe Cards: {gameState.Shoe.UndealtCardCount}");
        dealerHeader.Add($"Cut Card: {gameState.Shoe.CutCardPosition}");
        dealerHeader.Add($"Cut Card Reached: {(gameState.Shoe.CutCardReached ? "Yes" : "No ")}");
        dealerHeader.Add($"TableTake: {gameState.TableWinnings}");

        dealerInfo.Add(string.Join(" | ", dealerHeader));
        dealerInfo.Add(HorizontalRuleThin);
        dealerInfo.Add("Dealer");

        if (gameState.DealerHand.Cards.Count > 0)
        {
            dealerInfo.Add(HandTotalAndCardsText(gameState.DealerHand, true));
        }
        else
        {
            dealerInfo.Add(string.Empty);
        }
        return string.Join("\n", dealerInfo);
    }

    private static string PlayerInfo(GameState gameState, Player player)
    {
        List<string> playerInfo = [];
        List<string> playerHeader = [];
        playerHeader.Add($"${player.Bankroll}");
        playerHeader.Add(player.Name);

        playerInfo.Add(string.Join(" | ", playerHeader));

        bool playerHasOptions = gameState.CurrentPlayerOptions.Count > 0;
        bool isCurrentPlayer = playerHasOptions && player == gameState.Players[gameState.CurrentPlayerIndex];
        List<string> handInfos = [];
        foreach (Hand currentHand in player.Hands)
        {
            bool isCurrentHand = isCurrentPlayer && currentHand == player.Hands[gameState.CurrentHandIndex];
            string prefix = (isCurrentHand) ? "-> " : string.Empty;
            string currentHandInfo = prefix + HandInfoText(gameState, currentHand);
            if (currentHandInfo.Length > 0)
            {
                handInfos.Add(currentHandInfo);
            }
        }
        playerInfo.Add(string.Join("\n", handInfos));
        return string.Join("\n", playerInfo);
    }

    private static string PlayersInfoBlock(GameState gameState)
    {
        List<string> playersInfo = [];
        foreach (Player player in gameState.Players)
        {
            playersInfo.Add(PlayerInfo(gameState, player));
        }
        return string.Join("\n" + HorizontalRuleThin + "\n", playersInfo);
    }

    private static string GamePhaseInfoBlock(GameState gameState)
    {
        List<string> phaseInfoBlock = [];
        // Format 4 lines:
        // Game Phase title
        // HorizontalRuleThin
        // Game phase step description
        // Game phase advance step options with keybind

        phaseInfoBlock.Add($"{GamePhaseDescriptions[gameState.GamePhase]}");
        phaseInfoBlock.Add(HorizontalRuleThin);

        if(gameState.GamePhaseStage == GamePhaseStage.End)
        {
            phaseInfoBlock.Add($"{GamePhaseDescriptions[gameState.GamePhase]} phase complete");
            phaseInfoBlock.Add(ContinueOptionText);
        }
        else
        {
            switch (gameState.GamePhase)
            {
                case GamePhase.Betting:
                    phaseInfoBlock.Add(PlayerActionDescription(gameState));
                    phaseInfoBlock.Add(PlayerOptionsDescription(gameState));
                    break;
                case GamePhase.Dealing:
                    phaseInfoBlock.Add(string.Empty);
                    phaseInfoBlock.Add(ContinueOptionText);
                    break;
                case GamePhase.PlayerTurns:
                    phaseInfoBlock.Add(PlayerActionDescription(gameState));
                    phaseInfoBlock.Add(PlayerOptionsDescription(gameState));
                    break;
                case GamePhase.DealerTurn:
                    phaseInfoBlock.Add(string.Empty);
                    phaseInfoBlock.Add(ContinueOptionText);
                    break;
                case GamePhase.RoundEnd:
                    phaseInfoBlock.Add(string.Empty);
                    phaseInfoBlock.Add(ContinueOptionText);
                    break;
                default:
                    phaseInfoBlock.Add(string.Empty);
                    phaseInfoBlock.Add(ContinueOptionText);
                    break;
            }
        }

        return string.Join("\n", phaseInfoBlock);
    }

    private static string PreviousGamePhaseInfoBlock(GameState gameState)
    {
        List<string> previousPhaseInfo = [];
        switch (gameState.GamePhase)
        {
            case GamePhase.Betting:
                if (gameState.LastPlayerIntent == PlayerActions.Bet && gameState.LastHand is not null)
                {
                    List<string> handInfo = [];
                    Bet? betForHand = gameState.ActiveBets.Find(bet => bet.Hand == gameState.LastHand);
                    if (betForHand is not null)
                    {
                        previousPhaseInfo.Add($"{betForHand.Player.Name} bets ${betForHand.Amount}");
                    }
                }
                break;
            case GamePhase.Dealing:
                if (gameState.LastDrawnCard is not null)
                {
                    if(gameState.DealerHand.Cards.Count > 0 && gameState.LastDrawnCard == gameState.DealerHand.Cards.Last())
                    {
                        previousPhaseInfo.Add($"{CardText(gameState.LastDrawnCard)} dealt to dealer");
                    }
                    else if(gameState.LastPlayer is not null)
                    {
                        previousPhaseInfo.Add($"{CardText(gameState.LastDrawnCard)} dealt to {gameState.LastPlayer.Name}");
                    }
                }
                break;
            case GamePhase.PlayerTurns:
                if (gameState.LastPlayerIntent is not null && gameState.LastHand is not null)
                {
                    List<string> playerTurnInfo = [];
                    Bet? betForHand = gameState.ActiveBets.Find(bet => bet.Hand == gameState.LastHand);
                    if (betForHand is not null)
                    {
                        playerTurnInfo.Add($"{betForHand.Player.Name}");
                        playerTurnInfo.Add($"{PlayerActionTakenText[(PlayerActions)gameState.LastPlayerIntent]}");
                        playerTurnInfo.Add($"${betForHand.Amount}");
                        previousPhaseInfo.Add(string.Join(" ", playerTurnInfo));
                    }
                }
                break;
            case GamePhase.DealerTurn:
                if (gameState.LastDealerAction is not null)
                {
                    previousPhaseInfo.Add(DealerActionTakenText[(DealerActions)gameState.LastDealerAction]);
                }
                break;
            case GamePhase.RoundEnd:
                if (gameState.LastResolvedBet is not null)
                {
                    previousPhaseInfo.Add($"{gameState.LastResolvedBet.Player.Name} {gameState.LastResolvedBet.Resolution}");
                }
                break;
            default:
                previousPhaseInfo.Add("");
                break;
        }
        return string.Join("\n", previousPhaseInfo);
    }


    private static string DebugGameStateBlock(GameState gameState)
    {
        List<string> gameStateProperties = [];
        gameStateProperties.Add($"GamePhase: {gameState.GamePhase}; GamePhaseStage: {gameState.GamePhaseStage}");

        gameStateProperties.Add("== Current things ==");
        gameStateProperties.Add($"HandIndex: {gameState.CurrentHandIndex}; PlayerIndex: {gameState.CurrentPlayerIndex}");
        gameStateProperties.Add($"PlayerIntent: {gameState.CurrentPlayerIntent}");
        gameStateProperties.Add($"PlayerOptions: [{string.Join(", ", PlayerOptionsTexts(gameState))}]");
        gameStateProperties.Add($"DealerAction: {gameState.DealerAction}; DealerHand: {CardsTextInline(gameState.DealerHand, true)}");
        gameStateProperties.Add("====================");

        if(gameState.ActiveBets.Count > 0)
        {
            gameStateProperties.Add("== ActiveBets ======");
            foreach(Bet bet in gameState.ActiveBets)
            {
                gameStateProperties.Add($"{bet.Player.Name}; ${bet.Amount}; {bet.Type}; {bet.Resolution}, {CardsTextInline(bet.Hand, true)}");
            }
            gameStateProperties.Add("====================");
        }

        gameStateProperties.Add("== Last things =====");
        if(gameState.LastDealerAction is not null){
            gameStateProperties.Add($"LastDealerAction: {gameState.LastDealerAction}");
        }
        if(gameState.LastPlayer is not null){
            gameStateProperties.Add($"LastPlayer: {gameState.LastPlayer}; Name: {gameState.LastPlayer.Name}");
        }
        if(gameState.LastPlayerIntent is not null){
            gameStateProperties.Add($"LastPlayerIntent: {gameState.LastPlayerIntent}");
        }
        if(gameState.LastHand is not null){
            gameStateProperties.Add($"LastHand: {CardsTextInline(gameState.LastHand, true)}");
        }
        if(gameState.LastDrawnCard is not null){
            gameStateProperties.Add($"LastDrawnCard: {gameState.LastDrawnCard}; Card: {CardText(gameState.LastDrawnCard, true)}");
        }
        if(gameState.LastResolvedBet is not null){
            gameStateProperties.Add($"LastResolvedBet: {gameState.LastResolvedBet}; Player: ${gameState.LastResolvedBet.Player.Name};  Amount: ${gameState.LastResolvedBet.Amount}");
        }
        gameStateProperties.Add("====================");

        return string.Join("\n", gameStateProperties);
    }

}