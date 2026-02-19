using System;
using System.Collections.Generic;
using Gameplay.Cards.Data;
using Gameplay.Cards.Data.Properties;
using UnityEngine;

namespace Gameplay.Boss.TileSetup
{
    /// <summary>
    /// Per-tile setup for the boss opening on the back row.
    /// One optional unit card and a list of buff action cards that will be applied to that unit.
    /// </summary>
    [Serializable]
    public sealed class BossOpeningTileSetup
    {
        [field: Tooltip("Unit card that will be spawned on this tile in the boss back row. Leave empty for no unit.")]
        [field: SerializeField, BossUnitCard] public UnitCardDefinition UnitCard { get; private set; }

        [field: Tooltip("Action cards that will immediately be played on the spawned unit on this tile." +
                        " Only card of the Buff category will be used.")]
        [field: SerializeField, BossActionCard] public List<ActionCardDefinition> BuffActions { get; private set; } = new();
    }
}