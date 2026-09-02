#nullable enable
using UnityEngine;

namespace HackedDesign
{
    public class AiSteering
    {
        private const float JumpCooldown = 0.6f;
        private const int MaxFailedJumps = 3;
        private const float ProgressEpsilon = 0.5f;
        private const float StuckTime = 2f;
        private const float GapLandingDropMax = 4f;
        private const float GapJumpCommitTime = 0.7f;
        private const float GapJumpSafety = 0.85f;
        private const float GapMinDistance = 0.5f;
        private const int GapLandingSamples = 6;

        private float nextJumpTime;
        private int failedJumps;
        private float lastJumpX = float.NaN;
        private bool stuck;
        private float stuckUntil;
        private float gapJumpUntil;

        public void MoveToward(IAi ai, AiContext ctx, Vector3 target, bool allowGapJump = false)
        {
            int facing = target.x < ctx.position.x ? -1 : 1;
            ai.Character.ExecuteCommand(new FacingCommand(0, facing));

            if (ctx.flying)
            {
                var direction = (target - ctx.position).normalized;
                float climb = HoverClimb(ctx);
                float vertical;
                if (ctx.wallInFront)
                {
                    // A building/wall blocks the horizontal path: climb over it.
                    vertical = 1f;
                }
                else if (climb > 0)
                {
                    // Too close to the ground: force a climb before anything else.
                    vertical = climb;
                }
                else
                {
                    // Pursue the target vertically, but never descend into solid ground
                    // directly below. Only drop when there is open space (a gap) beneath us,
                    // so we ride along rooftops until the terrain opens up.
                    bool groundBelow = !float.IsInfinity(ctx.groundDistance);
                    vertical = (groundBelow && direction.y < 0) ? Mathf.Max(climb, 0f) : direction.y;
                }
                ai.Character.ExecuteCommand(new MoveCommand(facing, vertical));
                return;
            }

            if (stuck && Time.time >= stuckUntil)
            {
                stuck = false;
                failedJumps = 0;
                lastJumpX = float.NaN;
            }

            if (ctx.wallInFront && !stuck && Time.time >= nextJumpTime)
            {
                bool progressed = float.IsNaN(lastJumpX) || Mathf.Abs(ctx.position.x - lastJumpX) >= ProgressEpsilon;
                failedJumps = progressed ? 0 : failedJumps + 1;

                if (failedJumps >= MaxFailedJumps)
                {
                    stuck = true;
                    stuckUntil = Time.time + StuckTime;
                }
                else
                {
                    ai.Character.ExecuteCommand(new JumpCommand());
                    lastJumpX = ctx.position.x;
                    nextJumpTime = Time.time + JumpCooldown;
                }
            }

            if (allowGapJump && ctx.dropInFront && !stuck && Time.time >= nextJumpTime && CanClearGap(ai, ctx))
            {
                ai.Character.ExecuteCommand(new JumpCommand());
                nextJumpTime = Time.time + JumpCooldown;
                gapJumpUntil = Time.time + GapJumpCommitTime;
            }

            // While committed to a gap jump, keep driving forward so we carry momentum across.
            bool committedGapJump = Time.time < gapJumpUntil;
            float move = ((ctx.dropInFront && !committedGapJump) || stuck) ? 0 : facing;
            ai.Character.ExecuteCommand(new MoveCommand(move, 0));
        }

        private static bool CanClearGap(IAi ai, AiContext ctx)
        {
            float maxReach = ai.Character.MaxJumpReach * GapJumpSafety;
            if (maxReach <= GapMinDistance)
            {
                return false;
            }

            // Search columns across the gap for landing ground (above or below), probing
            // down from the jump apex so higher platforms are detected too, then test each
            // candidate against the actual jump arc.
            float apex = ai.Character.JumpApexHeight;
            float castDistance = apex + GapLandingDropMax;

            for (int i = 1; i <= GapLandingSamples; i++)
            {
                float dx = Mathf.Lerp(GapMinDistance, maxReach, i / (float)GapLandingSamples);
                var origin = (Vector2)ctx.position + new Vector2(ctx.facing * dx, apex);
                var hit = Physics2D.Raycast(origin, Vector2.down, castDistance, ctx.movementMask);
                if (hit.collider == null)
                {
                    continue;
                }

                float dy = hit.point.y - ctx.position.y;
                if (ai.Character.CanReachJump(dx, dy))
                {
                    return true;
                }
            }

            return false;
        }

        public static float HoverClimb(AiContext ctx)
        {
            // No ground within range => over a gap: hold altitude (coast level).
            if (float.IsInfinity(ctx.groundDistance))
            {
                return 0f;
            }

            float error = ctx.settings.HoverHeight - ctx.groundDistance; // +ve = too low, climb
            if (Mathf.Abs(error) <= ctx.settings.HoverDeadband)
            {
                return 0f;
            }

            return Mathf.Sign(error); // up if too low, down if too high (within probe range)
        }
    }
}
