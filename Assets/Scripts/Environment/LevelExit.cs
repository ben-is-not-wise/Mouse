
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
            //FIXME: The current state should locate this object and bind to an event,
            // instead of using this singleton reference
            if (Game.Instance.CurrentState.LevelComplete)
            {
                Game.Instance.SetStateLevelEnd();
            }
        }
    }
}
