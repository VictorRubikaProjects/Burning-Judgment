using Event_Bus;

public readonly struct PlayerComboEvent : IEvent
{
    public readonly float Multiplier;

    public PlayerComboEvent(float multiplier)
    {
        Multiplier = multiplier;
    }
}