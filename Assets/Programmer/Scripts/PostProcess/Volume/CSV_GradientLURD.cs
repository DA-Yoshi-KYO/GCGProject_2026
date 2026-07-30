using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostProcess/GradientLURD")]
public class CSV_GradientLURD : CSV_PostProcessVolumeBase
{
    [Header("色パラメータ(乗算)")]
    [Tooltip("最初に通過する色")] public ColorParameter startColor = new ColorParameter(Color.black);
    [Tooltip("2番目に通過する色")] public ColorParameter secondColor = new ColorParameter(Color.yellow);
    [Header("時間パラメータ")]
    [Tooltip("何秒でエフェクトが完了するか")] public ClampedFloatParameter maxTime = new ClampedFloatParameter(10f, 0.1f, 10f);
    [Header("見た目パラメータ")]
    [Tooltip("斜めグラデーションの表示幅(大きいほど3色が同時に画面上に並んで見える。小さいほど1色ずつ切り替わる)")] public ClampedFloatParameter gradientWidth = new ClampedFloatParameter(1f, 0.05f, 3f);
    [Header("デバッグ用パラメータ")]
    [Tooltip("デバッグ用の進行値を使用するか")] public BoolParameter useCustomProgress = new BoolParameter(false);
    [Tooltip("デバッグ用の進行値")] public ClampedFloatParameter customProgress = new ClampedFloatParameter(0f, 0f, 1f);
    [Tooltip("時間")]public FloatParameter time = new FloatParameter(0.0f);

    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetColor("_StartColor", startColor.value);
        materialBlock.SetColor("_SecondColor", secondColor.value);
        materialBlock.SetFloat("_MaxTimeFloat", maxTime.value);
        materialBlock.SetFloat("_GradientWidth", gradientWidth.value);
        materialBlock.SetFloat("_UseCustomProgress", useCustomProgress.value ? 1f : 0f);
        materialBlock.SetFloat("_CustomProgress", customProgress.value);
        materialBlock.SetFloat("_TimeFloat", time.value);
    }
}
