
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

[RequireComponent(typeof(RectTransform))]
public class GimmickSelectUI : MonoBehaviour
{
    // ──────────────────────────────────────────
    //  Inspector 参照
    // ──────────────────────────────────────────
    [Header("接続先")]
    [Tooltip("プレイヤーオブジェクト")]
    [SerializeField] private GameObject player;
    private PlayerAction playerAction;
    private PlayerData playerData;

    [Header("3枠スロット（固定）")]
    [Tooltip("左隣スロット（小さく表示）")]
    [SerializeField] private GimmickSlotItem slotLeft;
    [Tooltip("選択中スロット（大きく表示）")]
    [SerializeField] private GimmickSlotItem slotCenter;
    [Tooltip("右隣スロット（小さく表示）")]
    [SerializeField] private GimmickSlotItem slotRight;

    [Header("サイズ設定")]
    [Tooltip("選択中スロットの幅・高さ（px）")]
    [SerializeField] private Vector2 centerSlotSize = new Vector2(82f, 82f);
    [Tooltip("左右スロットの幅・高さ（px）")]
    [SerializeField] private Vector2 sideSlotSize = new Vector2(60f, 60f);

    [Header("情報テキスト")]
    [SerializeField] private TextMeshProUGUI soulText;
    [SerializeField] private TextMeshProUGUI modeBadgeText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCostText;
    [SerializeField] private TextMeshProUGUI itemIndexText;

    [Header("ナビゲーションボタン")]
    [SerializeField] private Button btnLeft;
    [SerializeField] private Button btnRight;

    [Header("カラー設定")]
    [SerializeField] private Color colorNormalMode = new Color(0.44f, 0.75f, 0.31f);
    [SerializeField] private Color colorSettingMode = new Color(0.91f, 0.79f, 0.25f);
    [SerializeField] private Color colorSelected = new Color(0.95f, 0.82f, 0.28f);
    [SerializeField] private Color colorSide = new Color(0.55f, 0.47f, 0.25f);

    // ──────────────────────────────────────────
    //  内部状態
    // ──────────────────────────────────────────
    private int cachedIndex = -1;
    private int cachedSoul = -1;
    private bool cachedMode;

    // ──────────────────────────────────────────
    //  初期化
    // ──────────────────────────────────────────
    private void Start()
    {
        
        player = GameObject.Find("Player");
        playerAction = SceneAsset.FindObjectOfType<PlayerAction>();
        playerData = SceneAsset.FindObjectOfType<PlayerData>();

        ApplySlotSizes();
        ForceUpdateAll();
    }

    // ──────────────────────────────────────────
    //  毎フレーム：変化検知 → UI更新
    // ──────────────────────────────────────────
    private void Update()
    {
        if (playerAction == null || playerData == null) return;

        int idx = playerAction.currentGimmickIndex;
        int soul = playerAction.currentSoul;
        bool isSetting = playerData.currentMode == PlayerData.PlayerMode.Setting;

        if (idx != cachedIndex || soul != cachedSoul || isSetting != cachedMode)
        {
            cachedIndex = idx;
            cachedSoul = soul;
            cachedMode = isSetting;
            Refresh(idx, soul, isSetting);
        }
    }

    // ──────────────────────────────────────────
    //  全UI更新
    // ──────────────────────────────────────────
    private void Refresh(int idx, int soul, bool isSetting)
    {
        var list = playerAction.gimmickKind;
        int count = list.Count;
        if (count == 0) return;

        int iLeft = (idx - 1 + count) % count;
        int iRight = (idx + 1) % count;

        SetupSlot(slotLeft, list[iLeft], false);
        SetupSlot(slotCenter, list[idx], true);
        SetupSlot(slotRight, list[iRight], false);

        // ソウル表示
        if (soulText != null)
            soulText.text = soul.ToString();

        // モードバッジ
        if (modeBadgeText != null)
        {
            modeBadgeText.text = isSetting ? "設置モード" : "通常モード";
            modeBadgeText.color = isSetting ? colorSettingMode : colorNormalMode;
        }

        // 選択中アイテム情報
        var gb = list[idx].GetComponent<GimmickBase>();
        if (itemNameText != null)
            itemNameText.text = gb != null ? gb.gimmick.ToString() : list[idx].name;
        if (itemCostText != null)
            itemCostText.text = gb != null ? gb.requiredSoul.ToString() : "—";
        if (itemIndexText != null)
            itemIndexText.text = $"{idx + 1} / {count}";
    }

    // ──────────────────────────────────────────
    //  1スロット分のセットアップ
    // ──────────────────────────────────────────
    private void SetupSlot(GimmickSlotItem slot, GameObject gimmickObj, bool isSelected)
    {
        if (slot == null || gimmickObj == null) return;

        var gb = gimmickObj.GetComponent<GimmickBase>();
        string name = gb != null ? gb.gimmick.ToString() : gimmickObj.name;
        int cost = gb != null ? gb.requiredSoul : 0;

        // アイコンSprite：GimmickBase側にSpriteフィールドを追加するか、
        // SpriteRenderer / UIの子Imageから取得する運用を推奨。
        slot.Setup(name, cost, null);
        slot.SetSelected(isSelected, colorSelected, colorSide);
    }

    // ──────────────────────────────────────────
    //  スロットサイズをRectTransformに適用
    // ──────────────────────────────────────────
    private void ApplySlotSizes()
    {
        SetRectSize(slotLeft, sideSlotSize);
        SetRectSize(slotCenter, centerSlotSize);
        SetRectSize(slotRight, sideSlotSize);
    }

    private void SetRectSize(GimmickSlotItem slot, Vector2 size)
    {
        if (slot == null) return;
        var rt = slot.GetComponent<RectTransform>();
        if (rt != null)
            rt.sizeDelta = size;
    }


    // ──────────────────────────────────────────
    //  強制フル更新
    // ──────────────────────────────────────────
    private void ForceUpdateAll()
    {
        if (playerAction == null) return;
        cachedIndex = playerAction.currentGimmickIndex;
        cachedSoul = playerAction.currentSoul;
        cachedMode = playerData.currentMode == PlayerData.PlayerMode.Setting;
        Refresh(cachedIndex, cachedSoul, cachedMode);
    }
}
