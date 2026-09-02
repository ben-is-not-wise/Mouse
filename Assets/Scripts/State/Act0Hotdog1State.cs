namespace HackedDesign
{
    public class Act0Hotdog1State : AbstractState
    {
        private readonly IPlayerController player;
        private readonly ILevelManager level;
        private readonly IDialogManager dialog;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => false;

        public Act0Hotdog1State(IPlayerController player, ILevelManager level, IDialogManager dialog)
        {
            this.player = player;
            this.level = level;
            this.dialog = dialog;
        }

        public override void Begin()
        {
            level.Clear();
            //level.ShowNamedRoom(NamedLevels.MouseStartingRoom, true, true, player);
            //player.Character.Shadow.enabled = false;
            player.Character.SetStateIdle();
            dialog.ShowDialog("intro_room1", Dialog1End);
        }

        //public override void End() => player.Character.Shadow.enabled = true;

        public void Dialog1End()
        {

        }

        public override void Update() => player.UpdateIdleBehaviour();

        public override void FixedUpdate() => player.FixedUpdateBehaviour();

        public override void LateUpdate() => player.LateUpdateBehaviour();
    }
}
