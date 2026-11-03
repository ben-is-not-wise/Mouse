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
        [SerializeField] private Text integrityText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Slider energySlider;
        [SerializeField] private Text energyText;
        [SerializeField] private Slider preallocatedSlider;
        [SerializeField] private Image visibilityImage;
        [SerializeField] private RectTransform attackPanel;
        [SerializeField] private RectTransform gunPanel;
        [SerializeField] private RectTransform meleePanel;
        [SerializeField] private RectTransform grenadePanel;

        [SerializeField] private Text ammoText;

        void OnEnable()
        {
            Debug.Log("enable");
            os.changeActions += Repaint;
            Repaint();
        }

        void OnDisable()
        {
            os.changeActions -= Repaint;
        }

        public override void Repaint()
        {
            RepaintHealth();
            RepaintWeapon();
            RepaintMomentum();
            RepaintVisibility();
        }

        private void RepaintWeapon()
        {
            var weaponType = os.ActiveActionWeapon?.definition.weaponType;

            //meleePanel.gameObject.SetActive(true);
            gunPanel.gameObject.SetActive(weaponType == WeaponType.Gun);
            grenadePanel.gameObject.SetActive(weaponType == WeaponType.Grenade);

            if (weaponType == WeaponType.Gun)
            {
                ammoText.text = os.Ammo.ToString();
            }
        }

        private void RepaintHealth()
        {
            healthSlider.maxValue = os.MaxHealth;
            healthSlider.value = os.Health;
            healthText.text = os.Health.ToString("N0");
            integrityText.text = os.Health > 50 ? "OK" : os.Health > 20 ? "HURT" : "CRITICAL";
        }

        private void RepaintVisibility()
        {
            var color = visibilityImage.color;
            color.a = os.Visibility;
            visibilityImage.color = color;
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
