using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomEnemyEntryDataSO.cs
 *  制作者      : 吉本竜
 *  内容        : 1つの敵出入口から出現できる敵の最大数を管理するScriptableObject
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// 1つの敵出入口から出現できる敵の最大数を管理するScriptableObjectです。
/// </summary>
[CreateAssetMenu(
    fileName = "RoomEnemyEntryDataSO",
    menuName = "ScriptableObjects/RoomEnemyEntryDataSO")]
public class CS_RoomEnemyEntryDataSO : ScriptableObject
{
    [Header("敵出現設定")]
    [SerializeField, Min(0)]
    private int int_MaxEnemySpawnCount = 1;

    /// <summary>
    /// この敵出入口から出現できる敵の最大数を取得します。
    /// </summary>
    /// <returns>敵の最大出現数。</returns>
    public int GetMaxEnemySpawnCount()
    {
        return int_MaxEnemySpawnCount;
    }

    /// <summary>
    /// 敵の侵入数を取得します。
    /// 既存処理との互換用です。
    /// </summary>
    /// <returns>敵の最大出現数。</returns>
    public int GetEnemyEntryCount()
    {
        return GetMaxEnemySpawnCount();
    }
}
