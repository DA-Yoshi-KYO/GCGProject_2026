using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CS_RockBurn : MonoBehaviour
{
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private float fadeStartTime = 3f;
    [SerializeField] private float fadeStartTimeOffSet = 0.3f;
    [SerializeField] private GameObject burnEffectPrefab;
    Material burnMaterial;
    float fadeTimer = 0f;

    void Start()
    {
        DecalProjector decalProjector = burnEffectPrefab.GetComponent<DecalProjector>();
        burnMaterial = new Material(decalProjector.material);
        decalProjector.material = burnMaterial;
        // シェーダー側でUV.Vに応じてフェード終了時間をずらすための係数
        burnMaterial.SetFloat("_FadeDuration", destroyTime - fadeStartTime);
        burnMaterial.SetFloat("_FadeSpread", fadeStartTimeOffSet);
        burnMaterial.SetFloat("_AlphaMultiple", -1f);
        Destroy(gameObject, destroyTime + fadeStartTimeOffSet);
    }

    void Update()
    {
        fadeTimer += Time.deltaTime;
        float progress = (fadeTimer - fadeStartTime) / (destroyTime - fadeStartTime);
        burnMaterial.SetFloat("_AlphaMultiple", progress);
    }
}
