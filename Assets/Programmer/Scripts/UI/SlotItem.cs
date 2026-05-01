
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class GimmickSlotItem : MonoBehaviour
{
    // ──────────────────────────────────────────
    //  Inspector 参照
    // ──────────────────────────────────────────
    [Header("子要素")]
    [SerializeField] private Image gimmickIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("背景")]
    [SerializeField] private Image slotBackground;

    [Header("カラー設定")]
    [SerializeField] private Color bgSelected = new Color(0.95f, 0.82f, 0.28f, 0.15f);
    [SerializeField] private Color bgDefault = new Color(1f, 1f, 1f, 0.04f);
    [SerializeField] private Color borderSelected = new Color(0.95f, 0.82f, 0.28f, 1f);
    [SerializeField] private Color borderDefault = new Color(0.42f, 0.35f, 0.17f, 1f);

    [Header("選択中の縁取り（Outline / Border Image）")]
    [Tooltip("選択中に表示する枠画像（省略可）")]
    [SerializeField] private Image selectionBorder;

    // ──────────────────────────────────────────
    //  セットアップ
    // ──────────────────────────────────────────
    /// <summary>
    /// GimmickSelectUI から呼ばれる初期化メソッド
    /// </summary>
    /// <param name="gimmickName">表示名</param>
    /// <param name="soulCost">必要ソウル数</param>
    /// <param name="icon">アイコンSprite（nullなら非表示）</param>
    public void Setup(string gimmickName, int soulCost, Sprite icon)
    {
        if (nameText != null)
            nameText.text = gimmickName;

        if (costText != null)
            costText.text = soulCost > 0 ? soulCost.ToString() : "—";

        if (gimmickIcon != null)
        {
            if (icon != null)
            {
                gimmickIcon.sprite = icon;
                gimmickIcon.enabled = true;
            }
            else
            {
                gimmickIcon.enabled = false;
            }
        }

        // 初期状態は非選択
        ApplyVisual(false, Color.clear, Color.clear);
    }

    // ──────────────────────────────────────────
    //  選択状態の反映
    // ──────────────────────────────────────────
    /// <summary>
    /// 選択状態を視覚的に反映する
    /// </summary>
    /// <param name="selected">選択中かどうか</param>
    /// <param name="activeColor">選択中のアクセントカラー</param>
    /// <param name="defaultColor">非選択時の文字色</param>
    public void SetSelected(bool selected, Color activeColor, Color defaultColor)
    {
        ApplyVisual(selected, activeColor, defaultColor);
    }

    // ──────────────────────────────────────────
    //  内部：ビジュアル更新
    // ──────────────────────────────────────────
    private void ApplyVisual(bool selected, Color activeColor, Color defaultColor)
    {
        // 背景色
        if (slotBackground != null)
            slotBackground.color = selected ? bgSelected : bgDefault;

        // テキスト色
        if (nameText != null)
            nameText.color = selected ? activeColor : defaultColor;

        // コスト色（常にソウルカラー）
        // costTextの色はInspectorで固定しておくことを推奨

        // 選択枠の表示切り替え
        if (selectionBorder != null)
        {
            selectionBorder.enabled = selected;
            if (selected)
                selectionBorder.color = borderSelected;
        }
    }

    // ──────────────────────────────────────────
    //  アイコンの動的差し替え（ランタイム用）
    // ──────────────────────────────────────────
    public void SetIcon(Sprite icon)
    {
        if (gimmickIcon == null) return;
        if (icon != null)
        {
            gimmickIcon.sprite = icon;
            gimmickIcon.enabled = true;
        }
        else
        {
            gimmickIcon.enabled = false;
        }
    }
}
