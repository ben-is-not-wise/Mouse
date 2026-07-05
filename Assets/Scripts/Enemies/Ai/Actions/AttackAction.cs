#nullable enable
using UnityEngine;

namespace HackedDesign
{
    public class AttackAction : IUtilityAction
    {
        private const float MeleeRange = 1.5f;

        private readonly AiSteering steering = new AiSteering();

        private float startTriggerTime;
        private float lastSawPlayer = -1f;

        private Vector3 prevPlayerPosition;
        private Vector3 playerVelocity;
        private bool hasPrevPlayer;

        public float Score(IAi ai, AiContext ctx)
        {
            if (ctx.canSeePlayer)
            {
                lastSawPlayer = Time.time;
                return 1f;
            }

            if (lastSawPlayer >= 0 && Time.time <= lastSawPlayer + ctx.settings.GiveUpTime)
            {
                return 0.9f;
            }

            return 0f;
        }

        public void Begin(IAi ai)
        {
            startTriggerTime = Time.time;
            hasPrevPlayer = false;
            playerVelocity = Vector3.zero;
            ai.Character.ExecuteCommand(new WalkCommand(false));
            ai.Icon.Alert();
        }

        public void End(IAi ai) => ai.Icon.Hide();

        public void Perform(IAi ai, AiContext ctx)
        {
            if (Game.Instance.Player.Character.IsDead)
            {
                return;
            }

            var playerPosition = Game.Instance.Player.transform.position;

            if (hasPrevPlayer && Time.fixedDeltaTime > 0)
            {
                var instant = (playerPosition - prevPlayerPosition) / Time.fixedDeltaTime;
                playerVelocity = Vector3.Lerp(playerVelocity, instant, 0.3f);
            }
            prevPlayerPosition = playerPosition;
            hasPrevPlayer = true;

            if (!ctx.settings.Stationary && ctx.canSeePlayer)
            {
                ai.Character.ExecuteCommand(new FacingCommand(0, playerPosition.x <= ai.Character.transform.position.x ? -1 : 1));
            }

            ai.Character.ExecuteCommand(new AimCommand(ctx.bullets > 0));

            if (!ctx.canSeePlayer)
            {
                return;
            }

            if (ctx.bullets > 0)
            {
                ai.Character.ExecuteCommand(new MoveCommand(0, 0));
                TryAttack(ai, ctx, aiming: true);
                return;
            }

            var toPlayer = ai.Character.transform.position - playerPosition;
            if (toPlayer.magnitude > MeleeRange)
            {
                steering.MoveToward(ai, ctx, playerPosition);
            }
            else
            {
                ai.Character.ExecuteCommand(new MoveCommand(0, 0));
                TryAttack(ai, ctx, aiming: false);
            }
        }

        private void TryAttack(IAi ai, AiContext ctx, bool aiming)
        {
            if (startTriggerTime + ctx.settings.RecognitionTime <= Time.time)
            {
                var aim = ctx.lastKnownPlayerPosition
                    + (playerVelocity * ctx.settings.TargetLeading)
                    + (Vector3)(Random.insideUnitCircle * ctx.settings.AimError);
                ai.Character.Attack(aim, aiming);
                startTriggerTime = Time.time + ai.Character.Settings.AttackRate;
            }
        }
    }
}
