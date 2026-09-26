using Event_Bus;

public struct RunEndedEvent : IEvent
{
    public readonly bool Victory;
    public readonly int FloorReached;

    public RunEndedEvent(bool victory, int floorReached)
    {
        Victory = victory;
        FloorReached = floorReached;
    }
}