public interface IStrikeable
{
    StrikeResult OnStruck(in StrikeContext context);
}