using UnityEngine;

public static class KnockbackUtility
{
    public static float CalculateKnockbackDistance(float incomingForce, float targetWeight, float targetScaling, float targetStability)
    {
        float safeWeight = Mathf.Max(targetWeight, 0.01f);
        float finalForce = incomingForce / safeWeight;
        finalForce /= Mathf.Pow(targetStability, targetScaling);
        return finalForce;
    }

    public static float CalculateNewStability(float incomingDestabilization, float targetStability)
    {
        return targetStability * (1 - incomingDestabilization);
    }

    /*
    public static float CalculateMultiplicatorForceChain(float targetForce, float multiplicatorForce, float )
    {
        
    }*/
}