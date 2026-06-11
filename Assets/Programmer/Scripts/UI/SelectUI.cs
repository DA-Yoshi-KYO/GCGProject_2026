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
    private Material ctMaskMat1;
    [Tooltip("消費2個目以降のグレーマスク Image（同設定, ctMask1より少し暗い色）")]
    [SerializeField] private Image ctMask2;
    private Material ctMaskMat2;

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

        InitCTMask(ctMask1, ctColor1, ref ctMaskMat1);
        InitCTMask(ctMask2, ctColor2, ref ctMaskMat2);
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

            float offset = slotSpacing * slideDir;
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

        UpdateCTMask(idx);
    }

    // ============================================================
    //  CT マスク
    // ============================================================

    /// <summary>
    /// Filled Image を CT マスク用に初期化する
    /// </summary>
    private void InitCTMask(Image mask, Color col, ref Material mat)
    {
        if (mask == null) return;
        mask.raycastTarget = false;

        mat = mask.material;
        mat.SetFloat("_FillAmount", 0f);
        mat.SetTexture("_MainTex", mask.sprite.texture);
        mat.SetColor("_Color", col);
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
            SetMaskFill(ctMaskMat1, 0f);
            SetMaskFill(ctMaskMat2, 0f);
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

        CoolTimeRatio = coolRatio;

        if (consumed <= 0)
        {
            SetMaskFill(ctMaskMat1, 0f);
            SetMaskFill(ctMaskMat2, 0f);
        }
        else if (consumed == 1)
        {
            SetMaskFill(ctMaskMat1, coolRatio);
            SetMaskFill(ctMaskMat2, 0f);
        }
        else
        {
            SetMaskFill(ctMaskMat1, 1f);
            SetMaskFill(ctMaskMat2, coolRatio);
        }
    }

    private System.Collections.Generic.Dictionary<Gimmick, float> ctMaxCache
        = new System.Collections.Generic.Dictionary<Gimmick, float>();

    private System.Collections.Generic.Dictionary<Gimmick, float> ctPrevTime
        = new System.Collections.Generic.Dictionary<Gimmick, float>();

    /// <summary>
    /// CT最大値を返す。残り時間が前フレームより増えた瞬間をCT開始と判断して記録する。
    /// </summary>
    private float GetTotalCoolTime(Gimmick tag)
    {
        float remaining = gimmickManager.GetCoolTime(tag);
        float prev = ctPrevTime.ContainsKey(tag) ? ctPrevTime[tag] : 0f;

        if (remaining > prev + 0.01f)
            ctMaxCache[tag] = remaining;

        ctPrevTime[tag] = remaining;

        return ctMaxCache.ContainsKey(tag) ? ctMaxCache[tag] : 1f;
    }

    private void RefreshCoolTimeCache(int idx) { }

    /// <summary>
    /// 現在選択中ギミックの CT 進捗（0.0=CT終了 / 1.0=CT最大）
    /// シェーダー側から参照する用
    /// </summary>
    public float CoolTimeRatio { get; private set; } = 0f;

    private static void SetMaskFill(Material maskMat, float fill)
    {
        if (maskMat == null) return;
        maskMat.SetFloat("_FillAmount", fill);
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
