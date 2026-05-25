using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomBlockGenerateExecutor.cs
 *  制作者      : 吉本竜
 *  内容        : RoomBlock生成処理の実行本体を担当するクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorからRoom生成実行処理を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomBlock生成処理の実行本体を担当するクラスです。
/// </summary>
public class CS_RoomBlockGenerateExecutor
{
    private CS_GeneratedRoomObjectService cs_GeneratedRoomObjectService;
    private CS_RoomConnectionBuilder cs_RoomConnectionBuilder;
    private CS_RoomBlockPrefabSelector cs_RoomBlockPrefabSelector;
    private CS_RoomCreatePointGenerateDataValidator cs_RoomCreatePointGenerateDataValidator;
    private CS_GeneratedRoomCameraSetup cs_GeneratedRoomCameraSetup;

    /// <summary>
    /// RoomBlock生成実行クラスを初期化します。
    /// </summary>
    /// <param name="generatedRoomObjectService">生成済みRoom操作クラス。</param>
    /// <param name="roomConnectionBuilder">Room接続構築クラス。</param>
    /// <param name="roomBlockPrefabSelector">RoomPrefab選択クラス。</param>
    /// <param name="roomCreatePointGenerateDataValidator">生成データ検証クラス。</param>
    /// <param name="generatedRoomCameraSetup">生成RoomのCamera用設定クラス。</param>
    public CS_RoomBlockGenerateExecutor(
        CS_GeneratedRoomObjectService generatedRoomObjectService,
        CS_RoomConnectionBuilder roomConnectionBuilder,
        CS_RoomBlockPrefabSelector roomBlockPrefabSelector,
        CS_RoomCreatePointGenerateDataValidator roomCreatePointGenerateDataValidator,
        CS_GeneratedRoomCameraSetup generatedRoomCameraSetup)
    {
        cs_GeneratedRoomObjectService = generatedRoomObjectService;
        cs_RoomConnectionBuilder = roomConnectionBuilder;
        cs_RoomBlockPrefabSelector = roomBlockPrefabSelector;
        cs_RoomCreatePointGenerateDataValidator = roomCreatePointGenerateDataValidator;
        cs_GeneratedRoomCameraSetup = generatedRoomCameraSetup;
    }

    /// <summary>
    /// 指定した生成方式のRoomを生成します。
    /// </summary>
    /// <param name="list_RoomCreatePointGenerateData">RoomCreatePoint生成データ一覧。</param>
    /// <param name="targetGenerateType">生成対象の方式。</param>
    /// <param name="bool_IsReplaceExisting">既存生成Roomを置き換える場合はtrue。</param>
    /// <param name="ownerTransform">生成管理元のTransform。</param>
    public void GenerateRoomBlocksByType(
        List<CS_RoomCreatePointGenerateData> list_RoomCreatePointGenerateData,
        CSE_RoomBlockGenerateType targetGenerateType,
        bool bool_IsReplaceExisting,
        Transform ownerTransform)
    {
        if (!IsGenerateServiceValid())
        {
            return;
        }

        if (list_RoomCreatePointGenerateData == null || list_RoomCreatePointGenerateData.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] 生成対象RoomCreatePointが登録されていません。");
            return;
        }

        cs_GeneratedRoomObjectService.DeleteOldGeneratedRoot(ownerTransform);

        int generatedCount = 0;

        for (int i = 0 ; i < list_RoomCreatePointGenerateData.Count ; i++)
        {
            if (TryGenerateRoomBlock(
                list_RoomCreatePointGenerateData[i],
                targetGenerateType,
                bool_IsReplaceExisting,
                i))
            {
                generatedCount++;
            }
        }

        cs_RoomConnectionBuilder.RebuildGeneratedRoomLinks(
            list_RoomCreatePointGenerateData);

