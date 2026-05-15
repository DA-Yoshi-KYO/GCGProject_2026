using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostProcess/HeatEffect")]
public class CSV_HeatEffectVolume : CSV_PostProcessVolumeBase
{
    public ClampedFloatParameter wiggleSpeed = new ClampedFloatParameter(2f, 0f, 5f);
    public ClampedFloatParameter effectIntensity = new ClampedFloatParameter(0.005f, 0f, 0.03f);
    public FloatParameter heatEffectTime = new FloatParameter(0f);

    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetFloat("_Speed", wiggleSpeed.value);
        materialBlock.SetFloat("_Intensity", effectIntensity.value);
        materialBlock.SetFloat("_CustomTime", heatEffectTime.value);
    }
}
