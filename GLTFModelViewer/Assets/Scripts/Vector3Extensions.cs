using UnityEngine;

public static class Vector3Extensions
{
    public static bool EqualToTolerance(this Vector3 lhs, Vector3 rhs, double tolerance)
    {
        return (
            (Mathf.Abs(lhs.x - rhs.x) <= tolerance) &&
            (Mathf.Abs(lhs.y - rhs.y) <= tolerance) &&
            (Mathf.Abs(lhs.z - rhs.z) <= tolerance));
    }
}
