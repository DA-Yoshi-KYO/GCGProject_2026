using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostProcess/ColorTint")]
public class CSV_ColorTintVolume : CSV_PostProcessVolumeBase
{
    [Tooltip("画面に乗せる色")] public ColorParameter tintColor = new ColorParameter(Color.red);
    [Tooltip("染める強さ(0:元の画面 1:単色で塗りつぶし)")] public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetColor("_TintColor", tintColor.value);
        materialBlock.SetFloat("_Intensity", intensity.value);
    }
}
