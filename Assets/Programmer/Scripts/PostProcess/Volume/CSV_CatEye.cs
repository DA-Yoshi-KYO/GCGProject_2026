using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostProcess/CatEye")]
public class CSV_CatEye : CSV_PostProcessVolumeBase
{
    public ClampedFloatParameter contractFloat = new ClampedFloatParameter(1.25f, 1f, 1.4f);
    public ClampedFloatParameter bloomFloat = new ClampedFloatParameter(2f, 2f, 4f);
    public ClampedFloatParameter darknessFloat = new ClampedFloatParameter(0.3f, 0f, 1f);
    public ClampedFloatParameter colorAdjust = new ClampedFloatParameter(0.5f, 0f, 1f);
    public ClampedFloatParameter progressFloat = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public FloatParameter maxRadiusFloat = new FloatParameter(5.0f);
    public FloatParameter featherFloat = new FloatParameter(5.0f);
    public FloatParameter frequencyFloat = new FloatParameter(5.0f);
    public FloatParameter speedFloat = new FloatParameter(5.0f);
    public FloatParameter amplitudeFloat = new FloatParameter(5.0f);

    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetFloat("_ContractFloat", contractFloat.value);
        materialBlock.SetFloat("_BloomFloat", bloomFloat.value);
        materialBlock.SetFloat("_DarknessFloat", darknessFloat.value);
        materialBlock.SetFloat("_ColorAdjust", colorAdjust.value);
        materialBlock.SetFloat("_ProgressFloat", progressFloat.value);
        materialBlock.SetFloat("_MaxRadiusFloat", maxRadiusFloat.value);
        materialBlock.SetFloat("_FeatherFloat", featherFloat.value);
        materialBlock.SetFloat("_FrequencyFloat", frequencyFloat.value);
        materialBlock.SetFloat("_SpeedFloat", speedFloat.value);
        materialBlock.SetFloat("_AmplitudeFloat", amplitudeFloat.value);
    }
}
