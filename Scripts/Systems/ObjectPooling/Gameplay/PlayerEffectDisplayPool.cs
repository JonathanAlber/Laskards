using Gameplay.Cards.Effects.Display;

namespace Systems.ObjectPooling.Gameplay
{
    /// <summary>
    /// Global pool for effect display visuals.
    /// </summary>
    public class PlayerEffectDisplayPool : BaseObjectPoolManager<PlayerEffectDisplay, PlayerEffectDisplayPool>
    {
        protected override void ResetInstance(PlayerEffectDisplay instance)
        {
            base.ResetInstance(instance);

            instance.ClearEffect();
        }
    }
}