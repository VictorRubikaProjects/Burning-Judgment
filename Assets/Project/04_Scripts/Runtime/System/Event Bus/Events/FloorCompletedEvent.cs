using Event_Bus;

public struct FloorCompletedEvent : IEvent
{
    public readonly int Floor;

    public FloorCompletedEvent(int floor)
    {
        Floor = floor;
    }
}