using SkiGame.Model.Core;
using TMPro;
using UnityEngine;

namespace SkiGame.View.UI
{
    public sealed class HUDMoneyView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _label;

        private void Start()
        {
            UpdateLabel(GameContext.Map.Economy.Money);
        }

        private void OnEnable() => GameContext.Map.Economy.OnMoneyChanged += UpdateLabel;

        private void OnDisable() => GameContext.Map.Economy.OnMoneyChanged -= UpdateLabel;

        private void UpdateLabel(int amount)
        {
            _label.text = $"${amount:N0}"; // Format as currency (e.g., $1,000)
        }
    }
}
