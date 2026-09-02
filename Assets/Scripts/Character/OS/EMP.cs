using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "EMP", menuName = "Mouse/OS/Emp")]
    public class Emp : Hack
    {
        public override void Trigger(GameObject target, PlayerController player)
        {
            var results = Physics2D.OverlapCircleAll(player.transform.position, player.Character.OperatingSystem.PingRadius);

            foreach (var result in results)
            {
                if ((result.CompareTag(Tags.Interactable) || result.CompareTag(Tags.Player) || result.CompareTag(Tags.Player)))
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
