using UnityEngine;
using Gameplay.Boss;
using Gameplay.Flow;
using UnityEngine.UI;
using Utility.Logging;
using Gameplay.Boss.Data;
using Systems.Tweening.Core;
using Systems.Tweening.Core.Data.Parameters;

namespace UI.Menus.BossSelection
{
    /// <summary>
    /// Displays the image of the chosen boss.
    /// </summary>
    public class BossImageDisplay : MonoBehaviour
    {
        private const float DefaultChromaticAberrationIntensity = 0f;

        private static readonly int ChromaticAberrationIntensity = Shader.PropertyToID("_ChromAberrAmount");

        [SerializeField] private Image bossImage;

        [Header("Chromatic Aberration Animation")]
        [SerializeField] private float targetIntensity = 1f;
        [SerializeField] private TweenData chromaticAberrationTweenData;

        private Material _shaderMaterial;
        private TweenBase _chromaticAberrationTween;

        private void Awake()
        {
            GameContextService.OnBossChosen += HandleBossChosen;
            BossModel.OnAggressive += HandleBossAggressive;

            _shaderMaterial = new Material(bossImage.material);
            bossImage.material = _shaderMaterial;

            _shaderMaterial.SetFloat(ChromaticAberrationIntensity, DefaultChromaticAberrationIntensity);
        }

        private void OnDestroy()
        {
            GameContextService.OnBossChosen -= HandleBossChosen;
            BossModel.OnAggressive -= HandleBossAggressive;
        }

        private void HandleBossChosen(BossData bossData)
        {
            if (bossData == null)
            {
                CustomLogger.LogWarning("Boss data is null.", this);
                return;
            }

            bossImage.sprite = bossData.BossImage;
        }

        private void HandleBossAggressive()
        {
            _chromaticAberrationTween?.Stop();
            _chromaticAberrationTween = TweenFX.FadeFloatTo(
                fromGetter: () => _shaderMaterial.GetFloat(ChromaticAberrationIntensity),
                setter: f => _shaderMaterial.SetFloat(ChromaticAberrationIntensity, f),
                targetValue: targetIntensity,
                data: chromaticAberrationTweenData,
                targetObj: this
            );
        }
    }
}