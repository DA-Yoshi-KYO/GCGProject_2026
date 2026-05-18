using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostProcess/FilmGrain")]
public class CSV_FilmGrainVolume : CSV_PostProcessVolumeBase
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0.1f, -1f, 1f);
    
    public override void Apply(MaterialPropertyBlock materialBlock)
    {
        materialBlock.SetFloat("_Intensity", intensity.value);
    }
}
