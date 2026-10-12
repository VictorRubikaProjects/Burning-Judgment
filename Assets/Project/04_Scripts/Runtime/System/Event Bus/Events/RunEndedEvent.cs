using Event_Bus;

public struct RunEndedEvent : IEvent
{
    public readonly int FloorReached;

    public RunEndedEvent(int floorReached)
    {
        FloorReached = floorReached;
    }
}