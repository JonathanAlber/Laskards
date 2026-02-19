using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CardExecution;
using Gameplay.Cards;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Effects.Display;
using Gameplay.Cards.Interaction;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Decks.Controller;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Movement.AI;
using Gameplay.Units;
using Systems.ObjectPooling.Gameplay;
using Systems.Services;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Utility.Logging;
using VFX;

namespace Gameplay.Player
{
    /// <summary>
    /// Manages the player's state, including health, energy, card draws and active effects.
    /// Internally uses a snapshot system to track changes and raise events accordingly.
    /// </summary>
    public class PlayerController : GameServiceBehaviour, IModifiable<PlayerEffect>, IPhaseSubscriber, ICardPlayer
    {
        /// <summary>
        /// Raised when the player tries to spend more energy than they have.
        /// </summary>
        public static event Action OnNotEnoughEnergy;

        /// <summary>
        /// Raised when current HP changes.
        /// The int parameter is the current health value.
        /// </summary>
        public static event Action<int> OnHpChanged;

        /// <summary>
        /// Raised when the player takes damage.
        /// The int parameter is the amount of damage taken.
        /// </summary>
        public static event Action<int> OnPlayerDamaged;

        /// <summary>
        /// Raised when current energy changes.
        /// </summary>
        public static event Action<int> OnEnergyChanged;

        /// <summary>
        /// Raised when the amount of drawable cards changes.
        /// </summary>
        public static event Action<int> OnDrawableCardAmountChanged;

        /// <summary>
        /// Raised when the player plays a card.
        /// </summary>
        public static event Action<CardController> OnCardPlayed;

        /// <summary>
        /// Raised when the expected next-turn energy gain changes.
        /// Passes the total amount of energy that will be gained next turn.
        /// </summary>
        public static event Action<int> OnGainedEnergyNextTurnChanged;

        /// <summary>
        /// Raised when the expected next-turn energy gain changes.
        /// Passes the total amount of energy that will be gained next turn.
        /// </summary>
        public static event Action<int> OnDrawableCardAmountNextTurnChanged;

        /// <summary>
        /// Raised when a new effect is added to the player.
        /// Passes the display instance for the added effect.
        /// </summary>
        public static event Action<EffectDisplay> OnEffectAdded;

        [Tooltip("Definition of player related data to be found in the ScriptableObjects folder.")]
        [SerializeField] private PlayerStateData playerStateData;

        [Tooltip("Parent transform for effect display UI elements.")]
        [SerializeField] private Transform effectDisplayParent;

        public List<PlayerEffect> ActiveEffects { get; } = new();

        public EGamePhase[] SubscribedPhases => new[]
        {
            EGamePhase.PlayerPrePlay,
            EGamePhase.PlayerDraw
        };

        public ETeam Team => ETeam.Player;

        /// <summary>
        /// Current snapshot of the player's state.
        /// </summary>
        public PlayerSnapshot Snapshot { get; private set; }

        private readonly List<UnitController> _bossUnits = new();
        private readonly List<PlayerEffectDisplay> _effectDisplays = new();
        private readonly Dictionary<UnitController, CardController> _playerUnitToCard = new();

        private CardController _lastPlayedCard;
        private ActionCardDeckController _actionDeck;
        private UnitCardDeckController _unitDeck;
        private DiscardPileController _discardPile;

        protected override void Awake()
        {
            base.Awake();

            Snapshot = PlayerSnapshot.FromData(playerStateData);

            if (!ServiceLocator.TryGet(out GameFlowSystem flow))
                return;

            foreach (EGamePhase phase in SubscribedPhases)
                flow.RegisterPhaseSubscriber(phase, this);

            UnitManager.OnBossUnitSpawned += HandleBossUnitSpawned;
            BossAutoMoveController.OnUnitEnteredLastRow += HandleBossUnitEnteredLastRow;
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _actionDeck);
            ServiceLocator.TryGet(out _unitDeck);
            ServiceLocator.TryGet(out _discardPile);

            RaiseHealthChangedEvent();
            OnEnergyChanged?.Invoke(Snapshot.CurrentEnergy);
            OnDrawableCardAmountChanged?.Invoke(Snapshot.CardsToDraw);
            UpdateStatsPreview();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (PlayerEffect effect in ActiveEffects)
                effect.Revert();

            foreach (UnitController unit in _playerUnitToCard.Keys)
                if (unit != null)
                    unit.OnUnitDied -= HandleUnitDied;

