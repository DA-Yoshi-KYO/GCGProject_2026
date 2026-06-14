using System.Collections.Generic;
using UnityEngine;

/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの壁のif処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉本  竜
 * ----------------------------------------------------------
 * 2026-06-15 初回作成
 */

public class CS_WarpWallSwitch : MonoBehaviour
{
    [Header("ワープの壁である場合にActiveにするオブジェクト"), SerializeField]
    List<GameObject> go_OnActiveGameObjects;
    [Header("ワープの壁である場合にDeactiveにするオブジェクト"), SerializeField]
    List<GameObject> go_OnDeactiveGameObjects;
    [Header("ワープの壁であるかどうか"), SerializeField]
    bool b_WarpWallFlag = true;

    /// <summary>
    /// 受け取ったbool値に応じて、ワープの壁であるかどうかを切り替える処理
    /// </summary>
    /// <param name="_flag">この壁はワープであるかどうか true = ワープ</param>
    public void SetWarpWallFlag(bool _flag)
    {
        b_WarpWallFlag = _flag;
        if (b_WarpWallFlag)
        {
            foreach (var go in go_OnActiveGameObjects)
            {
                go.SetActive(true);
            }
            foreach (var go in go_OnDeactiveGameObjects)
            {
                go.SetActive(false);
            }
        }
        else
        {
            foreach (var go in go_OnActiveGameObjects)
            {
                go.SetActive(false);
            }
            foreach (var go in go_OnDeactiveGameObjects)
            {
                go.SetActive(true);
            }
        }
    }
}
