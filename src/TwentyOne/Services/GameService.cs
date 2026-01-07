using System.Linq.Expressions;
using System.Runtime.InteropServices;
using TwentyOne.Constants;
using TwentyOne.Models;

namespace TwentyOne.Services
{
    /*
    Game rules reference:
    https://bicyclecards.com/how-to-play/blackjack

    Game flow:
    - Bets placed
    - 1 card face up to each player, 1 card face up to dealer
    - 1 card face up to each player, 1 card face down to dealer

    - Evaluate player Naturals (Ace and 10-value card)
        - Player has Natural, dealer does not -> player wins - 1.5x bet
        - Dealer has Natural, player does not -> dealer wins
        - Both have Naturals -> push - player bet returned

    - Cycle through each player
        - Evaluate Available actions for player's hand
            - If Hit, deal card, re-evaluate available actions
            - If Stand, move to next player
            - If Double Down, double bet, deal one card, move to next player
            - If Split,
                - Create new hand
                - move one card to new hand
                - deal one card to each hand
                - re-evaluate available actions for first hand

    - Dealer's turn
        - Reveal face down card
        - Dealer plays to 17
        - If dealer busts, round ends and all remaining players win
        - if dealer stands, evaluate each player's hand against dealer's hand
            - Player hand value > dealer hand value -> player wins 1x bet
            - Player hand value < dealer hand value -> dealer wins
            - Player hand value == dealer hand value -> push - player bet returned

    */

    public delegate void GameServiceStateChanged();

    public class GameService
    {
        public GameState GameState { get { return _gameState; } }
        public GameServiceStateChanged? StateChanged;

        private GameState _gameState;
        private Player _currentPlayer { get { return _gameState.Players[_gameState.CurrentPlayerIndex]; } }
        private Hand _currentHand { get { return _currentPlayer.Hands[_gameState.CurrentHandIndex]; } }
        private Hand _dealerHand { get { return _gameState.DealerHand; } }

        public GameService()
        {
            _gameState = new();
        }

        public GameService(ref GameState gameState)
        {
            _gameState = gameState;
            UpdateCurrentPlayerOptions();
        }

        public GameState StartNewGame()
        {
            return StartNewGame(GameConstants.MinPlayers, GameConstants.DefaultStartingBankroll, GameConstants.DefaultShoeDeckCount);
        }

        public GameState StartNewGame(int playerCount)
        {
            return StartNewGame(playerCount, GameConstants.DefaultStartingBankroll, GameConstants.DefaultShoeDeckCount);
        }

        public GameState StartNewGame(int playerCount, decimal startingBankroll)
        {
            return StartNewGame(playerCount, startingBankroll, GameConstants.DefaultShoeDeckCount);
        }

        public GameState StartNewGame(int playerCount, decimal startingBankroll, int shoeDeckCount)
        {
            _gameState = new()
            {
                Shoe = new Shoe(shoeDeckCount),
                DealerHand = new Hand(),
                Players = [],
            };

            _gameState.Shoe.TendShoe();

            int playersToAdd = playerCount;
            // Validate Player Count
            if (playerCount < GameConstants.MinPlayers)
            {
                playersToAdd = GameConstants.MinPlayers;
            }
            else if (playerCount > GameConstants.MaxPlayers)
            {
                playersToAdd = GameConstants.MaxPlayers;
            }

            for (int i = 1; i <= playersToAdd; i++)
            {
                _gameState.Players.Add(new Player($"Player {i}", startingBankroll));
            }

            GamePhaseBegin(GamePhase.Betting);
            StateChanged?.Invoke();
            return _gameState;
        }

        // Advance the game
        public void ContinueGame()
        {
            switch (_gameState.GamePhase)
            {
                case GamePhase.Betting:
                    HandlePlayerAction();
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.Dealing:
                    DealInitialCards();
                    break;
                case GamePhase.Naturals:
                    HandleNaturalsPhase();
                    break;
                case GamePhase.PlayerTurns:
                    HandlePlayerAction();
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.DealerTurn:
                    HandleDealerAction();
                    break;
                case GamePhase.RoundEnd:
                    HandleRoundEnd();
                    break;
                default:
                    break;
            }
            StateChanged?.Invoke();
        }

        public bool SelectPlayerAction(PlayerActions playerAction)
        {
            if (_gameState.CurrentPlayerIntent == PlayerActions.None && _gameState.CurrentPlayerOptions.Contains(playerAction))
            {
                _gameState.CurrentPlayerIntent = playerAction;
                return true;
            }
            return false;
        }

