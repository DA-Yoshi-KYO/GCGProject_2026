using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    [Header("EnemyIcon プレハブ")]
    [SerializeField] private GameObject enemyIconPrefab;

    [Header("EnemyParentの名前 ")]
    [SerializeField] private string enemyParentName;

    [Header("アイコン配置間隔（ピクセル）")]
    [SerializeField] private float iconSpacing = 50f;

    [Header("actor")]
    [SerializeField] private GameObject actor;

    [Header("別の部屋にいる敵をまとめて表示するUIのプレハブ")]
    [SerializeField] private GameObject otherRoomsEnemyPrefab;

    [Header("並び順が変わった際のスライド移動の速さ")]
    [SerializeField] private float repositionSpeed = 8f;

    // ──────────────────────────────
    private GameObject enemyManager;
    private CS_RoomPlayerPosition roomPlayerPosition;
    private GameObject otherRoomsEnemyInstance;
    private HashSet<GameObject> placedIcons = new HashSet<GameObject>();


    private Dictionary<Transform, GameObject> iconMap = new Dictionary<Transform, GameObject>();

    // ──────────────────────────────
    void Start()
    {
        GameObject roomManager = GameObject.Find("RoomManager");
        if (roomManager != null)
        {
            roomPlayerPosition = roomManager.GetComponent<CS_RoomPlayerPosition>();
        }

        // 別の部屋にいる敵をまとめて表示するUIは、最初から1つだけ表示し続ける（敵が0体になっても消さない）
        if (otherRoomsEnemyPrefab != null)
        {
            otherRoomsEnemyInstance = Instantiate(otherRoomsEnemyPrefab, transform);
        }

        enemyManager = GameObject.Find(enemyParentName);
        if (enemyManager == null) {
            Debug.LogError($"EnemyUI: '{enemyParentName}' という名前のGameObjectが見つかりませんでした。");
            return;
        }

        for (int i = 0 ; i < enemyManager.transform.childCount ; i++)
            TryAddIcon(enemyManager.transform.GetChild(i));

        RealignIcons();
    }

    void Update()
    {
        if (enemyManager == null) {
            enemyManager = GameObject.Find(enemyParentName);
            return;
        }


        var currentChildren = new HashSet<Transform>();
        for (int i = 0 ; i < enemyManager.transform.childCount ; i++)
            currentChildren.Add(enemyManager.transform.GetChild(i));

        // 消えた敵、またはプレイヤーと別の部屋に移動した敵のアイコンを削除
        // （別の部屋にいる敵は個別表示せず、OtherRoomsEnemy側でまとめてカウントする）
        var removed = new List<Transform>();
        foreach (var kv in iconMap)
        {
            if (!currentChildren.Contains(kv.Key) || !IsSameRoomAsPlayer(kv.Key))
            {
                Destroy(kv.Value);
                placedIcons.Remove(kv.Value);
                removed.Add(kv.Key);
            }
        }
        foreach (var key in removed)
            iconMap.Remove(key);

        // プレイヤーと同じ部屋に新しく来た敵のアイコンを生成
        foreach (var child in currentChildren)
        {
            if (!iconMap.ContainsKey(child))
                TryAddIcon(child);
        }

        // アイコンの位置を再整列
        RealignIcons();
    }

    /// <summary>
    /// 敵がプレイヤーと同じ部屋にいるかどうかを判定する処理
    /// </summary>
    private bool IsSameRoomAsPlayer(Transform enemyTransform)
    {
        if (roomPlayerPosition == null || roomPlayerPosition.PlayerRoomData == null) return false;

        CS_ThiefAI thiefAI = enemyTransform.GetComponent<CS_ThiefAI>();
        if (thiefAI == null || thiefAI.read_MemorySystem == null) return false;

        return thiefAI.read_MemorySystem.read_CurrentRoomPoint == roomPlayerPosition.PlayerRoomData.transform;
    }

    // ============================================================
    //  アイコン追加（プレイヤーと同じ部屋にいる敵のみ）
    // ============================================================
    private void TryAddIcon(Transform enemyTransform)
    {
        if (!IsSameRoomAsPlayer(enemyTransform)) return;
        AddIcon(enemyTransform);
    }

    private void AddIcon(Transform enemyTransform)
    {
        GameObject icon = Instantiate(enemyIconPrefab, transform);

        EnemyIcon iconScript = icon.GetComponent<EnemyIcon>();
        if (iconScript != null)
        {
            CS_ThiefAI thiefAI = enemyTransform.GetComponent<CS_ThiefAI>();
            iconScript.SetScript(thiefAI);
            // 追加表示アニメーション（90度傾いた状態から回転しながら拡大）を再生する
            iconScript.PlayAppearAnimation();
        }

        iconMap[enemyTransform] = icon;
    }
    private void RealignIcons()
    {
        if (actor == null) return;
        RectTransform actorRT = actor.GetComponent<RectTransform>();
        if (actorRT == null) return;

        int index = 0;

        foreach (var kv in iconMap)
        {
            PlaceIcon(kv.Value, actorRT.anchoredPosition + new Vector2(0, -index * iconSpacing));
            index++;
        }

        // OtherRoomsEnemyは同じ並びの一番下に表示する
        if (otherRoomsEnemyInstance != null)
        {
            PlaceIcon(otherRoomsEnemyInstance, actorRT.anchoredPosition + new Vector2(0, -index * iconSpacing));
            index++;
        }
    }

    /// <summary>
    /// アイコンを指定位置へ配置する処理。
    /// 追加されたばかりのアイコンは登場演出（回転＋拡大）に専念させるため即座に配置し、
    /// 既に表示されているアイコンは、並び順が変わった際に一つずつずれながらスライド移動する。
    /// </summary>
    private void PlaceIcon(GameObject icon, Vector2 targetPosition)
    {
        RectTransform rt = icon.GetComponent<RectTransform>();
        if (rt == null) return;

        if (!placedIcons.Contains(icon))
        {
            rt.anchoredPosition = targetPosition;
            placedIcons.Add(icon);
            return;
        }

        rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPosition, Time.unscaledDeltaTime * repositionSpeed);
    }
}
