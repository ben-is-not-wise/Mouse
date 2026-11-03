using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Ping", menuName = "Mouse/OS/Ping")]
    public class Ping : Hack
    {
        [Header("Wave")]
        [SerializeField] private Material waveMaterial;
        [SerializeField] private float waveDuration = 0.6f;
        [SerializeField, SortingLayer] private string waveSortingLayer = "Default";
        [SerializeField] private int waveSortingOrder = 100;

        private readonly List<Interactable> hits = new();

        public override bool RequiresTarget => false;

        public override bool HasOwnButton => true;

        public override void Trigger(GameObject target, PlayerController player)
        {
            var origin = player.transform.position;
            var radius = range;
            var results = Physics2D.OverlapCircleAll(origin, radius);

            hits.Clear();
            foreach (var result in results)
            {
                if ((result.CompareTag(Tags.Interactable) || result.CompareTag(Tags.Player) || result.CompareTag(Tags.Enemy)))
                {
                    if (result.TryGetComponent<Interactable>(out var hl)) 
                    {
                        hits.Add(hl);
                    }
                }
            }

            if (waveMaterial == null)
            {
                foreach (var hl in hits)
                {
                    hl.Ping();
                }
                return;
            }

            PingWave.Spawn(waveMaterial, waveSortingLayer, waveSortingOrder, origin, radius, waveDuration, hits);
        }
    }
}
