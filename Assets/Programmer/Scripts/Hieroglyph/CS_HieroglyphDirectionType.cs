using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*==================================================
 *  ファイル名  : CS_HieroglyphDirectionType.cs
 *  制作者     : 吉本竜
 *  内容       : 部屋のヒエログラフの方向を管理するクラス
 *  履歴       : 2026/07/16 新規作成(ヨシモト)
 *==================================================*/


public class CS_HieroglyphDirectionType : MonoBehaviour
{
    [Header("ヒエログラフの扉の方向")]
    [SerializeField]
    private CSE_RoomDoorDirection direction = CSE_RoomDoorDirection.Front;

    /// <summary>
    /// 部屋のヒエログラフの方向を取得する
    /// </summary>
    /// <returns>CSE_RoomDoorDirection</returns>
    public CSE_RoomDoorDirection GetDirection()
    {
        return direction;
    }
}