        private void HandlePlayerAction()
        {
            bool shouldMoveToNextHand = false;
            switch (_gameState.CurrentPlayerIntent)
            {
                case PlayerActions.Bet:
                    PlayerBet();
                    shouldMoveToNextHand = true;
                    break;
                case PlayerActions.Hit:
                    PlayerHit();
                    if (RulesService.HandIsBust(_currentHand))
                    {
                        shouldMoveToNextHand = true;
                    }
                    break;
                case PlayerActions.Stand:
                    shouldMoveToNextHand = true;
                    break;
                case PlayerActions.DoubleDown:
                    PlayerDoubleDown();
                    shouldMoveToNextHand = true;
                    break;
                case PlayerActions.Split:
                    PlayerSplit();
                    break;
                default:
                    break;
            }

            if(_gameState.CurrentPlayerIntent != PlayerActions.None)
            {
                _gameState.LastPlayerIntent = _gameState.CurrentPlayerIntent;
            }

            _gameState.CurrentPlayerIntent = PlayerActions.None;

            if (shouldMoveToNextHand)
            {
                if (!MoveToNextHand())
                {
                    ResetCurrentPlayerAndHandIndex();
                    AdvanceGamePhase();
                }
            }
            StateChanged?.Invoke();
        }

        private void UpdateCurrentPlayerOptions()
        {
            _gameState.CurrentPlayerOptions.Clear();

            switch (_gameState.GamePhase)
            {
                case GamePhase.Betting:
                    if (_currentPlayer.Bankroll > 0m)
                    {
                        _gameState.CurrentPlayerOptions.Add(PlayerActions.Bet);
                    }
                    break;
                case GamePhase.PlayerTurns:
                    if (RulesService.HandIsBust(_currentHand) || RulesService.HandIsNatural(_currentHand))
                    {
                        _gameState.CurrentPlayerOptions.Add(PlayerActions.None);
                        break;
                    }

                    _gameState.CurrentPlayerOptions.Add(PlayerActions.Hit);
                    _gameState.CurrentPlayerOptions.Add(PlayerActions.Stand);

                    if (RulesService.CanDoubleDown(_currentHand))
                    {
                        _gameState.CurrentPlayerOptions.Add(PlayerActions.DoubleDown);
                    }

                    if (RulesService.CanSplitHand(_currentHand, _currentPlayer.Hands.Count))
                    {
                        _gameState.CurrentPlayerOptions.Add(PlayerActions.Split);
                    }

                    break;
                default:
                    _gameState.CurrentPlayerOptions.Add(PlayerActions.None);
                    break;
            }
            StateChanged?.Invoke();
        }

        private void HandleDealerAction()
        {
            // Determine Dealer action
            if (_dealerHand.Cards[1].FaceUp == false)
            {
                _gameState.DealerAction = DealerActions.ShowFaceDown;
            }
            else if (RulesService.DealerShouldDraw(_dealerHand))
            {
                _gameState.DealerAction = DealerActions.Draw;
            }
            else
            {
                _gameState.DealerAction = DealerActions.Stand;
            }

            // Dealer takes action
            switch (_gameState.DealerAction)
            {
                case DealerActions.ShowFaceDown:
                    _gameState.DealerHand.Cards[1].FaceUp = true;
                    break;
                case DealerActions.Draw:
                    DealCardToDealer();
                    break;
                case DealerActions.Stand:
                    AdvanceGamePhase(); // Dealer turn ends
                    break;
                default:
                    break;
            }
            _gameState.LastDealerAction = _gameState.DealerAction;
            _gameState.DealerAction = DealerActions.None;
        }

        private void AdvanceGamePhase()
        {
            switch (_gameState.GamePhase)
            {
                case GamePhase.Betting:
                    GamePhaseEnd(GamePhase.Betting);
                    GamePhaseBegin(GamePhase.Dealing);
                    break;
                case GamePhase.Dealing:
                    GamePhaseEnd(GamePhase.Dealing);
                    if (ShouldProceedToNaturalsPhase())
                    {
                        GamePhaseBegin(GamePhase.Naturals);
                    }
                    else
                    {
                        GamePhaseBegin(GamePhase.PlayerTurns);
                    }
                    break;
                case GamePhase.Naturals:
                    GamePhaseEnd(GamePhase.Naturals);
                    GamePhaseBegin(GamePhase.RoundEnd);
                    break;
                case GamePhase.PlayerTurns:
                    GamePhaseEnd(GamePhase.PlayerTurns);
                    GamePhaseBegin(GamePhase.DealerTurn);
                    break;
                case GamePhase.DealerTurn:
                    GamePhaseEnd(GamePhase.DealerTurn);
                    GamePhaseBegin(GamePhase.RoundEnd);
                    break;
                case GamePhase.RoundEnd:
                    GamePhaseEnd(GamePhase.RoundEnd);
                    GamePhaseBegin(GamePhase.Betting);
                    break;
                default:
                    break;
            }
        }

