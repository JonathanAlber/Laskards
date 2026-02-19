using Gameplay.CardExecution.Targeting.Modifier;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Cards.Modifier.Data.Runtime;
using Gameplay.Movement.AI;
using Utility.Logging;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// AI resolver for action card targets.
    /// </summary>
    public class AiActionCardResolver : AiCardResolver<ActionCardModel>
    {
        private readonly AiTileModifierTargetSelector _tileSelector;
        private readonly AiUnitModiferTargetSelector _unitSelector;

        public AiActionCardResolver(int searchDepth, BossMinimaxSearch minimax) : base(searchDepth, minimax)
        {
            _tileSelector = new AiTileModifierTargetSelector(searchDepth, minimax);
            _unitSelector = new AiUnitModiferTargetSelector(searchDepth, minimax);
        }

        public override bool TryResolveTarget(ActionCardModel actionCardModel, out IModifiableBase target)
        {
            target = null;

            if (actionCardModel == null)
            {
                CustomLogger.LogWarning("Received a null action card model.", null);
                return false;
            }

            ModifierRuntimeState modifierState = actionCardModel.ModifierState;
            if (modifierState == null)
            {
                CustomLogger.LogWarning("Received an action card without modifier state.", null);
                return false;
            }

            switch (modifierState.ActionCategory)
            {
                case EActionCategory.Buff:
                {
                    return _unitSelector.TrySelectTarget(actionCardModel, out target);
                }
                case EActionCategory.Tile:
                {
                    return _tileSelector.TrySelectTarget(actionCardModel, out target);
                }
                case EActionCategory.None:
                case EActionCategory.Unit:
                case EActionCategory.Player:
                case EActionCategory.Resource:
                default:
                {
                    CustomLogger.LogWarning($"Unhandled action category '{actionCardModel.ModifierState.ActionCategory}'" +
                                            " in AI action card resolver.", null);
                    return false;
                }
            }
        }
    }
}