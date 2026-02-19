using System;
using Interaction.Rotation;
using UnityEngine;
using Utility;

namespace Gameplay.Cards.Interaction
{
    /// <summary>
    /// Handles front/back face visibility based on rotation.
    /// </summary>
    public sealed class CardFlipper : MonoBehaviour, IRotationNotifiable
    {
        /// <summary>
        /// Invoked when the flip state changes. <c>true</c> if front is visible.
        /// </summary>
        public event Action<bool> OnFlipStateChanged;

        [Header("References")]
        [Tooltip("The front face GameObject of the card.")]
        [SerializeField] private GameObject frontFace;

        [Tooltip("The back face GameObject of the card.")]
        [SerializeField] private GameObject backFace;

        private bool _isBackVisible;
        private bool _initialized;

        private void Start() => OnRotationChanged(transform.rotation);

        public void OnRotationChanged(Quaternion newRotation)
        {
            Vector3 euler = newRotation.eulerAngles;
            float x = RotationUtility.NormalizeAngle(euler.x);
            float y = RotationUtility.NormalizeAngle(euler.y);

            bool showBack = Mathf.Abs(x) > 90f || Mathf.Abs(y) > 90f;

            if (_initialized && showBack == _isBackVisible)
                return;

            _initialized = true;
            _isBackVisible = showBack;
            ApplyFlip();
            OnFlipStateChanged?.Invoke(!_isBackVisible);
        }

        private void ApplyFlip()
        {
            frontFace.SetActive(!_isBackVisible);
            backFace.SetActive(_isBackVisible);
        }
    }
}