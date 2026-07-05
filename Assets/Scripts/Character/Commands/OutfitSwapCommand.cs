#nullable enable

namespace HackedDesign
{
    public class OutfitSwapCommand : ICharacterCommand
    {
        private readonly string name;
        public OutfitSwapCommand(string name) => this.name = name;
        public void Execute(CharController controller) => controller.SetOutfit(name);
    }
}
