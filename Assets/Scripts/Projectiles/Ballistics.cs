using UnityEngine;

namespace HackedDesign
{
    public static class Ballistics
    {
        public static Vector2 SolveArc(Vector2 from, Vector2 to, float speed)
        {
            float g = Mathf.Abs(Physics2D.gravity.y);
            float dx = to.x - from.x;
            float dy = to.y - from.y;
            float dir = dx < 0 ? -1f : 1f;
            float range = Mathf.Abs(dx);
            float speedSquared = speed * speed;
            float discriminant = speedSquared * speedSquared - g * (g * range * range + 2f * dy * speedSquared);

            float theta;
            if (g <= 0f || range < Mathf.Epsilon)
            {
                theta = Mathf.Atan2(dy, range);
            }
            else if (discriminant >= 0f)
            {
                theta = Mathf.Atan((speedSquared - Mathf.Sqrt(discriminant)) / (g * range));
            }
            else
            {
                theta = (Mathf.PI / 4f) + (Mathf.Atan2(dy, range) / 2f);
            }

            return new Vector2(dir * speed * Mathf.Cos(theta), speed * Mathf.Sin(theta));
        }

        public static Vector2 PositionAt(Vector2 start, Vector2 velocity, float time) =>
            start + (velocity * time) + (0.5f * time * time * Physics2D.gravity);
    }
}
