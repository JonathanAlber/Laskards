using Gameplay.Cards.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace Gameplay.Cards.View
{
    /// <summary>
    /// View for an <see cref="ActionCardModel"/>.
    /// Binds UI elements to the model's data and updates them on changes.
    /// </summary>
    public sealed class ActionCardView : CardView
    {
        [Header("Action Card View")]
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image effectIcon;

        protected override void Refresh()
        {
            base.Refresh();
            ActionCardModel model = (ActionCardModel)Model;
            descriptionText.text = model.Description;

            Sprite icon = model.ModifierState.EffectData.Icon;
            if (icon == null)
            {
                CustomLogger.LogWarning($"Action card '{name}' has no effect icon set in its modifier data.", this);
                effectIcon.gameObject.SetActive(false);
                return;
            }

            effectIcon.sprite = model.ModifierState.EffectData.Icon;
        }
    }
}