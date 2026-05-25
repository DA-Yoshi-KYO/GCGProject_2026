using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    private const string ROOM_CREATE_POINT_TAG = "RoomCreatePoint";
    private const string CENTER_NAME = "Center";

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

            if (!IsValidGenerateData(generateData, i))
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

            GameObject roomPrefab = GetRoomBlockPrefab(generateData, i);

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

            SetupGeneratedRoomForPlayerCamera(generatedRoom);

            generatedCount++;
        }

        cs_RoomConnectionBuilder.RebuildGeneratedRoomLinks(
            list_RoomCreatePointGenerateData);

        Debug.Log("[RoomBlockPrefabGenerator] " + targetGenerateType + " のRoomを生成しました。生成数 : " + generatedCount);
    }

    /// <summary>
    /// PlayerCameraが参照しやすいように生成Roomの階層を整えます。
    /// </summary>
    /// <param name="generatedRoom">生成されたRoom。</param>
    private void SetupGeneratedRoomForPlayerCamera(GameObject generatedRoom)
    {
        if (generatedRoom == null)
        {
            return;
        }

        generatedRoom.transform.SetSiblingIndex(0);
        EnsureDirectCenterChild(generatedRoom);
    }

    /// <summary>
    /// 生成Room直下にCenterが存在する状態を保証します。
    /// </summary>
    /// <param name="generatedRoom">生成されたRoom。</param>
    private void EnsureDirectCenterChild(GameObject generatedRoom)
    {
        if (generatedRoom == null)
        {
            return;
        }

        Transform directCenterTransform = generatedRoom.transform.Find(CENTER_NAME);

        if (directCenterTransform != null)
        {
            return;
        }

        Transform existingCenterTransform =
            FindChildByNameRecursive(generatedRoom.transform, CENTER_NAME);

        GameObject centerObject = new GameObject(CENTER_NAME);
        centerObject.transform.SetParent(generatedRoom.transform);

        if (existingCenterTransform != null)
        {
            centerObject.transform.SetPositionAndRotation(
                existingCenterTransform.position,
                existingCenterTransform.rotation
            );

            return;
        }

        centerObject.transform.SetPositionAndRotation(
            generatedRoom.transform.position,
            generatedRoom.transform.rotation
        );
    }

    /// <summary>
    /// 子階層から指定名のTransformを探します。
    /// </summary>
    /// <param name="rootTransform">検索開始Transform。</param>
    /// <param name="targetName">探す名前。</param>
    /// <returns>見つかったTransform。</returns>
    private Transform FindChildByNameRecursive(Transform rootTransform, string targetName)
    {
        if (rootTransform == null)
        {
            return null;
        }

        Transform[] childTransforms =
            rootTransform.GetComponentsInChildren<Transform>(true);

        for (int i = 0 ; i < childTransforms.Length ; i++)
        {
            if (childTransforms[i].name == targetName)
            {
                return childTransforms[i];
            }
        }

        return null;
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
    /// 生成データが有効か確認します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>有効な場合はtrue。</returns>
    private bool IsValidGenerateData(CS_RoomCreatePointGenerateData generateData, int index)
    {
        if (generateData == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 生成データがnullです。Index : " + index);
            return false;
        }

        if (generateData.RoomCreatePointObject == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] RoomCreatePointObjectが登録されていません。Index : " + index);
            return false;
        }

        if (generateData.RoomCreatePoint == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] CS_RoomCreatePointが付いていません : " + generateData.RoomCreatePointObject.name);
            return false;
        }

        if (!IsRoomCreatePointTagValid(generateData.RoomCreatePointObject))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// RoomCreatePointタグが正しく設定されているか確認します。
    /// </summary>
    /// <param name="target">確認対象。</param>
    /// <returns>正しい場合はtrue。</returns>
    private bool IsRoomCreatePointTagValid(GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        try
        {
            if (!target.CompareTag(ROOM_CREATE_POINT_TAG))
            {
                Debug.LogWarning("[RoomBlockPrefabGenerator] RoomCreatePointタグが付いていません : " + target.name);
                return false;
            }
        }
        catch (UnityException)
        {
            Debug.LogError("[RoomBlockPrefabGenerator] Tag「" + ROOM_CREATE_POINT_TAG + "」が存在しません。UnityのTagsに追加してください。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 生成方式に応じたRoomPrefabを取得します。
    /// </summary>
    /// <param name="generateData">生成データ。</param>
    /// <param name="index">リスト番号。</param>
    /// <returns>生成に使うRoomPrefab。</returns>
    private GameObject GetRoomBlockPrefab(CS_RoomCreatePointGenerateData generateData, int index)
    {
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
    private GameObject GetFixedRoomBlockPrefab(CS_RoomCreatePointGenerateData generateData, int index)
    {
        if (generateData.FixedRoomPrefab == null)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] 固定生成用RoomPrefabが設定されていません。Index : " + index);
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
    private GameObject GetRandomRoomBlockPrefab(CS_RoomCreatePointGenerateData generateData, int index)
    {
        List<GameObject> validPrefabs = GetValidRoomBlockPrefabs(generateData.RandomRoomBlockPrefabs);

        if (validPrefabs.Count <= 0)
        {
            Debug.LogWarning("[RoomBlockPrefabGenerator] ランダム生成候補RoomPrefabが設定されていません。Index : " + index);
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
