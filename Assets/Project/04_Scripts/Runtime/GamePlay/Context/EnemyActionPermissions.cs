using System;

public readonly struct EnemyActionPermissions
{
    public readonly bool CanAct;

    public EnemyActionPermissions(bool canAct)
    {
        CanAct = canAct;
    }

    public static EnemyActionPermissions FromContext(EnemyControlContext context)
    {
        return context switch
        {
            EnemyControlContext.RoundCountdown => new EnemyActionPermissions(
                canAct: false),
            EnemyControlContext.RoundGameplay => new EnemyActionPermissions(
                canAct: true),
            EnemyControlContext.RoundWon => new EnemyActionPermissions(
                canAct: false),
            _ => throw new ArgumentOutOfRangeException(nameof(context), context, null)
        };
    }
}