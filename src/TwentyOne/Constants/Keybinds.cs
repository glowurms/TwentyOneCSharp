using TwentyOne.Models;
namespace TwentyOne.Constants;

public class Keybinds
{
    // Key bindings for game actions
    public static readonly Dictionary<ConsoleKey, GameActions> GameAction = new()
    {
        { ConsoleKey.Spacebar, GameActions.Continue },
        { ConsoleKey.Escape, GameActions.Cancel },
        { ConsoleKey.I, GameActions.Instructions },
        { ConsoleKey.Q, GameActions.Quit }
    };

    // Key bindings for player actions
    public static readonly Dictionary<ConsoleKey, PlayerActions> PlayerAction = new()
    {
        { ConsoleKey.B, PlayerActions.Bet },
        { ConsoleKey.F, PlayerActions.Hit },
        { ConsoleKey.S, PlayerActions.Stand },
        { ConsoleKey.D, PlayerActions.DoubleDown },
        { ConsoleKey.A, PlayerActions.Split }
    };
}