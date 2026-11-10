using Event_Bus;

public readonly struct PlayerComboEvent : IEvent
{
    public readonly int Combo;

    public PlayerComboEvent(int combo)
    {
        Combo = combo;
    }
}