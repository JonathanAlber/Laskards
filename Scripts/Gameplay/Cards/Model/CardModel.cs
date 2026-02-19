using System;
using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Data.States;
using Gameplay.StatLayers.Cards;
using Utility.Logging;

namespace Gameplay.Cards.Model
{
    /// <summary>
    /// Represents a <see cref="CardController"/> in the game with its associated definition.
    /// </summary>
    public class CardModel : IStatLayerHost<ICommonCardStatLayer>
    {
        /// <summary>
        /// Invoked whenever any common data changes.
        /// </summary>
        public event Action OnCommonDataChanged;

        /// <summary>
        /// The common state of this card.
        /// </summary>
        public CardCommonState Common { get; }

        private readonly List<ICommonCardStatLayer> _commonLayers;

        IReadOnlyList<ICommonCardStatLayer> IStatLayerHost<ICommonCardStatLayer>.Layers => _commonLayers;

        private readonly ECardTeamAffiliation _teamAffiliation;

        protected CardModel(CardDefinition definition)
        {
            Common = definition.Common.Clone();
            _commonLayers = new List<ICommonCardStatLayer>();
            _teamAffiliation = definition.TeamAffiliation;
        }

        /// <summary>
        /// Applies all common stat layers to compute the final energy cost.
        /// </summary>
        public int GetFinalEnergyCost()
        {
            int result = Common.EnergyCost;

            foreach (ICommonCardStatLayer layer in _commonLayers)
                result = layer.ModifyEnergyCost(result);

            return result;
        }

        /// <summary>
        /// Determines if the card can be played by the specified player.
        /// </summary>
        /// <param name="player">The player attempting to play the card.</param>
        /// <param name="failReason">The reason for failure, if any.</param>
        /// <returns><c>true</c> if the card can be played; otherwise, <c>false</c>.</returns>
        public bool CanBePlayedBy(ICardPlayer player, out string failReason)
        {
            return ValidateCardPlayerTeamAffiliation(player, out failReason);
        }

        private bool ValidateCardPlayerTeamAffiliation(ICardPlayer player, out string failReason)
        {
            switch (player.Team)
            {
                case ETeam.Player:
                    if (_teamAffiliation is not ECardTeamAffiliation.Player and not ECardTeamAffiliation.Both)
                    {
                        failReason = $"The card {Common.DisplayName} can't be played by the player." +
                                     " Please review the ScriptableObjects.";
                        return false;
                    }
                    break;
                case ETeam.Boss:
                    if (_teamAffiliation is not ECardTeamAffiliation.Boss and not ECardTeamAffiliation.Both)
                    {
                        failReason = $"The card {Common.DisplayName} can't be played by the boss." +
                                     " Please review the ScriptableObjects.";
                        return false;
                    }
                    break;
                default:
                    CustomLogger.LogWarning($"Tried to play a unit card with an unknown team: {player.Team}.", null);
                    break;
            }

            failReason = null;
            return true;
        }

        void IStatLayerHost<ICommonCardStatLayer>.AddLayer(ICommonCardStatLayer layer)
        {
            _commonLayers.Add(layer);
            OnCommonDataChanged?.Invoke();
        }

        void IStatLayerHost<ICommonCardStatLayer>.RemoveLayer(ICommonCardStatLayer layer)
        {
            _commonLayers.Remove(layer);
            OnCommonDataChanged?.Invoke();
        }

        void IStatLayerHost<ICommonCardStatLayer>.ClearLayers()
        {
            _commonLayers.Clear();
            OnCommonDataChanged?.Invoke();
        }
    }
}