using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign.UI
{
    public class InventoryPresenter : AbstractPresenter
    {
        [Header("Data")]
        //[SerializeField] private CharacterData gameData;
        [SerializeField] private OperatingSystem os;
        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Text kineticText;
        [SerializeField] private UnityEngine.UI.Text digitalText;
        [SerializeField] private UnityEngine.UI.Text stealthText;
        [SerializeField] private UnityEngine.UI.Text weaponDesc;
        public override void Repaint()
        {
            kineticText.text = os.KineticLevel.ToString();
            digitalText.text = os.DigitalLevel.ToString();
            stealthText.text = os.GhostLevel.ToString();
            weaponDesc.text = os.CurrentWeapon.longDescription;
        }
    }
}
