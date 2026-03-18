namespace ArrowWar.Core
{
    public enum GameState
    {
        Menu,
        Battle,
        RoundTransition, // Between rounds: enemies cleared, next round not yet started.
        Victory,
        Defeat
    }
}
