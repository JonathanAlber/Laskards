using System.Linq;
using UnityEngine;
using Gameplay.Cards;
using Systems.Services;
using Gameplay.Decks.Layout;
using Systems.Tweening.Core.Data;
using Gameplay.Cards.Interaction;
using System.Collections.Generic;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Player;
using Systems.Tweening.Core.Data.Parameters;
using CollectionExtensions = Utility.Collections.CollectionExtensions;

namespace Gameplay.Decks.View
{
    /// <summary>
    /// Visual component for the player's hand deck. Handles fan-like layout, focus and draw animations.
    /// </summary>
    public sealed class HandDeckView : DeckView
    {
        [Header("References")]

        [Tooltip("ScriptableObject defining hand layout parameters.")]
        [SerializeField] private HandLayoutSettings handLayoutSettings;

        [Tooltip("The canvas in which the hand exists.")]
        [SerializeField] private Canvas canvas;

        [Header("Animation")]

        [Tooltip("Delay between each card when drawing multiple cards.")]
        [SerializeField] private float drawDelayPerCard = 0.1f;

        [Tooltip("Tween data defining how the card moves when animating the layout.")]
        [SerializeField] private TweenData moveTweenData = new(1.2f, EEasingType.EaseInOut);

        [Tooltip("Tween data defining how the card rotates when animating the layout.")]
        [SerializeField] private TweenData rotationTweenData = new(1.2f, EEasingType.EaseInOut);

        [Tooltip("Tween data defining how the card scales when animating the layout.")]
        [SerializeField] private TweenData scaleTweenData = new(0.5f, EEasingType.EaseInOut);

        [Header("Focus / Hover Layout")]

        [Tooltip("Scale multiplier to apply to the focused card.")]
        [SerializeField] private float focusScaleMultiplier = 1.25f;

        [Tooltip("Horizontal spacing to push non-focused cards away from the focused card.")]
        [SerializeField] private float focusPushSpacing = 60f;

        [Tooltip("Multiplier to increase the inner gap between focused card and adjacent cards.")]
        [SerializeField] private float focusInnerGapMultiplier = 1.5f;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private Color gizmoColor = Color.cyan;
        [SerializeField] private int previewCount = 6;
        [SerializeField] private float gizmoCardSize = 35f;
        [SerializeField] private float gizmoLineLength = 200f;
        [SerializeField] private bool drawCurve = true;
#endif

        private readonly Dictionary<CardController, int> _savedSiblingIndex = new();

        private CardController _focusedCard;

        private Vector3[] _basePosArray;
        private Quaternion[] _baseRotArray;
        private Vector3[] _baseScaleArray;
        private int _baseComputedForCount = -1;

        private void Awake()
        {
            GameFlowSystem.OnPhaseStarted += HandlePhaseStarted;
            PlayerController.OnEnergyChanged += HandleEnergyChanged;
        }

        private void OnDestroy()
        {
            GameFlowSystem.OnPhaseStarted -= HandlePhaseStarted;
            PlayerController.OnEnergyChanged -= HandleEnergyChanged;
        }

        /// <summary>
        /// Focuses the given card in the hand, expanding it and pushing other cards aside.
        /// </summary>
        /// <param name="cardToFocus">The card to focus.</param>
        public void Focus(CardController cardToFocus)
        {
            if (_focusedCard == cardToFocus)
                return;

            SetFocusedCard(cardToFocus);

            _savedSiblingIndex.Clear();
            IReadOnlyList<CardController> cards = CurrentCards;
            foreach (CardController c in cards)
                _savedSiblingIndex[c] = c.transform.GetSiblingIndex();

            AnimateLayout(null);
            BringFocusToFront();
        }

        /// <summary>
        /// Unfocuses the currently focused card, restoring normal layout.
        /// </summary>
        /// <param name="card">The card to unfocus.</param>
        public void Unfocus(CardController card)
        {
            if (_focusedCard != card)
                return;

            foreach (KeyValuePair<CardController, int> kvp in _savedSiblingIndex.Where(kvp => kvp.Key != null))
                kvp.Key.transform.SetSiblingIndex(kvp.Value);

            _savedSiblingIndex.Clear();
            SetFocusedCard(null);

            AnimateLayout(null);
        }

        public override void OnHoverEnter(CardController card) { }

        public override void OnHoverExit(CardController card) { }

        protected override void OnModelCardAdded(CardController card)
        {
            base.OnModelCardAdded(card);

            card.CardInteractionAdapter.InitializeCanvas(canvas);
            AnimateLayout(CollectionExtensions.Single(card));
            UpdateGrayedOutState();
        }

        protected override void OnModelCardRemoved(CardController card)
        {
            card.View.GrayOut(false);

            if (_focusedCard == card)
            {
                SetFocusedCard(null);
                _savedSiblingIndex.Clear();
            }

            AnimateLayout(null);
            UpdateGrayedOutState();
        }

