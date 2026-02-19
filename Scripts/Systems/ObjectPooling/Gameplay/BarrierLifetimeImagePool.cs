using Systems.Tweening.Components.System;

namespace Systems.ObjectPooling.Gameplay
{
    /// <summary>
    /// Global pool for barrier lifetime images.
    /// </summary>
    public class BarrierLifetimeImagePool : BaseObjectPoolManager<TweenGroup, BarrierLifetimeImagePool>
    {
        protected override void ResetInstance(TweenGroup instance)
        {
            base.ResetInstance(instance);

            instance.Stop();
            instance.Reverse();
        }
    }
}