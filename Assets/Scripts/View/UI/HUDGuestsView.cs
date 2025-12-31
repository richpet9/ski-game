using SkiGame.Model.Agents;
using SkiGame.Model.Core;
using TMPro;
using UnityEngine;

namespace SkiGame.View.UI
{
    public class HUDGuestsView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _label;

        private void Start()
        {
            UpdateLabel(GuestManager.GuestCount);
        }

        private void OnEnable() => GuestManager.OnGuestCountChanged += UpdateLabel;

        private void OnDisable() => GuestManager.OnGuestCountChanged -= UpdateLabel;

        private void UpdateLabel(ushort guestCount)
        {
            _label.text = $"{guestCount} guests";
        }
    }
}
