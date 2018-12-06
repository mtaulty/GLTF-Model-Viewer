using UnityEngine;

public static class QuaternionExtensions
{
    public static bool EqualToTolerance(this Quaternion lhs, Quaternion rhs, double tolerance)
    {
        var angle = Quaternion.Angle(lhs, rhs);
        return (angle <= tolerance);
    }
}
