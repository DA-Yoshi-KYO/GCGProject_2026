using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostProcess/SandStorm")]
public class CSV_SandStormVolume : CSV_PostProcessVolumeBase
{
    public FloatParameter blowOutFactor = new FloatParameter(2.5f);
    public FloatParameter distortStrength = new FloatParameter(0.04f);
    public Vector2Parameter speed = new Vector2Parameter(new Vector2(-0.04f, 0.7f));
    public BoolParameter isTimeUpdate = new BoolParameter(true);
    public ClampedFloatParameter frequency = new ClampedFloatParameter(1f, 0f, 5f);

    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetFloat("_BlowOutFactor", blowOutFactor.value);
        materialBlock.SetFloat("_DistortStrength", distortStrength.value);
        materialBlock.SetVector("_Speed", speed.value);
        materialBlock.SetFloat("_IsTimeUpdate", isTimeUpdate.value ? 1f : 0f);
        materialBlock.SetFloat("_Frequency", frequency.value);
    }
}
