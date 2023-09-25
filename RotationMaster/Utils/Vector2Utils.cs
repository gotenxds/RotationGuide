using System.Numerics;

namespace RotationMaster.Utils;

public static class Vector2Utils
{
    public static Vector2 Transpose(this Vector2 v)
    {
        return new Vector2(v.Y, v.X);
    }
}