        protected override void OnModelShuffled() => AnimateLayout(null);

        protected override void OnModelCleared(List<CardController> removedCards)
        {
            SetFocusedCard(null);
            _savedSiblingIndex.Clear();
            SetAllCardsGrayedOut(removedCards, false);
        }

        private static void SetAllCardsGrayedOut(List<CardController> cardsToReset, bool state)
        {
            foreach (CardController card in cardsToReset)
                card.View.GrayOut(state);
        }

        private void HandlePhaseStarted(GameState gameState) => UpdateGrayedOutState();

        private void HandleEnergyChanged(int newEnergy) => UpdateGrayedOutState();

        private void UpdateGrayedOutState()
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            if (GameFlowSystem.CurrentPhase == EGamePhase.PlayerPlay)
            {
                foreach (CardController card in CurrentCards)
                {
                    bool isAffordable = player.HasEnoughEnergy(card.Model);
                    card.View.GrayOut(!isAffordable);
                }
            }
            else
            {
                SetAllCardsGrayedOut(CurrentCards.ToList(), true);
            }
        }

        private void AnimateLayout(IEnumerable<CardController> newCards)
        {
            if (!ServiceLocator.TryGet(out CardTweenController mover))
                return;

            int total = CurrentCount;
            if (total == 0)
                return;

            HashSet<CardController> newSet = null;
            CardController singleNewCard = null;

            if (newCards != null)
            {
                newSet = new HashSet<CardController>();
                int addedCount = 0;
                foreach (CardController c in newCards)
                {
                    if (addedCount == 0)
                        singleNewCard = c;

                    newSet.Add(c);
                    addedCount++;
                }

                if (newSet.Count != 1)
                    singleNewCard = null;
            }

            int focusedIndex = -1;
            if (_focusedCard != null)
                focusedIndex = IndexOfCard(_focusedCard);

            EnsureBaseBuffers(total);
            if (_baseComputedForCount != total)
                RecomputeBaseLayout(total);

            float focusedBaseX = focusedIndex >= 0 ? _basePosArray[focusedIndex].x : 0f;
            int drawIndex = 0;

            IReadOnlyList<CardController> cards = CurrentCards;

            for (int i = 0; i < total; i++)
            {
                CardController card = cards[i];
                if (card.CardGate.IsBeingDragged)
                    continue;

                Vector3 pos;
                Quaternion rot;
                Vector3 scale;

                if (focusedIndex >= 0)
                {
                    (pos, rot, scale) = CalculateFocusedExpandedLayout(i, focusedIndex, focusedBaseX,
                        _basePosArray, _baseRotArray, _baseScaleArray);
                }
                else
                {
                    pos = _basePosArray[i];
                    rot = _baseRotArray[i];
                    scale = _baseScaleArray[i];
                }

                float delay = 0f;
                bool isSingleMatch = singleNewCard != null && card == singleNewCard;
                bool isInSet = newSet != null && newSet.Contains(card);
                if (isSingleMatch || isInSet)
                {
                    delay = drawIndex * drawDelayPerCard;
                    drawIndex++;
                }

                mover.TweenCardTo(card, pos, rot, scale, moveTweenData.WithDelay(delay),
                    rotationTweenData.WithDelay(delay), scaleTweenData.WithDelay(delay), false);
            }
        }

        private void SetFocusedCard(CardController card)
        {
            if (card == null)
            {
                _focusedCard?.View.AnimateShine(false);
                _focusedCard = card;
            }
            else
            {
                _focusedCard = card;
                _focusedCard.View.AnimateShine(true);
            }
        }

        private int IndexOfCard(CardController card)
        {
            IReadOnlyList<CardController> cards = CurrentCards;
            for (int i = 0; i < cards.Count; i++)
                if (cards[i] == card)
                    return i;

            return -1;
        }

        private void BringFocusToFront()
        {
            if (_focusedCard == null)
                return;

            int highest = _focusedCard.transform.GetSiblingIndex();

            IReadOnlyList<CardController> cards = CurrentCards;
            highest = cards.Select(card => card.transform.GetSiblingIndex()).Prepend(highest).Max();

            _focusedCard.transform.SetSiblingIndex(highest + 1);
        }

        private void EnsureBaseBuffers(int required)
        {
            if (_basePosArray != null && _basePosArray.Length >= required)
                return;

            int newSize = _basePosArray == null ? Mathf.Max(4, required) : Mathf.Max(required, _basePosArray.Length * 2);
            _basePosArray = new Vector3[newSize];
            _baseRotArray = new Quaternion[newSize];
            _baseScaleArray = new Vector3[newSize];
            _baseComputedForCount = -1;
        }

