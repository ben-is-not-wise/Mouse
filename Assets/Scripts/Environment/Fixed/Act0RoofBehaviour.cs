using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class Act0RoofBehaviour: MonoBehaviour, ICutscene
    {
        [SerializeField] private GameObject phase1;
        [SerializeField] private GameObject phase2;
        [SerializeField] private GameObject phase3;
        [SerializeField] private GameObject phase4;
        [SerializeField] private GameObject phase5;

        public void ExitRoomInteract() => Game.Instance.SetStateAct0Roof();

        public void Play() 
        {
            Phase1();
        }

        public void Stop() 
        {
            DialogManager.Instance.HideDialog();
        }

        private void Phase1()
        {
            Debug.Log("Phase 1");
            Game.Instance.Player.Character.ExecuteCommand(new FacingCommand(0, 1f));
            Game.Instance.Player.Character.SetStateSitting();
            Game.Instance.Player.Character.Animate();
            phase1.SetActive(true);
            phase2.SetActive(false);
            phase3.SetActive(false);
            phase4.SetActive(false);
            phase5.SetActive(false);
            DialogManager.Instance.ShowDialog("intro_roof1", new UnityAction(Phase1DialogOver));
        }

        private void Phase1DialogOver()
        {
            Phase2();
        }

        private void Phase2()
        {
            Debug.Log("Phase 2");
            Game.Instance.Player.Character.SetStateIdle();
            Game.Instance.Player.Character.Animate();
            phase1.SetActive(false);
            phase2.SetActive(true);
            phase3.SetActive(false);
            phase4.SetActive(false);
            phase5.SetActive(false);

            DialogManager.Instance.ShowDialog("intro_roof2", new UnityAction(Phase2DialogOver));
        }

        private void Phase2DialogOver()
        {
            Phase3();   
        }

        private void Phase3()
        {
            Debug.Log("Phase 3");
            phase1.SetActive(false);
            phase2.SetActive(false);
            phase3.SetActive(true);
            phase4.SetActive(false);
            phase5.SetActive(false);
            Game.Instance.Player.Character.SetStateSleeping();
            Game.Instance.Player.Character.Animate();
            DialogManager.Instance.ShowDialog("intro_roof3", new UnityAction(Phase3DialogOver));
        }

        private void Phase3DialogOver()
        {
            Phase4();
        }

        private void Phase4()
        {
            phase1.SetActive(false);
            phase2.SetActive(false);
            phase3.SetActive(false);
            phase4.SetActive(true);
            phase5.SetActive(false);
            Game.Instance.Player.Character.ExecuteCommand(new FacingCommand(0, -1f));
            Game.Instance.Player.Character.SetStateIdle();
            Game.Instance.Player.Character.Animate();
            DialogManager.Instance.ShowDialog("intro_roof4", new UnityAction(Phase4DialogOver));
            Debug.Log("phase 4");
        }

        private void Phase4DialogOver()
        {
            Debug.Log("phase 4 over");
            Phase5();
        }

        private void Phase5()
        {
            Debug.Log("phase 5");

            phase1.SetActive(false);
            phase2.SetActive(false);
            phase3.SetActive(false);
            phase4.SetActive(false);
            phase5.SetActive(true);
            Game.Instance.Player.Character.SetStateIdle();
            Game.Instance.Player.Character.Animate();
            DialogManager.Instance.ShowDialog("intro_roof5", new UnityAction(Phase5DialogOver));
        }

        private void Phase5DialogOver()
        {
            phase1.SetActive(false);
            phase2.SetActive(false);
            phase3.SetActive(false);
            phase4.SetActive(false);
            phase5.SetActive(false);
            Debug.Log("phase 5 over");
            Game.Instance.Player.Reset();
            Game.Instance.SetStateAct0LoadTutorialLevel();
        }
    }
}
