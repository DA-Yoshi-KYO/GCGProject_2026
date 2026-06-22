using UnityEngine;

/// <summary>
/// オブジェクトごとにアウトラインの色・太さを制御する
/// SkinnedMeshRenderer または MeshRenderer を持つ GameObject にアタッチする
/// </summary>
[RequireComponent(typeof(Renderer))]
public class CS_OutlineController
{
    Renderer renderer;
    MaterialPropertyBlock mpb;

    static readonly int colorID = Shader.PropertyToID("_OutlineColor");
    static readonly int widthID = Shader.PropertyToID("_OutlineWidth");

    // ── コンストラクタ ────────────────────────────────────────

    public CS_OutlineController(GameObject go)
    {
        renderer = go.GetComponent<Renderer>();
        Init();
    }

    public CS_OutlineController(Renderer rend)
    {
        renderer = rend;
        Init();
    }

    void Init()
    {
        if (renderer == null)
        {
            Debug.LogWarning("[OutlineController] Renderer が見つかりません");
            return;
        }
        mpb = new MaterialPropertyBlock();
        // 既存のMPBがあれば引き継ぐ
        renderer.GetPropertyBlock(mpb);
    }

    // ── 外部API ───────────────────────────────────────────────

    public void SetOutlineAlpha(float alpha)
    {
        if (renderer == null) return;
        renderer.GetPropertyBlock(mpb);
        Color currentColor = mpb.GetColor(colorID);
        currentColor.a = alpha;
        mpb.SetColor(colorID, currentColor);
        Debug.Log("color" + currentColor.a);
        renderer.SetPropertyBlock(mpb);
    }

    public void SetOutlineColor(Color color)
    {
        if (renderer == null) return;
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorID, color);
        renderer.SetPropertyBlock(mpb);
    }

    public void SetOutlineWidth(float width)
    {
        if (renderer == null) return;
        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat(widthID, width);
        renderer.SetPropertyBlock(mpb);
    }

    public void SetOutline(Color color, float width)
    {
        if (renderer == null) return;
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorID, color);
        mpb.SetFloat(widthID, width);
        renderer.SetPropertyBlock(mpb);
    }

    /// <summary>MPBをリセットしてデフォルト値に戻す</summary>
    public void ResetOutline()
    {
        if (renderer == null) return;
        mpb.Clear();
        renderer.SetPropertyBlock(mpb);
    }
}
