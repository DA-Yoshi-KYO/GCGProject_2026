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

    // 子インデックス → アイコン GameObject の対応表
    private List<GameObject> enemyIcons = new List<GameObject>();

    // ──────────────────────────────
    void Start()
    {
        // EnemyParent を名前で検索
        enemyManager = GameObject.Find(enemyParentName);
        if (enemyManager == null)
        {
            Debug.LogError($"[EnemyUI] 名前 '{enemyParentName}' のオブジェクトが見つかりません");
            return;
        }

        int childCount = enemyManager.transform.childCount;
        for (int i = 0 ; i < childCount ; i++)
            AddIcon(i);
    }

    void Update()
    {
        if (enemyManager == null)
        {
            enemyManager = GameObject.Find(enemyParentName);
            if (enemyManager == null)
            {
                Debug.LogError($"[EnemyUI] 名前 '{enemyParentName}' のオブジェクトが見つかりません");
            }
                return;
        }

        int childCount = enemyManager.transform.childCount;

        while (enemyIcons.Count < childCount)
            AddIcon(enemyIcons.Count);

        while (enemyIcons.Count > childCount)
        {
            int last = enemyIcons.Count - 1;
            Destroy(enemyIcons[last]);
            enemyIcons.RemoveAt(last);
        }
    }

    // ============================================================
    //  アイコン追加
    // ============================================================
    private void AddIcon(int index)
    {
        Debug.Log($"[EnemyUI] アイコン追加: 敵インデックス {index}");
        // プレハブを複製
        GameObject icon = Instantiate(enemyIconPrefab, transform);

        // EnemyIcon に CS_ThiefAI を渡す
        EnemyIcon iconScript = icon.GetComponent<EnemyIcon>();
        if (iconScript != null)
        {
            CS_ThiefAI thiefAI = enemyManager.transform
                .GetChild(index).GetComponent<CS_ThiefAI>();
            iconScript.SetScript(thiefAI);
        }


        // アイコンの位置を設定
        RectTransform rt = icon.GetComponent<RectTransform>();
        if (rt != null)
        {
            RectTransform actorRT = actor.GetComponent<RectTransform>();
            if (actorRT != null)
            {
                float y = -index * iconSpacing;
                Vector2 SetPos = actorRT.anchoredPosition + new Vector2(0, -y);
                rt.anchoredPosition = SetPos;

            }
        }
        enemyIcons.Add(icon);
    }
}
