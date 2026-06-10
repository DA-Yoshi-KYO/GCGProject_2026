using UnityEngine;

/// <summary>
/// オブジェクトごとにアウトラインの色・太さを制御する
/// SkinnedMeshRenderer または MeshRenderer を持つ GameObject にアタッチする
/// </summary>
[RequireComponent(typeof(Renderer))]
public class CS_OutlineController : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] Color  _outlineColor = new Color(0.03f, 0.03f, 0.03f, 1f);
    [SerializeField, Range(0f, 1f)] float _outlineWidth = 1f;

    Renderer          _renderer;
    MaterialPropertyBlock _mpb;

    // ShaderのプロパティID（文字列検索のコストを初回だけにする）
    static readonly int ColorID = Shader.PropertyToID("_OutlineColor");
    static readonly int WidthID = Shader.PropertyToID("_OutlineWidth");

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb      = new MaterialPropertyBlock();
        Apply();
    }

    // ── 外部から呼ぶAPI ───────────────────────────────────────

    /// <summary>アウトラインの色を変更する</summary>
    public void SetOutlineColor(Color color)
    {
        _outlineColor = color;
        Apply();
    }

    /// <summary>アウトラインの太さを変更する</summary>
    public void SetOutlineWidth(float width)
    {
        _outlineWidth = width;
        Apply();
    }

    /// <summary>色と太さを同時に変更する</summary>
    public void SetOutline(Color color, float width)
    {
        _outlineColor = color;
        _outlineWidth = width;
        Apply();
    }

    // ── 内部処理 ─────────────────────────────────────────────

    void Apply()
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorID, _outlineColor);
        _mpb.SetFloat(WidthID, _outlineWidth);
        _renderer.SetPropertyBlock(_mpb);
    }

#if UNITY_EDITOR
    // Inspectorで値を変えたときリアルタイム反映
    void OnValidate()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_mpb      == null) _mpb      = new MaterialPropertyBlock();
        Apply();
    }
#endif
}
