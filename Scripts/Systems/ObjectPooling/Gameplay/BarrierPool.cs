using Gameplay.Board;

namespace Systems.ObjectPooling.Gameplay
{
    /// <summary>
    /// Global pool for barrier objects.
    /// </summary>
    public sealed class BarrierPool : BaseObjectPoolManager<Barrier, BarrierPool>
    {
        protected override void ResetInstance(Barrier instance)
        {
            base.ResetInstance(instance);

            instance.ReleaseAll();
        }
    }
}