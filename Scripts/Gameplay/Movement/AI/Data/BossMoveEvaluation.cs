namespace Gameplay.Movement.AI.Data
{
    /// <summary>
    /// Breakdown of evaluation components for AI scoring.
    /// </summary>
    public struct BossMoveEvaluation
    {
        /// <summary>
        /// How much the player's HP contributes to the score.
        /// </summary>
        public float playerHp;

        /// <summary>
        /// How much the boss's units contribute to the score.
        /// </summary>
        public float bossUnits;

        /// <summary>
        /// Number of boss units that contributed to bossUnits.
        /// </summary>
        public int bossUnitCount;

        /// <summary>
        /// Sum of raw boss unit values before applying UnitValueWeight.
        /// This helps debug why bossUnits changed between states.
        /// </summary>
        public float bossUnitRawSum;

        /// <summary>
        /// How much the player's units contribute to the score.
        /// </summary>
        public float playerUnits;

        /// <summary>
        /// How much the boss's position contributes to the score.
        /// </summary>
        public float bossPosition;

        /// <summary>
        /// How much the player's position contributes to the score.
        /// </summary>
        public float playerPosition;

        /// <summary>
        /// How much center bias contributes to the score.
        /// </summary>
        public float centerBias;

        /// <summary>
        /// How much unit lifetime contributes to the score.
        /// </summary>
        public float lifetime;

        /// <summary>
        /// How much player threats contribute to the score.
        /// </summary>
        public float playerThreats;

        /// <summary>
        /// How much spawn threats contribute to the score.
        /// </summary>
        public float spawnThreats;

        /// <summary>
        /// How much tactical danger (attacked boss units) contributes to the score.
        /// </summary>
        public float dangerPenalty;

        /// <summary>
        /// How much relative mobility contributes to the score.
        /// </summary>
        public float mobility;

        /// <summary>
        /// Total evaluation score.
        /// </summary>
        public float Total => playerHp + bossUnits + playerUnits + bossPosition + playerPosition + centerBias
                              + lifetime + playerThreats + spawnThreats + dangerPenalty + mobility;
    }
}