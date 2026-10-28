using UnityEngine;

namespace HackedDesign
{
    [RequireComponent(typeof(Collider2D))]
    public class VisibilityZone : MonoBehaviour
    {
        [SerializeField] private bool isShadow = false;
        [SerializeField, Range(0f, 1f)] private float visibility = 1f;

        public bool IsShadow => this.isShadow;
        public float Visibility => this.visibility;
    }
}
