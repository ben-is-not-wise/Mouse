#nullable enable

namespace HackedDesign
{
    public class RollCommand: ICharacterCommand
    {
        public void Execute(CharController controller)
        {
            if (controller.CanRoll)
            {
                controller.Roll();
            }
        }
    }
}