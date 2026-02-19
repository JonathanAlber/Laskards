using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Systems.ObjectPooling.Gameplay;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents a barrier effect applied to a tile on the game board.
    /// </summary>
    public sealed class BarrierTileEffect : TileEffect
    {
        private Barrier _barrierInstance;

        public BarrierTileEffect(Tile target, ETeam creatorTeam, EDurationType type, int duration,
            EffectData effectData)
            : base(target, creatorTeam, type, duration, effectData) { }

        public override bool OccupiesTile => true;

        protected override void OnApply()
        {
            base.OnApply();

            if (_barrierInstance != null)
            {
                CustomLogger.LogWarning("Barrier visual instance already exists on the tile. " +
                                        "Skipping instantiation.", null);
                return;
            }

            _barrierInstance = BarrierPool.Instance.Get();
            _barrierInstance.transform.SetParent(Target.transform, false);
            _barrierInstance.transform.localPosition = Vector3.zero;
            _barrierInstance.InitializeLifetime(RemainingDuration);
        }

        public override void OnDurationChanged()
        {
            base.OnDurationChanged();

            if (_barrierInstance == null)
                return;

            _barrierInstance.UpdateLifetime(RemainingDuration);
        }

        protected override void OnRevert()
        {
            base.OnRevert();

            if (_barrierInstance == null)
                return;

            BarrierPool.Instance.Release(_barrierInstance);
            _barrierInstance = null;
        }
    }
}