using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.CardExecution.Targeting;
using Gameplay.Cards.Model;
using Gameplay.Units;
using Utility.Logging;
using UnityEngine;

namespace Gameplay.Boss
{
    /// <summary>
    /// Executes boss card plays using the game's card executor architecture.
    /// </summary>
    public sealed class BossPlayExecutor
    {
        /// <summary>
        /// Event invoked when the boss starts thinking.
        /// </summary>
        public static event Action OnBossThinkingStarted;

        /// <summary>
        /// Event invoked when the boss has finished thinking.
        /// </summary>
        public static event Action OnBossThinkingEnded;

        private readonly ICardPlayer _boss;
        private readonly ICardTargetResolver _resolver;

        public BossPlayExecutor(ICardPlayer boss, ICardTargetResolver resolver)
        {
            _boss = boss;
            _resolver = resolver;
        }

        /// <summary>
        /// Plays multiple card models immediately without delay.
        /// </summary>
        public void Execute(List<CardModel> modelsToPlay)
        {
            foreach (CardModel model in modelsToPlay)
                Execute(model);
        }

        /// <summary>
        /// Plays multiple card models with a small random delay between each card.
        /// </summary>
        /// <param name="modelsToPlay">The list of card models to play.</param>
        /// <param name="delayProvider">Function returning delay in seconds between each card.</param>
        public IEnumerator ExecuteWithDelay(List<CardModel> modelsToPlay, Func<float> delayProvider)
        {
            if (modelsToPlay == null)
            {
                CustomLogger.LogWarning("Received null list of card models to play.", null);
                yield break;
            }

            OnBossThinkingStarted?.Invoke();

            foreach (CardModel model in modelsToPlay)
            {
                float delay = delayProvider();
                if (delay > 0f)
                    yield return new WaitForSeconds(delay);

                Execute(model);
            }

            OnBossThinkingEnded?.Invoke();
        }

        private void Execute(CardModel model)
        {
            switch (model)
            {
                case UnitCardModel unit:
                {
                    if (!UnitCardExecutor.TryExecute(unit, _boss, _resolver, out string fail, out UnitController _))
                        CustomLogger.Log("Boss failed to execute unit card: " + fail, null);

                    return;
                }
                case ActionCardModel action:
                {
                    if (!ActionCardExecutor.TryExecute(action, _boss, _resolver, out string fail))
                        CustomLogger.Log("Boss failed to execute action card: " + fail, null);

                    return;
                }
                default:
                    CustomLogger.LogWarning("Boss attempted to play unsupported card model.", null);
                    break;
            }
        }
    }
}