using System;

public readonly struct PlayerActionPermissions
{
    public readonly bool InputAvailable;
    
    public PlayerActionPermissions(bool inputAvailable)
    {
        InputAvailable = inputAvailable;
    }

    public static PlayerActionPermissions FromContext(PlayerControlContext context)
    {
        return context switch
        {
            PlayerControlContext.RoundCountdown => new PlayerActionPermissions(
                inputAvailable: false
                ),
            PlayerControlContext.RoundGameplay => new PlayerActionPermissions(
                inputAvailable: true
            ),
            PlayerControlContext.RoundWon => new PlayerActionPermissions(
                inputAvailable: true
            ),
            PlayerControlContext.RoundLose => new PlayerActionPermissions(
                inputAvailable: false
            ),
        };
    }
}