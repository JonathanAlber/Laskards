using System.Collections.Generic;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// A ScriptableObject that holds a collection of <see cref="CardDefinition"/>
    /// assets that can be used by the boss.
    /// </summary>
    [CreateAssetMenu(fileName = "BossCardDefinitionCollection",
        menuName = "ScriptableObjects/Cards/Boss Card Collection")]
    public class BossCardDefinitionCollection : ScriptableObject
    {
        [field: Tooltip("All cards that the boss can use in the game.")]
        [field: SerializeField] public List<CardDefinition> Cards { get; private set; } = new();

        private void OnValidate()
        {
            if (Cards == null)
                return;

            for (int i = Cards.Count - 1; i >= 0; i--)
            {
                CardDefinition card = Cards[i];
                if (card == null)
                    continue;

                if (card.TeamAffiliation is ECardTeamAffiliation.Boss or ECardTeamAffiliation.Both)
                    continue;

                CustomLogger.LogWarning($"Removed card '{card.name}' because its Team-Affiliation is" +
                                        $" '{card.TeamAffiliation}', not Boss or Both.", this);

                Cards.RemoveAt(i);
            }
        }
    }
}