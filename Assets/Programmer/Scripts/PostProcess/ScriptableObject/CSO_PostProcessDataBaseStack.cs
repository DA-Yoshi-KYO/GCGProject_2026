using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ScriptableObjects/PostProcessStack")]
public class CSO_PostProcessStack : ScriptableObject
{
    [System.Serializable]
    public class EffectEntry
    {
        public Material material;
        [SerializeField] private string volumeComponentTypeName;

        private MaterialPropertyBlock _propertyBlock;
        public MaterialPropertyBlock PropertyBlock
            => _propertyBlock ??= new MaterialPropertyBlock();

        public void SetFloat(string name, float value) => PropertyBlock.SetFloat(name, value);
        public void SetColor(string name, Color value) => PropertyBlock.SetColor(name, value);
        public void SetVector(string name, Vector4 value) => PropertyBlock.SetVector(name, value);
        public void SetTexture(string name, Texture value) => PropertyBlock.SetTexture(name, value);

        public CSV_PostProcessVolumeBase GetVolumeComponent(VolumeStack stack)
        {
            var type = System.Type.GetType(volumeComponentTypeName);
            if (type == null) return null;
            return stack.GetComponent(type) as CSV_PostProcessVolumeBase;
        }

        public bool IsActive(VolumeStack stack)
            => GetVolumeComponent(stack)?.IsActive() ?? false;

        public void Apply(VolumeStack stack)
            => GetVolumeComponent(stack)?.Apply(PropertyBlock);

        // エディタ用
        public string VolumeComponentTypeName
        {
            get => volumeComponentTypeName;
            set => volumeComponentTypeName = value;
        }
    }

    public List<EffectEntry> effects = new();
}
