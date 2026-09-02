
namespace HackedDesign
{
    public interface IState
    {
        /// <summary>
        /// Called by Game when a state change occurs, allowing the new state to perform any initialisation
        /// </summary>
        void Begin();
        /// <summary>
        /// Called by Game within the Unity update loop
        /// </summary>
        void Update();

        /// <summary>
        /// Called by Game within the Unity LateUpdate loop
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// Called by Game within the Unity FixedUpdate loop
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// Called by Game just before a state change occurs, allowing the state to clean itself up.
        /// </summary>
        void End();

        /// <summary>
        /// Called by Game when this state is temporarily paused (e.g. by PausedState), without ending it
        /// </summary>
        void Suspend();

        /// <summary>
        /// Called by Game when this state resumes after being suspended, without re-running Begin()
        /// </summary>
        void Resume();

        /// <summary>
        /// Allows a state to handle if the 'Menu' controller button is pressed by the player
        /// </summary>
        void Pause();

        /// <summary>
        /// Allows a state to handle if the 'Select' controller button is pressed by the player
        /// </summary>
        void Select();
        
        /// <summary>
        /// Is the player allowed to do anything in this state
        /// </summary>
        bool PlayerActionAllowed { get; }

        /// <summary>
        /// Is the player considered in battle in this state
        /// </summary>
        bool Battle { get; }

        /// <summary>
        /// Does reaching the level exit in this state complete the level
        /// </summary>
        bool LevelComplete { get; }
    }
}