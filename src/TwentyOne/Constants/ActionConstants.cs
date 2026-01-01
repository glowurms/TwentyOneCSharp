using TwentyOne.Models;

namespace TwentyOne.Constants;

public static class ActionConstants
{
    public static readonly Dictionary<PlayerHandActions, string> ActionDescriptions = new()
    {
        { PlayerHandActions.Bet, "Place a bet" },
        { PlayerHandActions.Hit, "Take another card" },
        { PlayerHandActions.Stand, "Keep your current hand" },
        { PlayerHandActions.DoubleDown, "Double your bet and take one more card" },
        { PlayerHandActions.Split, "Split your hand into two hands" }
    };

    public static readonly Dictionary<PlayerHandActions, string> ActionTakenText = new()
    {
        { PlayerHandActions.Bet, "places a bet" },
        { PlayerHandActions.Hit, "hits and gets another card" },
        { PlayerHandActions.Stand, "stays" },
        { PlayerHandActions.DoubleDown, "doubles down and gets one more card" },
        { PlayerHandActions.Split, "splits" },
        { PlayerHandActions.None, "has no action to take" }
    };
}