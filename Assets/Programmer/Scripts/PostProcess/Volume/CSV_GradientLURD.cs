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
    [Tooltip("それぞれの進行度がどれだけで次の色に移るか(x:黒開始, y:黒→オレンジ, z:オレンジ→シーン。区間の幅(y-x, z-y)が太さになる。黒はy-xを広く、オレンジはz-yを狭くすると狙いの見た目になる。y-xが十分広ければ進行度0.5付近で黒が画面全体を覆う)")] public Vector3Parameter nextPhaseProgress = new Vector3Parameter(new Vector3(0.15f, 0.85f, 0.93f));
    [Header("見た目パラメータ")]
    [Tooltip("反転フラグ")] public BoolParameter isInverse = new BoolParameter(false);
    [Header("デバッグ用パラメータ")]
    [Tooltip("デバッグ用の進行値を使用するか")] public BoolParameter useCustomProgress = new BoolParameter(false);
    [Tooltip("デバッグ用の進行値")] public ClampedFloatParameter customProgress = new ClampedFloatParameter(0f, 0f, 1f);
    [Tooltip("時間")]public FloatParameter time = new FloatParameter(0.0f);

    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetColor("_StartColor", startColor.value);
        materialBlock.SetColor("_SecondColor", secondColor.value);
        materialBlock.SetFloat("_MaxTimeFloat", maxTime.value);
        materialBlock.SetFloat("_UseCustomProgress", useCustomProgress.value ? 1f : 0f);
        materialBlock.SetFloat("_CustomProgress", customProgress.value);
        materialBlock.SetFloat("_TimeFloat", time.value);
        materialBlock.SetFloat("_IsInverse", isInverse.value ? 1f : 0f);
        materialBlock.SetVector("_PhaseDuration", nextPhaseProgress.value);
    }
}
