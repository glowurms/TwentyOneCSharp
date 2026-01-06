namespace TwentyOne.Models;

public class Bet(Player targetPlayer, Hand targetHand, decimal betAmount)
{
    public readonly Player Player = targetPlayer;
    public readonly Hand Hand = targetHand;
    public decimal Amount = betAmount;
}