using TwentyOne.Models;

namespace TwentyOne.Constants;

public static class CardConstants 
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

    public static readonly Dictionary<Rank, string> RankAbbreviations = new()
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

    public static readonly Dictionary<Suit, string> SuitAbbreviations = new()
    {
        { Suit.Clubs, "♣" },
        { Suit.Diamonds, "♦" },
        { Suit.Hearts, "♥" },
        { Suit.Spades, "♠" }
    };
}
