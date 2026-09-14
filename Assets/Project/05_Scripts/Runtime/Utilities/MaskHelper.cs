namespace Helpers.Runtime.Math
{
    public static class MaskHelper
    {
        public static bool ContainsFlag(this int mask, int toCheck)
        {
            return (mask & toCheck) == toCheck;
        }
        
        public static int AddFlag(this int mask, int toAdd)
        {
            return mask |= toAdd;
        }

        public static int RemoveFlag(this int mask, int toRemove)
        {
            return mask &= ~toRemove;
        }
    }
}