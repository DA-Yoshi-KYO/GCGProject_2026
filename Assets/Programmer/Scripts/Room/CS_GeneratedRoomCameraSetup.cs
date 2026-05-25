using UnityEngine;

/*==================================================
 *  ファイル名  : CS_GeneratedRoomCameraSetup.cs
 *  制作者      : 吉本竜
 *  内容        : 生成RoomのCamera参照用階層を整えるクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorからCamera用Center補正処理を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// 生成RoomのCamera参照用階層を整えるクラスです。
/// </summary>
public class CS_GeneratedRoomCameraSetup
{
    private const string CENTER_NAME = "Center";

    /// <summary>
    /// PlayerCameraが参照しやすいように生成Roomの階層を整えます。
    /// </summary>
    /// <param name="generatedRoom">生成されたRoom。</param>
    public void SetupGeneratedRoomForPlayerCamera(GameObject generatedRoom)
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
}
