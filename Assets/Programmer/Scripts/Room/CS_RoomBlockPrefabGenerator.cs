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
 *                2026/05/25 リファクタリング(ヨシモト)
 *==================================================*/


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

    private CS_RoomBlockGenerateExecutor cs_RoomBlockGenerateExecutor;

    /// <summary>
    /// RoomBlock生成実行クラスを初期化します。
    /// </summary>
    private void InitializeRoomBlockGenerateExecutor()
    {
        if (cs_RoomBlockGenerateExecutor != null)
        {
            return;
        }

        cs_RoomBlockGenerateExecutor =
            new CS_RoomBlockGenerateExecutor(
                cs_GeneratedRoomObjectService,
                cs_RoomConnectionBuilder,
                cs_RoomBlockPrefabSelector,
                cs_RoomCreatePointGenerateDataValidator,
                cs_GeneratedRoomCameraSetup
            );
    }

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
        InitializeRoomBlockGenerateExecutor();

        cs_RoomBlockGenerateExecutor.GenerateRoomBlocksByType(
            list_RoomCreatePointGenerateData,
            targetGenerateType,
            bool_IsReplaceExisting,
            transform
        );
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
        InitializeRoomBlockGenerateExecutor();

        cs_RoomBlockGenerateExecutor.DeleteGeneratedRoomBlocksByType(
            list_RoomCreatePointGenerateData,
            targetGenerateType
        );
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

    /// <summary>
    /// 登録されている全RoomCreatePointからヒエログリフを取得します。
    /// </summary>
    public void SetAllRoomHieroglyphObjects()
    {
        if (list_RoomCreatePointGenerateData == null)
        {
            return;
        }

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            CS_RoomCreatePointGenerateData generateData =
                list_RoomCreatePointGenerateData[i];

            if (generateData == null)
            {
                continue;
            }

            CS_RoomCreatePoint cs_RoomCreatePoint =
                generateData.RoomCreatePoint;

            if (cs_RoomCreatePoint == null)
            {
                Debug.LogWarning(
                    "[Hieroglyph取得]"
                    + " CS_RoomCreatePointが取得できません。"
                    + " / 登録番号 : " + i);

                continue;
            }

            cs_RoomCreatePoint.SetHieroglyphObjects();
        }
    }
}
