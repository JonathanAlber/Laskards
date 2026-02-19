using System.Collections.Generic;
using Gameplay.Board.Data;
using Gameplay.Cards.Data;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Boss.TileSetup
{
    /// <summary>
    /// ScriptableObject defining the boss's opening setup on the back row of the board.
    /// For each of the tiles on the boss back row (left to right), a unit
    /// can be placed and multiple Buff action cards can be applied to it.
    /// </summary>
    [CreateAssetMenu(fileName = "BossOpeningSetup", menuName = "ScriptableObjects/Boss/Boss Opening Setup")]
    public sealed class BossOpeningSetupDefinition : ScriptableObject
    {
        private const int ExpectedColumns = 8;

        [field: Tooltip("Delay in seconds between placing each unit/buff during the opening setup.")]
        [field: SerializeField] public float DelayBetweenMoves { get; private set; } = 0.35f;

        [field: Tooltip("One entry per column on the boss back row (left to right). Must contain exactly 8 entries.")]
        [field: SerializeField] public List<BossOpeningTileSetup> BackRowEntries { get; private set; }
            = new(ExpectedColumns);

        private void OnValidate() => ValidateInternal(null, false);

        /// <summary>
        /// Validates the configuration of this asset, logging warnings if something looks wrong.
        /// Call this from your own Awake/Start where appropriate.
        /// </summary>
        /// <param name="board">Optional board configuration to cross-check column count.</param>
        /// <returns><c>true</c> if the setup looks valid; otherwise, <c>false</c>.</returns>
        public bool Validate(BoardConfiguration board) => ValidateInternal(board, true);

        private bool ValidateInternal(BoardConfiguration board, bool validateBoard)
        {
            bool valid = true;

            if (validateBoard)
            {
                if (board == null)
                {
                    CustomLogger.LogWarning("Boss opening setup validation received null board configuration.", this);
                    valid = false;
                }
                else if (board.Columns != ExpectedColumns)
                {
                    CustomLogger.LogWarning($"Boss opening setup expects {ExpectedColumns} columns," +
                                            $" but board configuration '{board.name}' has {board.Columns}.", this);
                    valid = false;
                }
            }

            if (BackRowEntries == null)
            {
                CustomLogger.LogWarning("Back row entries list is null on boss opening setup.", this);
                BackRowEntries = new List<BossOpeningTileSetup>(ExpectedColumns);
                valid = false;
            }

            if (BackRowEntries.Count != ExpectedColumns)
            {
                CustomLogger.LogWarning($"Boss opening setup must contain exactly {ExpectedColumns} back row entries," +
                                        $" but has {BackRowEntries.Count}.", this);

                // Make sure we always have exactly the right amount of entries
                // so the designer clearly sees one slot per tile.
                while (BackRowEntries.Count < ExpectedColumns)
                    BackRowEntries.Add(new BossOpeningTileSetup());

                if (BackRowEntries.Count > ExpectedColumns)
                    BackRowEntries.RemoveRange(ExpectedColumns, BackRowEntries.Count - ExpectedColumns);

                valid = false;
            }

            for (int i = 0; i < BackRowEntries.Count; i++)
            {
                BossOpeningTileSetup entry = BackRowEntries[i];
                if (entry == null)
                {
                    BackRowEntries[i] = new BossOpeningTileSetup();
                    CustomLogger.LogWarning($"Back row entry at column {i} was null and has been reset.", this);
                    valid = false;
                    continue;
                }

                // Unit required if there are buffs
                if (entry.UnitCard == null && entry.BuffActions is { Count: > 0 })
                {
                    CustomLogger.LogWarning($"Back row entry of definition {name} at column {i} defines buff action " +
                                            "cards but no unit card. Buffs will be ignored at runtime.", this);
                    valid = false;
                }

                if (entry.BuffActions == null)
                    continue;

                for (int j = entry.BuffActions.Count - 1; j >= 0; j--)
                {
                    ActionCardDefinition action = entry.BuffActions[j];
                    if (action == null)
                        continue;

                    if (action.Modifier == null)
                    {
                        CustomLogger.LogWarning(
                            $"Action card '{action.name}' in back row entry {i} has no modifier assigned.",
                            this);
                        valid = false;
                        continue;
                    }

                    if (action.Modifier.ActionCategory == EActionCategory.Buff)
                        continue;

                    CustomLogger.LogWarning($"Action card '{action.name}' in back row entry {i} has ActionCategory" +
                                            $" '{action.Modifier.ActionCategory}', expected '{EActionCategory.Buff}'" +
                                            ". It will be ignored at runtime.", this);
                    valid = false;
                }
            }

            return valid;
        }
    }
}