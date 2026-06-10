using UnityEngine;

/// <summary>
/// オブジェクトごとにアウトラインの色・太さを制御する
/// SkinnedMeshRenderer または MeshRenderer を持つ GameObject にアタッチする
/// </summary>
[RequireComponent(typeof(Renderer))]
public class CS_OutlineController : MonoBehaviour
{
    Renderer _renderer;
    MaterialPropertyBlock _mpb;

    static readonly int ColorID = Shader.PropertyToID("_OutlineColor");
    static readonly int WidthID = Shader.PropertyToID("_OutlineWidth");

    // ── コンストラクタ ────────────────────────────────────────

    public CS_OutlineController(GameObject go)
    {
        _renderer = go.GetComponent<Renderer>();
        Init();
    }

    public CS_OutlineController(Renderer renderer)
    {
        _renderer = renderer;
        Init();
    }

    void Init()
    {
        if (_renderer == null)
        {
            Debug.LogWarning("[OutlineController] Renderer が見つかりません");
            return;
        }
        _mpb = new MaterialPropertyBlock();
        // 既存のMPBがあれば引き継ぐ
        _renderer.GetPropertyBlock(_mpb);
    }

    // ── 外部API ───────────────────────────────────────────────

    public void SetOutlineColor(Color color)
    {
        if (_renderer == null) return;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorID, color);
        _renderer.SetPropertyBlock(_mpb);
    }

    public void SetOutlineWidth(float width)
    {
        if (_renderer == null) return;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(WidthID, width);
        _renderer.SetPropertyBlock(_mpb);
    }

    public void SetOutline(Color color, float width)
    {
        if (_renderer == null) return;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorID, color);
        _mpb.SetFloat(WidthID, width);
        _renderer.SetPropertyBlock(_mpb);
    }

    /// <summary>MPBをリセットしてデフォルト値に戻す</summary>
    public void ResetOutline()
    {
        if (_renderer == null) return;
        _mpb.Clear();
        _renderer.SetPropertyBlock(_mpb);
    }
}
