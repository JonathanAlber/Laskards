using Systems.Tooltip;
using Utility.Logging;

namespace Gameplay.Cards.Effects.Display
{
    /// <summary>
    /// Binds an effect's dynamic tooltip values to its UI description.
    /// Call UpdateTooltip manually when runtime values change.
    /// </summary>
    public class EffectTooltipBinding
    {
        private readonly TooltipTrigger _tooltipTrigger;
        private readonly IEffectTooltipData _dataSource;
        private readonly string _template;

        /// <summary>
        /// Creates a tooltip binding for an effect or modifier.
        /// </summary>
        /// <param name="trigger">UI tooltip trigger.</param>
        /// <param name="rawTemplate">Template text containing {placeholders}.</param>
        /// <param name="source">Effect or runtime-state that supplies values.</param>
        public EffectTooltipBinding(TooltipTrigger trigger, string rawTemplate, IEffectTooltipData source)
        {
            _tooltipTrigger = trigger;
            _template = rawTemplate;
            _dataSource = source;
            _dataSource.OnRemainingDurationChanged += UpdateTooltip;

            UpdateTooltip();
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Dispose() => _dataSource.OnRemainingDurationChanged -= UpdateTooltip;

        private void UpdateTooltip()
        {
            if (!TooltipFormatter.TryFormat(_template, _dataSource.GetTooltipValues(), out string formatted,
                    out string error))
            {
                CustomLogger.LogWarning($"Failed to format tooltip for effect '{_dataSource.DisplayName}.'" +
                                        $"Error: {error}", null);
                return;
            }

            _tooltipTrigger.SetText(formatted);
        }
    }
}