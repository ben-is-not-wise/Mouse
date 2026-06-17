
using UnityEngine;


namespace HackedDesign
{
    public class LevelExit : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(Tags.Player))
            {
                CheckLevelComplete();
            }
        }

        private static void CheckLevelComplete()
        {
            //if (Level.Instance.LevelComplete)
            //{
                Game.Instance.SetStateLevelEnd();
            //}
        }
    }
}
