// AmmoUI.cs
using UnityEngine;
using TMPro; // Changed from UnityEngine.UI

namespace GMTK.PlatformerToolkit {
    [RequireComponent(typeof(TextMeshProUGUI))] // Changed from Text
    public class AmmoUI : MonoBehaviour {
        private TextMeshProUGUI ammoText; // Changed from Text

        private void Awake() {
            ammoText = GetComponent<TextMeshProUGUI>(); // Changed from Text
        }

        public void UpdateAmmo(int currentAmmo, int maxAmmo) {
            if (ammoText != null) {
                ammoText.text = $"Ammo: {currentAmmo} / {maxAmmo}";
            }
        }
    }
}
