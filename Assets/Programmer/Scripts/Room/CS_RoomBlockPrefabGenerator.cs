using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomBlockPrefabGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : 登録されたRoomCreatePointの子としてRoomPrefabを生成し、
 *                生成後にRoomMovePoint同士を動的接続する
 *                Element0の固定Room内にあるStartPlayerPointへPlayerPrefabを生成する
 *  履歴        : 2026/04/27 RoomCreatePointの子に生成する形へ修正(ヨシモト)
 *                2026/04/27 実行時の自動再生成処理を追加(ヨシモト)
 *                2026/04/28 登録リスト生成方式へ変更(ヨシモト)
 *                2026/04/28 生成方式をRoomCreatePointごとの設定へ変更(ヨシモト)
 *                2026/04/28 RoomPlayerPositionを同一GameObjectから取得する形へ変更(ヨシモト)
 *                2026/04/28 Fixedは事前生成、Randomはゲーム開始時生成へ変更(ヨシモト)
 *                2026/04/29 Element0のStartPlayerPointへPlayerPrefabを生成する処理を追加(ヨシモト)
 *                2026/05/03 Player生成時はRaycastではなくRoomCreatePointを直接設定する形へ変更(ヨシモト)
 *                2026/05/25 リファクタリング
 *==================================================*/

/// <summary>
/// RoomPrefabの生成方式です。
/// </summary>
public enum CSE_RoomBlockGenerateType
{
    Fixed,
    Random
}

/// <summary>
/// 登録されたRoomCreatePointの子としてRoomPrefabを生成するクラスです。
/// Fixedはゲーム開始前に生成し、Randomはゲーム開始時に自動再生成します。
/// </summary>
public class CS_RoomBlockPrefabGenerator : MonoBehaviour
{
    [Header("生成対象RoomCreatePoint一覧")]
    [SerializeField]
    private List<CS_RoomCreatePointGenerateData> list_RoomCreatePointGenerateData =
        new List<CS_RoomCreatePointGenerateData>();

    private CS_GeneratedRoomObjectService cs_GeneratedRoomObjectService =
        new CS_GeneratedRoomObjectService();

    private CS_RoomConnectionBuilder cs_RoomConnectionBuilder =
        new CS_RoomConnectionBuilder();

    private CS_RoomPlayerSpawnService cs_RoomPlayerSpawnService =
        new CS_RoomPlayerSpawnService();

    private CS_GeneratedRoomCameraSetup cs_GeneratedRoomCameraSetup =
        new CS_GeneratedRoomCameraSetup();

    private CS_RoomBlockPrefabSelector cs_RoomBlockPrefabSelector =
        new CS_RoomBlockPrefabSelector();

    private CS_RoomCreatePointGenerateDataValidator cs_RoomCreatePointGenerateDataValidator =
        new CS_RoomCreatePointGenerateDataValidator();

    /// <summary>
    /// 生成済みRoomのRoomMovePoint接続だけを再構築します。
    /// </summary>
    public void RebuildGeneratedRoomLinks()
    {
        cs_RoomConnectionBuilder.RebuildGeneratedRoomLinks(
            list_RoomCreatePointGenerateData);
    }

    /// <summary>
    /// 指定した生成方式のRoomを生成します。
    /// </summary>
    /// <param name="targetGenerateType">生成対象の方式。</param>
    /// <param name="bool_IsReplaceExisting">既存生成Roomを置き換える場合はtrue。</param>
    public void GenerateRoomBlocksByType(
        CSE_RoomBlockGenerateType targetGenerateType,
        bool bool_IsReplaceExisting)
    {
        if (list_RoomCreatePointGenerateData == null || list_RoomCreatePointGenerateData.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 生成対象RoomCreatePointが登録されていません。");
            return;
        }

        cs_GeneratedRoomObjectService.DeleteOldGeneratedRoot(transform);

        int generatedCount = 0;

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            CS_RoomCreatePointGenerateData generateData = list_RoomCreatePointGenerateData[i];

            if (generateData == null)
            {
                continue;
            }

            if (generateData.GenerateType != targetGenerateType)
            {
                continue;
            }

            if (!cs_RoomCreatePointGenerateDataValidator.IsValidGenerateData(generateData, i))
            {
                continue;
            }

            Transform pointTransform = generateData.RoomCreatePointTransform;

            if (pointTransform == null)
            {
                continue;
            }

            if (bool_IsReplaceExisting)
            {
                cs_GeneratedRoomObjectService.DeleteGeneratedChildren(pointTransform);
            }
            else
            {
                if (cs_GeneratedRoomObjectService.FindGeneratedRoomChild(pointTransform) != null)
                {
                    Debug.LogWarning("[RoomBlockPrefabGenerator] すでに生成済みのRoomがあります。再生成したい場合は再生成メニューを使ってください : " + generateData.RoomCreatePointObject.name);
                    continue;
                }
            }

            GameObject roomPrefab =
                cs_RoomBlockPrefabSelector.GetRoomBlockPrefab(generateData, i);

            if (roomPrefab == null)
            {
                continue;
            }

            Vector3 spawnPosition = pointTransform.position;
            Quaternion spawnRotation = pointTransform.rotation;

            GameObject generatedRoom = cs_GeneratedRoomObjectService.CreateRoomInstance(
                roomPrefab,
                spawnPosition,
                spawnRotation,
                pointTransform
            );

            generatedRoom.name = cs_GeneratedRoomObjectService.CreateGeneratedRoomName(roomPrefab, i);

            cs_GeneratedRoomCameraSetup.SetupGeneratedRoomForPlayerCamera(generatedRoom);

            generatedCount++;
        }

        cs_RoomConnectionBuilder.RebuildGeneratedRoomLinks(
            list_RoomCreatePointGenerateData);

        Debug.Log("[RoomBlockPrefabGenerator] " + targetGenerateType + " のRoomを生成しました。生成数 : " + generatedCount);
    }

    /// <summary>
    /// 以前の設計でRoomManager下に生成されたRootを削除します。
    /// </summary>
    public void DeleteOldGeneratedRoot()
    {
        cs_GeneratedRoomObjectService.DeleteOldGeneratedRoot(transform);
    }

    /// <summary>
    /// 指定した生成方式の生成済みRoomを削除します。
    /// </summary>
    /// <param name="targetGenerateType">削除対象の方式。</param>
    public void DeleteGeneratedRoomBlocksByType(CSE_RoomBlockGenerateType targetGenerateType)
    {
        if (list_RoomCreatePointGenerateData == null)
        {
            return;
        }

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            CS_RoomCreatePointGenerateData generateData = list_RoomCreatePointGenerateData[i];

            if (generateData == null)
            {
                continue;
            }

            if (generateData.GenerateType != targetGenerateType)
            {
                continue;
            }

            if (generateData.RoomCreatePointTransform == null)
            {
                continue;
            }

            cs_GeneratedRoomObjectService.DeleteGeneratedChildren(
                generateData.RoomCreatePointTransform);
        }

        Debug.Log("[RoomBlockPrefabGenerator] " + targetGenerateType + " の生成済みRoomを削除しました。");
    }

    /// <summary>
    /// Element0の生成Room内にあるStartPlayerPointへPlayerPrefabを生成します。
    /// </summary>
    public void CreatePlayerAtFirstRoomStartPoint()
    {
        cs_RoomPlayerSpawnService.CreatePlayerAtFirstRoomStartPoint(
            this,
            list_RoomCreatePointGenerateData,
            cs_GeneratedRoomObjectService);
    }
}
