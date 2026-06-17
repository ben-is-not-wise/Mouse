using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HackedDesign
{
    public class CharacterSleepingState : ICharacterState
    {
        private readonly Animator animator;

        public bool IsAlive => true;
        public bool CanAttack => false;

        public CharacterSleepingState(Animator animator)
        {
            this.animator = animator;
        }

        public void Animate(CharacterAnimationContext ctx)
        {
            if (this.animator == null)
            {
                return;
            
            }

            this.animator.SetBool(AnimatorParams.Sleep, true);
            this.animator.SetBool(AnimatorParams.Grounded, true);
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

        public void Attack(CharacterAttackContext ctx)
        {

        }
        public float CurrentSpeed(CharacterSpeedContext ctx) => 0;
    }
}
