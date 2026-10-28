using UnityEngine;


namespace HackedDesign
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private Transform exterior;
        [SerializeField] private Transform interior;

        void Awake() => SetExternal();

        public void SetExternal()
        {
            if (interior.OrNull() != null)
            {
                interior.gameObject.SetActive(false);
            }

            if (exterior.OrNull() != null)
            {
                exterior.gameObject.SetActive(true);
            }
            EnemyManager.Instance.SetOutside(true);
        }

        public void SetInternal()
        {
            if (exterior.OrNull() != null)
            {
                exterior.gameObject.SetActive(false);
            }

            if (interior.OrNull() != null)
            {
                interior.gameObject.SetActive(true);
            }
            EnemyManager.Instance.SetOutside(false);
        }

        public EnemySpawn GetSpawns() => gameObject.GetComponentInChildren<EnemySpawn>(true);
    }
}
