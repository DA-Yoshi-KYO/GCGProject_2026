using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class TreasureUI : MonoBehaviour
{
    [Header("表示位置actor")]
    [SerializeField] private GameObject Actor;

    [Header("お宝アイコンプレハブ")]
    [SerializeField] private GameObject TreasureIconPrefab;

    [Header("シフト距離")]
    [SerializeField] private Vector3 ShiftDistance = new Vector3(50f, 0, 0);

    [Header("最大お宝アイコン数")]
    [SerializeField] private int MaxTreasureCount = 5;

    CS_EndManager endManager;
    bool isFirstUpdate = true;

    List<GameObject> treasureIcons = new List<GameObject>();

    int tikcCount = 0;

    CS_RoomBlockRandomGenerator roomBlockRandomGenerator;

    private void Start()
    {
        endManager = GameObject.FindObjectOfType<CS_EndManager>();

        if(endManager == null)
        {
            Debug.LogError("CS_EndManagerが見つかりません。");
        }

        roomBlockRandomGenerator = GameObject.FindObjectOfType<CS_RoomBlockRandomGenerator>();
        if(roomBlockRandomGenerator == null)
        {
            Debug.LogError("CS_RoomBlockRandomGeneratorが見つかりません。");
        }

    }

    private void Update()
    {
        if(roomBlockRandomGenerator == null || endManager == null) return;

        if(!roomBlockRandomGenerator.b_IsRuntimeRegenerating) return;

        int count = endManager.read_TreasureList.Count;
        if (count < MaxTreasureCount || !isFirstUpdate) return;
        
        isFirstUpdate = false;

        for (int i = 0 ; i < count ; i++)
        {
            Vector3 pos = Actor.transform.position - ShiftDistance * i;
            GameObject icon = Instantiate(TreasureIconPrefab, pos, Quaternion.identity, this.transform);
            TreasureItem treasureItem = icon.GetComponent<TreasureItem>();
            if (treasureItem != null)
            {
                treasureItem.SetCS(endManager.read_TreasureList[i]);
            }
            treasureIcons.Add(icon);
        }
    }
}
