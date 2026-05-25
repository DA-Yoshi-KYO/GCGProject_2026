using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*==================================================
 *  ファイル名  : CS_GeneratedRoomObjectService.cs
 *  制作者      : 吉本竜
 *  内容        : 生成済みRoomの生成・削除・検索を管理するクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorから生成済みRoom操作を分離(ヨシモト)
 *==================================================*/

/// <summary>
/// 生成済みRoomの生成・削除・検索を管理するクラスです。
/// </summary>
public class CS_GeneratedRoomObjectService
{
    private const string GENERATED_NAME_PREFIX = "__GeneratedRoom_";
    private const string DELETING_NAME_PREFIX = "__DeletingRoom_";
    private const string OLD_GENERATED_ROOT_NAME = "__GeneratedRoomBlocks";

    /// <summary>
    /// 生成Room名を作成します。
    /// </summary>
    /// <param name="roomPrefab">生成元RoomPrefab。</param>
    /// <param name="index">生成番号。</param>
    /// <returns>生成Room名。</returns>
    public string CreateGeneratedRoomName(GameObject roomPrefab, int index)
    {
        if (roomPrefab == null)
        {
            return GENERATED_NAME_PREFIX + "NullRoom_" + index.ToString("00");
        }

        return GENERATED_NAME_PREFIX + roomPrefab.name + "_" + index.ToString("00");
    }

    /// <summary>
    /// RoomPrefabを生成します。
    /// Editor上ではPrefab接続を維持して生成します。
    /// </summary>
    /// <param name="prefab">生成するPrefab。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="rotation">生成回転。</param>
    /// <param name="parent">生成先の親Transform。</param>
    /// <returns>生成されたRoom。</returns>
    public GameObject CreateRoomInstance(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent)
    {
        GameObject instance = null;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;

            if (instance != null)
            {
                Undo.RegisterCreatedObjectUndo(instance, "Generate Room Block");
            }
        }
#endif

        if (instance == null)
        {
            instance = Object.Instantiate(prefab, parent);
        }

        instance.transform.SetPositionAndRotation(position, rotation);

        return instance;
    }

    /// <summary>
    /// RoomCreatePointの子にある生成済みRoomだけを削除します。
    /// </summary>
    /// <param name="parent">RoomCreatePointのTransform。</param>
    public void DeleteGeneratedChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1 ; i >= 0 ; i--)
        {
            Transform child = parent.GetChild(i);

            if (!IsGeneratedRoomName(child.name))
            {
                continue;
            }

            DestroyObjectSafe(child.gameObject);
        }
    }

    /// <summary>
    /// 以前の設計でRoomManager下に生成されたRootを削除します。
    /// </summary>
    /// <param name="ownerTransform">RoomManagerなど、この処理を呼ぶ基準Transform。</param>
    public void DeleteOldGeneratedRoot(Transform ownerTransform)
    {
        if (ownerTransform == null)
        {
            return;
        }

        Transform oldRoot = ownerTransform.Find(OLD_GENERATED_ROOT_NAME);

        if (oldRoot == null)
        {
            return;
        }

        DestroyObjectSafe(oldRoot.gameObject);
    }

    /// <summary>
    /// RoomCreatePointの子から生成済みRoomを取得します。
    /// </summary>
    /// <param name="parent">RoomCreatePointのTransform。</param>
    /// <returns>生成済みRoom。</returns>
    public GameObject FindGeneratedRoomChild(Transform parent)
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
    public bool IsGeneratedRoomName(string objectName)
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
    /// Play中とEditor中の両方に対応して安全にオブジェクトを削除します。
    /// </summary>
    /// <param name="target">削除対象のGameObject。</param>
    private void DestroyObjectSafe(GameObject target)
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.DestroyObjectImmediate(target);
            return;
        }
#endif

        target.name = DELETING_NAME_PREFIX + target.name;
        target.SetActive(false);
        Object.Destroy(target);
    }
}
