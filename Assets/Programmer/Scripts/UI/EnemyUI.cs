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

    // ──────────────────────────────
    private GameObject enemyManager;


    private Dictionary<Transform, GameObject> iconMap = new Dictionary<Transform, GameObject>();

    // ──────────────────────────────
    void Start()
    {
        enemyManager = GameObject.Find(enemyParentName);
        if (enemyManager == null) {
            Debug.LogError($"EnemyUI: '{enemyParentName}' という名前のGameObjectが見つかりませんでした。");
            return; 
        }

        for (int i = 0 ; i < enemyManager.transform.childCount ; i++)
            AddIcon(enemyManager.transform.GetChild(i));

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

        // 消えた敵のアイコンを削除
        var removed = new List<Transform>();
        foreach (var kv in iconMap)
        {
            if (!currentChildren.Contains(kv.Key))
            {
                Destroy(kv.Value);
                removed.Add(kv.Key);
            }
        }
        foreach (var key in removed)
            iconMap.Remove(key);

        // 新しく追加された敵のアイコンを生成
        foreach (var child in currentChildren)
        {
            if (!iconMap.ContainsKey(child))
                AddIcon(child);
        }

        // アイコンの位置を再整列
        RealignIcons();
    }

    // ============================================================
    //  アイコン追加
    // ============================================================
    private void AddIcon(Transform enemyTransform)
    {
        GameObject icon = Instantiate(enemyIconPrefab, transform);

        EnemyIcon iconScript = icon.GetComponent<EnemyIcon>();
        if (iconScript != null)
        {
            CS_ThiefAI thiefAI = enemyTransform.GetComponent<CS_ThiefAI>();
            iconScript.SetScript(thiefAI);
        }

        iconMap[enemyTransform] = icon;
    }
    private void RealignIcons()
    {
        RectTransform actorRT = actor.GetComponent<RectTransform>();
        if (actorRT == null) return;

        int index = 0;
        foreach (var kv in iconMap)
        {
            RectTransform rt = kv.Value.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = actorRT.anchoredPosition + new Vector2(0, index * iconSpacing);
            }
            index++;
        }
    }
}
