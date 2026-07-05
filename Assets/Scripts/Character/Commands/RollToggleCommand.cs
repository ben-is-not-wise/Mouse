#nullable enable

namespace HackedDesign
{
    public class RollToggleCommand: ICharacterCommand
    {
        private bool flag;

        public RollToggleCommand(bool flag)
        {
            this.flag = flag;
        }
        public void Execute(CharController controller) => controller.SetCanRoll(flag);
    }
}