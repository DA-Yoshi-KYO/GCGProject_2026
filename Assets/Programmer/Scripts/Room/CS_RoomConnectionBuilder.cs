using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomConnectionBuilder.cs
 *  制作者      : 吉本竜
 *  内容        : 生成済みRoom同士のRoomMovePoint接続を構築するクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorからRoom接続処理を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// 生成済みRoom同士のRoomMovePoint接続を構築するクラスです。
/// </summary>
public class CS_RoomConnectionBuilder
{
    private const string GENERATED_NAME_PREFIX = "__GeneratedRoom_";
    private const string DELETING_NAME_PREFIX = "__DeletingRoom_";

    /// <summary>
    /// 登録済みRoomCreatePoint一覧から生成済みRoomの接続を再構築します。
    /// </summary>
    /// <param name="list_RoomCreatePointGenerateData">RoomCreatePoint生成設定一覧。</param>
    public void RebuildGeneratedRoomLinks(
        List<CS_RoomCreatePointGenerateData> list_RoomCreatePointGenerateData)
    {
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap =
            BuildGeneratedRoomMapFromRegisteredList(list_RoomCreatePointGenerateData);

        if (dic_GeneratedRoomMap.Count <= 0)
        {
            Debug.LogWarning("[RoomConnectionBuilder] 接続更新できる生成済みRoomがありません。");
            return;
        }

        ConnectGeneratedRooms(dic_GeneratedRoomMap);
    }

    /// <summary>
    /// 登録リストから、RoomCreatePointと生成Roomの対応表を作成します。
    /// </summary>
    /// <param name="list_RoomCreatePointGenerateData">RoomCreatePoint生成設定一覧。</param>
    /// <returns>RoomCreatePointと生成Roomの対応表。</returns>
    private Dictionary<CS_RoomCreatePoint, GameObject> BuildGeneratedRoomMapFromRegisteredList(
        List<CS_RoomCreatePointGenerateData> list_RoomCreatePointGenerateData)
    {
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap =
            new Dictionary<CS_RoomCreatePoint, GameObject>();

        if (list_RoomCreatePointGenerateData == null)
        {
            return dic_GeneratedRoomMap;
        }

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            CS_RoomCreatePointGenerateData generateData = list_RoomCreatePointGenerateData[i];

            if (generateData == null)
            {
                continue;
            }

            CS_RoomCreatePoint createPoint = generateData.RoomCreatePoint;

            if (createPoint == null)
            {
                continue;
            }

            Transform pointTransform = generateData.RoomCreatePointTransform;

            if (pointTransform == null)
            {
                continue;
            }

            GameObject generatedRoom = FindGeneratedRoomChild(pointTransform);

            if (generatedRoom == null)
            {
                continue;
            }

            if (!dic_GeneratedRoomMap.ContainsKey(createPoint))
            {
                dic_GeneratedRoomMap.Add(createPoint, generatedRoom);
            }
        }

