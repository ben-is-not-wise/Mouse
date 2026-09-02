using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class IntermissionBehaviour : MonoBehaviour, ICutscene
    {
        private IGame game;

        [SerializeField] private Transform anarchist;


        public void Play(IGame game)
        {
            this.game = game;
            Phase1();
        }

        public void Stop(IGame game) { }

        public void HotDogManInteract()
        {
        }

        public void Intro1DialogOver()
        {
            anarchist.position = anarchist.position - (Vector3.left * 2);
        }

        public void AnarchistInteract()
        {
            game.SetStateMissionSelect();
        }

        private void Phase1()
        {

        }
    }
}