        private void GamePhaseBegin(GamePhase targetPhase)
        {
            switch (targetPhase)
            {
                case GamePhase.Betting:
                    _gameState.GamePhase = GamePhase.Betting;
                    ResetHands();
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.Dealing:
                    _gameState.GamePhase = GamePhase.Dealing;
                    _gameState.Shoe.TendShoe();
                    break;
                case GamePhase.Naturals:
                    _gameState.GamePhase = GamePhase.Naturals;
                    // TODO: Simulate or implement Naturals GamePhaseBegin Scenario
                    break;
                case GamePhase.PlayerTurns:
                    _gameState.GamePhase = GamePhase.PlayerTurns;
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.DealerTurn:
                    _gameState.GamePhase = GamePhase.DealerTurn;
                    break;
                case GamePhase.RoundEnd:
                    _gameState.GamePhase = GamePhase.RoundEnd;
                    break;
                default:
                    break;
            }
        }

        private void GamePhaseEnd(GamePhase targetPhase)
        {
            switch (targetPhase)
            {
                case GamePhase.Betting:
                    break;
                case GamePhase.Dealing:
                    break;
                case GamePhase.PlayerTurns:
                    break;
                case GamePhase.DealerTurn:
                    break;
                case GamePhase.RoundEnd:
                    ResetHands();
                    break;
                default:
                    break;
            }
            _gameState.LastDealerAction = null;
            _gameState.LastDrawnCard = null;
            _gameState.LastHand = null;
            _gameState.LastPlayer = null;
            _gameState.LastPlayerIntent = null;
            _gameState.LastResolvedBet = null;
        }

        private bool MoveToNextHand()
        {
            int playerIndex = _gameState.CurrentPlayerIndex;
            int handIndex = _gameState.CurrentHandIndex;

            _gameState.LastPlayer = _currentPlayer;
            _gameState.LastHand = _currentHand;

            if (handIndex + 1 < _gameState.Players[playerIndex].Hands.Count)
            {
                // Move to next hand for current player
                _gameState.CurrentHandIndex++;
            }
            else if (playerIndex + 1 < _gameState.Players.Count)
            {
                // Move to next player
                _gameState.CurrentPlayerIndex++;
                _gameState.CurrentHandIndex = 0;
            }
            else
            {
                // Last player reached
                return false;
            }
            return true;
        }

        private void ResetCurrentPlayerAndHandIndex()
        {
            _gameState.CurrentPlayerIndex = 0;
            _gameState.CurrentHandIndex = 0;
        }

        private bool ShouldProceedToNaturalsPhase()
        {
            foreach(Player player in _gameState.Players)
            {
                if (RulesService.HandIsNatural(player.Hands[0]))
                {
                    return true;
                }
            }
            return false;
        }

        private void PlayerBet()
        {
            decimal currentBetAmount = Math.Min(_currentPlayer.Bankroll, GameConstants.DefaultBetAmount);
            _currentPlayer.Bankroll -= currentBetAmount;
            Bet betForCurrentHand = new(_currentPlayer, _currentHand, currentBetAmount, BetType.Normal);
            _gameState.ActiveBets.Add(betForCurrentHand);
        }

        private void PlayerDoubleDown()
        {
            Bet? betForCurrentHand = _gameState.ActiveBets.Find(bet => bet.Player == _currentPlayer && bet.Hand == _currentHand);
            if(betForCurrentHand == null) return; // TODO: Maybe should throw error here
            _gameState.ActiveBets.Remove(betForCurrentHand);
            Bet doubleDownBetForCurrentHand = new(betForCurrentHand.Player, betForCurrentHand.Hand, betForCurrentHand.Amount * 2, BetType.DoubleDown);
            _currentPlayer.Bankroll -= betForCurrentHand.Amount;
            _gameState.ActiveBets.Add(doubleDownBetForCurrentHand);
        }

        private void PlayerHit()
        {
            DealCardToPlayer();
        }

