using UnityEngine;

/// <summary>
/// アウトライン対象にアタッチするコンポーネント。
/// 自身のMaskMaterial(インスタンス)を持ち、色・太さを直接セットする。
/// </summary>
[RequireComponent(typeof(Renderer))]
public class CS_OutlineTarget : MonoBehaviour
{
    [Header("Outline Settings")]
    [ColorUsage(true, true)]
    public Color outlineColor = Color.white;
    public float outlineWidth = 6f;
    public float emissionIntensity = 6f;

    [Tooltip("ONにすると遮蔽物越しにアウトラインが透けて見える（Editorの選択アウトラインと同じ挙動）")]
    public bool xRayOutline = false;

    public Renderer CachedRenderer => cachedRenderer;
    public Material MaskMaterial => maskMaterial;   // Pass1 はこれで描く

    Renderer cachedRenderer;
    Material maskMaterial;  // OutlineMask の per-object インスタンス

    static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");
    static readonly int IntensityId = Shader.PropertyToID("_EmissionIntensity");
    static readonly int ZTestModeId = Shader.PropertyToID("_ZTestMode");

    // UnityEngine.Rendering.CompareFunction の実値
    const float ZTEST_LEQUAL = 4f; // 通常の深度テスト(遮蔽物越しは見えない)
    const float ZTEST_ALWAYS = 8f; // 深度無視(常に見える = X線表示)

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

    public void SetXRay(bool enabled)
    {
        xRayOutline = enabled;
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
        maskMaterial.SetFloat(ZTestModeId, xRayOutline ? ZTEST_ALWAYS : ZTEST_LEQUAL);
    }

#if UNITY_EDITOR
    void OnValidate() => ApplyToMaterial();
#endif
}
