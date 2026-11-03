#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    /// <summary>
    /// HUD carousel for the action bar: previous, selected and next slot are always shown, the
    /// neighbours smaller and dimmer (size them in the layout; this only sets their alpha). Under it
    /// one text line, e.g. "BREAKER  1 MOM", with the button prompt (LT) on its own label under the icon. The selected icon gets a stepped cooldown fill and
    /// the line turns pink when Mouse is short on momentum. Ping sits apart as a fixed pip, because
    /// it has its own button.
    /// Put this on the HUD, wire the references, and it repaints itself.
    /// </summary>
    public class ActionCarousel : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private OperatingSystem? os;

        [Header("Carousel")]
        [SerializeField] private Image? prevIcon;
        [SerializeField] private Image? selectedIcon;
        [SerializeField] private Image? nextIcon;
        [Tooltip("Optional. Splits wider for a moment when the selection changes.")]
        [SerializeField] private ChromaticSplit? selectedSplit;
        [Tooltip("Image Type = Filled. Covers the selected icon while it cools down.")]
        [SerializeField] private Image? cooldownFill;
        [SerializeField] private Text? label;
        [Tooltip("Button prompt shown under the selected icon, e.g. LT.")]
        [SerializeField] private Text? firePromptLabel;
        [SerializeField, Range(0f, 1f)] private float neighbourAlpha = 0.35f;

        [Header("Ping pip")]
        [SerializeField] private Image? pingIcon;
        [SerializeField] private Image? pingCooldownFill;
        [SerializeField] private Text? pingLabel;
        [Tooltip("Button prompt shown under the Ping icon, e.g. X.")]
        [SerializeField] private Text? pingPromptLabel;
        [SerializeField] private string pingPrompt = "X";

        [Header("Style")]
        [SerializeField] private string firePrompt = "LT";
        [SerializeField, Min(1)] private int cooldownSteps = 8;
        [SerializeField] private Color readyColour = Color.white;
        [SerializeField] private Color shortColour = new(1f, 0.31f, 0.639f, 1f); // pink: not enough momentum
        [SerializeField] private Color coolingColour = new(1f, 1f, 1f, 0.5f);

        private int lastSlot = -1;

        void OnEnable()
        {
            if (os != null)
            {
                os.changeActions += Repaint;
            }
            Repaint();
        }

        void OnDisable()
        {
            if (os != null)
            {
                os.changeActions -= Repaint;
            }
        }

        // Cooldown and momentum change every frame, so the cheap parts update here.
        void Update()
        {
            if (os == null)
            {
                return;
            }

            RepaintSelectedState();
            RepaintPing();
        }

        public void Repaint()
        {
            if (os == null)
            {
                return;
            }

            var slots = os.ActionSlots;
            int current = os.CurrentActionSlot;

            SetIcon(selectedIcon, IconFor(os.ActiveAction), 1f);
            SetIcon(prevIcon, IconFor(Neighbour(slots, current, -1)), neighbourAlpha);
            SetIcon(nextIcon, IconFor(Neighbour(slots, current, 1)), neighbourAlpha);

            if (current != lastSlot)
            {
                if (lastSlot >= 0 && selectedSplit != null)
                {
                    selectedSplit.Glitch();
                }
                lastSlot = current;
            }

            RepaintSelectedState();
            RepaintPing();
        }

        private void RepaintSelectedState()
        {
            var action = os!.ActiveAction;
            float cooldown = 0f;
            bool shortOfMomentum = false;
            string text = "";

            switch (action)
            {
                case HackInstance hack when hack.definition != null:
                    cooldown = CooldownLeft(hack);
                    shortOfMomentum = os.Momentum < hack.definition.puUsage;
                    text = $"{NameFor(hack.definition).ToUpperInvariant()}  {hack.definition.puUsage:0.#} MOM";
                    break;
                case WeaponInstance weapon when weapon.definition != null:
                    text = weapon.definition.weaponType == WeaponType.Gun || weapon.definition.weaponType == WeaponType.Grenade
                        ? $"{NameFor(weapon.definition).ToUpperInvariant()}  x{os.Ammo}"
                        : NameFor(weapon.definition).ToUpperInvariant();
                    break;
            }

            SetFill(cooldownFill, cooldown);

            if (firePromptLabel != null)
            {
                firePromptLabel.text = action != null ? firePrompt : "";
            }

            if (label != null)
            {
                label.text = text;
                label.color = shortOfMomentum ? shortColour : cooldown > 0f ? coolingColour : readyColour;
            }
        }

        private void RepaintPing()
        {
            var ping = os!.PingHack;
            bool has = ping?.definition != null;

            if (pingIcon != null)
            {
                pingIcon.gameObject.SetActive(has);
                if (has)
                {
                    pingIcon.sprite = IconFor(ping);
                }
            }

            if (pingLabel != null)
            {
                pingLabel.gameObject.SetActive(has);
                if (has)
                {
                    pingLabel.text = NameFor(ping!.definition).ToUpperInvariant();
                    pingLabel.color = os.Momentum < ping.definition.puUsage ? shortColour : readyColour;
                }
            }

            if (pingPromptLabel != null)
            {
                pingPromptLabel.gameObject.SetActive(has);
                pingPromptLabel.text = pingPrompt;
            }

            SetFill(pingCooldownFill, has ? CooldownLeft(ping!) : 0f);
        }

        // 1 = just fired, 0 = ready. Stepped so the fill moves in whole chunks.
        private float CooldownLeft(HackInstance hack)
        {
            float total = hack.definition.puTime;
            if (total <= 0f)
            {
                return 0f;
            }
            float left = Mathf.Clamp01((hack.readyTime - Time.time) / total);
            return Mathf.Ceil(left * cooldownSteps) / cooldownSteps;
        }

        private static ItemInstance? Neighbour(System.Collections.Generic.IReadOnlyList<ItemInstance> slots, int start, int direction)
        {
            int count = slots.Count;
            for (int step = 1; step < count; step++)
            {
                int index = ((start + direction * step) % count + count) % count;
                if (slots[index] != null)
                {
                    return index == start ? null : slots[index];
                }
            }
            return null;
        }

        private static Sprite? IconFor(ItemInstance? item) => item switch
        {
            HackInstance h when h.definition != null => h.definition.buttonIcon != null ? h.definition.buttonIcon : h.definition.icon,
            WeaponInstance w when w.definition != null => w.definition.icon,
            _ => null
        };

        private static string NameFor(Item item)
        {
            if (item is Hack hack && !string.IsNullOrEmpty(hack.shortName))
            {
                return hack.shortName;
            }
            return string.IsNullOrEmpty(item.displayName) ? item.name : item.displayName;
        }

        private static void SetIcon(Image? image, Sprite? sprite, float alpha)
        {
            if (image == null)
            {
                return;
            }

            image.enabled = sprite != null;
            image.sprite = sprite;
            var c = image.color;
            c.a = alpha;
            image.color = c;
        }

        private static void SetFill(Image? image, float amount)
        {
            if (image == null)
            {
                return;
            }

            image.enabled = amount > 0f;
            image.fillAmount = amount;
        }
    }
}