        return dic_GeneratedRoomMap;
    }

    /// <summary>
    /// 生成されたRoom同士のRoomMovePointを接続します。
    /// </summary>
    /// <param name="dic_GeneratedRoomMap">RoomCreatePointと生成Roomの対応表。</param>
    private void ConnectGeneratedRooms(
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap)
    {
        foreach (GameObject generatedRoom in dic_GeneratedRoomMap.Values)
        {
            CS_RoomMovePoint[] movePoints =
                generatedRoom.GetComponentsInChildren<CS_RoomMovePoint>(true);

            for (int i = 0 ; i < movePoints.Length ; i++)
            {
                movePoints[i].ClearTarget();
            }
        }

        foreach (KeyValuePair<CS_RoomCreatePoint, GameObject> pair in dic_GeneratedRoomMap)
        {
            CS_RoomCreatePoint currentCreatePoint = pair.Key;
            GameObject currentRoom = pair.Value;

            ConnectOneDirection(
                currentCreatePoint,
                currentRoom,
                CSE_RoomDoorDirection.Right,
                dic_GeneratedRoomMap);

            ConnectOneDirection(
                currentCreatePoint,
                currentRoom,
                CSE_RoomDoorDirection.Left,
                dic_GeneratedRoomMap);

            ConnectOneDirection(
                currentCreatePoint,
                currentRoom,
                CSE_RoomDoorDirection.Front,
                dic_GeneratedRoomMap);

            ConnectOneDirection(
                currentCreatePoint,
                currentRoom,
                CSE_RoomDoorDirection.Back,
                dic_GeneratedRoomMap);
        }

        Debug.Log("[RoomConnectionBuilder] RoomMovePoint同士の接続を更新しました。");
    }

    /// <summary>
    /// 1方向分のRoomMovePoint接続を行います。
    /// </summary>
    /// <param name="currentCreatePoint">現在のRoomCreatePoint。</param>
    /// <param name="currentRoom">現在の生成Room。</param>
    /// <param name="currentDirection">現在Roomの出口方向。</param>
    /// <param name="dic_GeneratedRoomMap">RoomCreatePointと生成Roomの対応表。</param>
    private void ConnectOneDirection(
        CS_RoomCreatePoint currentCreatePoint,
        GameObject currentRoom,
        CSE_RoomDoorDirection currentDirection,
        Dictionary<CS_RoomCreatePoint, GameObject> dic_GeneratedRoomMap)
    {
        CS_RoomMovePoint currentMovePoint =
            FindMovePoint(currentRoom, currentDirection);

        if (currentMovePoint == null)
        {
            return;
        }

        if (!currentCreatePoint.TryGetConnection(
                currentDirection,
                out CS_RoomMoveConnection connection))
        {
            currentMovePoint.ClearTarget();
            return;
        }

        if (connection.TargetCreatePoint == null)
        {
            currentMovePoint.ClearTarget();

            Debug.LogWarning(
                "[RoomConnectionBuilder] 接続先RoomCreatePointがnullです。"
                + " / 現在RoomCreatePoint : " + currentCreatePoint.name
                + " / 現在Room : " + currentRoom.name
                + " / 現在出口方向 : " + currentDirection
            );

            return;
        }

        if (!dic_GeneratedRoomMap.TryGetValue(
                connection.TargetCreatePoint,
                out GameObject targetRoom))
        {
            currentMovePoint.ClearTarget();

            Debug.LogWarning(
                "[RoomConnectionBuilder] 移動先RoomCreatePointに生成Roomがありません。"
                + " / 現在RoomCreatePoint : " + currentCreatePoint.name
                + " / 現在Room : " + currentRoom.name
                + " / 現在出口方向 : " + currentDirection
                + " / 移動先RoomCreatePoint : " + connection.TargetCreatePoint.name
                + " / 移動先で探す方向 : " + connection.TargetOutDirection
            );

            return;
        }

        CS_RoomMovePoint targetMovePoint =
            FindMovePoint(targetRoom, connection.TargetOutDirection);

        if (targetMovePoint == null)
        {
            currentMovePoint.ClearTarget();

            Debug.LogWarning(
                "[RoomConnectionBuilder] 移動先Roomに指定方向のRoomMovePointがありません。"
                + " / 現在RoomCreatePoint : " + currentCreatePoint.name
                + " / 現在Room : " + currentRoom.name
                + " / 現在出口方向 : " + currentDirection
                + " / 移動先RoomCreatePoint : " + connection.TargetCreatePoint.name
                + " / 移動先Room : " + targetRoom.name
                + " / 移動先で必要なRoomMovePoint方向 : " + connection.TargetOutDirection
                + " / 移動先Room階層 : " + GetHierarchyPath(targetRoom.transform)
            );

            return;
        }

        currentMovePoint.SetTargetMovePoint(targetMovePoint);
    }

    /// <summary>
    /// RoomCreatePointの子から生成済みRoomを取得します。
    /// </summary>
    /// <param name="parent">RoomCreatePointのTransform。</param>
    /// <returns>生成済みRoom。</returns>
    private GameObject FindGeneratedRoomChild(Transform parent)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0 ; i < parent.childCount ; i++)
        {
            Transform child = parent.GetChild(i);

            if (!IsGeneratedRoomName(child.name))
            {
                continue;
            }

            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }

            return child.gameObject;
        }

        return null;
    }

    /// <summary>
    /// 生成されたRoom名かどうか確認します。
    /// </summary>
    /// <param name="objectName">確認するオブジェクト名。</param>
    /// <returns>生成Room名の場合はtrue。</returns>
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

    /// <summary>
    /// 指定Room内から指定方向のRoomMovePointを取得します。
    /// </summary>
    /// <param name="roomObject">生成されたRoom。</param>
    /// <param name="direction">探したい方向。</param>
    /// <returns>指定方向のRoomMovePoint。</returns>
    private CS_RoomMovePoint FindMovePoint(
        GameObject roomObject,
        CSE_RoomDoorDirection direction)
    {
        if (roomObject == null)
        {
            return null;
        }

        CS_RoomMovePoint[] movePoints =
            roomObject.GetComponentsInChildren<CS_RoomMovePoint>(true);

        for (int i = 0 ; i < movePoints.Length ; i++)
        {
            if (movePoints[i].MoveDirection == direction)
            {
                return movePoints[i];
            }
        }

        return null;
    }

    /// <summary>
    /// TransformのHierarchy上のパスを取得します。
    /// </summary>
    /// <param name="targetTransform">対象Transform。</param>
    /// <returns>Hierarchyパス。</returns>
    private string GetHierarchyPath(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return "null";
        }

        string path = targetTransform.name;
        Transform currentTransform = targetTransform.parent;

        while (currentTransform != null)
        {
            path = currentTransform.name + "/" + path;
            currentTransform = currentTransform.parent;
        }

        return path;
    }
}
