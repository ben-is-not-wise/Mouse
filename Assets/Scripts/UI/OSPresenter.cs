using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign.UI
{
    public class OSPresenter : AbstractPresenter
    {
        [Header("Data")]
        //[SerializeField] private CharacterData gameData;
        [SerializeField] private OperatingSystem os;
        [Header("UI")]
        //[SerializeField] private AbstractPresenter inventory;
        [SerializeField] private AbstractPresenter charPanel;
        [SerializeField] private AbstractPresenter invPanel;
        [SerializeField] private AbstractPresenter shopPanel;
        [SerializeField] private AbstractPresenter musicPanel;
        [SerializeField] private AbstractPresenter infoPanel;

        void Awake()
        {
            this.AutoBind(ref charPanel);
            this.AutoBind(ref invPanel);
            this.AutoBind(ref shopPanel);
            this.AutoBind(ref musicPanel);
            this.AutoBind(ref infoPanel);
            os.changeActions += Repaint;
        }

        public override void Repaint()
        {
            /*
            switch (os.CurrentTab)
            {
                case OSTab.Character:
                    invPanel.Hide();
                    shopPanel.Hide();
                    musicPanel.Hide();
                    infoPanel.Hide();
                    charPanel.Show();
                    charPanel.Repaint();
                    break;
                case OSTab.Inventory:
                    shopPanel.Hide();
                    musicPanel.Hide();
                    infoPanel.Hide();
                    charPanel.Hide();
                    invPanel.Show();
                    invPanel.Repaint();
                    break;
                case OSTab.Shop:
                    musicPanel.Hide();
                    infoPanel.Hide();
                    charPanel.Hide();
                    invPanel.Hide();
                    shopPanel.Show();
                    shopPanel.Repaint();
                    break;
                case OSTab.Music:
                    shopPanel.Hide();
                    infoPanel.Hide();
                    charPanel.Hide();
                    invPanel.Hide();
                    musicPanel.Show();
                    musicPanel.Repaint();
                    break;
                case OSTab.Info:
                    shopPanel.Hide();
                    charPanel.Hide();
                    invPanel.Hide();
                    musicPanel.Hide();
                    infoPanel.Show();
                    infoPanel.Repaint();
                    break;
            }*/
        }
/*
        public void CharTabClick()
        {
            os.CurrentTab = OSTab.Character;
        }

        public void InvTabClick()
        {
            os.CurrentTab = OSTab.Inventory;
        }
        public void ShopTabClick()
        {
            os.CurrentTab = OSTab.Shop;

        }

        public void MusicTabClick()
        {
            os.CurrentTab = OSTab.Music;
        }

        public void InfoTabClick()
        {
            os.CurrentTab = OSTab.Info;
        }*/
    }
}
