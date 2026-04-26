using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomBlockGenerator.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePointタグの位置を中心にランダムなルームブロックを生成・再生成・削除する
 *  履歴        : 2026/04/26 新規作成(ヨシモト)
 *              : 2026/04/26 再生成処理と削除処理を追加
 *              : 2026/04/26 複数のRoomBlockDataからランダム生成する処理を追加
 *==================================================*/

/// <summary>
/// RoomCreatePointタグが付いたオブジェクトの位置を中心に、
/// 複数のRoomBlockDataからランダムに選択してルームブロックを生成するクラスです。
/// </summary>
public class CS_RoomBlockGenerator : MonoBehaviour
{
    private const string TAG_ROOM_CREATE_POINT = "RoomCreatePoint";
    private const string GENERATED_ROOT_NAME = "__GeneratedRoomBlocks";

    [Header("ランダム生成に使うルームデータ一覧")]
    [SerializeField]
    private List<CSS_RoomBlockData> css_RoomBlockDataList = new List<CSS_RoomBlockData>();

    /// <summary>
    /// RoomCreatePointタグが付いた全ての位置に、ランダムなルームブロックを生成します。
    /// すでに生成済みの場合は重複生成しません。
    /// </summary>
    [ContextMenu("ルームブロックを生成")]
    public void GenerateRoomBlocks()
    {
        if (!CanGenerate())
        {
            return;
        }

        GameObject[] go_RoomCreatePoints = FindRoomCreatePoints();

        if (go_RoomCreatePoints.Length == 0)
        {
            return;
        }

        foreach (GameObject go_RoomCreatePoint in go_RoomCreatePoints)
        {
            Transform tf_RoomCreatePoint = go_RoomCreatePoint.transform;

            if (tf_RoomCreatePoint.Find(GENERATED_ROOT_NAME) != null)
            {
                Debug.LogWarning(go_RoomCreatePoint.name + " はすでに生成済みです。作り直す場合は「ルームブロックを再生成」を使用してください。");
                continue;
            }

            CSS_RoomBlockData css_SelectedRoomBlockData = GetRandomRoomBlockData();

            if (css_SelectedRoomBlockData == null)
            {
                continue;
            }

            GenerateRoomBlocksAtCreatePoint(tf_RoomCreatePoint, css_SelectedRoomBlockData);
        }
    }

    /// <summary>
    /// 生成済みのルームブロックを削除してから、全てのRoomCreatePointにランダムなルームブロックを再生成します。
    /// </summary>
    [ContextMenu("ルームブロックを再生成")]
    public void RegenerateRoomBlocks()
    {
        if (!CanGenerate())
        {
            return;
        }

        DeleteGeneratedRoomBlocks();

        GameObject[] go_RoomCreatePoints = FindRoomCreatePoints();

        if (go_RoomCreatePoints.Length == 0)
        {
            return;
        }

        foreach (GameObject go_RoomCreatePoint in go_RoomCreatePoints)
        {
            CSS_RoomBlockData css_SelectedRoomBlockData = GetRandomRoomBlockData();

            if (css_SelectedRoomBlockData == null)
            {
                continue;
            }

            GenerateRoomBlocksAtCreatePoint(go_RoomCreatePoint.transform, css_SelectedRoomBlockData);
        }
    }

    /// <summary>
    /// RoomCreatePointタグが付いた全てのオブジェクトから、生成済みルームブロックを削除します。
    /// </summary>
    [ContextMenu("生成済みルームブロックを削除")]
    public void DeleteGeneratedRoomBlocks()
    {
        GameObject[] go_RoomCreatePoints = FindRoomCreatePoints();

        if (go_RoomCreatePoints.Length == 0)
        {
            return;
        }

        foreach (GameObject go_RoomCreatePoint in go_RoomCreatePoints)
        {
            Transform tf_GeneratedRoot = go_RoomCreatePoint.transform.Find(GENERATED_ROOT_NAME);

            if (tf_GeneratedRoot == null)
            {
                continue;
            }

            DestroyGameObject(tf_GeneratedRoot.gameObject);
        }
    }

    /// <summary>
    /// ルームブロック生成に必要なデータが揃っているか確認します。
    /// </summary>
    /// <returns>生成可能な場合はtrueを返します。</returns>
    private bool CanGenerate()
    {
        if (css_RoomBlockDataList == null || css_RoomBlockDataList.Count == 0)
        {
            Debug.LogWarning("ルーム生成データ一覧が設定されていません。");
            return false;
        }

        for (int i = 0 ; i < css_RoomBlockDataList.Count ; i++)
        {
            CSS_RoomBlockData css_RoomBlockData = css_RoomBlockDataList[i];

            if (css_RoomBlockData == null)
            {
                continue;
            }

            if (css_RoomBlockData.GetBlockPrefab == null)
            {
                continue;
            }

            return true;
        }

        Debug.LogWarning("使用可能なルーム生成データがありません。RoomBlockDataとBlockPrefabを設定してください。");
        return false;
    }

    /// <summary>
    /// RoomCreatePointタグが付いたオブジェクトを全て取得します。
    /// </summary>
    /// <returns>RoomCreatePointタグが付いたGameObject配列。</returns>
    private GameObject[] FindRoomCreatePoints()
    {
        GameObject[] go_RoomCreatePoints = GameObject.FindGameObjectsWithTag(TAG_ROOM_CREATE_POINT);

        if (go_RoomCreatePoints.Length == 0)
        {
            Debug.LogWarning("RoomCreatePointタグが付いたオブジェクトがシーン内にありません。");
        }

        return go_RoomCreatePoints;
    }

