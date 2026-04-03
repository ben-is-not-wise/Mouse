using HackedDesign.UI;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class Targeter : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask laserMask;
        [SerializeField] private float lineInnerRadius = 1f;
        [SerializeField] private float targetRadius = 20f;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private TargetPresenter targetPresenter;

        public UnityAction<Interactable> targetUpdateAction;

        private Interactable currentTarget;
        private ICharacterExecute charExecute;

        private readonly Vector3[] linePoints = new Vector3[2];

        private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[1];

        private readonly AimCommand falseAimCommand = new(false);
        private readonly AimCommand trueAimCommand = new(true);

        void Awake()
        {
            charExecute = GetComponent<ICharacterExecute>();
            targetUpdateAction += targetPresenter.Repaint;
        }

        public bool IsTargetInRange => (pivot.position - currentTarget.transform.position).sqrMagnitude < (Game.Instance.GameSettings.InteractDistance * Game.Instance.GameSettings.InteractDistance);

        public void TriggerInteract()
        {
            if (currentTarget && IsTargetInRange)
            {
                charExecute.ExecuteCommand(new InteractCommand());
                currentTarget.TriggerInteract();
            }
        }

        public void Reset() => ShowTarget(Vector3.zero, false, false);

        public void ShowTarget(Vector3 direction, bool aiming, bool hasPistol)
        {
            int hitCount = Physics2D.RaycastNonAlloc(pivot.position,direction.normalized,hitBuffer,targetRadius,targetMask);

            var hit = hitCount > 0 ? hitBuffer[0] : default;
            UpdateHover(hit);

            if (aiming)
            {
                DrawAimLine(direction, hasPistol);
                AnimateAiming(hasPistol);
            }
            else
            {
                if (lineRenderer.positionCount > 0)
                {
                    lineRenderer.positionCount = 0;
                    AnimateAiming(false);
                }
            }
        }

        private void AnimateAiming(bool flag) => charExecute.ExecuteCommand(flag ? trueAimCommand : falseAimCommand);

        private void DrawAimLine(Vector3 direction, bool hasPistol)
        {
            if (hasPistol)
            {
                lineRenderer.positionCount = 2;

                var startPosition = pivot.position + (direction.normalized * lineInnerRadius);

                int hitCount = Physics2D.RaycastNonAlloc(pivot.position, direction.normalized, hitBuffer, targetRadius, laserMask);

                Vector3 endPosition = hitCount > 0 ? hitBuffer[0].point : startPosition + (direction.normalized * targetRadius);

                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, endPosition);
            }
            else
            {
                if(lineRenderer.positionCount > 0)
                {
                    lineRenderer.positionCount = 0;
                    lineRenderer.enabled = false;
                }
            }
        }

        private void UpdateHover(RaycastHit2D hit)
        {
            if (hit && hit.collider.TryGetComponent(out Interactable newTarget))
            {
                if (currentTarget != newTarget)
                {
                    ClearHighlightable();
                    currentTarget = newTarget;
                    currentTarget.Target(true);
                }
                targetUpdateAction.Invoke(currentTarget);
            }
            else if (currentTarget != null)
            {
                ClearHighlightable();
            }
        }

        private void ClearHighlightable()
        {
            if (currentTarget)
            {
                currentTarget.Target(false);
                currentTarget = null;
                targetUpdateAction.Invoke(null);
            }
        }
    }
}
