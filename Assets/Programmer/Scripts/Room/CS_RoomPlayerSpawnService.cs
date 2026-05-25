using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomPlayerSpawnService.cs
 *  制作者      : 吉本竜
 *  内容        : 生成済みRoom内のStartPlayerPointからPlayer初期生成を行うクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorからPlayer初期生成処理を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// 生成済みRoom内のStartPlayerPointからPlayer初期生成を行うクラスです。
/// </summary>
public class CS_RoomPlayerSpawnService
{
    private const string START_PLAYER_POINT_NAME = "StartPlayerPoint";

    private CS_RoomPlayerPosition cs_RoomPlayerPosition;

    /// <summary>
    /// Element0の生成Room内にあるStartPlayerPointへPlayerPrefabを生成します。
    /// </summary>
    /// <param name="ownerComponent">この処理を呼び出すComponent。</param>
    /// <param name="list_RoomCreatePointGenerateData">RoomCreatePoint生成設定一覧。</param>
    /// <param name="cs_GeneratedRoomObjectService">生成済みRoom操作クラス。</param>
    public void CreatePlayerAtFirstRoomStartPoint(
        Component ownerComponent,
        List<CS_RoomCreatePointGenerateData> list_RoomCreatePointGenerateData,
        CS_GeneratedRoomObjectService cs_GeneratedRoomObjectService)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (ownerComponent == null)
        {
            Debug.LogWarning("[RoomPlayerSpawnService] 呼び出し元Componentがnullです。");
            return;
        }

        if (cs_GeneratedRoomObjectService == null)
        {
            Debug.LogWarning("[RoomPlayerSpawnService] CS_GeneratedRoomObjectServiceがnullです。");
            return;
        }

        CacheRoomPlayerPosition(ownerComponent);

        if (cs_RoomPlayerPosition == null)
        {
            Debug.LogWarning("[RoomPlayerSpawnService] 同じGameObjectにCS_RoomPlayerPositionが付いていません。");
            return;
        }

        if (list_RoomCreatePointGenerateData == null || list_RoomCreatePointGenerateData.Count <= 0)
        {
            Debug.LogWarning("[RoomPlayerSpawnService] Element0のRoomCreatePointがありません。");
            return;
        }

        CS_RoomCreatePointGenerateData firstGenerateData = list_RoomCreatePointGenerateData[0];

        if (firstGenerateData == null || firstGenerateData.RoomCreatePointTransform == null)
        {
            Debug.LogWarning("[RoomPlayerSpawnService] Element0のRoomCreatePointが設定されていません。");
            return;
        }

        GameObject firstGeneratedRoom =
            cs_GeneratedRoomObjectService.FindGeneratedRoomChild(
                firstGenerateData.RoomCreatePointTransform);

        if (firstGeneratedRoom == null)
        {
            Debug.LogWarning("[RoomPlayerSpawnService] Element0のRoomCreatePoint内に生成済みRoomがありません。");
            return;
        }

        Transform startPlayerPoint = FindStartPlayerPoint(firstGeneratedRoom.transform);

        if (startPlayerPoint == null)
        {
            Debug.LogWarning("[RoomPlayerSpawnService] Element0の生成Room内にStartPlayerPointがありません。");
            return;
        }

        cs_RoomPlayerPosition.CreatePlayerAtStartPoint(
            startPlayerPoint,
            firstGenerateData.RoomCreatePointObject
        );
    }

    /// <summary>
    /// Room内からStartPlayerPointを探します。
    /// </summary>
    /// <param name="roomRoot">探す対象のRoomルート。</param>
    /// <returns>StartPlayerPointのTransform。</returns>
    private Transform FindStartPlayerPoint(Transform roomRoot)
    {
        if (roomRoot == null)
        {
            return null;
        }

        Transform[] childTransforms =
            roomRoot.GetComponentsInChildren<Transform>(true);

        for (int i = 0 ; i < childTransforms.Length ; i++)
        {
            if (childTransforms[i].name == START_PLAYER_POINT_NAME)
            {
                return childTransforms[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 同じGameObjectに付いているCS_RoomPlayerPositionを取得します。
    /// </summary>
    /// <param name="ownerComponent">この処理を呼び出すComponent。</param>
    private void CacheRoomPlayerPosition(Component ownerComponent)
    {
        if (cs_RoomPlayerPosition != null)
        {
            return;
        }

        if (ownerComponent == null)
        {
            return;
        }

        cs_RoomPlayerPosition =
            ownerComponent.GetComponent<CS_RoomPlayerPosition>();
    }
}
