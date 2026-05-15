using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class IntermissionBehaviour: MonoBehaviour, ICutscene
    {

        [SerializeField] private Transform anarchist;


        public void Play() { }

        public void Stop() { }

        public void HotDogManInteract()
        {

            //Game.Instance.SetStateIntermission();
        }

        public void Intro1DialogOver()
        {
            anarchist.position = anarchist.position - (Vector3.left * 2);
        }

        public void AnarchistInteract()
        {
            Game.Instance.SetStateMissionSelect();
        }
    }
}
