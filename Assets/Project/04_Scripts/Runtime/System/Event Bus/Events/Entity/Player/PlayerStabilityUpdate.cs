using Event_Bus;

public readonly struct PlayerStabilityUpdate : IEvent
{
    public readonly float Stability;
    
    public PlayerStabilityUpdate(float stability)
    {
        Stability = stability;
    }
}