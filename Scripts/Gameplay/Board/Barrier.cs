using UnityEngine;
using System.Collections.Generic;
using Systems.ObjectPooling.Gameplay;
using Systems.Tooltip;
using Systems.Tweening.Components.System;

namespace Gameplay.Board
{
    /// <summary>
    /// Represents a barrier on the game board.
    /// </summary>
    public class Barrier : MonoBehaviour
    {
        [SerializeField] private Transform lifetimeImageParent;

        [Header("Tooltip")]
        [Tooltip("The format for the tooltip text. {0} will be replaced with the remaining duration." +
                 " {1} will be replaced with 's' if duration larger than 1.")]
        [SerializeField] private string tooltipTextFormat = "The Barrier Blocks the Tile for {0} Turn{1}.";
        [SerializeField] private TooltipTrigger tooltipTrigger;

        private readonly List<TweenGroup> _lifetimeImages = new();

        private int _aliveCount;

        /// <summary>
        /// Initializes the lifetime images.
        /// </summary>
        /// <param name="duration">The duration to represent with lifetime images.</param>
        public void InitializeLifetime(int duration)
        {
            ReleaseAll();

            int target = Mathf.Max(0, duration - 1);

            for (int i = 0; i < target; i++)
            {
                TweenGroup img = BarrierLifetimeImagePool.Instance.Get();
                img.transform.SetParent(lifetimeImageParent, false);

                _lifetimeImages.Add(img);
            }

            SetAliveCount(target);
        }

        /// <summary>
        /// Updates the lifetime images to match the new duration.
        /// </summary>
        /// <param name="newDuration">The new duration to represent with lifetime images.</param>
        public void UpdateLifetime(int newDuration)
        {
            int target = Mathf.Max(0, newDuration - 1);

            while (_lifetimeImages.Count < target)
            {
                TweenGroup img = BarrierLifetimeImagePool.Instance.Get();
                img.transform.SetParent(lifetimeImageParent, false);
                _lifetimeImages.Add(img);
            }

            for (int i = _aliveCount - 1; i >= target; i--)
                _lifetimeImages[i].Play();

            SetAliveCount(target);
        }

        /// <summary>
        /// Releases all lifetime images back to the pool.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (TweenGroup image in _lifetimeImages)
                BarrierLifetimeImagePool.Instance.Release(image);

            _lifetimeImages.Clear();
            SetAliveCount(0);
        }

        private void SetAliveCount(int count)
        {
            _aliveCount = count;
            SetTooltipText(count);
        }

        private void SetTooltipText(int duration)
        {
            duration++;
            string s = duration <= 1 ? string.Empty : "s";
            string text = string.Format(tooltipTextFormat, duration, s);
            tooltipTrigger.SetText(text);
        }
    }
}