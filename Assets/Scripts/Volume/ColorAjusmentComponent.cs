using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorAjusmentComponent : MonoBehaviour, IVolumeComponent
{
    private ColorAdjustments adjustments;

    [SerializeField] private float PostExposure;
    [SerializeField] private AnimationCurve postExposureCurve;
    [Space]
    [SerializeField] private float contrast;
    [SerializeField] private AnimationCurve contrastCurve;
    [Space]
    [SerializeField] private Color colorFilter;
    [SerializeField] private AnimationCurve colorFilterCurve;
    [Space]
    [SerializeField] private float hueShift;
    [SerializeField] private AnimationCurve hueShiftCurve;
    [Space]
    [SerializeField] private float saturation;
    [SerializeField] private AnimationCurve saturationCurve;
    [field:SerializeField] public float duration { get; set; }
    [field:SerializeField] public Volume _volume { get; set; }

    private void Start()
    {
        _volume.profile.TryGet(out adjustments);
    }

    public void Execute()
    {
        if (adjustments == null) return;
        var time = 0f;
        DOTween.To(() => time, x => time = x, 1, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                adjustments.postExposure.value = postExposureCurve.Evaluate(time) * PostExposure;
                adjustments.contrast.value = contrastCurve.Evaluate(time) * contrast;

            });
    }
}
