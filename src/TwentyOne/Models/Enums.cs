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
        Naturals,
        PlayerTurns,
        DealerTurn,
        RoundEnd
    }

    public enum DealerActions
    {
       ShowFaceDown, Draw, Stand, None
    }

    public enum PlayerActions
    {
       Bet, Stand, Hit, DoubleDown, Split, None
    }

    public enum GameActions
    {
        Continue, Cancel, Instructions, Quit
    }
}
