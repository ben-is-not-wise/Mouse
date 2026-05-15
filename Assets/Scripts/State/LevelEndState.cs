using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackedDesign
{
    public class LevelEndState : IState
    {
        private readonly IPlayerController player;

        public bool PlayerActionAllowed => false;

        public bool Battle => false;

        public LevelEndState(IPlayerController player)
        {
            this.player = player;
        }

        public void Begin()
        {
            player.Stop();
            player.Character.SetStateIdle();
        }

        public void End()
        {
            
        }

        public void FixedUpdate()
        {
            
        }

        public void LateUpdate()
        {
            
        }

        public void Menu()
        {
            
        }

        public void Select()
        {
            
        }

        public void Update()
        {
            player.UpdateIdleBehaviour();
        }
    }
}
