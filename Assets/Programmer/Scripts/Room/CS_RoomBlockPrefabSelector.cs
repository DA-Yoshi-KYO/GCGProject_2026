using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomBlockPrefabSelector.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePoint生成設定から生成に使うRoomPrefabを選択するクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorからRoomPrefab選択処理を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePoint生成設定から生成に使うRoomPrefabを選択するクラスです。
/// </summary>
public class CS_RoomBlockPrefabSelector
{
    /// <summary>
    /// 生成方式に応じたRoomPrefabを取得します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>生成に使うRoomPrefab。</returns>
    public GameObject GetRoomBlockPrefab(
        CS_RoomCreatePointGenerateData generateData,
        int index)
    {
        if (generateData == null)
        {
            Debug.LogWarning("[RoomBlockPrefabSelector] 生成データがnullです。Index : " + index);
            return null;
        }

        if (generateData.GenerateType == CSE_RoomBlockGenerateType.Fixed)
        {
            return GetFixedRoomBlockPrefab(generateData, index);
        }

        return GetRandomRoomBlockPrefab(generateData, index);
    }

    /// <summary>
    /// 固定生成用のRoomPrefabを取得します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>固定生成用RoomPrefab。</returns>
    private GameObject GetFixedRoomBlockPrefab(
        CS_RoomCreatePointGenerateData generateData,
        int index)
    {
        if (generateData.FixedRoomPrefab == null)
        {
            Debug.LogWarning("[RoomBlockPrefabSelector] 固定生成用RoomPrefabが設定されていません。Index : " + index);
            return null;
        }

        return generateData.FixedRoomPrefab;
    }

    /// <summary>
    /// ランダム生成用のRoomPrefabを取得します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>ランダムに選ばれたRoomPrefab。</returns>
    private GameObject GetRandomRoomBlockPrefab(
        CS_RoomCreatePointGenerateData generateData,
        int index)
    {
        List<GameObject> validPrefabs =
            GetValidRoomBlockPrefabs(generateData.RandomRoomBlockPrefabs);

        if (validPrefabs.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabSelector] ランダム生成候補RoomPrefabが設定されていません。Index : " + index);
            return null;
        }

        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }

    /// <summary>
    /// nullではないRoomPrefabだけを取得します。
    /// </summary>
    /// <param name="roomPrefabs">確認対象Prefabリスト。</param>
    /// <returns>有効なRoomPrefabリスト。</returns>
    private List<GameObject> GetValidRoomBlockPrefabs(List<GameObject> roomPrefabs)
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        if (roomPrefabs == null)
        {
            return validPrefabs;
        }

        for (int i = 0 ; i < roomPrefabs.Count ; i++)
        {
            if (roomPrefabs[i] == null)
            {
                continue;
            }

            validPrefabs.Add(roomPrefabs[i]);
        }

        return validPrefabs;
    }
}
