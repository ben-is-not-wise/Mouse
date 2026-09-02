namespace HackedDesign
{
    public abstract class AbstractState : IState
    {
        public virtual void Begin() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }
        public virtual void End() { }
        public virtual void Suspend() { }
        public virtual void Resume() { }
        public virtual void Pause() { }
        public virtual void Select() { }

        public abstract bool PlayerActionAllowed { get; }
        public abstract bool Battle { get; }
        public virtual bool LevelComplete => false;
    }
}
