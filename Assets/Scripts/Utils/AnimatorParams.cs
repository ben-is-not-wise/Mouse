using UnityEngine;

namespace HackedDesign
{
    public static class AnimatorParams
    {
        public const string IsDeadTag = "Dead";

        public static readonly int Interact = Animator.StringToHash("interact");
        public static readonly int Knockback = Animator.StringToHash("knockback");
        public static readonly int Grounded = Animator.StringToHash("grounded");
        public static readonly int Roll = Animator.StringToHash("roll");
        public static readonly int Dead = Animator.StringToHash("dead");
        public static readonly int Dying = Animator.StringToHash("dying");
        public static readonly int Splat = Animator.StringToHash("splat");
        public static readonly int Melee = Animator.StringToHash("melee");
        public static readonly int MeleeAnticipate = Animator.StringToHash("meleeAnticipate");
        public static readonly int Shoot = Animator.StringToHash("basicAttack");
        public static readonly int ShootAnticipate = Animator.StringToHash("shootAnticipate");
        public static readonly int StrongAttack = Animator.StringToHash("strongAttack");
        public static readonly int Jump = Animator.StringToHash("jump");
        public static readonly int Crouched = Animator.StringToHash("crouched");
        public static readonly int Sit = Animator.StringToHash("sit");
        public static readonly int Hang = Animator.StringToHash("hang");
        public static readonly int VelocityY = Animator.StringToHash("velocityY");
        public static readonly int MovementMagnitude = Animator.StringToHash("movementMagnitude");
        public static readonly int RollOnLand = Animator.StringToHash("rollOnLand");
        public static readonly int Aiming = Animator.StringToHash("aiming");
        public static readonly int LedgeStart = Animator.StringToHash("ledgeStart");
        public static readonly int Punch = Animator.StringToHash("punch");
        public static readonly int PunchAnticipate = Animator.StringToHash("punchAnticipate");
        public static readonly int Kick = Animator.StringToHash("kick");
        public static readonly int KickAnticipate = Animator.StringToHash("kickAnticipate");
        public static readonly int Idle = Animator.StringToHash("idle");
        public static readonly int FlyAway = Animator.StringToHash("flyaway");
        public static readonly int LookAround = Animator.StringToHash("lookaround");
        public static readonly int Attack = Animator.StringToHash("attack");
        public static readonly int Sleep = Animator.StringToHash("sleep");

    }
}
