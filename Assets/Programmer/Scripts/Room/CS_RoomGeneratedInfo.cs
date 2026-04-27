using UnityEngine;

/*==================================================
 *  ファイル名  : CS_RoomGeneratedInfo.cs
 *  制作者      : 吉本竜
 *  内容        : 生成Roomと元RoomCreatePointの対応情報
 *  履歴        : 2026/04/27 新規作成(ヨシモト)
 *==================================================*/

/// <summary>
/// 生成されたRoomが、どのRoomCreatePointから生成されたかを保持します。
/// </summary>
[DisallowMultipleComponent]
public class CS_RoomGeneratedInfo : MonoBehaviour
{
    [SerializeField]
    private CS_RoomCreatePoint cs_SourceCreatePoint;

    /// <summary>
    /// 元になったRoomCreatePointを取得します。
    /// </summary>
    public CS_RoomCreatePoint SourceCreatePoint => cs_SourceCreatePoint;

    /// <summary>
    /// 元になったRoomCreatePointを設定します。
    /// </summary>
    /// <param name="cs_CreatePoint">元になったRoomCreatePoint。</param>
    public void SetSourceCreatePoint(CS_RoomCreatePoint cs_CreatePoint)
    {
        cs_SourceCreatePoint = cs_CreatePoint;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
