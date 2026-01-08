using TwentyOne.Models;
namespace TwentyOne.Constants;

public class Keybinds
{
    // Key bindings for game actions
    public static readonly Dictionary<ConsoleKey, GameActions> KeyToGameAction = new()
    {
        { ConsoleKey.Spacebar, GameActions.Continue },
        { ConsoleKey.Escape, GameActions.Cancel },
        { ConsoleKey.I, GameActions.Instructions },
        { ConsoleKey.Q, GameActions.Quit }
    };
    public static readonly Dictionary<GameActions, ConsoleKey> GameActionToKey = new()
    {
        { GameActions.Continue, ConsoleKey.Spacebar },
        { GameActions.Cancel, ConsoleKey.Escape },
        { GameActions.Instructions, ConsoleKey.I },
        { GameActions.Quit, ConsoleKey.Q }
    };

    // Key bindings for player actions
    public static readonly Dictionary<ConsoleKey, PlayerActions> KeyToPlayerAction = new()
    {
        { ConsoleKey.B, PlayerActions.Bet },
        { ConsoleKey.F, PlayerActions.Hit },
        { ConsoleKey.S, PlayerActions.Stand },
        { ConsoleKey.D, PlayerActions.DoubleDown },
        { ConsoleKey.A, PlayerActions.Split }
    };
    public static readonly Dictionary<PlayerActions, ConsoleKey> ToPlayerActionToKey = new()
    {
        { PlayerActions.Bet, ConsoleKey.B },
        { PlayerActions.Hit, ConsoleKey.F },
        { PlayerActions.Stand, ConsoleKey.S },
        { PlayerActions.DoubleDown, ConsoleKey.D },
        { PlayerActions.Split, ConsoleKey.A }
    };
}