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

        public GameState GameState { get { return _gameState; } }

        private GameState _gameState;

        public GameService()
        {
            _gameState = new();
        }

        public GameService(ref GameState gameState)
        {
            _gameState = gameState;
        }

        public GameState StartNewGame(int startingBankroll, int shoeDeckCount, int playerCount = 1)
        {
            _gameState = new()
            {
                Shoe = new Shoe(shoeDeckCount),
                DealerHand = new Hand(),
                Players = [],
            };

            for (int i = 1; i <= playerCount; i++)
            {
                _gameState.Players.Add(new Player($"Player {i}", startingBankroll));
            }

            _gameState.StatusMessage = "New game started.";
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
                player.HandsInPlay[0].AddCard(DealCardFromShoe());
            }
            _gameState.DealerHand = new Hand();
            _gameState.DealerHand.AddCard(DealCardFromShoe());

            // Deal second card
            foreach (Player player in _gameState.Players)
            {
                player.HandsInPlay[0].AddCard(DealCardFromShoe());
            }
            _gameState.DealerHand.AddCard(DealCardFromShoe());
            _gameState.StatusMessage = "Cards Dealt.";
        }

        public Player CurrentPlayer()
        {
            return _gameState.Players[_gameState.CurrentPlayerIndex];
        }

        public Hand CurrentHand()
        {
            return CurrentPlayer().HandsInPlay[_gameState.CurrentHandIndex];
        }

        public List<PlayerHandActions> AvailablePlayerActions()
        {
            List<PlayerHandActions> actions = [];

            switch (_gameState.CurrentGamePhase)
            {
                case GamePhase.Betting:
                    actions.Add(PlayerHandActions.Bet);
                    break;
                case GamePhase.PlayerTurns:
                    Hand hand = CurrentHand();
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
            // TODO: add logic for advancing the game
        }

        public void HandlePlayerAction(PlayerHandActions action)
        {
            // TODO: validate action and bail if not valid
            switch (action)
            {
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

        public static bool HandIsBust(Hand hand)
        {
            if (HandValue(hand) > 21)
            {
                return true;
            }
            return false;
        }

        public static bool HandIsTwentyOne(Hand hand)
        {
            if (HandValue(hand) == 21)
            {
                return true;
            }
            return false;
        }

        public static bool HandIsNatural(Hand hand)
        {
            if (hand.TotalCardCount == 2 && HandValue(hand) == 21)
            {
                return true;
            }
            return false;
        }

        private static bool DealerShouldDraw(Hand hand)
        {
            return HandValue(hand) < 17;
        }

        private static bool CanSplitHand(Hand hand)
        {
            if(hand.CardsInHand[0].Rank == hand.CardsInHand[1].Rank)
            {
                return true;
            }
            return false;
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
                // Move to next player
                _gameState.CurrentPlayerIndex = 0;
                _gameState.CurrentHandIndex = 0;
            }
        }

        private void PlayerBet(decimal betAmount)
        {
            _gameState.StatusMessage = $"{CurrentPlayer().Name} bets ${betAmount}.";
            AdvanceHandOrPlayer();
        }

        private void PlayerStand()
        {
            _gameState.StatusMessage = $"{CurrentPlayer().Name} stands.";
            AdvanceHandOrPlayer();
        }

        private void PlayerDoubleDown()
        {
            _gameState.StatusMessage = $"{CurrentPlayer().Name} doubles down.";
            // TODO: Logic to increase bet
            AdvanceHandOrPlayer();
        }

        private void PlayerHit()
        {
            Card dealtCard = DealCardFromShoe();
            int playerIndex = _gameState.CurrentPlayerIndex;
            int handIndex = _gameState.CurrentHandIndex;
            _gameState.Players[playerIndex].HandsInPlay[handIndex].AddCard(dealtCard);
            _gameState.StatusMessage = $"{CurrentPlayer().Name} hits.";
            AdvanceHandOrPlayer();
        }

        private void PlayerSplit()
        {
            Hand currentHand = CurrentHand();
            int playerIndex = _gameState.CurrentPlayerIndex;
            _gameState.Players[playerIndex].HandsInPlay.Clear();
            _gameState.Players[playerIndex].HandsInPlay.Add(new Hand());
            _gameState.Players[playerIndex].HandsInPlay.Add(new Hand());
            _gameState.Players[playerIndex].HandsInPlay[0].AddCard(currentHand.CardsInHand[0]);
            _gameState.Players[playerIndex].HandsInPlay[1].AddCard(currentHand.CardsInHand[1]);
            _gameState.StatusMessage = $"{CurrentPlayer().Name} splits.";
        }

        private Card DealCardFromShoe()
        {
            Card? card = _gameState.Shoe.DealCard();
            return card ?? throw new Exception("Not enough cards in the shoe.");
        }
    }
}