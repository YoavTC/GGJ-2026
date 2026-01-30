using UnityEngine;

public struct KnockbackData
{
    public Vector2 Direction;
    public float Force;

    public KnockbackData(Vector2 direction, float force)
    {
        Direction = direction;
        Force = force;
    }

    public static KnockbackData Empty => new KnockbackData(Vector2.zero, 0f);
}