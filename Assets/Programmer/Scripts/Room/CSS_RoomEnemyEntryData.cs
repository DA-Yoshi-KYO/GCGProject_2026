using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CSS_RoomEnemyEntryData.cs
 *  制作者      : 吉本竜
 *  内容        : 1つの敵出入口から出現できる敵の最大数と盗賊データを管理するScriptableObject
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *                2026/05/06 出現候補の盗賊データリストを追加(ヨシモト)
 *==================================================*/

/// <summary>
/// 1つの敵出入口から出現できる敵の最大数と、
/// 出現候補となる盗賊データを管理するScriptableObjectです。
/// </summary>
[CreateAssetMenu(
    fileName = "RoomEnemyEntryData",
    menuName = "ScriptableObjects/RoomEnemyEntryData")]
public class CSS_RoomEnemyEntryData : ScriptableObject
{
    [Header("敵出現設定")]
    [SerializeField, Min(0)]
    private int int_MaxEnemySpawnCount = 1;

    [Header("出現候補の盗賊データ")]
    [SerializeField]
    private List<CSS_ThiefStatusData> list_ThiefStatusData = new List<CSS_ThiefStatusData>();

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
    /// 互換用として、最大出現数を返します。
    /// </summary>
    /// <returns>敵の最大出現数。</returns>
    public int GetEnemyEntryCount()
    {
        return GetMaxEnemySpawnCount();
    }

    /// <summary>
    /// 出現候補の盗賊データリストを取得します。
    /// </summary>
    /// <returns>盗賊データリスト。</returns>
    public IReadOnlyList<CSS_ThiefStatusData> GetThiefStatusDataList()
    {
        return list_ThiefStatusData;
    }

    /// <summary>
    /// 出現候補の盗賊データ数を取得します。
    /// </summary>
    /// <returns>盗賊データ数。</returns>
    public int GetThiefStatusDataCount()
    {
        if (list_ThiefStatusData == null)
        {
            return 0;
        }

        return list_ThiefStatusData.Count;
    }

    /// <summary>
    /// 指定番号の盗賊データを取得します。
    /// </summary>
    /// <param name="int_Index">取得したい番号。</param>
    /// <returns>盗賊データ。取得できない場合はnull。</returns>
    public CSS_ThiefStatusData GetThiefStatusData(int int_Index)
    {
        if (list_ThiefStatusData == null)
        {
            return null;
        }

        if (int_Index < 0 || int_Index >= list_ThiefStatusData.Count)
        {
            return null;
        }

        return list_ThiefStatusData[int_Index];
    }
}
