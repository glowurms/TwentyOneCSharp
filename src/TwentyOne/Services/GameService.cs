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

            _gameState.CurrentGamePhase = GamePhase.Betting;
            UpdateCurrentPlayerOptions();
            return _gameState;
        }

        public void DealInitialCards()
        {
            if (_gameState.Shoe.CutCardReached || _gameState.Shoe.ShoeIsNotShuffled)
            {
                _gameState.Shoe.Shuffle();
            }

            // Setup hands and deal first card
            foreach (Player player in _gameState.Players)
            {
                player.HandsInPlay.Clear();
                player.HandsInPlay.Add(new Hand());
                player.HandsInPlay[0].AddCard(DealCard());
            }
            _gameState.DealerHand = new Hand();
            _gameState.DealerHand.AddCard(DealCard());

            // Deal second card
            foreach (Player player in _gameState.Players)
            {
                player.HandsInPlay[0].AddCard(DealCard());
            }

            // Second dealer card is face down
            Card secondDealerCard = DealCard();
            secondDealerCard.FaceUp = false;
            _gameState.DealerHand.AddCard(secondDealerCard);

            AdvanceGamePhase();
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

        // Advance the game
        public void ContinueGame()
        {
            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    UpdateCurrentPlayerOptions();
                    HandlePlayerAction();
                    break;
                case GamePhase.Dealing:
                    DealInitialCards();
                    break;
                case GamePhase.PlayerTurns:
                    UpdateCurrentPlayerOptions();
                    HandlePlayerAction();
                    break;
                case GamePhase.DealerTurn:
                    HandleDealerAction();
                    DetermineDealerAction();
                    break;
                case GamePhase.RoundEnd:
                    break;
                default:
                    break;
            }
        }

        public bool SelectPlayerAction(PlayerActions playerAction)
        {
            if (_currentPlayer.SelectedAction == PlayerActions.None && _gameState.CurrentPlayerOptions.Contains(playerAction))
            {
                _currentPlayer.SelectedAction = playerAction;
                return true;
            }
            return false;
        }

        private void HandlePlayerAction()
        {
            switch (_currentPlayer.SelectedAction)
            {
                case PlayerActions.Bet:
                    PlayerBet();
                    AdvanceHandOrPlayer();
                    break;
                case PlayerActions.Hit:
                    PlayerHit();
                    if (RulesService.HandIsBust(_currentHand))
                    {
                        AdvanceHandOrPlayer();
                    }
                    break;
                case PlayerActions.Stand:
                    AdvanceHandOrPlayer();
                    break;
                case PlayerActions.DoubleDown:
                    PlayerDoubleDown();
                    AdvanceHandOrPlayer();
                    break;
                case PlayerActions.Split:
                    PlayerSplit();
                    break;
                default:
                    break;
            }
            _currentPlayer.SelectedAction = PlayerActions.None;

        }

        private void DetermineDealerAction()
        {
            if (_gameState.DealerHand.CardsInHand[1].FaceUp == false)
            {
                _gameState.DealerAction = DealerActions.ShowFaceDown;
            }
            else if (RulesService.DealerShouldDraw(_gameState.DealerHand))
            {
                _gameState.DealerAction = DealerActions.Draw;
            }
            else
            {
                _gameState.DealerAction = DealerActions.Stand;
            }
        }

        private void HandleDealerAction()
        {
            switch (_gameState.DealerAction)
            {
                case DealerActions.ShowFaceDown:
                    _gameState.DealerHand.CardsInHand[1].FaceUp = true;
                    _gameState.DealerAction = DealerActions.None;
                    break;
                case DealerActions.Draw:
                    _gameState.DealerHand.AddCard(DealCard());
                    _gameState.DealerAction = DealerActions.None;
                    break;
                case DealerActions.Stand:
                    _gameState.DealerAction = DealerActions.None;
                    AdvanceGamePhase();
                    break;
                default:
                    break;
            }
        }

        private void AdvanceGamePhase()
        {
            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    _gameState.CurrentGamePhase = GamePhase.Dealing;
                    break;
                case GamePhase.Dealing:
                    _gameState.CurrentGamePhase = GamePhase.PlayerTurns;
                    UpdateCurrentPlayerOptions();
                    break;
                case GamePhase.PlayerTurns:
                    _gameState.CurrentGamePhase = GamePhase.DealerTurn;
                    DetermineDealerAction();
                    break;
                case GamePhase.DealerTurn:
                    _gameState.CurrentGamePhase = GamePhase.RoundEnd;
                    break;
                case GamePhase.RoundEnd:
                    _gameState.CurrentGamePhase = GamePhase.Betting;
                    UpdateCurrentPlayerOptions();
                    break;
                default:
                    break;
            }
        }

        private void AdvanceHandOrPlayer()
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
                // Last player reached. Reset index and advance game phase.
                _gameState.CurrentPlayerIndex = 0;
                _gameState.CurrentHandIndex = 0;
                AdvanceGamePhase();
            }
        }

        private void PlayerBet()
        {
            decimal bet = Math.Min(_currentPlayer.Bankroll, GameConstants.DefaultBetAmount);
            _currentPlayer.Bankroll -= bet;
            _currentPlayer.Bet += bet;
        }

        private void PlayerDoubleDown()
        {
            decimal bet = _currentPlayer.Bet;
            _currentPlayer.Bankroll -= bet;
            _currentPlayer.Bet += bet;
        }

        private void PlayerHit()
        {
            _currentHand.AddCard(DealCard());
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

        private Card DealCard()
        {
            Card? card = _gameState.Shoe.DealCard();
            return card ?? throw new Exception("Not enough cards in the shoe.");
        }
    }
}