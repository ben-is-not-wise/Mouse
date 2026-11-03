using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Ghost", menuName = "Mouse/OS/Ghost")]
    public class Sandy : Hack
    {
        public override bool RequiresTarget => false;

        public override void Trigger(GameObject target, PlayerController player)
        {
            var results = Physics2D.OverlapCircleAll(player.transform.position, range);

            foreach (var result in results)
            {
                if ((result.CompareTag(Tags.Interactable)  || result.CompareTag(Tags.Player) || result.CompareTag(Tags.Enemy)))
                {
                    if (result.TryGetComponent<Interactable>(out var hl)) 
                    {
                        hl.Ping();
                    }
                }
            }
        }
    }
}
