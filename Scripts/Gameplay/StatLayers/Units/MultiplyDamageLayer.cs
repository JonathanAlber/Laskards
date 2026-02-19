using UnityEngine;

namespace Gameplay.StatLayers.Units
{
    /// <summary>
    /// A stat layer that multiplies the unit's damage by a fixed multiplier.
    /// </summary>
    public class MultiplyDamageLayer : IUnitStatLayer
    {
        private readonly float _multiplier;

        public MultiplyDamageLayer(float multiplier) => _multiplier = multiplier;

        public int ModifyDamage(int value) => Mathf.RoundToInt(value * _multiplier);

        public int ModifyMaxHealth(int value) => value;

        public int ModifyMoveCount(int value) => value;
    }
}