        private void PlayerSplit()
        {
            Bet? betForCurrentHand = _gameState.ActiveBets.Find(bet => bet.Player == _currentPlayer && bet.Hand == _currentHand);
            if(betForCurrentHand == null) return; // TODO: Maybe should throw error here
            _gameState.ActiveBets.Remove(betForCurrentHand);

            Card splitCard = _currentHand.Cards.Last();
            Hand splitHand = new();
            splitHand.AddCard(splitCard);
            _currentPlayer.Hands.Add(splitHand);

            Bet splitBetForCurrentHand = new(betForCurrentHand.Player, betForCurrentHand.Hand, betForCurrentHand.Amount, BetType.Split);
            Bet splitBetForSplittHand = new(betForCurrentHand.Player, splitHand, betForCurrentHand.Amount, BetType.Split);
            _currentPlayer.Bankroll -= betForCurrentHand.Amount;
            _gameState.ActiveBets.Add(splitBetForCurrentHand);
            _gameState.ActiveBets.Add(splitBetForSplittHand);
        }

        private Card DrawCard(bool faceUp = true)
        {
            Card? card = _gameState.Shoe.DealCard() ?? throw new Exception("Not enough cards in the shoe.");
            card.FaceUp = faceUp;
            _gameState.LastDrawnCard = card;
            return card;
        }

        private void DealCardToPlayer()
        {
            _currentHand.AddCard(DrawCard());
        }

        private void DealCardToDealer()
        {
            _gameState.DealerHand.AddCard(DrawCard());
        }

        private void DealCardFaceDownToDealer()
        {
            _gameState.DealerHand.AddCard(DrawCard(false));
        }

        private void ResetHands()
        {
            foreach (Player player in _gameState.Players)
            {
                player.Hands.Clear();
                player.Hands.Add(new Hand());
            }
            _gameState.DealerHand = new Hand();
        }

        private void DealInitialCards()
        {
            bool atFirstPlayerIndex = _gameState.CurrentPlayerIndex == 0;

            // Draw Card for Dealer
            if(atFirstPlayerIndex && _dealerHand.Cards.Count < _currentHand.Cards.Count)
            {
                if(_dealerHand.Cards.Count == 1)
                {
                    DealCardFaceDownToDealer();
                }
                else
                {
                    DealCardToDealer();
                }
                _gameState.LastPlayer = null;
            }
            // Draw Card for Player
            else if(_currentHand.Cards.Count < 2)
            {
                DealCardToPlayer();
                if (!MoveToNextHand())
                {
                    ResetCurrentPlayerAndHandIndex();
                }
            }
            else
            {
                AdvanceGamePhase();
            }
        }

        private void HandleNaturalsPhase()
        {
            // A Player has Ace and a 10 card

            // Dealer face up card is 10 card or Ace
                // Dealer checks face down and has 21
                    // Collect bets from any players who do not
                    // Return bets to players with 21
                    // Proceed to End Round
                // Dealer checks face down and doesn't have 21
                    // Pay out 1.5 to players with 21
                    // Proceed to player turns

            // Dealer face up card not 10 card or Ace
                // Pay out 1.5 to players with 21
                // Proceed to player turns
                

            AdvanceGamePhase();
        }

        private void ResolveBet(Bet targetBet)
        {
            bool dealerIsBust = RulesService.HandIsBust(_dealerHand);
            bool handdIsBust = RulesService.HandIsBust(targetBet.Hand);

            bool resultSameHands = RulesService.HandValue(targetBet.Hand) == RulesService.HandValue(_dealerHand);
            bool handBeatsDealer = RulesService.HandValue(targetBet.Hand) > RulesService.HandValue(_dealerHand);

            if (handdIsBust)
            {
                _gameState.TableWinnings += targetBet.Amount;
                targetBet.Resolution = BetResolutionType.Busted;
            }
            else if (dealerIsBust || handBeatsDealer)
            {
                targetBet.Player.Bankroll += targetBet.Amount * 2;
                _gameState.TableWinnings -= targetBet.Amount;
                targetBet.Resolution = BetResolutionType.Win;
            }
            else if (resultSameHands)
            {
                targetBet.Player.Bankroll += targetBet.Amount;
                targetBet.Resolution = BetResolutionType.Standoff;
            }
            else
            {
                _gameState.TableWinnings += targetBet.Amount;
                targetBet.Resolution = BetResolutionType.Lose;
            }
        }

        private void HandleRoundEnd()
        {
            // Step through Players and Hands in sequential order until the next unresolved bet is found
            Bet? targetBet = null;
            foreach(Player targetPlayer in _gameState.Players)
            {
                foreach(Hand targetHand in targetPlayer.Hands)
                {
                    targetBet ??= _gameState.ActiveBets.Find(bet => bet.Player == targetPlayer && bet.Hand == targetHand);
                }
            }

            if(targetBet == null)
            {
                AdvanceGamePhase();
                return;
            }

            ResolveBet(targetBet);
            _gameState.ActiveBets.Remove(targetBet);
            _gameState.LastResolvedBet = targetBet;
        }
    }
}