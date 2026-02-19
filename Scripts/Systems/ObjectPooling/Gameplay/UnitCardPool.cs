using Gameplay.Cards;

namespace Systems.ObjectPooling.Gameplay
{
    /// <summary>
    /// Global pool for <see cref="UnitCardController"/> instances.
    /// </summary>
    public class UnitCardPool : BaseObjectPoolManager<UnitCardController, UnitCardPool> { }
}