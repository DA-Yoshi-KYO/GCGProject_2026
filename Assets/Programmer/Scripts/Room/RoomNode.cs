/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *  　部屋の情報を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

// 部屋の情報を管理するクラス
public class RoomNode : MonoBehaviour
{
    [Tooltip("部屋のID")]
    public int roomID;

    [Tooltip("部屋に設置されているオブジェクトリスト")]
    public List<VisionTarget> roomObjects;

    [Tooltip("部屋の移動ポイントリスト")]
    public List<ThiefTarget> movePoints;

    [Tooltip("移動ポイントの回り方")]
    public bool isRight;// trueなら右回り、falseなら左回り
}
