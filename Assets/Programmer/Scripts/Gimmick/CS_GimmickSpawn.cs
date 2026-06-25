using System.Collections.Generic;
using UnityEngine;

public class CS_GimmickSpawn : MonoBehaviour
{
    [Header("ポップさせるギミックの候補")]
    [SerializeField] List<GameObject> spawnGimmicks;
    [Header("ポップさせるギミックの数")]
    [SerializeField] int spawnGimmicksNum = 3;
    [Header("ポップさせる部屋の親オブジェクト")]
    [SerializeField] GameObject roomParent;
    private List<GameObject> spawnRooms;

    void Start()
    {
        CS_RoomCreatePoint[] rooms = roomParent.GetComponentsInChildren<CS_RoomCreatePoint>();

        foreach (var item in rooms)
        {
            if (item.RoomType == CSE_RoomTypeEnum.Normal)
                spawnRooms.Add(item.gameObject);
        }
    }

    private void Update()
    {
        // 部屋が生成されるまで待つ
        if (spawnRooms[0].transform.childCount == 0) return;

        // 条件確認
        // 即時終了するもの
        if (spawnGimmicksNum <= 0)
        {
            Debug.LogWarning("ポップするギミックの数に0以下が指定されています。");
            return;
        }
        if (spawnRooms.Count <= 0)
        {
            Debug.LogWarning("ポップさせる部屋の候補数に0以下が指定されています。");
            return;
        }
        if (spawnGimmicks.Count <= 0)
        {
            Debug.LogWarning("ポップさせるギミックの候補数に0以下が指定されています。");
            return;
        }
        // 一部実行するもの
        if (spawnGimmicksNum > spawnGimmicks.Count)
            Debug.LogWarning("ポップするギミックの数が候補数を上回っています。ギミックの候補数分のみポップさせます。");
        else if (spawnGimmicksNum > spawnRooms.Count)
            Debug.LogWarning("ポップするギミックの数が部屋の候補数を上回っています。部屋の候補数分のみポップさせます。");

        // 生成処理
        // 一度使った部屋やギミックは2度使わないので、動的配列に移してRemove処理をする
        List<GameObject> roomList = new List<GameObject>(spawnRooms);
        List<GameObject> gimmickList = new List<GameObject>(spawnGimmicks);
        for (int i = 0 ; i < spawnGimmicksNum ; i++)
        {
            if (roomList.Count <= 0 || gimmickList.Count <= 0) break;   // 一部実行時にnullエラーを吐かないようにする為の例外処理

            // スポーンポイントを取得
            GameObject roomObject = roomList[Random.Range(0, roomList.Count - 1)];
            CS_GimmickSpawnPoint spawnPoint = roomObject.GetComponentInChildren<CS_GimmickSpawnPoint>();
            if (spawnPoint == null)
            {
                Debug.LogWarning(roomObject.name + ":この部屋にギミックのポップポイントがありません。");
                continue;
            }

            // ギミック用アイテムをスポーンさせる
            GameObject gimmickObject = gimmickList[Random.Range(0, gimmickList.Count - 1)];
            spawnPoint.GimmickSpawn(gimmickObject);

            // 一度使ったギミックアイテムや部屋は2度使わない
            roomList.Remove(roomObject);
            gimmickList.Remove(gimmickObject);
        }
    }
}