            foreach (UnitController bossUnit in _bossUnits)
                if (bossUnit != null)
                    bossUnit.OnUnitDied -= HandleBossUnitDefeated;

            BossAutoMoveController.OnUnitEnteredLastRow -= HandleBossUnitEnteredLastRow;
            UnitManager.OnBossUnitSpawned -= HandleBossUnitSpawned;
        }

        private void HandleBossUnitSpawned(UnitController unitController)
        {
            if (unitController == null)
            {
                CustomLogger.LogWarning("Spawned boss unit controller is null.", this);
                return;
            }

            if (unitController.Team != ETeam.Boss)
            {
                CustomLogger.LogWarning("Spawned unit is not a boss unit.", this);
                return;
            }

            _bossUnits.Add(unitController);
            unitController.OnUnitDied += HandleBossUnitDefeated;
        }

        private void HandleBossUnitEnteredLastRow(UnitController unitController)
        {
            if (unitController.Team != ETeam.Boss)
                return;

            if (!ServiceLocator.TryGet(out ParticleTweenDataProvider provider))
                return;

            float delay = provider.Config.BossAttackTweenData.Delay + provider.Config.BossAttackTweenData.Duration;
            CoroutineRunner.Instance.RunAfterSeconds(()
                => TakeDamage(unitController.Model.GetFinalDamage()), delay);
        }

        public void OnPhaseStarted(GameState state, GameFlowSystem flow)
        {
            if (state.CurrentTurn != ETurnOwner.Player)
                return;

            switch (state.CurrentPhase)
            {
                case EGamePhase.PlayerPrePlay:
                    TickEffects();
                    UpdateStats();
                    flow.MarkSubscriberDone(this);
                    break;

                case EGamePhase.PlayerDraw:
                    CoroutineRunner.Instance.RunCoroutine(new WaitUntil(() =>
                        Snapshot.CardsToDraw == 0 || !CanStillDraw()), () =>
                    {
                        flow.MarkSubscriberDone(this);
                    });
                    break;
                default:
                    return;
            }
        }

        public void OnPhaseEnded(GameState state) { }

        /// <summary>
        /// Deal damage to the player by the specified amount.
        /// </summary>
        public void TakeDamage(int amount)
        {
            PlayerSnapshot next = Snapshot.WithDamage(amount);
            ApplySnapshot(next);

            if (next.IsDead)
                Die();
        }

        /// <summary>
        /// Heal the player by the specified amount.
        /// </summary>
        public void Heal(int amount) => ApplySnapshot(Snapshot.WithHeal(amount));

