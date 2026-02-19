namespace VFX.Data
{
    /// <summary>
    /// Enumeration of different particle effect types.
    /// </summary>
    public enum EParticleEffectType : byte
    {
        None = 0,

        /// <summary>
        /// Enemy unit attacks the groundline and
        /// deals damage to the player
        /// </summary>
        BossAttack = 1,

        /// <summary>
        /// Enemy unit effect reaches the player
        /// and now a hit effect should be played
        /// </summary>
        BossAttackHit = 2,

        /// <summary>
        /// Player unit reached the groundline
        /// and deals damage to the boss
        /// </summary>
        UnitAttack = 3,

        /// <summary>
        /// Player unit attack hit the boss and
        /// now a hit effect should be played
        /// </summary>
        UnitAttackHit = 4,

        /// <summary>
        /// Player spawned an effect of type 'player'
        /// that is added to the effect list
        /// </summary>
        EffectSpawned = 5,

        /// <summary>
        /// Player effect dealt damage to the boss
        /// </summary>
        EffectDamage = 6,

        /// <summary>
        /// Player effect hit the boss and
        /// now a hit effect should be played
        /// </summary>
        EffectDamageHit = 7
    }
}