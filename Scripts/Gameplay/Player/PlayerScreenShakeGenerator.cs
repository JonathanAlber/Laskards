using Gameplay.Cards;
using Gameplay.ScreenShake;
using Systems.ScreenShake;
using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Generates screen shake effects in response to player actions, such as playing cards.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSourceWrapper))]
    public class PlayerScreenShakeGenerator : BaseScreenShakeGenerator
    {
        private void OnEnable() => PlayerController.OnCardPlayed += HandleCardPlayed;

        private void OnDisable() => PlayerController.OnCardPlayed -= HandleCardPlayed;

        private void HandleCardPlayed(CardController card)
        {
            ImpulseSourceWrapper.GenerateShake(card.transform.position, multiplier);
        }
    }
}