        public void AddEffect(PlayerEffect effect)
        {
            if (effect == null)
                return;

            ActiveEffects.Add(effect);
            effect.Apply();

            PlayerEffectDisplay display = PlayerEffectDisplayPool.Instance.Get();
            display.transform.SetParent(effectDisplayParent);
            display.SetEffect(effect);
            _effectDisplays.Add(display);

            // Rebuild layout to ensure correct positioning
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)effectDisplayParent);
            display.SetActive(false);

            UpdateStatsPreview();

            OnEffectAdded?.Invoke(display);

            // Delay activation to only show after spawn animation
            if (!ServiceLocator.TryGet(out ParticleTweenDataProvider provider))
            {
                display.SetActive(true);
                return;
            }

            float delay = provider.Config.EffectSpawnedTweenData.Delay + provider.Config.EffectSpawnedTweenData.Duration;
            CoroutineRunner.Instance.RunAfterSeconds(() => display.SetActive(true), delay);
        }

        public void AddEffect(IEffect effect)
        {
            if (effect is PlayerEffect playerEffect)
                AddEffect(playerEffect);
        }

        public void RemoveEffect(PlayerEffect effect)
        {
            if (effect == null)
                return;

            ActiveEffects.Remove(effect);

            foreach (PlayerEffectDisplay effectDisplay in _effectDisplays)
            {
                if (effectDisplay.EffectData != effect.EffectData)
                    continue;

                PlayerEffectDisplayPool.Instance.Release(effectDisplay);
                _effectDisplays.Remove(effectDisplay);
                break;
            }

            UpdateStatsPreview();
        }

        public void ClearEffects()
        {
            foreach (PlayerEffect effect in ActiveEffects)
                RemoveEffect(effect);
        }

        public void RemoveEffect(IEffect effect)
        {
            if (effect is PlayerEffect playerEffect)
                RemoveEffect(playerEffect);
        }

        public void TickEffects()
        {
            List<PlayerEffect> expired = new();
            foreach (PlayerEffect e in ActiveEffects)
            {
                e.Tick();
                if (e.IsExpired)
                    expired.Add(e);
            }

            foreach (PlayerEffect e in expired)
            {
                e.Revert();
                RemoveEffect(e);
            }
        }

        public bool CanPlay(CardModel cardModel)
        {
            bool canPlay = HasEnoughEnergy(cardModel);

            if (!canPlay)
                OnNotEnoughEnergy?.Invoke();

            return canPlay;
        }

        /// <summary>
        /// Check if the player has enough energy to spend the specified amount.
        /// </summary>
        public bool HasEnoughEnergy(CardModel cardModel)
        {
            int finalCost = cardModel.GetFinalEnergyCost();
            return Snapshot.CanSpend(finalCost);
        }

        /// <summary>
        /// Check if the player can draw a card.
        /// </summary>
        /// <returns><c>true</c> if the player can draw a card; otherwise, <c>false</c>.</returns>
        public bool CanDraw() => Snapshot.CanDrawCards(1);

        /// <summary>
        /// Called when the player has drawn a card.
        /// </summary>
        public void CardDrawn() => ApplySnapshot(Snapshot.WithCardsToDrawRemaining(Snapshot.CardsToDraw - 1));

        public void OnCardExecuted(CardController card)
        {
            _lastPlayedCard = card;

            if (card is ActionCardController)
                MoveCardToDiscard(card);

            SpendEnergy(card.Model.GetFinalEnergyCost());
            OnCardPlayed?.Invoke(card);
        }

        public void OnNewUnitSpawned(UnitController unit)
        {
            if (_lastPlayedCard == null)
            {
                CustomLogger.LogWarning("No last played card to associate with spawned unit.", this);
                return;
            }

            if (_lastPlayedCard is not UnitCardController unitCard)
            {
                CustomLogger.LogWarning("Last played card is not a unit card.", this);
                return;
            }

            unitCard.CardView.SetVisible(false);
            unitCard.transform.SetParent(unit.transform, false);
            unitCard.transform.localPosition = Vector3.zero;
            unitCard.transform.localRotation = Quaternion.identity;

            _playerUnitToCard[unit] = _lastPlayedCard;

            unit.OnUnitDied += HandleUnitDied;
        }

        /// <summary>
        /// Applies a new snapshot to the player, raising events for any changed values.
        /// </summary>
        public void ApplySnapshot(PlayerSnapshot next)
        {
            PlayerSnapshot prev = Snapshot;
            Snapshot = next;

            if (prev.CurrentHp != next.CurrentHp)
                RaiseHealthChangedEvent();

            if (prev.CurrentHp > next.CurrentHp)
                OnPlayerDamaged?.Invoke(prev.CurrentHp - next.CurrentHp);

            if (prev.CurrentEnergy != next.CurrentEnergy)
                OnEnergyChanged?.Invoke(next.CurrentEnergy);

            if (prev.CardsToDraw != next.CardsToDraw)
                OnDrawableCardAmountChanged?.Invoke(next.CardsToDraw);

            UpdateStatsPreview();
        }

        public bool HasMaxEffectsReached() => false;

        public bool ValidatePlayer(ETeam team) => team == ETeam.Player;

        private bool CanStillDraw()
        {
            return _actionDeck?.CardCount > 0 || _unitDeck?.CardCount > 0 || _discardPile?.CardCount > 0;
        }

        private void MoveCardToDiscard(CardController card)
        {
            if (card == null)
            {
                CustomLogger.LogWarning("Cannot move a null card to discard pile.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out DiscardPileController discard))
            {
                CustomLogger.LogWarning("Discard pile controller not found.", this);
                return;
            }

            if (card is UnitCardController unitCard)
                unitCard.CardView.SetVisible(true);

            discard.Add(card);

            if (!ServiceLocator.TryGet(out CardTweenController mover))
                return;

            if (card.CardGate.IsTransitioning)
            {
                CustomLogger.LogWarning("Card is transitioning, cannot tween to discard pile.", this);
                return;
            }

            mover.TweenCardTo(card, Vector3.zero, Quaternion.identity, discard.BaseScale);
        }

        private void HandleUnitDied(UnitController unit)
        {
            unit.OnUnitDied -= HandleUnitDied;

            if (!_playerUnitToCard.Remove(unit, out CardController card))
                return;

            MoveCardToDiscard(card);
        }

        private void SpendEnergy(int cost)
        {
            if (!Snapshot.CanSpend(cost))
            {
                CustomLogger.LogWarning($"Not enough energy to spend {cost}. Current energy: {Snapshot.CurrentEnergy}", this);
                return;
            }

            ApplySnapshot(Snapshot.WithEnergySpent(cost));
        }

        private void GainEnergy(int amount) => ApplySnapshot(Snapshot.WithEnergyGained(amount));

        private void Die()
        {
            if (Snapshot.CurrentHp != 0)
            {
                ApplySnapshot(new PlayerSnapshot(
                    Snapshot.MaxHealth, 0, Snapshot.CurrentEnergy,
                    Snapshot.EnergyGainPerRound, Snapshot.CardsToDrawPerRound, Snapshot.CardsToDraw));
            }

            if (ServiceLocator.TryGet(out GameFlowSystem flow))
                flow.InvokeGameOver(new GameState(EGamePhase.None, ETurnOwner.Boss, true));
        }

        private void UpdateStats()
        {
            int additionalEnergy = ActiveEffects.Sum(effect => effect.AdditionalEnergyGain);
            int additionalCardDraws = ActiveEffects.Sum(effect => effect.AdditionalActionCardDraws);

            int totalEnergyGain = Snapshot.EnergyGainPerRound + additionalEnergy;
            int totalDrawsThisRound = Snapshot.CardsToDrawPerRound + additionalCardDraws;

            if (totalEnergyGain > 0)
                GainEnergy(totalEnergyGain);

            AddExtraDraws(totalDrawsThisRound);
        }

        private void AddExtraDraws(int totalForThisRound)
        {
            int newTotal = Mathf.Max(0, Snapshot.CardsToDraw + totalForThisRound);
            ApplySnapshot(Snapshot.WithCardsToDrawRemaining(newTotal));
        }

        private void UpdateStatsPreview()
        {
            UpdateEnergyPreview();
            UpdateDrawableCardAmountPreview();
        }

        private void UpdateEnergyPreview()
        {
            int nextTurnGain = CalculateNextTurnEnergyGain();
            OnGainedEnergyNextTurnChanged?.Invoke(nextTurnGain);
        }

        private void UpdateDrawableCardAmountPreview()
        {
            int nextTurnGain = CalculateNextTurnDrawableCardAmount();
            OnDrawableCardAmountNextTurnChanged?.Invoke(nextTurnGain);
        }

        private void RaiseHealthChangedEvent() => OnHpChanged?.Invoke(Snapshot.CurrentHp);

        private int CalculateNextTurnDrawableCardAmount()
        {
            int baseGain = Snapshot.CardsToDrawPerRound;
            int totalGain = baseGain;

            // Simulate ticking effects for next turn preview
            List<PlayerEffect> simulatedEffects = new();
            foreach (PlayerEffect effect in ActiveEffects)
            {
                PlayerEffect clone = (PlayerEffect)effect.Clone();
                simulatedEffects.Add(clone);
            }

            foreach (PlayerEffect clone in simulatedEffects)
            {
                clone.Tick();
                if (!clone.IsExpired)
                    totalGain += clone.AdditionalActionCardDraws;
            }

            return Mathf.Max(0, totalGain);
        }

        private int CalculateNextTurnEnergyGain()
        {
            int baseGain = Snapshot.EnergyGainPerRound;
            int totalGain = baseGain;

            List<PlayerEffect> simulatedEffects = new();
            foreach (PlayerEffect effect in ActiveEffects)
            {
                PlayerEffect clone = (PlayerEffect)effect.Clone();
                simulatedEffects.Add(clone);
            }

            foreach (PlayerEffect clone in simulatedEffects)
            {
                clone.Tick();
                if (!clone.IsExpired)
                    totalGain += clone.AdditionalEnergyGain;
            }

            return Mathf.Max(0, totalGain);
        }

        private void HandleBossUnitDefeated(UnitController unitController)
        {
            if (unitController == null)
            {
                CustomLogger.LogWarning("Defeated unit controller is null.", this);
                return;
            }

            if (unitController.Team != ETeam.Boss)
            {
                CustomLogger.LogWarning("Defeated unit is not a boss unit.", this);
                return;
            }

            if (unitController.Model == null)
            {
                CustomLogger.LogWarning("Defeated unit model is null.", this);
                return;
            }

            int gain = unitController.Model.Worth;
            GainEnergy(gain);
            unitController.OnUnitDied -= HandleBossUnitDefeated;
            _bossUnits.Remove(unitController);
        }
    }
}