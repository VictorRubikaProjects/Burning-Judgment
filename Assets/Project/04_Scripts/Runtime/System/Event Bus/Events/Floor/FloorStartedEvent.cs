using Event_Bus;

public struct FloorStartedEvent : IEvent
{
    public readonly int Floor;

    public FloorStartedEvent(int floor)
    {
        Floor = floor;
    }
}