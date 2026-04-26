using UnityEngine;

/*==================================================
 *  ファイル名  : CSS_RoomBlockData.cs
 *  制作者      : 吉本竜
 *  内容        : ルームブロック生成用のデータを保持するScriptableObject
 *  履歴        : 2026/04/26 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// ルームブロック生成用のデータを保持するScriptableObjectです。
/// </summary>
[CreateAssetMenu(fileName = "CSS_RoomBlockData", menuName = "Room/Room Block Data")]
public class CSS_RoomBlockData : ScriptableObject
{
    [Header("生成するブロック")]
    [SerializeField]
    private GameObject go_BlockPrefab;

    [Header("ルームサイズ")]
    [SerializeField]
    private int i_RoomWidth = 5;

    [SerializeField]
    private int i_RoomDepth = 5;

    [Header("ブロック間隔")]
    [SerializeField]
    private float f_BlockSize = 1.0f;

    /// <summary>
    /// 生成するブロックPrefabを取得します。
    /// </summary>
    public GameObject GetBlockPrefab => go_BlockPrefab;

    /// <summary>
    /// ルームの横幅を取得します。
    /// </summary>
    public int GetRoomWidth => i_RoomWidth;

    /// <summary>
    /// ルームの奥行きを取得します。
    /// </summary>
    public int GetRoomDepth => i_RoomDepth;

    /// <summary>
    /// ブロックの配置間隔を取得します。
    /// </summary>
    public float GetBlockSize => f_BlockSize;
}
