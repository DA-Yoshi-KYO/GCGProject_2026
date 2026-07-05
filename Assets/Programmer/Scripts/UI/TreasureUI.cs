using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureUI : MonoBehaviour
{
    [Header("表示位置actor")]
    [SerializeField] private GameObject Actor;

    [Header("お宝アイコンプレハブ")]
    [SerializeField] private GameObject TreasureIconPrefab;

    [Header("シフト距離")]
    [SerializeField] private Vector3 ShiftDistance = new Vector3(50f, 0, 0);

    CS_EndManager endManager;

    List<GameObject> treasureIcons = new List<GameObject>();

    private void Awake()
    {
        endManager = GameObject.FindObjectOfType<CS_EndManager>();

        if(endManager == null)
        {
            Debug.LogError("CS_EndManagerが見つかりません。");
        }

        int count = endManager.read_TotalTreasureCount;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = Actor.transform.position - ShiftDistance * i;
            GameObject icon = Instantiate(TreasureIconPrefab, pos, Quaternion.identity, this.transform );
            treasureIcons.Add(icon);
        }
    }


}
