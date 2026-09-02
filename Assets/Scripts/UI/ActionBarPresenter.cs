using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    public class ActionBarPresenter : AbstractPresenter
    {
        [Header("Data")]
        //[SerializeField] private CharacterData gameData;
        [SerializeField] private OperatingSystem os;
        [Header("UI")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Slider energySlider;
        [SerializeField] private Text energyText;
        [SerializeField] private Slider preallocatedSlider;
        [SerializeField] private RectTransform attackPanel;
        [SerializeField] private RectTransform gunPanel;
        [SerializeField] private RectTransform meleePanel;
        [SerializeField] private RectTransform grenadePanel;

        [SerializeField] private Text ammoText;

        [SerializeField] private List<Button> buttonList = new List<Button>(6);
        [SerializeField] private List<Image> imageList = new List<Image>(6);


        void Awake()
        {
            os.changeActions += Repaint;
        }

        public override void Repaint()
        {
            RepaintHealth();
            RepaintWeapon();
            RepaintHacks();
            RepaintMomentum();
        }

        private void RepaintWeapon()
        {

            meleePanel.gameObject.SetActive(os.CurrentWeapon.weaponType == WeaponType.Unarmed);
            gunPanel.gameObject.SetActive(os.CurrentWeapon.weaponType == WeaponType.Gun);
            grenadePanel.gameObject.SetActive(os.CurrentWeapon.weaponType == WeaponType.Grenade);

            if (os.CurrentWeapon.weaponType == WeaponType.Unarmed)
            {
                
            }

            

            if (os.CurrentWeapon.weaponType == WeaponType.Gun)
            {
                ammoText.text = os.Ammo.ToString();
            }

            if(os.CurrentWeapon.weaponType == WeaponType.Grenade)
            {

            }
        }

        private void RepaintHealth()
        {
            healthSlider.value = os.Health;
            healthText.text = os.Health.ToString("N0");
        }

        private void RepaintHacks()
        {
            if (!os.Enabled)
            {
                for (int i = 0; i < buttonList.Count; i++)
                {
                    buttonList[i].gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < buttonList.Count; i++)
                {
                    if (os.ActiveHacks.Count > i)
                    {
                        Hack hack = os.ActiveHacks[i];
                        var text = buttonList[i].gameObject.GetComponentInChildren<Text>();
                        if (text != null)
                        {
                            text.text = hack.shortName ?? hack.name;
                        }
                        else
                        {
                            Debug.LogWarning("Action button has no text component");
                        }

                        var img = imageList[i];
                        if (img != null)
                        {
                            img.sprite = hack.buttonIcon;
                        }
                        else
                        {
                            Debug.LogWarning("Action button has no image component");
                        }
                    }
                }
            }
        }

        private void RepaintMomentum()
        {
            if (!os.Enabled)
            {
                energySlider.value = 0;
                preallocatedSlider.value = 0;
                preallocatedSlider.gameObject.SetActive(false);
                energySlider.gameObject.SetActive(false);
                energyText.gameObject.SetActive(false);
                energyText.text = "";
                return;
            }

            preallocatedSlider.gameObject.SetActive(true);
            energySlider.gameObject.SetActive(true);
            energyText.gameObject.SetActive(true);
            preallocatedSlider.maxValue = os.MaxMomentum;
            preallocatedSlider.value = os.PreallocatedEnergy;
            energySlider.maxValue = os.MaxMomentum;
            energySlider.value = os.Momentum;
            energyText.text = (os.Momentum * 10).ToString("N0");
            
        }

        public void Action1Click()
        {
            os.Trigger(0);
        }

        public void Action2Click()
        {
            os.Trigger(1);
        }
        public void Action3Click()
        {
            os.Trigger(2);
        }
        public void Action4Click()
        {
            os.Trigger(3);
        }
        public void Action5Click()
        {
            os.Trigger(4);
        }
        public void Action6Click()
        {
            os.Trigger(5);
        }
    }
}
