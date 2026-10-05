using UnityEngine;

public static class KnockbackUtility
{
    public static float CalculateKnockbackDistance(float incomingForce, float targetWeight)
    {
        float safeWeight = Mathf.Max(targetWeight, 0.01f);

        return incomingForce / safeWeight;
    }
}