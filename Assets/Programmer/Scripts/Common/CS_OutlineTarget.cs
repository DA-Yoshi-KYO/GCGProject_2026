using UnityEngine;

// ========================================================
// アウトライン対象にアタッチするコンポーネント
// AddComponent するだけで登録/解除が自動管理される
// CS_OutlineController の置き換え（APIは同じ）
// ========================================================
[RequireComponent(typeof(Renderer))]
public class CS_OutlineTarget : MonoBehaviour
{
    [Header("Outline Settings")]
    [ColorUsage(true, true)]
    public Color outlineColor = Color.gray;
    public float outlineWidth = 6f;
    public float emissionIntensity = 6f;

    public Renderer CachedRenderer => cachedRenderer;
    public Material MaskMaterial => maskMaterial;   // Pass1 はこれで描く

    Renderer cachedRenderer;
    Material maskMaterial;  // OutlineMask の per-object インスタンス

    static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");
    static readonly int IntensityId = Shader.PropertyToID("_EmissionIntensity");

    void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        ApplyToMaterial(); // 内部で maskMaterial を生成・値セットする
        OutlineMaskPass.Register(this);
    }

    void OnDisable()
    {
        OutlineMaskPass.Unregister(this);
        if (maskMaterial != null)
        {
            Destroy(maskMaterial);
            maskMaterial = null;
        }
    }

    // ── 外部API ─────────────────────────────────────────────

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        ApplyToMaterial();
    }

    public void SetOutlineAlpha(float alpha)
    {
        outlineColor.a = alpha;
        ApplyToMaterial();
    }

    public void SetOutlineWidth(float width)
    {
        outlineWidth = width;
        ApplyToMaterial();
    }

    public void SetEmissionIntensity(float intensity)
    {
        emissionIntensity = intensity;
        ApplyToMaterial();
    }

    public void SetOutline(Color color, float width)
    {
        outlineColor = color;
        outlineWidth = width;
        ApplyToMaterial();
    }

    public void ResetOutline()
    {
        outlineColor = Color.white;
        outlineWidth = 2f;
        emissionIntensity = 6f;
        ApplyToMaterial();
    }

    // ── 内部 ────────────────────────────────────────────────

    void ApplyToMaterial()
    {
        // OnValidate が OnEnable より先に呼ばれるケース(編集モードでの値変更など)への対処
        if (maskMaterial == null)
        {
            var baseMat = OutlineMaskPass.BaseMaskMaterial;
            if (baseMat == null) return; // RendererFeature側の初期化がまだの場合は諦める
            maskMaterial = new Material(baseMat);
        }

        maskMaterial.SetColor(ColorId, outlineColor);
        maskMaterial.SetFloat(WidthId, outlineWidth);
        maskMaterial.SetFloat(IntensityId, emissionIntensity);
    }

#if UNITY_EDITOR
    void OnValidate() => ApplyToMaterial();
#endif
}
