namespace RotationGuide.Utils;

public static class AnimationEasings
{
    public static float EaseInQuart(float t)
    {
        return t * t * t * t;
    }
}
