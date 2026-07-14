using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class GimmickSelectUI : MonoBehaviour
{
    // ==============================================================
    //  Inspector（既存のシリアライズ名は Scene 側の参照を壊さないよう維持）
    // ==============================================================

    [Header("位置アクター")]
    [SerializeField] private GameObject Center;
    [SerializeField] private GameObject Left;
    [SerializeField] private GameObject Right;
    [SerializeField] private GameObject TextImage;

    [Header("所持数テキスト（TMP_Text）")]
    [SerializeField] private NumberView countText;

    [Header("アニメーションスピード")]
    [SerializeField] private float animationSpeed = 8f;

    [Header("左右スロットの透明度 (0〜1)")]
    [SerializeField] private float sideAlpha = 0.45f;

    [Header("CT マスク（Center スロット直下の Filled Image）")]
    [Tooltip("消費1個目のグレーマスク Image（ImageType=Filled, FillMethod=Radial360）")]
    [SerializeField] private Image ctMask1;
    [Tooltip("消費2個目以降のグレーマスク Image（同設定, ctMask1より少し暗い色）")]
    [SerializeField] private Image ctMask2;

    [Header("CT マスク 基本色")]
    [SerializeField] private Color ctColor1 = new Color(0.25f, 0.25f, 0.25f, 0.85f);
    [SerializeField] private Color ctColor2 = new Color(0.10f, 0.10f, 0.10f, 0.85f);

    // ==============================================================
    //  内部型
    // ==============================================================

    /// <summary>
    /// 1スロット分（Center / Left / Right）の Image・RectTransform・
    /// スライドアニメーション用の from/to をまとめたもの。
    /// これを配列化することで Update 内の3重の重複コードを解消する。
    /// </summary>
    private class Slot
    {
        public readonly Image Image;
        public readonly RectTransform Rect;
        public readonly Vector2 Anchor;
        public readonly float TargetAlpha;

        public Vector2 From;
        public Vector2 To;

        public Slot(GameObject go, float targetAlpha)
        {
            Image = go.GetComponent<Image>();
            Rect = go.GetComponent<RectTransform>();
            Anchor = Rect.anchoredPosition;
            TargetAlpha = targetAlpha;
        }

        public void SetSprite(Sprite sprite)
        {
            if (Image == null) return;
            Image.sprite = sprite;
            Color c = Image.color;
            c.a = (sprite != null) ? TargetAlpha : 0f;
            Image.color = c;
        }
    }

    /// <summary>
    /// CT（クールタイム）マスク1枚分。マテリアルの取得と
    /// シェーダープロパティの書き込みをここに閉じ込める。
    /// </summary>
    private class CTMask
    {
        private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Image image;
        private readonly Material material;

        public CTMask(Image maskImage, Color baseColor)
        {
            image = maskImage;
            if (image == null) return;

            image.raycastTarget = false;
            material = image.material;

            if (image.sprite != null)
            {
                material.SetTexture(MainTexId, image.sprite.texture);
            }
            material.SetColor(ColorId, baseColor);
            material.SetFloat(FillAmountId, 1f);
        }

        public void SetFill(float ratio)
        {
            material?.SetFloat(FillAmountId, ratio);
        }
    }

    // ==============================================================
    //  内部状態
    // ==============================================================

    private CS_PlayerAction playerAction;
    private GimmickList gimmickManager;

    private Slot centerSlot, leftSlot, rightSlot;
    private Image textImg;

    private CTMask ctMask1Ctrl;
    private CTMask ctMask2Ctrl;

    // 「タグ → 最大CT時間」のキャッシュ。毎フレーム線形探索しないための索引。
    private Dictionary<Gimmick, float> maxCoolTimeByTag;

    private int prevIndex = -1;
    private int prevGimmickCount = -1;
    private bool isAnimating = false;
    private float animT = 0f;

    /// <summary>
    /// 現在選択中ギミックの CT 進捗（0.0=CT終了 / 1.0=CT最大）。シェーダー側から参照する用。
    /// </summary>
    public float CoolTimeRatio { get; private set; } = 0f;

    // ==============================================================
    //  初期化
    // ==============================================================

    private void Start()
    {
        centerSlot = new Slot(Center, 1f);
        leftSlot = new Slot(Left, sideAlpha);
        rightSlot = new Slot(Right, sideAlpha);
        textImg = TextImage.GetComponent<Image>();

        ctMask1Ctrl = new CTMask(ctMask1, ctColor1);
        ctMask2Ctrl = new CTMask(ctMask2, ctColor2);

        countText.SetTensView(false);
    }

    // ==============================================================
    //  メインループ
    // ==============================================================

    private void Update()
    {
        if (!TryResolvePlayer()) return;

        int idx = playerAction.currentGimmickIndex;

        UpdateCountText(idx);

        int gimmickCount = gimmickManager != null ? gimmickManager.GetCurrentGimmick().Count : 0;

        if (idx != prevIndex)
        {
            OnIndexChanged(idx);
        }
        else if (gimmickCount != prevGimmickCount)
        {
            // 選択中インデックスは変わらなくても、所持ギミックの種類数が増減すると
            // 左右に表示される候補が変わるため、アニメーション無しで即座に反映する
            RefreshImages(idx);
        }

        prevGimmickCount = gimmickCount;

        AnimateSlots();
        UpdateCTMask(idx);
    }

    /// <summary>
    /// プレイヤー参照の解決。見つかるまでは何もしない。
    /// 見つかった時点で CT キャッシュも一度だけ構築する。
    /// </summary>
    private bool TryResolvePlayer()
    {
        if (playerAction != null) return true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        playerAction = player.GetComponent<CS_PlayerAction>();
        gimmickManager = player.GetComponent<GimmickList>();

        BuildCoolTimeCache();
        return playerAction != null;
    }

    private void BuildCoolTimeCache()
    {
        maxCoolTimeByTag = new Dictionary<Gimmick, float>();
        if (gimmickManager == null) return;

        foreach (var item in gimmickManager.GetGimmickInfoDataList())
        {
            maxCoolTimeByTag[item.gimmickTag] = item.gimmickInfo.coolTime;
        }
    }

    // ==============================================================
    //  インデックス切り替え（スライドアニメーション開始）
    // ==============================================================

    private void OnIndexChanged(int idx)
    {
        int slideDir = 1;
        if (prevIndex >= 0)
        {
            int count = gimmickManager.GetCurrentGimmick().Count;
            int rawDiff = idx - prevIndex;
            slideDir = (Mathf.Abs(rawDiff) <= count / 2)
                ? (int)Mathf.Sign(rawDiff)
                : -(int)Mathf.Sign(rawDiff);
        }

        RefreshImages(idx);
        BeginSlide(slideDir);

        prevIndex = idx;
    }

    private void BeginSlide(int slideDir)
    {
        float offset = 0f; // slotSpacing はスロット間の距離。中央-左の差分から算出する。
        if (centerSlot != null && leftSlot != null)
        {
            offset = (centerSlot.Anchor.x - leftSlot.Anchor.x) * slideDir;
        }

        foreach (var slot in EnumerateSlots())
        {
            slot.From = slot.Anchor + new Vector2(offset, 0f);
            slot.To = slot.Anchor;
            slot.Rect.anchoredPosition = slot.From;
        }

        isAnimating = true;
        animT = 0f;
    }

    private void AnimateSlots()
    {
        if (!isAnimating) return;

        animT += Time.unscaledDeltaTime * animationSpeed;
        float t = Mathf.Clamp01(animT);
        float ease = 1f - Mathf.Pow(1f - t, 3f);

        foreach (var slot in EnumerateSlots())
        {
            slot.Rect.anchoredPosition = Vector2.Lerp(slot.From, slot.To, ease);
        }

        if (t >= 1f)
        {
            foreach (var slot in EnumerateSlots())
            {
                slot.Rect.anchoredPosition = slot.To;
            }
            isAnimating = false;
        }
    }

    private IEnumerable<Slot> EnumerateSlots()
    {
        yield return centerSlot;
        yield return leftSlot;
        yield return rightSlot;
    }

    // ==============================================================
    //  表示更新
    // ==============================================================

    private void RefreshImages(int idx)
    {
        var kinds = gimmickManager.GetCurrentGimmick();
        int count = kinds.Count;

        if (count == 0)
        {
            // ギミックを何も所持していない場合は全スロットを非表示にする
            centerSlot.SetSprite(null);
            leftSlot.SetSprite(null);
            rightSlot.SetSprite(null);
            SetTextImage(null);
            return;
        }

        int leftIdx = (idx - 1 + count) % count;
        int rightIdx = (idx + 1) % count;

        centerSlot.SetSprite(GetSprite(idx));
        leftSlot.SetSprite(GetSprite(leftIdx));
        rightSlot.SetSprite(GetSprite(rightIdx));

        SetTextImage(GetGimmickBase(idx)?.gimmickTextImage);
    }

    private void SetTextImage(Sprite sprite)
    {
        if (textImg == null) return;
        textImg.sprite = sprite;
        Color c = textImg.color;
        c.a = (sprite != null) ? 1f : 0f;
        textImg.color = c;
    }

    private void UpdateCountText(int idx)
    {
        if (countText == null || gimmickManager == null) return;

        GimmickBase gb = GetGimmickBase(idx);
        if (gb == null) return;

        int current = gimmickManager.GetCurrentNum(gb.GetGimmickTag());
        countText.SetNumber(current);
    }

    private void UpdateCTMask(int idx)
    {
        GimmickBase gb = GetGimmickBase(idx);
        if (gb == null || maxCoolTimeByTag == null)
        {
            CoolTimeRatio = 0f;
            ctMask1Ctrl?.SetFill(0f);
            ctMask2Ctrl?.SetFill(0f);
            return;
        }

        Gimmick tag = gb.GetGimmickTag();
        float currentTime = gimmickManager.GetCoolTime(tag);

        if (!maxCoolTimeByTag.TryGetValue(tag, out float maxTime))
        {
            // キャッシュに無ければ辞書を再構築して一度だけリトライする
            BuildCoolTimeCache();
            maxCoolTimeByTag.TryGetValue(tag, out maxTime);
        }

        float ratio = (maxTime > 0f) ? Mathf.Clamp01(currentTime / maxTime) : 0f;
        CoolTimeRatio = ratio;
        ctMask1Ctrl?.SetFill(ratio);
        ctMask2Ctrl?.SetFill(ratio);
    }

    // ==============================================================
    //  ヘルパー
    // ==============================================================

    private Sprite GetSprite(int idx)
    {
        return GetGimmickBase(idx)?.gimmickImage;
    }

    private GimmickBase GetGimmickBase(int idx)
    {
        var kinds = gimmickManager.GetCurrentGimmick();
        if (idx < 0 || idx >= kinds.Count) return null;
        return kinds[idx].gimmickPrefab?.GetComponent<GimmickBase>();
    }
}
