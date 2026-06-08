using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class GimmickSelectUI : MonoBehaviour
{
    [Header("位置アクター")]
    [SerializeField] private GameObject Center;
    [SerializeField] private GameObject Left;
    [SerializeField] private GameObject Right;

    [Header("所持数テキスト（TMP_Text）")]
    [SerializeField] private TMP_Text countText;

    [Header("アニメーションスピード")]
    [SerializeField] private float animationSpeed = 8f;

    [Header("左右スロットの透明度 (0〜1)")]
    [SerializeField] private float sideAlpha = 0.45f;

    // ──────────────────────────────────────────
    // CTマスク設定
    // ──────────────────────────────────────────
    [Header("CT マスク（Center スロット直下の Filled Image）")]
    [Tooltip("消費1個目のグレーマスク Image（ImageType=Filled, FillMethod=Radial360）")]
    [SerializeField] private Image ctMask1;
    [Tooltip("消費2個目以降のグレーマスク Image（同設定, ctMask1より少し暗い色）")]
    [SerializeField] private Image ctMask2;

    [Header("CT マスク 基本色")]
    [SerializeField] private Color ctColor1 = new Color(0.25f, 0.25f, 0.25f, 0.85f);
    [SerializeField] private Color ctColor2 = new Color(0.10f, 0.10f, 0.10f, 0.85f);

    // ──────────────────────────────
    private CS_PlayerAction playerAction;
    private GimmickManager gimmickManager;

    private Image centerImg, leftImg, rightImg;
    private RectTransform centerRT, leftRT, rightRT;

    private Vector2 centerAnchor, leftAnchor, rightAnchor;
    private float slotSpacing;

    private int prevIndex = -1;
    private bool isAnimating = false;
    private float animT = 0f;

    private Vector2 centerFrom, leftFrom, rightFrom;
    private Vector2 centerTo, leftTo, rightTo;

    // ──────────────────────────────
    private void Start()
    {
        centerImg = Center.GetComponent<Image>();
        leftImg = Left.GetComponent<Image>();
        rightImg = Right.GetComponent<Image>();

        centerRT = Center.GetComponent<RectTransform>();
        leftRT = Left.GetComponent<RectTransform>();
        rightRT = Right.GetComponent<RectTransform>();

        centerAnchor = centerRT.anchoredPosition;
        leftAnchor = leftRT.anchoredPosition;
        rightAnchor = rightRT.anchoredPosition;

        slotSpacing = centerAnchor.x - leftAnchor.x;

        InitCTMask(ctMask1, ctColor1);
        InitCTMask(ctMask2, ctColor2);
    }

    private void Update()
    {
        if (playerAction == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerAction = player.GetComponent<CS_PlayerAction>();
                gimmickManager = player.GetComponent<GimmickManager>();
            }
            return;
        }

        int idx = playerAction.currentGimmickIndex;
        UpdateCountText(idx);
        // ── インデックス変化検知 ──
        if (idx != prevIndex)
        {
            int slideDir = 1;
            if (prevIndex >= 0)
            {
                int count = playerAction.gimmickKind.Count;
                int rawDiff = idx - prevIndex;
                slideDir = (Mathf.Abs(rawDiff) <= count / 2)
                    ? (int)Mathf.Sign(rawDiff)
                    : -(int)Mathf.Sign(rawDiff);
            }

            RefreshImages(idx);

            float offset = slotSpacing * -slideDir;
            centerFrom = centerAnchor + new Vector2(offset, 0f);
            leftFrom = leftAnchor + new Vector2(offset, 0f);
            rightFrom = rightAnchor + new Vector2(offset, 0f);
            centerTo = centerAnchor;
            leftTo = leftAnchor;
            rightTo = rightAnchor;

            centerRT.anchoredPosition = centerFrom;
            leftRT.anchoredPosition = leftFrom;
            rightRT.anchoredPosition = rightFrom;

            isAnimating = true;
            animT = 0f;
            prevIndex = idx;
        }

        // ── スライドアニメーション ──
        if (isAnimating)
        {
            animT += Time.deltaTime * animationSpeed;
            float t = Mathf.Clamp01(animT);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            centerRT.anchoredPosition = Vector2.Lerp(centerFrom, centerTo, ease);
            leftRT.anchoredPosition = Vector2.Lerp(leftFrom, leftTo, ease);
            rightRT.anchoredPosition = Vector2.Lerp(rightFrom, rightTo, ease);

            if (t >= 1f)
            {
                centerRT.anchoredPosition = centerTo;
                leftRT.anchoredPosition = leftTo;
                rightRT.anchoredPosition = rightTo;
                isAnimating = false;
            }
        }

        // ── CT マスク更新（毎フレーム） ──
        UpdateCTMask(idx);
    }

    /// <summary>
    /// Filled Image を CT マスク用に初期化する
    /// </summary>
    private void InitCTMask(Image mask, Color col)
    {
        if (mask == null) return;
        mask.type = Image.Type.Filled;
        mask.fillMethod = Image.FillMethod.Radial360;
        // 左（9時方向）を起点にして時計回り
        mask.fillOrigin = (int)Image.Origin360.Left;
        mask.fillClockwise = true;
        mask.fillAmount = 0f;
        mask.color = col;
        mask.raycastTarget = false;
    }

    /// <summary>
    /// 現在選択中ギミックの CT 状態をマスクに反映する
    /// </summary>
    private void UpdateCTMask(int idx)
    {
        if (gimmickManager == null) return;

        GimmickBase gb = GetGimmickBase(idx);
        if (gb == null)
        {
            SetMaskFill(ctMask1, 0f);
            SetMaskFill(ctMask2, 0f);
            return;
        }

        Gimmick tag = gb.GetGimmickTag();
        int maxNum = gimmickManager.GetMaxNum(tag);
        int currentNum = gimmickManager.GetCurrentNum(tag);
        int consumed = maxNum - currentNum;
        float coolTime = gimmickManager.GetCoolTime(tag);
        float totalCool = GetTotalCoolTime(tag);

        float coolRatio = (totalCool > 0f)
            ? Mathf.Clamp01(coolTime / totalCool)
            : 0f;

        if (consumed <= 0)
        {
            SetMaskFill(ctMask1, 0f);
            SetMaskFill(ctMask2, 0f);
        }
        else if (consumed == 1)
        {
            SetMaskFill(ctMask1, coolRatio);
            SetMaskFill(ctMask2, 0f);
        }
        else
        {
            SetMaskFill(ctMask1, 1f);
            SetMaskFill(ctMask2, coolRatio);
        }
    }

    /// <summary>
    /// GimmickInfo から登録済みクールタイムを取得するラッパー
    /// ※ GimmickManager に GetTotalCoolTime が無い場合はここで管理
    /// </summary>
    private float GetTotalCoolTime(Gimmick tag)
    {
        return totalCoolTimeCache;
    }

    private float totalCoolTimeCache = 1f;

    private void RefreshCoolTimeCache(int idx)
    {
        if (gimmickManager == null) return;
        GimmickBase gb = GetGimmickBase(idx);
        if (gb == null) return;

        Gimmick tag = gb.GetGimmickTag();
        float coolTime = gimmickManager.GetCoolTime(tag);
        if (coolTime > totalCoolTimeCache)
            totalCoolTimeCache = coolTime;
    }

    private static void SetMaskFill(Image mask, float fill)
    {
        if (mask == null) return;
        mask.fillAmount = fill;
        mask.enabled = fill > 0f;
    }

    private void UpdateCountText(int idx)
    {
        if (countText == null || gimmickManager == null) return;
        GimmickBase gb = GetGimmickBase(idx);
        if (gb == null) return;
        int current = gimmickManager.GetCurrentNum(gb.GetGimmickTag());
        countText.text = current.ToString();
    }

    private void RefreshImages(int idx)
    {
        int count = playerAction.gimmickKind.Count;
        if (count == 0) return;

        int leftIdx = (idx - 1 + count) % count;
        int rightIdx = (idx + 1) % count;

        SetImage(centerImg, GetSprite(idx), 1f);
        SetImage(leftImg, GetSprite(leftIdx), sideAlpha);
        SetImage(rightImg, GetSprite(rightIdx), sideAlpha);

        // CT キャッシュ更新
        RefreshCoolTimeCache(idx);
    }

    private Sprite GetSprite(int idx)
    {
        var kinds = playerAction.gimmickKind;
        if (idx < 0 || idx >= kinds.Count) return null;
        return kinds[idx]?.GetComponent<GimmickBase>()?.gimmickImage;
    }

    private GimmickBase GetGimmickBase(int idx)
    {
        var kinds = playerAction.gimmickKind;
        if (idx < 0 || idx >= kinds.Count) return null;
        return kinds[idx]?.GetComponent<GimmickBase>();
    }

    private static void SetImage(Image img, Sprite sprite, float alpha)
    {
        if (img == null) return;
        img.sprite = sprite;
        Color c = img.color;
        c.a = (sprite != null) ? alpha : 0f;
        img.color = c;
    }
}