        Debug.Log("[RoomBlockGenerateExecutor] " + targetGenerateType + " のRoomを生成しました。生成数 : " + generatedCount);
    }

    /// <summary>
    /// 指定した生成方式の生成済みRoomを削除します。
    /// </summary>
    /// <param name="list_RoomCreatePointGenerateData">RoomCreatePoint生成データ一覧。</param>
    /// <param name="targetGenerateType">削除対象の方式。</param>
    public void DeleteGeneratedRoomBlocksByType(
        List<CS_RoomCreatePointGenerateData> list_RoomCreatePointGenerateData,
        CSE_RoomBlockGenerateType targetGenerateType)
    {
        if (cs_GeneratedRoomObjectService == null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] CS_GeneratedRoomObjectServiceがnullです。");
            return;
        }

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

        Debug.Log("[RoomBlockGenerateExecutor] " + targetGenerateType + " の生成済みRoomを削除しました。");
    }

    /// <summary>
    /// 1つのRoomCreatePointに対してRoom生成を試みます。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="targetGenerateType">生成対象の方式。</param>
    /// <param name="bool_IsReplaceExisting">既存生成Roomを置き換える場合はtrue。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>生成できた場合はtrue。</returns>
    private bool TryGenerateRoomBlock(
        CS_RoomCreatePointGenerateData generateData,
        CSE_RoomBlockGenerateType targetGenerateType,
        bool bool_IsReplaceExisting,
        int index)
    {
        if (generateData == null)
        {
            return false;
        }

        if (generateData.GenerateType != targetGenerateType)
        {
            return false;
        }

        if (!cs_RoomCreatePointGenerateDataValidator.IsValidGenerateData(generateData, index))
        {
            return false;
        }

        Transform pointTransform = generateData.RoomCreatePointTransform;

        if (pointTransform == null)
        {
            return false;
        }

        if (!PrepareGeneratedRoomParent(generateData, pointTransform, bool_IsReplaceExisting))
        {
            return false;
        }

        GameObject roomPrefab =
            cs_RoomBlockPrefabSelector.GetRoomBlockPrefab(generateData, index);

        if (roomPrefab == null)
        {
            return false;
        }

        GameObject generatedRoom =
            cs_GeneratedRoomObjectService.CreateRoomInstance(
                roomPrefab,
                pointTransform.position,
                pointTransform.rotation,
                pointTransform
            );

        if (generatedRoom == null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] RoomPrefabの生成に失敗しました。Index : " + index);
            return false;
        }

        generatedRoom.name =
            cs_GeneratedRoomObjectService.CreateGeneratedRoomName(roomPrefab, index);

        cs_GeneratedRoomCameraSetup.SetupGeneratedRoomForPlayerCamera(generatedRoom);

        return true;
    }

    /// <summary>
    /// Room生成前に、既存Roomの扱いを決めます。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="pointTransform">生成先Transform。</param>
    /// <param name="bool_IsReplaceExisting">既存生成Roomを置き換える場合はtrue。</param>
    /// <returns>生成を続行してよい場合はtrue。</returns>
    private bool PrepareGeneratedRoomParent(
        CS_RoomCreatePointGenerateData generateData,
        Transform pointTransform,
        bool bool_IsReplaceExisting)
    {
        if (bool_IsReplaceExisting)
        {
            cs_GeneratedRoomObjectService.DeleteGeneratedChildren(pointTransform);
            return true;
        }

        if (cs_GeneratedRoomObjectService.FindGeneratedRoomChild(pointTransform) != null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] すでに生成済みのRoomがあります。再生成したい場合は再生成メニューを使ってください : " + generateData.RoomCreatePointObject.name);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 生成処理に必要なServiceが設定されているか確認します。
    /// </summary>
    /// <returns>有効な場合はtrue。</returns>
    private bool IsGenerateServiceValid()
    {
        if (cs_GeneratedRoomObjectService == null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] CS_GeneratedRoomObjectServiceがnullです。");
            return false;
        }

        if (cs_RoomConnectionBuilder == null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] CS_RoomConnectionBuilderがnullです。");
            return false;
        }

        if (cs_RoomBlockPrefabSelector == null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] CS_RoomBlockPrefabSelectorがnullです。");
            return false;
        }

        if (cs_RoomCreatePointGenerateDataValidator == null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] CS_RoomCreatePointGenerateDataValidatorがnullです。");
            return false;
        }

        if (cs_GeneratedRoomCameraSetup == null)
        {
            Debug.LogWarning("[RoomBlockGenerateExecutor] CS_GeneratedRoomCameraSetupがnullです。");
            return false;
        }

        return true;
    }
}
