using HackedDesign.UI;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace HackedDesign.UI
{
    public class MissionPresenter : AbstractPresenter
    {
        [SerializeField] UnityEngine.UI.Text seedText;
        [SerializeField] UnityEngine.UI.Text corpText;
        [SerializeField] UnityEngine.UI.Text missionTypeText;
        [SerializeField] UnityEngine.UI.Text descriptionText;
        [SerializeField] RectTransform selectMission;
        [SerializeField] UnityEngine.UI.Button missionButtonPrefab;

        public override void Repaint()
        {
            var mission = Game.Instance.GameData.CurrentMission;
            seedText.text = mission.seed.ToHexString();
            corpText.text = mission.corp;
            missionTypeText.text = mission.missionType.ToString();
            descriptionText.text = mission.missionType.ToString();
        }



        public void SelectClick()
        {
            Game.Instance.SetStateIntermission();
        }

        public void RejectClick()
        {

        }

        public void ContinueClick()
        {
            Game.Instance.SetStateLoadLevel();
            //Game.Instance.SetStateIntermission();
            //Game.Instance.GameData.SelectMission(mission);
        }
    }
}
