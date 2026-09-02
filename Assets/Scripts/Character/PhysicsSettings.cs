using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "PhysicsSettings", menuName = "Mouse/Settings/Physics")]
    public class PhysicsSettings : ScriptableObject
    {
        [Header("Gravity")]
        [SerializeField] public bool fly = false;
        [SerializeField, Range(0, 5f)] public float defaultGravityScale = 1f;
        [SerializeField, Range(0, 5f)] public float fallingGravityScale = 3f;
        [SerializeField, Range(0, 5f)] public float risingGravityScale = 1.7f;
        [Header("Jump")]
        [SerializeField, Range(0f, 10f)] public float jumpHeight = 3f;
        [SerializeField, Range(0, 5)] public int maxAirJumps = 1;
        [SerializeField, Range(0f, 1.0f)] public float coyoteTime = 0.2f;
        [SerializeField, Range(0f, 0.5f)] public float jumpBufferTime = 0.2f;
        [SerializeField] public float fallingTimeDeath = 1.0f;
        [SerializeField] public float fallingDeathYLimit = -100.0f;

        [SerializeField] public float fallDeathHeight = 27f;
        //[SerializeField, Range(0f, 10f)] public float jumpSpeed = 20f;              
        [Header("Move")]
        [SerializeField, Range(0f, 100f)] public float maxAcceleration = 35f;
        [SerializeField, Range(0f, 100f)] public float maxAirAcceleration = 20f;
        [SerializeField] public float wallClimbSpeed = 3.0f;
        [Header("Wall Jump/Slide")]
        [SerializeField] public bool wallStick = true;
        [SerializeField][Range(0.0f, 5f)] public float wallSlideMaxSpeed = 0.25f;
        [SerializeField] public Vector2 wallJumpClimb = new(4f, 12f);
        [SerializeField] public Vector2 wallJumpBounce = new(10.7f, 10f);
        [SerializeField] public Vector2 wallJumpLeap = new(14f, 12f);

    }
}