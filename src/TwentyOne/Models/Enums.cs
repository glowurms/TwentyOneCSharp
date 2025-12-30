namespace TwentyOne.Models
{
    public enum Suit
    {
        Clubs, Diamonds, Hearts, Spades
    }

    public enum Rank
    {
        Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
    }

    public enum GamePhase
    {
        Betting,
        Dealing,
        PlayerTurns,
        DealerTurn,
        RoundEnd
    }

    public enum PlayerHandActions
    {
       Bet, Stand, Hit, DoubleDown, Split
    }

    public enum PlayerStatus
    {
        Active, Busted, Standing, Blackjack, Bankrupt
    }

    public enum PlayerGameActions
    {
        Continue, Cancel, Instructions, Quit
    }
}
