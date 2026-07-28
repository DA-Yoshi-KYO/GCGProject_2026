using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostProcess/GradientLURD")]
public class CSV_GradientLURD : CSV_PostProcessVolumeBase
{
    [Header("色パラメータ(乗算)")]
    [Tooltip("最初に通過する色")] public ColorParameter rightDownColor = new ColorParameter(Color.black);
    [Tooltip("2番目に通過する色")] public ColorParameter middleColor = new ColorParameter(Color.yellow);
    [Tooltip("最終的な色")] public ColorParameter leftUpColor = new ColorParameter(Color.white);
    [Header("時間パラメータ")]
    [Tooltip("何秒でエフェクトが完了するか")] public ClampedFloatParameter maxTime = new ClampedFloatParameter(10f, 1f, 10f);
    [Header("デバッグ用パラメータ")]
    [Tooltip("デバッグ用の進行値を使用するか")] public BoolParameter useCustomProgress = new BoolParameter(false);
    [Tooltip("デバッグ用の進行値")] public ClampedFloatParameter customProgress = new ClampedFloatParameter(0f, 0f, 1f);
    [Tooltip("時間")]public FloatParameter time = new FloatParameter(0.0f);

    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetColor("_LeftUpColor", leftUpColor.value);
        materialBlock.SetColor("_MiddleColor", middleColor.value);
        materialBlock.SetColor("_RightDownColor", rightDownColor.value);
        materialBlock.SetFloat("_MaxTimeFloat", maxTime.value);
        materialBlock.SetFloat("_UseCustomProgress", useCustomProgress.value ? 1f : 0f);
        materialBlock.SetFloat("_CustomProgress", customProgress.value);
        materialBlock.SetFloat("_TimeFloat", time.value);
    }
}