    /// <summary>
    /// ルームデータ一覧から使用可能なデータをランダムに1つ取得します。
    /// </summary>
    /// <returns>ランダムに選ばれたRoomBlockData。</returns>
    private CSS_RoomBlockData GetRandomRoomBlockData()
    {
        List<CSS_RoomBlockData> css_ValidRoomBlockDataList = new List<CSS_RoomBlockData>();

        for (int i = 0 ; i < css_RoomBlockDataList.Count ; i++)
        {
            CSS_RoomBlockData css_RoomBlockData = css_RoomBlockDataList[i];

            if (css_RoomBlockData == null)
            {
                continue;
            }

            if (css_RoomBlockData.GetBlockPrefab == null)
            {
                continue;
            }

            css_ValidRoomBlockDataList.Add(css_RoomBlockData);
        }

        if (css_ValidRoomBlockDataList.Count == 0)
        {
            Debug.LogWarning("ランダム選択できるRoomBlockDataがありません。");
            return null;
        }

        int i_RandomIndex = Random.Range(0, css_ValidRoomBlockDataList.Count);

        return css_ValidRoomBlockDataList[i_RandomIndex];
    }

    /// <summary>
    /// 指定したRoomCreatePointの位置を中心に、指定したルームデータを使ってブロックを生成します。
    /// </summary>
    /// <param name="tf_RoomCreatePoint">ルーム生成の中心になるTransform。</param>
    /// <param name="css_SelectedRoomBlockData">生成に使用するルームデータ。</param>
    private void GenerateRoomBlocksAtCreatePoint(Transform tf_RoomCreatePoint, CSS_RoomBlockData css_SelectedRoomBlockData)
    {
        Transform tf_GeneratedRoot = CreateGeneratedRoot(tf_RoomCreatePoint, css_SelectedRoomBlockData.name);

        GameObject go_BlockPrefab = css_SelectedRoomBlockData.GetBlockPrefab;

        int i_RoomWidth = css_SelectedRoomBlockData.GetRoomWidth;
        int i_RoomDepth = css_SelectedRoomBlockData.GetRoomDepth;
        float f_BlockSize = css_SelectedRoomBlockData.GetBlockSize;

        float f_StartX = -((i_RoomWidth - 1) * f_BlockSize) * 0.5f;
        float f_StartZ = -((i_RoomDepth - 1) * f_BlockSize) * 0.5f;

        for (int i_Z = 0 ; i_Z < i_RoomDepth ; i_Z++)
        {
            for (int i_X = 0 ; i_X < i_RoomWidth ; i_X++)
            {
                Vector3 v3_LocalBlockPos = new Vector3(
                    f_StartX + i_X * f_BlockSize,
                    0.0f,
                    f_StartZ + i_Z * f_BlockSize
                );

                GameObject go_Block = Instantiate(
                    go_BlockPrefab,
                    tf_GeneratedRoot
                );

                go_Block.transform.localPosition = v3_LocalBlockPos;
                go_Block.transform.localRotation = Quaternion.identity;
                go_Block.transform.localScale = Vector3.one;

                go_Block.name = go_BlockPrefab.name
                              + "_X" + i_X.ToString("00")
                              + "_Z" + i_Z.ToString("00");
            }
        }

        Debug.Log(tf_RoomCreatePoint.name + " に " + css_SelectedRoomBlockData.name + " を生成しました。");
    }

    /// <summary>
    /// 生成済みブロックをまとめる親オブジェクトを作成します。
    /// </summary>
    /// <param name="tf_RoomCreatePoint">ルーム生成の中心になるTransform。</param>
    /// <param name="s_SelectedRoomDataName">生成に使用したルームデータ名。</param>
    /// <returns>生成済みブロックをまとめる親Transform。</returns>
    private Transform CreateGeneratedRoot(Transform tf_RoomCreatePoint, string s_SelectedRoomDataName)
    {
        GameObject go_GeneratedRoot = new GameObject(GENERATED_ROOT_NAME);

        go_GeneratedRoot.transform.SetParent(tf_RoomCreatePoint);
        go_GeneratedRoot.transform.localPosition = Vector3.zero;
        go_GeneratedRoot.transform.localRotation = Quaternion.identity;
        go_GeneratedRoot.transform.localScale = Vector3.one;

        go_GeneratedRoot.name = GENERATED_ROOT_NAME;

        GameObject go_RoomDataNameObject = new GameObject("RoomData_" + s_SelectedRoomDataName);
        go_RoomDataNameObject.transform.SetParent(go_GeneratedRoot.transform);
        go_RoomDataNameObject.transform.localPosition = Vector3.zero;
        go_RoomDataNameObject.transform.localRotation = Quaternion.identity;
        go_RoomDataNameObject.transform.localScale = Vector3.one;

        return go_GeneratedRoot.transform;
    }

    /// <summary>
    /// GameObjectを削除します。
    /// EditModeではDestroyImmediate、PlayModeではDestroyを使用します。
    /// </summary>
    /// <param name="go_Target">削除対象のGameObject。</param>
    private void DestroyGameObject(GameObject go_Target)
    {
        if (Application.isPlaying)
        {
            Destroy(go_Target);
        }
        else
        {
            DestroyImmediate(go_Target);
        }
    }
}
