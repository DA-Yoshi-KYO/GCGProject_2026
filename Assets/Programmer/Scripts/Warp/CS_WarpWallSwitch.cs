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

    [Header("ワープのスポーンオブジェクトを出す位置"), SerializeField]
    GameObject go_WarpPointObject;

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
                if (go != null)
                {
                    go.SetActive(true);
                }
            }

            foreach (var go in go_OnDeactiveGameObjects)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }
        else
        {
            foreach (var go in go_OnActiveGameObjects)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }

            foreach (var go in go_OnDeactiveGameObjects)
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// ワープオブジェクトを生成する位置を返します。
    /// </summary>
    public Transform GetWarpPointTransform()
    {
        if (go_WarpPointObject == null)
        {
            Debug.LogWarning(
                "[CS_WarpWallSwitch] go_WarpPointObject が設定されていません : "
                + gameObject.name,
                this
            );

            return transform;
        }

        return go_WarpPointObject.transform;
    }
}
