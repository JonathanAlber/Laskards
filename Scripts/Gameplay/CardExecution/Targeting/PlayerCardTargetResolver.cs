using Gameplay.Board;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Systems.Services;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;
using Utility.Raycasting;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// Mouse-based card target resolver. Uses <see cref="RaycastUtility"/> behind the scenes.
    /// </summary>
    public sealed class PlayerCardTargetResolver : GameServiceBehaviour, ICardTargetResolver
    {
        [Tooltip("Graphic raycaster for the gameplay canvas.")]
        [SerializeField] private GraphicRaycaster graphicRaycaster;

        public bool TryResolveTarget(CardModel cardModel, out IModifiableBase target)
        {
            target = null;
            switch (cardModel)
            {
                case ActionCardModel actionCardModel:
                {
                    return ResolveActionCard(ref target, actionCardModel);
                }
                case UnitCardModel:
                {
                    return ResolveUnitCard(ref target);
                }
                default:
                {
                    CustomLogger.LogWarning($"Encountered unsupported card model type {cardModel.GetType()}.", this);
                    return false;
                }
            }
        }

        private bool ResolveActionCard(ref IModifiableBase target, ActionCardModel actionCardModel)
        {
            switch (actionCardModel.ModifierState.ActionCategory)
            {
                case EActionCategory.Resource:
                case EActionCategory.Player:
                {
                    if (!RaycastUtility.TryGetUIElement(graphicRaycaster, out PlayerActionCardDropZone dropZone))
                        return false;

                    target = dropZone.PlayerController;
                    return true;
                }
                case EActionCategory.Buff:
                case EActionCategory.Tile:
                case EActionCategory.Unit:
                {
                    if (!RaycastUtility.TryGetUIElement(graphicRaycaster, out IModifiableBase modifiableBase))
                        return false;

                    target = modifiableBase;
                    return true;
                }
                case EActionCategory.None:
                default:
                {
                    CustomLogger.LogWarning($"Action card {actionCardModel.Common.DisplayName} has no or unknown action" +
                                            $" category {actionCardModel.ModifierState.ActionCategory}.", this);
                    return false;
                }
            }
        }

        private bool ResolveUnitCard(ref IModifiableBase target)
        {
            if (!RaycastUtility.TryGetUIElement(graphicRaycaster, out Tile tile))
                return false;

            target = tile;
            return true;
        }
    }
}