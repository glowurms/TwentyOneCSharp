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
    public class GameService
    {
        public GameState GameState { get { return _gameState; } }

        private GameState _gameState;
        private Player _currentPlayer { get { return _gameState.Players[_gameState.CurrentPlayerIndex]; } }
        private Hand _currentHand { get { return _currentPlayer.HandsInPlay[_gameState.CurrentHandIndex]; } }
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
            return _gameState;
        }

        // Advance the game
        public void ContinueGame()
        {
            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    HandlePlayerAction();
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.Dealing:
                    DealInitialCards();
                    break;
                case GamePhase.PlayerTurns:
                    HandlePlayerAction();
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.DealerTurn:
                    HandleDealerAction();
                    break;
                case GamePhase.RoundEnd:
                    break;
                default:
                    break;
            }
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

            _gameState.CurrentPlayerIntent = PlayerActions.None;

            if (shouldMoveToNextHand)
            {
                if (!MoveToNextHand())
                {
                    ResetCurrentPlayerAndHandIndex();
                    AdvanceGamePhase();
                }
            }
        }

        private void UpdateCurrentPlayerOptions()
        {
            _gameState.CurrentPlayerOptions.Clear();

            switch (_gameState.CurrentGamePhase)
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

                    if (RulesService.CanSplitHand(_currentHand, _currentPlayer.HandsInPlay.Count))
                    {
                        _gameState.CurrentPlayerOptions.Add(PlayerActions.Split);
                    }

                    break;
                default:
                    _gameState.CurrentPlayerOptions.Add(PlayerActions.None);
                    break;
            }
        }

        private void HandleDealerAction()
        {
            // Determine Dealer action
            if (_dealerHand.CardsInHand[1].FaceUp == false)
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
                    _gameState.DealerHand.CardsInHand[1].FaceUp = true;
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

            _gameState.DealerAction = DealerActions.None;
        }

        private void AdvanceGamePhase()
        {
            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    GamePhaseEnd(GamePhase.Betting);
                    GamePhaseBegin(GamePhase.Dealing);
                    break;
                case GamePhase.Dealing:
                    GamePhaseEnd(GamePhase.Dealing);
                    // TODO: Simulate or implement Naturals GamePhase Scenario
                    GamePhaseBegin(GamePhase.PlayerTurns);
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
                    _gameState.CurrentGamePhase = GamePhase.Betting;
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.Dealing:
                    _gameState.CurrentGamePhase = GamePhase.Dealing;
                    _gameState.Shoe.TendShoe();
                    ResetHands();
                    break;
                case GamePhase.Naturals:
                    _gameState.CurrentGamePhase = GamePhase.Naturals;
                    // TODO: Simulate or implement Naturals GamePhaseBegin Scenario
                    break;
                case GamePhase.PlayerTurns:
                    _gameState.CurrentGamePhase = GamePhase.PlayerTurns;
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.DealerTurn:
                    _gameState.CurrentGamePhase = GamePhase.DealerTurn;
                    break;
                case GamePhase.RoundEnd:
                    _gameState.CurrentGamePhase = GamePhase.RoundEnd;
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
        }

        private bool MoveToNextHand()
        {
            int playerIndex = _gameState.CurrentPlayerIndex;
            int handIndex = _gameState.CurrentHandIndex;

            if (handIndex + 1 < _gameState.Players[playerIndex].HandsInPlay.Count)
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

        private void PlayerBet()
        {
            decimal bet = Math.Min(_currentPlayer.Bankroll, GameConstants.DefaultBetAmount);
            _currentPlayer.Bankroll -= bet;
            _currentPlayer.Bet += bet;
        }

        private void PlayerDoubleDown()
        {
            // TODO: Refactor to couple double down with a specific hand, deal 1 card and ignore till round end
            decimal bet = _currentPlayer.Bet;
            _currentPlayer.Bankroll -= bet;
            _currentPlayer.Bet += bet;
        }

        private void PlayerHit()
        {
            DealCardToPlayer();
        }

        private void PlayerSplit()
        {
            Hand splitHandOne = new();
            splitHandOne.AddCard(_currentHand.CardsInHand[0]);
            Hand splitHandTwo = new();
            splitHandTwo.AddCard(_currentHand.CardsInHand[1]);
            _currentPlayer.HandsInPlay.Clear();
            _currentPlayer.HandsInPlay.Add(splitHandOne);
            _currentPlayer.HandsInPlay.Add(splitHandTwo);
        }

        private Card DrawCard()
        {
            Card? card = _gameState.Shoe.DealCard();
            return card ?? throw new Exception("Not enough cards in the shoe.");
        }

        private Card DealCardToPlayer()
        {
            Card card = DrawCard();
            _currentHand.AddCard(DrawCard());
            return card;
        }

        private Card DealCardToDealer(bool FaceUp = true)
        {
            Card card = DrawCard();
            _gameState.DealerHand.AddCard(DrawCard());
            return card;
        }

        private void ResetHands()
        {
            foreach (Player player in _gameState.Players)
            {
                player.HandsInPlay.Clear();
                player.HandsInPlay.Add(new Hand());
            }
            _gameState.DealerHand = new Hand();
        }

        private void DealInitialCards()
        {
            bool atFirstPlayerIndex = _gameState.CurrentPlayerIndex == 0;

            // Draw Card for Dealer
            if(atFirstPlayerIndex && _dealerHand.TotalCardCount < _currentHand.TotalCardCount)
            {
                bool faceUp = _dealerHand.TotalCardCount != 1;
                DealCardToDealer(faceUp);
            }
            // Draw Card for Player
            else if(_currentHand.TotalCardCount < 2)
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
    }
}