        private void RecomputeBaseLayout(int total)
        {
            for (int i = 0; i < total; i++)
            {
                (Vector3 pos, Quaternion rot, Vector3 scale) normalLayout = CalculateLayoutUI(i, total);
                _basePosArray[i] = normalLayout.pos;
                _baseRotArray[i] = normalLayout.rot;
                _baseScaleArray[i] = normalLayout.scale;
            }

            _baseComputedForCount = total;
        }

        private (Vector3 pos, Quaternion rot, Vector3 scale) CalculateLayoutUI(int index, int total)
        {
            float baseArc = handLayoutSettings.ArcDegrees;
            float radius = handLayoutSettings.Radius;
            float spacing = handLayoutSettings.CardSpacing;
            float maxAngle = handLayoutSettings.MaxAnglePerCard;
            float minArcDegrees = handLayoutSettings.MinArcDegrees;
            int fullSpreadAtCount = handLayoutSettings.FullSpreadAtCount;

            float spreadT = Mathf.Clamp01((total - 1f) / Mathf.Max(1f, fullSpreadAtCount - 1f));
            float effectiveArc = Mathf.Lerp(minArcDegrees, baseArc, spreadT);

            float halfArc = effectiveArc * 0.5f;
            float step = total > 1 ? effectiveArc / (total - 1) : 0f;
            float angle = total > 1 ? -halfArc + step * index : 0f;
            float radians = angle * Mathf.Deg2Rad;

            Vector2 localPos2D = new(Mathf.Sin(radians) * radius, -Mathf.Cos(radians) * radius);

            localPos2D.x += spacing * (index - (total - 1) * 0.5f);

            float rotationZ = Mathf.Clamp(angle, -maxAngle, maxAngle);
            Quaternion localRot = Quaternion.Euler(0f, 0f, rotationZ);

            Vector3 pos = localPos2D;
            Vector3 scale = BaseScale;

            return (pos, localRot, scale);
        }

        private (Vector3 pos, Quaternion rot, Vector3 scale) CalculateFocusedExpandedLayout(
            int index, int focusedIndex, float focusedBaseX,
            Vector3[] basePosArray, Quaternion[] baseRotArray, Vector3[] baseScaleArray)
        {
            Vector3 basePos = basePosArray[index];
            Quaternion baseRot = baseRotArray[index];
            Vector3 baseScale = baseScaleArray[index];

            if (index == focusedIndex)
            {
                Vector3 focusedPos = basePos;
                Quaternion focusedRot = Quaternion.identity;
                Vector3 focusedScale = baseScale * focusScaleMultiplier;
                return (focusedPos, focusedRot, focusedScale);
            }

            int distance = Mathf.Abs(index - focusedIndex);
            float inverseDistance = 1f / distance;
            float boost = distance == 1 ? focusInnerGapMultiplier : 1f;
            float pushAmount = focusPushSpacing * inverseDistance * boost;

            float newX = basePos.x < focusedBaseX ? basePos.x - pushAmount : basePos.x + pushAmount;
            Vector3 newPos = new(newX, basePos.y, basePos.z);

            return (newPos, baseRot, baseScale);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (cardParent == null || handLayoutSettings == null)
                return;

            Gizmos.color = gizmoColor;
            Matrix4x4 localToWorld = cardParent.localToWorldMatrix;

            for (int i = 0; i < previewCount; i++)
            {
                (Vector3 pos, Quaternion rot, Vector3 scale) layout = CalculateLayoutUI(i, previewCount);

                Vector3 worldPos = localToWorld.MultiplyPoint3x4(layout.pos);
                Quaternion worldRot = cardParent.rotation * layout.rot;

                Gizmos.DrawCube(worldPos, Vector3.one * gizmoCardSize);
                Gizmos.DrawLine(worldPos, worldPos + worldRot * Vector3.up * gizmoLineLength);
            }

            DrawCurveArc(localToWorld);
        }

        private void DrawCurveArc(Matrix4x4 localToWorld)
        {
            if (!drawCurve)
                return;

            float baseArc = handLayoutSettings.ArcDegrees;
            float radius = handLayoutSettings.Radius;
            float minArcDegrees = handLayoutSettings.MinArcDegrees;
            int fullSpreadAtCount = handLayoutSettings.FullSpreadAtCount;

            float spreadT = Mathf.Clamp01((previewCount - 1f) / Mathf.Max(1f, fullSpreadAtCount - 1f));
            float effectiveArc = Mathf.Lerp(minArcDegrees, baseArc, spreadT);

            const int segments = 32;
            Vector3 prev = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float t = Mathf.Lerp(-effectiveArc * 0.5f, effectiveArc * 0.5f, i / (float)segments);
                float rad = t * Mathf.Deg2Rad;

                Vector3 local = new(Mathf.Sin(rad) * radius, -Mathf.Cos(rad) * radius, 0f);

                Vector3 world = localToWorld.MultiplyPoint3x4(local);

                if (i > 0)
                    Gizmos.DrawLine(prev, world);

                prev = world;
            }
        }
#endif
    }
}