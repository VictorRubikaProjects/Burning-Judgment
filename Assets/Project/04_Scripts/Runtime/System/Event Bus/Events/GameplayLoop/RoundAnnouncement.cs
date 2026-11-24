using Event_Bus;

public readonly struct RoundAnnouncement : IEvent
{
	public readonly float Duration;
	
	public RoundAnnouncement(float duration)
	{
		Duration = duration;
	}
}