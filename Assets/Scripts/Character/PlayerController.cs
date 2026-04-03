#nullable enable
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HackedDesign
{
    public interface IPlayerController
    {
        CharController Character { get; set; }
        void Stop();
        void FixedUpdateBehaviour();
        void LateUpdateBehaviour();
        void UpdateBattleBehaviour();
        void UpdateSitBehaviour();
        void Teleport(Vector3 position);
        void UpdateIdleBehaviour();
        void Reset();
    }

    [RequireComponent(typeof(CharController))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Header("References")]
        [field: SerializeField, NotNull] private Game Game { get; set; } = null!;
        [field: SerializeField, NotNull] private Camera MainCamera { get; set; } = null!;
        [field: SerializeField, NotNull] private PlayerInput PlayerInput { get; set; } = null!;
        [field: SerializeField, NotNull] public CharController Character { get; set; } = null!;
        [field: SerializeField, NotNull] private Targeter Targeter { get; set; } = null!;
        [field: SerializeField, NotNull] private Transform AimPivot { get; set; } = null!;

        private const float DefaultTimeScale = 1f;
        private const string GamepadControlMethod = "Gamepad";

        private bool crouchToggle = false;
        private bool timeToggle = false;

        private InputAction? moveAction = null;
        private InputAction? climbAction = null;
        private InputAction? jumpAction = null;
        private InputAction? crouchToggleAction = null;
        private InputAction? walkToggleAction = null;
        private InputAction? crouchAction = null;
        private InputAction? rollAction = null;
        private InputAction? attackAction = null;
        private InputAction? menuAction = null;
        private InputAction? mousePosAction = null;
        private InputAction? lookPosAction = null;
        private InputAction? interactAction = null;
        private InputAction? selectAction = null;
        private InputAction? hackAction = null;
        private InputAction? action1Action = null;

        void Awake()
        {
            Character.Require(nameof(Character));
            PlayerInput.Require(nameof(PlayerInput));
            MainCamera.Require(nameof(MainCamera));
            AimPivot.Require(nameof(AimPivot));
            Targeter.Require(nameof(Targeter));

            Character.DieActions.AddListener(Die);

            BindInputs();
        }

        void OnDestroy()
        {
            selectAction!.performed -= SelectEvent;
            menuAction!.performed -= MenuEvent;
            interactAction!.performed -= InteractEvent;
            action1Action!.performed -= Action1Event;
        }

        private void BindInputs()
        {
            menuAction = PlayerInput.actions["Menu"];
            moveAction = PlayerInput.actions["Move"];
            climbAction = PlayerInput.actions["Climb"];
            jumpAction = PlayerInput.actions["Jump"];
            crouchAction = PlayerInput.actions["Crouch"];
            crouchToggleAction = PlayerInput.actions["Crouch Toggle"];
            walkToggleAction = PlayerInput.actions["Walk Toggle"];
            rollAction = PlayerInput.actions["Roll"];
            attackAction = PlayerInput.actions["Attack"];
            interactAction = PlayerInput.actions["Interact"];
            mousePosAction = PlayerInput.actions["Mouse Position"];
            lookPosAction = PlayerInput.actions["Look"];
            selectAction = PlayerInput.actions["OperatingSystem"];
            hackAction = PlayerInput.actions["Hack"];
            action1Action = PlayerInput.actions["Action 1"];

            selectAction.performed += SelectEvent;
            menuAction.performed += MenuEvent;
            interactAction.performed += InteractEvent;
            action1Action.performed += Action1Event;
        }

        void Start() => Reset();
        public void Reset()
        {
            Stop();
            Character.Reset();
            Targeter.Reset();
            SetStartingWeapon();
        }

        public void Stop() => Character!.ExecuteCommand(new StopCommand());

        public void MenuEvent(InputAction.CallbackContext context) => Game.CurrentState.Menu();

        public void SelectEvent(InputAction.CallbackContext context) => Game.CurrentState.Select();

        public void InteractEvent(InputAction.CallbackContext context) => Targeter.TriggerInteract();

        public void Action1Event(InputAction.CallbackContext context)
        {
            timeToggle = !timeToggle;
            Time.timeScale = timeToggle ? Character.Settings!.TimeSlowSpeed : DefaultTimeScale;
            Character.ExecuteCommand(new GhostToggleCommand());
        }

        public void Die()
        {
            Targeter.ShowTarget(Vector3.zero, false, false);
            Game.SetStateDeath();
        }

        public void UpdateSitBehaviour()
        {
            UpdateAimLine();

            if (attackAction!.triggered)
            {
                Targeter.TriggerInteract();
            }
        }

        public void UpdateIdleBehaviour()
        {
            float movement = moveAction!.ReadValue<float>();
            float climb = climbAction!.ReadValue<float>();

            Vector3 targetPos = GetTargetingPosition();
            //UpdateAimLine();

            Character.ExecuteCommand(new FacingCommand(movement, CalcForward(targetPos)));
            Character.ExecuteCommand(new MoveCommand(movement, climb));
        }

        public void UpdateBattleBehaviour()
        {
            float movement = 0;
            float climb = 0;

            Vector3 targetPos = GetTargetingPosition();

            UpdateCrouchToggle();
            UpdateWalkToggle();
            UpdateHackMode();

            Character.ExecuteCommand(new CrouchCommand(crouchToggle || crouchAction!.IsPressed()));
            Character.OperatingSystem.UpdateBehaviour();

            UpdateAimLine();

            if (Character.IsAnimatingAttack) // If we're playing the attacking animation, don't let the player take another action
            {
                movement = 0;
                climb = 0;
                //Attacking
            }
            else if (attackAction!.triggered)
            {
                movement = 0;
                climb = 0;
                //Character.OperatingSystem.momentum = 0;
                //Character.Attack(targetPos, IsAiming());
                Character.Attack(targetPos, true);
            }
            else if (rollAction!.triggered)
            {
                Character.ExecuteCommand(new RollCommand());
            }
            else
            {
                movement = moveAction!.ReadValue<float>();
                climb = climbAction!.ReadValue<float>();
                //movement = 1;
                //climb = 1;

                if (jumpAction!.triggered)
                {
                    Character.ExecuteCommand(new JumpCommand());
                }

                Character.JumpHoldFlag |= jumpAction!.IsPressed();
            }

            Character.ExecuteCommand(new FacingCommand(movement, CalcForward(targetPos)));
            Character.ExecuteCommand(new MoveCommand(movement, climb));
        }

        private void UpdateHackMode()
        {
            //Game.Instance.HackMode = hackAction.IsPressed();
        }

        public void Teleport(Vector3 position) => transform.position = position;

        private void SetStartingWeapon() => Character.OperatingSystem.SetWeapon(Character.OperatingSystem.GetWeaponSlotByName(Game.Instance.GameSettings.StartPistol ? "357 Magnum" : "Unarmed"));

        private void UpdateCrouchToggle()
        {
            if (crouchToggleAction!.triggered)
            {
                crouchToggle = !crouchToggle;
            }
        }

        private void UpdateWalkToggle()
        {
            if (walkToggleAction!.triggered)
            {
                Character.ExecuteCommand(new WalkToggleCommand());
            }
        }

        public void FixedUpdateBehaviour() => Character.Physics();

        public void LateUpdateBehaviour() => Character.Animate();

        private void UpdateAimLine()
        {
            if (PlayerInput.currentControlScheme == GamepadControlMethod)
            {
                var lookPos = lookPosAction!.ReadValue<Vector2>();

                if (lookPos.magnitude > 0.1f)
                {
                    Vector3 direction = GetGamepadLookDirection(ref lookPos);
                    Targeter.ShowTarget(direction, true, Character.OperatingSystem.CurrentWeapon.weaponType == WeaponType.Gun);

                }
                else
                {
                    Targeter.Reset();
                }
            }
            else
            {
                Vector3 direction = GetMouseLookDirection();
                Targeter.ShowTarget(direction, true /*IsAiming()*/, Character.OperatingSystem.CurrentWeapon.weaponType == WeaponType.Gun);
            }
        }

        private Vector3 GetGamepadLookDirection(ref Vector2 lookPos) => lookPos.normalized;

        private Vector3 GetMouseLookDirection() => (CalcMouseWorldPosition() - AimPivot.position).normalized;

        private Vector3 CalcMouseWorldPosition()
        {
            var mousePos = mousePosAction!.ReadValue<Vector2>();
            var worldPos = MainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            worldPos = new Vector3(worldPos.x, worldPos.y, AimPivot.position.z);
            return worldPos;
        }

        private Vector3 GetTargetingPosition()
        {
            if (PlayerInput.currentControlScheme == GamepadControlMethod)
            {
                var look = lookPosAction!.ReadValue<Vector2>();
                return transform.position + new Vector3(look.x, look.y);
            }
            else
            {
                var mouseScreen = mousePosAction!.ReadValue<Vector2>();
                var screenPoint = new Vector3(mouseScreen.x, mouseScreen.y, MainCamera.transform.position.z);
                return MainCamera.ScreenToWorldPoint(screenPoint);
            }
        }

        private float CalcForward(Vector3 targetPos) => Mathf.Clamp(targetPos.x - transform.position.x, -1, 1);
    }
}