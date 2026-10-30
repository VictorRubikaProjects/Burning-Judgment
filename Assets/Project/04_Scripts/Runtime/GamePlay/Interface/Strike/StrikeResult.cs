public readonly struct StrikeResult
{
    public readonly bool Accepted;
    public readonly bool CancelsDash;
    public readonly bool AllowsChain;

    public StrikeResult(bool accepted, bool cancelsDash, bool allowsChain)
    {
        Accepted = accepted;
        CancelsDash = cancelsDash;
        AllowsChain = allowsChain;
    }

    public static readonly StrikeResult Ignored = new(false, false, false);
}