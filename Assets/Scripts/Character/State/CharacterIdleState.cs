using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(CharacterBattleState), typeof(CharacterSittingState), typeof(CharacterDeadState))]
    public class CharacterIdleState : ICharacterState
    {
        private readonly ICharacterExecute characterExecute;
        private readonly Animator animator;

        public bool IsAlive => true;
        public bool CanAttack => false;

        public CharacterIdleState(ICharacterExecute charExecute, Animator animator)
        {
            this.animator = animator;
            this.characterExecute = charExecute;
        }

        public void Animate(CharacterAnimationContext ctx)
        {
            if (this.animator == null)
            {
                return;
            }

            //this.animator.Play("Idle");

            this.animator.SetBool(AnimatorParams.Sit, false);
            this.animator.SetBool(AnimatorParams.Dead, false);
            this.animator.SetBool(AnimatorParams.Crouched, ctx.crouched);
            this.animator.SetBool(AnimatorParams.Grounded, ctx.onGround);
            this.animator.SetBool(AnimatorParams.Hang, ctx.onWall);
            this.animator.SetFloat(AnimatorParams.VelocityY, ctx.velocityY);
            this.animator.SetFloat(AnimatorParams.MovementMagnitude, ctx.movementMagnitude);
            this.animator.SetBool(AnimatorParams.RollOnLand, ctx.rollOnLand);
            this.animator.SetFloat(AnimatorParams.Aiming, ctx.aiming ? 1 : 0);
            this.animator.SetBool(AnimatorParams.LedgeStart, ctx.isClimbingLedge);
        }

        public void ResetAnimationTriggers()
        {
            if (this.animator == null)
            {
                return;
            }
            this.animator.ResetTrigger(AnimatorParams.Sleep);
            this.animator.ResetTrigger(AnimatorParams.Interact);
            this.animator.ResetTrigger(AnimatorParams.Roll);
            this.animator.ResetTrigger(AnimatorParams.Melee);
            this.animator.ResetTrigger(AnimatorParams.Shoot);
            this.animator.ResetTrigger(AnimatorParams.StrongAttack);
            this.animator.ResetTrigger(AnimatorParams.Jump);
            this.animator.ResetTrigger(AnimatorParams.MeleeAnticipate);
            this.animator.ResetTrigger(AnimatorParams.KickAnticipate);
            this.animator.ResetTrigger(AnimatorParams.PunchAnticipate);
            this.animator.ResetTrigger(AnimatorParams.ShootAnticipate);
        }

        public void Begin()
        {
        }

        public void End()
        {
        }

        public float CurrentSpeed(CharacterSpeedContext ctx) => ctx.crouched ? ctx.crouchSpeed : ctx.walkSpeed;

        public void Attack(CharacterAttackContext ctx) 
        {
            
        }
    }
}
