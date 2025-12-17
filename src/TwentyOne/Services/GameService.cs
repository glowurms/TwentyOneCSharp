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
        public static readonly Dictionary<Rank, int> RankValues = new()
        {
            { Rank.Ace, 11 },
            { Rank.Two, 2 },
            { Rank.Three, 3 },
            { Rank.Four, 4 },
            { Rank.Five, 5 },
            { Rank.Six, 6 },
            { Rank.Seven, 7 },
            { Rank.Eight, 8 },
            { Rank.Nine, 9 },
            { Rank.Ten, 10 },
            { Rank.Jack, 10 },
            { Rank.Queen, 10 },
            { Rank.King, 10 }
        };        
        
        public static readonly Dictionary<Rank, string> RankShort = new()
        {
            { Rank.Ace, "A" },
            { Rank.Two, "2" },
            { Rank.Three, "3" },
            { Rank.Four, "4" },
            { Rank.Five, "5" },
            { Rank.Six, "6" },
            { Rank.Seven, "7" },
            { Rank.Eight, "8" },
            { Rank.Nine, "9" },
            { Rank.Ten, "10" },
            { Rank.Jack, "J" },
            { Rank.Queen, "Q" },
            { Rank.King, "K" }
        };

        public static readonly Dictionary<Suit, string> SuitSymbol = new()
        {
            { Suit.Clubs, "♣" },
            { Suit.Diamonds, "♦" },
            { Suit.Hearts, "♥" },
            { Suit.Spades, "♠" }
        };

        public static readonly Dictionary<PlayerHandActions, string> ActionDescriptions = new()
        {
            { PlayerHandActions.Bet, "Place a bet" },
            { PlayerHandActions.Hit, "Take another card" },
            { PlayerHandActions.Stand, "Keep your current hand" },
            { PlayerHandActions.DoubleDown, "Double your bet and take one more card" },
            { PlayerHandActions.Split, "Split your hand into two hands" }
        };

        // TODO: Move this to a UI service
        // Key bindings for player actions
        public static readonly Dictionary<PlayerHandActions, ConsoleKey> ActionKeys = new()
        {
            { PlayerHandActions.Bet, ConsoleKey.B },
            { PlayerHandActions.Hit, ConsoleKey.Spacebar },
            { PlayerHandActions.Stand, ConsoleKey.S },
            { PlayerHandActions.DoubleDown, ConsoleKey.D },
            { PlayerHandActions.Split, ConsoleKey.F }
        };

        // TODO: Move this to a UI service
        // Key bindings for game actions
        public static readonly Dictionary<PlayerGameActions, ConsoleKey> GameActionKeys = new()
        {
            { PlayerGameActions.Instructions, ConsoleKey.I },
            { PlayerGameActions.Quit, ConsoleKey.Q }
        };
        public static readonly int MinPlayers = 1;
        public static readonly int MaxPlayers = 6;
        public static readonly decimal BetAmount = 2m;

        public GameState GameState { get { return _gameState; } }

        private GameState _gameState;
        private Player _currentPlayer { get { return _gameState.Players[_gameState.CurrentPlayerIndex]; } }
        private Hand _currentHand { get { return _currentPlayer.HandsInPlay[_gameState.CurrentHandIndex]; } }

        public GameService()
        {
            _gameState = new()
            {
                InfoMessage = ""
            };
        }

        public GameService(ref GameState gameState)
        {
            _gameState = gameState;
            _gameState.InfoMessage = "Loaded game state.";
        }

        public GameState StartNewGame(int playerCount = 1, decimal startingBankroll = 500m, int shoeDeckCount = 6)
        {
            _gameState = new()
            {
                Shoe = new Shoe(shoeDeckCount),
                DealerHand = new Hand(),
                Players = [],
            };

            int playersToAdd = playerCount;
            // Validate Player Count
            if (playerCount < MinPlayers)
            {
                playersToAdd = MinPlayers;
                _gameState.InfoMessage = $"Minimum player count is {MinPlayers}.";
            }
            else if (playerCount > MaxPlayers)
            {
                playersToAdd = MaxPlayers;
                _gameState.InfoMessage = $"Maximum player count is {MaxPlayers}.";
            }

            for (int i = 1; i <= playersToAdd; i++)
            {
                _gameState.Players.Add(new Player($"Player {i}", startingBankroll));
            }

            string pluralizePlayers = (_gameState.Players.Count > 1) ? "s" : "";
            _gameState.StatusMessage = $"New game created with {playersToAdd} player{pluralizePlayers}.";
            _gameState.CurrentGamePhase = GamePhase.Betting;
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
            _gameState.DealerHand.AddCard(DealCard());
            _gameState.StatusMessage = "Cards Dealt.";
            _gameState.InfoMessage = "Taking bets.";
        }


        public List<PlayerHandActions> AvailablePlayerActions()
        {
            List<PlayerHandActions> actions = [];

            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    if (_currentPlayer.Bankroll > 0m)
                    {
                        actions.Add(PlayerHandActions.Bet);
                    }
                    break;
                case GamePhase.PlayerTurns:
                    Hand hand = _currentHand;
                    if (HandIsBust(hand) || HandIsNatural(hand))
                    {
                        break;
                    }
                    actions.Add(PlayerHandActions.Hit);
                    actions.Add(PlayerHandActions.Stand);
                    if (hand.TotalCardCount == 2)
                    {
                        actions.Add(PlayerHandActions.DoubleDown);

                        if (CanSplitHand(hand))
                        {
                            actions.Add(PlayerHandActions.Split);
                        }
                    }
                    break;
                default:
                    break;
            }

            return actions;
        }

        public void ContinueGame()
        {
            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    // Game phase advances after last player finishes actions
                    break;
                case GamePhase.Dealing:
                    break;
                case GamePhase.PlayerTurns:
                    // Game phase advances after last player finishes actions
                    break;
                case GamePhase.RoundEnd:
                    break;
                default:
                    break;
            }
        }

        public void HandlePlayerAction(PlayerHandActions action)
        {
            if (_gameState.AvailableActions.Contains(action)) return;
            switch (action)
            {
                case PlayerHandActions.Bet:
                    PlayerBet();
                    break;
                case PlayerHandActions.Hit:
                    PlayerHit();
                    break;
                case PlayerHandActions.Stand:
                    PlayerStand();
                    break;
                case PlayerHandActions.DoubleDown:
                    PlayerDoubleDown();
                    break;
                case PlayerHandActions.Split:
                    PlayerSplit();
                    break;
            }
        }

        public static int HandValue(Hand hand)
        {
            int value = 0;
            int aceCount = hand.CardsInHand.Count(c => c.Rank == Rank.Ace);
            foreach (var card in hand.CardsInHand)
            {
                if (card.Rank != Rank.Ace)
                {
                    value += RankValues[card.Rank];
                }
            }
            value += aceCount; // Count all aces as 1 initially
            while (value <= 11 && aceCount > 0)
            {
                value += 10; // Upgrade an ace from 1 to 11
                aceCount--;
            }
            return value;
        }

        public static bool HandIsBust(Hand hand) { return (HandValue(hand) > 21); }

        public static bool HandIsTwentyOne(Hand hand) { return (HandValue(hand) == 21); }

        public static bool HandIsNatural(Hand hand) { return (hand.TotalCardCount == 2 && HandValue(hand) == 21); }

        private static bool DealerShouldDraw(Hand hand) { return HandValue(hand) < 17; }

        private static bool CanSplitHand(Hand hand) { return (hand.CardsInHand[0].Rank == hand.CardsInHand[1].Rank); }

        private void AdvanceGamePhase()
        {
            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    _gameState.CurrentGamePhase = GamePhase.Dealing;
                    break;
                case GamePhase.Dealing:
                    _gameState.CurrentGamePhase = GamePhase.PlayerTurns;
                    break;
                case GamePhase.PlayerTurns:
                    _gameState.CurrentGamePhase = GamePhase.DealerTurn;
                    break;
                case GamePhase.RoundEnd:
                    _gameState.CurrentGamePhase = GamePhase.Betting;
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
            decimal bet = Math.Min(_currentPlayer.Bankroll, BetAmount);
            _currentPlayer.Bankroll -= bet;
            _currentPlayer.Bet += bet;
            _gameState.InfoMessage = $"{_currentPlayer.Name} bets ${bet}.";
            AdvanceHandOrPlayer();
        }

        private void PlayerStand()
        {
            _gameState.InfoMessage = $"{_currentPlayer.Name} stands.";
            AdvanceHandOrPlayer();
        }

        private void PlayerDoubleDown()
        {
            decimal bet = _currentPlayer.Bet;
            _currentPlayer.Bankroll -= bet;
            _currentPlayer.Bet += bet;
            _gameState.InfoMessage = $"{_currentPlayer.Name} doubles down.";
            AdvanceHandOrPlayer();
        }

        private void PlayerHit()
        {
            _currentHand.AddCard(DealCard());
            _gameState.InfoMessage = $"{_currentPlayer.Name} hits.";
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
            _gameState.InfoMessage = $"{_currentPlayer.Name} splits.";
        }

        private Card DealCard()
        {
            Card? card = _gameState.Shoe.DealCard();
            return card ?? throw new Exception("Not enough cards in the shoe.");
        }
    }
}