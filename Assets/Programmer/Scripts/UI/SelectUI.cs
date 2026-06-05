
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class GimmickSelectUI : MonoBehaviour
{
    // ──────────────────────────────────────────
    //  Inspector 参照
    // ──────────────────────────────────────────
    private CS_PlayerAction playerAction;
    private CS_PlayerData playerData;

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

    [Header("ナビゲーションボタン")]
    [SerializeField] private Button btnLeft;
    [SerializeField] private Button btnRight;

    [Header("所有数表示")]
    [SerializeField] private TextMeshProUGUI Count;

    [Header("カラー設定")]
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
        ApplySlotSizes();
        ForceUpdateAll();
    }

    // ──────────────────────────────────────────
    //  毎フレーム：変化検知 → UI更新
    // ──────────────────────────────────────────
    private void Update()
    {
        if (playerAction == null || playerData == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;
            playerAction = player.GetComponent<CS_PlayerAction>();
            playerData = player.GetComponent<CS_PlayerData>();
        }

        int idx = playerAction.currentGimmickIndex;
        bool isSetting = playerData.currentMode == CS_PlayerData.PlayerMode.Setting;

        if (idx != cachedIndex ||isSetting != cachedMode)
        {
            cachedIndex = idx;
            cachedMode = isSetting;
            Refresh(idx,isSetting);
        }
    }

    // ──────────────────────────────────────────
    //  全UI更新
    // ──────────────────────────────────────────
    private void Refresh(int idx,bool isSetting)
    {
        var list = playerAction.gimmickKind;
        int count = list.Count;
        if (count == 0) return;

        int iLeft = (idx - 1 + count) % count;
        int iRight = (idx + 1) % count;

        SetupSlot(slotLeft, list[iLeft], false);
        SetupSlot(slotCenter, list[idx], true);
        SetupSlot(slotRight, list[iRight], false);

    }

    // ──────────────────────────────────────────
    //  1スロット分のセットアップ
    // ──────────────────────────────────────────
    private void SetupSlot(GimmickSlotItem slot, GameObject gimmickObj, bool isSelected)
    {
        if (slot == null || gimmickObj == null) return;

        var gb = gimmickObj.GetComponent<GimmickBase>();
        string name = gb != null ? gb.gimmick.ToString() : gimmickObj.name;
        int cost = 0;

        slot.Setup(name, cost, gb.gimmickImage);
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
        cachedMode = playerData.currentMode == CS_PlayerData.PlayerMode.Setting;
        Refresh(cachedIndex,cachedMode);
    }
}
