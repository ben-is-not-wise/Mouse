using UnityEngine;

namespace HackedDesign
{
    public class Thermoptic: MonoBehaviour
    {
        [SerializeField] new private Renderer renderer;
        [SerializeField] private Material defaultMaterial = null;
        [SerializeField] private Material thermopticMaterial = null;

        [SerializeField] private bool active = false;

        public bool Active { 
            get => active; 
            set { 
                active = value; 
                renderer.material = active ? thermopticMaterial : defaultMaterial; 
            } 
        }

        void Awake() => this.AutoBind(ref renderer);
    }
}
