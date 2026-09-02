using UnityEngine;

namespace HackedDesign
{
    [RequireComponent(typeof(Collider2D))]
    public class Ladder : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
    }
}
