/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの場所作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-08 | 初回作成
 */
using UnityEngine;

public class CS_WarpPoint : MonoBehaviour
{
    public CS_WarpPoint targetPoint;//ワープ先
    public Transform warpExitPosition;//ワープ後に出る位置
    public bool warping = false;
    public float warpTimeCount = 0.0f;
}
