using UnityEngine;

/*==================================================
 *  ファイル名  : CSS_ThiefStatusData.cs
 *  制作者      : 吉本竜
 *  内容        : 盗賊のステータスデータを管理するScriptableObject
 *  履歴        : 2026/05/06 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// 盗賊のステータスデータを管理するScriptableObjectです。
/// 現在は盗賊名のみを管理します。
/// </summary>
[CreateAssetMenu(
    fileName = "ThiefStatusData",
    menuName = "ScriptableObjects/ThiefStatusData")]
public class CSS_ThiefStatusData : ScriptableObject
{
    [Header("盗賊基本情報")]
    [SerializeField]
    private string str_ThiefName = "盗賊";

    /// <summary>
    /// 盗賊名を取得します。
    /// </summary>
    /// <returns>盗賊名。</returns>
    public string GetThiefName()
    {
        return str_ThiefName;
    }
}
