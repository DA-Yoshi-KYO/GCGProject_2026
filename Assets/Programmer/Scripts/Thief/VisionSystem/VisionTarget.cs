/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    視界検出ターゲットクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-19 | 初回作成
 * 
 */
using UnityEngine;

// 視界に入る対象を示すクラス
public class VisionTarget : MonoBehaviour
{
    // ターゲットの種類
    public enum TargetType
    {
        [Tooltip("プレイヤー")]
        Player,
        [Tooltip("出入口")]
        Exit,
        [Tooltip("罠")]
        Trap,
        [Tooltip("宝物")]
        Treasure,
        [Tooltip("宝物以外の部屋オブジェクト")]
        RoomObject
    }

    [Tooltip("ターゲットの種類")]
    public TargetType targetType;

    // Exitの場合隣の部屋を示す変数
    // public RoomNode connectedRoom;

    // Trap / Treasureの場合
    public bool isDangerous; // 危険なターゲットかどうか
}
