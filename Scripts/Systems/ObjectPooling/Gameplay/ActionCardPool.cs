using Gameplay.Cards;

namespace Systems.ObjectPooling.Gameplay
{
    /// <summary>
    /// Global pool for <see cref="ActionCardController"/> instances.
    /// </summary>
    public class ActionCardPool : BaseObjectPoolManager<ActionCardController, ActionCardPool> { }
}