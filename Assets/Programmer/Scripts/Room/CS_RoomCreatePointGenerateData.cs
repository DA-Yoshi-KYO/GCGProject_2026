using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomCreatePointGenerateData.cs
 *  制作者      : 吉本竜
 *  内容        : RoomCreatePointごとのRoom生成設定データを管理するクラス
 *  履歴        : 2026/05/25 CS_RoomBlockPrefabGeneratorから生成設定データを分離(ヨシモト)
 *==================================================*/

/// <summary>
/// RoomCreatePointごとのRoom生成設定データです。
/// </summary>
[System.Serializable]
public class CS_RoomCreatePointGenerateData
{
    [Header("生成先RoomCreatePoint")]
    [SerializeField]
    private GameObject go_RoomCreatePointObject;

    [Header("このRoomCreatePointの生成方式")]
    [SerializeField]
    private CSE_RoomBlockGenerateType e_GenerateType = CSE_RoomBlockGenerateType.Random;

    [Header("固定生成で使うRoomPrefab")]
    [SerializeField]
    private GameObject go_FixedRoomPrefab;

    [Header("ランダム生成で使うRoomPrefab候補")]
    [SerializeField]
    private List<GameObject> list_RandomRoomBlockPrefabs = new List<GameObject>();

    /// <summary>
    /// RoomCreatePointのGameObjectを取得します。
    /// </summary>
    public GameObject RoomCreatePointObject => go_RoomCreatePointObject;

    /// <summary>
    /// このRoomCreatePointの生成方式を取得します。
    /// </summary>
    public CSE_RoomBlockGenerateType GenerateType => e_GenerateType;

    /// <summary>
    /// 固定生成で使うRoomPrefabを取得します。
    /// </summary>
    public GameObject FixedRoomPrefab => go_FixedRoomPrefab;

    /// <summary>
    /// ランダム生成で使うRoomPrefab候補を取得します。
    /// </summary>
    public List<GameObject> RandomRoomBlockPrefabs => list_RandomRoomBlockPrefabs;

    /// <summary>
    /// RoomCreatePointのTransformを取得します。
    /// </summary>
    public Transform RoomCreatePointTransform
    {
        get
        {
            if (go_RoomCreatePointObject == null)
            {
                return null;
            }

            return go_RoomCreatePointObject.transform;
        }
    }

    /// <summary>
    /// CS_RoomCreatePointを取得します。
    /// </summary>
    public CS_RoomCreatePoint RoomCreatePoint
    {
        get
        {
            if (go_RoomCreatePointObject == null)
            {
                return null;
            }

            return go_RoomCreatePointObject.GetComponent<CS_RoomCreatePoint>();
        }
    }
}
