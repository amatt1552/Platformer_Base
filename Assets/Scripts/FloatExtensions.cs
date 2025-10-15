using UnityEngine;

public static class FloatExtensions
{
    //Moves float closer to value linearly by approachingSpeed
    public static void ApproachValue(this ref float currentValue, float startingValue, float approachingSpeed, float approachingValue = 0)
    {
        if (startingValue > approachingValue)
        {
            currentValue -= approachingSpeed;
            if (currentValue < approachingValue)
            {
                currentValue = approachingValue;
            }
        }
        else
        {
            currentValue += approachingSpeed;
            if (currentValue > approachingValue)
            {
                currentValue = approachingValue;
            }
        }
    }
}
