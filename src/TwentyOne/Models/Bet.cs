namespace TwentyOne.Models;

public class Bet(Player targetPlayer, Hand targetHand, decimal betAmount, BetType betType)
{
    public readonly Player Player = targetPlayer;
    public readonly Hand Hand = targetHand;
    public readonly decimal Amount = betAmount;
    public readonly BetType Type = betType;
    public BetResolutionType Resolution = BetResolutionType.None;
}