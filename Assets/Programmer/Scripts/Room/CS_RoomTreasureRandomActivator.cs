using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomTreasureRandomActivator.cs
 *  制作者      : 吉本竜
 *  内容        : 指定RoomTypeの生成済みRoom内Treasureだけをランダムに有効化する
 *  履歴        : 2026/07/01 新規作成
 *==================================================*/

[DisallowMultipleComponent]
public class CS_RoomTreasureRandomActivator : MonoBehaviour
{
    private const string GENERATED_NAME_PREFIX = "__GeneratedRoom_";
    private const string DELETING_NAME_PREFIX = "__DeletingRoom_";

    [Header("RoomCreatePointsの親")]
    [SerializeField]
    private Transform tr_RoomCreatePointsRoot;

    [Header("Treasureを出す対象RoomType")]
    [SerializeField]
    private CSE_RoomTypeEnum e_TargetRoomType = CSE_RoomTypeEnum.Normal;

    [Header("有効化するTreasure数")]
    [SerializeField, Min(0)]
    private int i_ActiveTreasureCount = 1;

    [Header("探すTreasureの名前")]
    [SerializeField]
    private string str_TreasureObjectName = "Treasure";

    [Header("デバッグログを出すか")]
    [SerializeField]
    private bool bool_IsDebugLog = true;

    private readonly List<GameObject> list_TargetTreasures = new List<GameObject>();

    [ContextMenu("Treasureをランダム表示")]
    public void ActivateRandomTreasures()
    {
        CollectTargetTreasures();

        // 対象RoomTypeのTreasureだけを全部OFFにします。
        SetTreasureListActive(list_TargetTreasures, false);

        int activeCount = Mathf.Clamp(
            i_ActiveTreasureCount,
            0,
            list_TargetTreasures.Count);

        ShuffleTreasures(list_TargetTreasures);

        for (int i = 0 ; i < activeCount ; i++)
        {
            if (list_TargetTreasures[i] == null)
            {
                continue;
            }

            list_TargetTreasures[i].SetActive(true);
        }

        if (bool_IsDebugLog)
        {
            Debug.Log(
                "[RoomTreasureRandomActivator] RoomType "
                + e_TargetRoomType
                + " 対象Treasure数 : "
                + list_TargetTreasures.Count
                + " / 表示数 : "
                + activeCount);
        }
    }

    private void CollectTargetTreasures()
    {
        list_TargetTreasures.Clear();

        if (tr_RoomCreatePointsRoot == null)
        {
            Debug.LogWarning("[RoomTreasureRandomActivator] RoomCreatePointsRootが設定されていません。");
            return;
        }

        CS_RoomCreatePoint[] roomCreatePoints =
            tr_RoomCreatePointsRoot.GetComponentsInChildren<CS_RoomCreatePoint>(true);

        for (int i = 0 ; i < roomCreatePoints.Length ; i++)
        {
            CS_RoomCreatePoint roomCreatePoint = roomCreatePoints[i];

            if (roomCreatePoint == null)
            {
                continue;
            }

            // 指定したRoomType以外は完全に無視します。
            if (roomCreatePoint.RoomType != e_TargetRoomType)
            {
                continue;
            }

            Transform generatedRoomTransform =
                FindGeneratedRoomChild(roomCreatePoint.transform);

            if (generatedRoomTransform == null)
            {
                if (bool_IsDebugLog)
                {
                    Debug.Log(
                        "[RoomTreasureRandomActivator] 生成Roomなし : "
                        + roomCreatePoint.name
                        + " / RoomType : "
                        + roomCreatePoint.RoomType);
                }

                continue;
            }

            List<GameObject> treasuresInRoom =
                FindTreasuresInRoom(generatedRoomTransform);

            for (int treasureIndex = 0 ; treasureIndex < treasuresInRoom.Count ; treasureIndex++)
            {
                GameObject treasure = treasuresInRoom[treasureIndex];

                if (treasure == null)
                {
                    continue;
                }

                list_TargetTreasures.Add(treasure);
            }

            if (bool_IsDebugLog)
            {
                Debug.Log(
                    "[RoomTreasureRandomActivator] 対象Room : "
                    + roomCreatePoint.name
                    + " / RoomType : "
                    + roomCreatePoint.RoomType
                    + " / 生成Room : "
                    + generatedRoomTransform.name
                    + " / Treasure数 : "
                    + treasuresInRoom.Count);
            }
        }
    }

    private Transform FindGeneratedRoomChild(Transform roomCreatePointTransform)
    {
        if (roomCreatePointTransform == null)
        {
            return null;
        }

        for (int i = 0 ; i < roomCreatePointTransform.childCount ; i++)
        {
            Transform child = roomCreatePointTransform.GetChild(i);

            if (!IsGeneratedRoomName(child.name))
            {
                continue;
            }

            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }

            return child;
        }

        return null;
    }

    private bool IsGeneratedRoomName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return false;
        }

        if (objectName.StartsWith(DELETING_NAME_PREFIX))
        {
            return false;
        }

        return objectName.StartsWith(GENERATED_NAME_PREFIX)
               || objectName.Contains("_Generated_");
    }

    private List<GameObject> FindTreasuresInRoom(Transform generatedRoomTransform)
    {
        List<GameObject> treasures = new List<GameObject>();

        if (generatedRoomTransform == null)
        {
            return treasures;
        }

        Transform[] childTransforms =
            generatedRoomTransform.GetComponentsInChildren<Transform>(true);

        for (int i = 0 ; i < childTransforms.Length ; i++)
        {
            Transform child = childTransforms[i];

            if (child == generatedRoomTransform)
            {
                continue;
            }

            if (!IsTreasureName(child.name))
            {
                continue;
            }

            treasures.Add(child.gameObject);
        }

        return treasures;
    }

    private bool IsTreasureName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return false;
        }

        return objectName == str_TreasureObjectName
               || objectName.StartsWith(str_TreasureObjectName + " ")
               || objectName.StartsWith(str_TreasureObjectName + "(");
    }

    private void SetTreasureListActive(List<GameObject> treasureList, bool isActive)
    {
        for (int i = 0 ; i < treasureList.Count ; i++)
        {
            if (treasureList[i] == null)
            {
                continue;
            }

            treasureList[i].SetActive(isActive);
        }
    }

    private void ShuffleTreasures(List<GameObject> treasureList)
    {
        if (treasureList == null)
        {
            return;
        }

        for (int i = 0 ; i < treasureList.Count ; i++)
        {
            int randomIndex = Random.Range(i, treasureList.Count);

            GameObject temp = treasureList[i];
            treasureList[i] = treasureList[randomIndex];
            treasureList[randomIndex] = temp;
        }
    }
}
