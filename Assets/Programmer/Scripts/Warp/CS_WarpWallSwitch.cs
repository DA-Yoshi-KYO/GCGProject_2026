using System.Collections.Generic;
using UnityEngine;

/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの壁のif処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉本  竜
 * ----------------------------------------------------------
 * 2026-06-15 初回作成
 * 2026-07-15 ワープ後の位置を追加
 */

public class CS_WarpWallSwitch : MonoBehaviour
{
    [Header("ワープの壁である場合にActiveにするオブジェクト")]
    [SerializeField]
    private List<GameObject> go_OnActiveGameObjects;

    [Header("ワープの壁である場合にDeactiveにするオブジェクト")]
    [SerializeField]
    private List<GameObject> go_OnDeactiveGameObjects;

    [Header("ワープの壁であるかどうか")]
    [SerializeField]
    private bool b_WarpWallFlag = true;

    [Header("ワープのスポーンオブジェクトを出す位置")]
    [SerializeField]
    private GameObject go_WarpPointObject;

    [Header("ワープ後の位置")]
    [SerializeField]
    private GameObject go_WarpAfterPosition;

    /// <summary>
    /// 受け取ったbool値に応じて、
    /// ワープの壁であるかどうかを切り替える処理です。
    /// </summary>
    /// <param name="_flag">
    /// この壁がワープであるかどうか。
    /// true = ワープの壁
    /// </param>
    public void SetWarpWallFlag(bool _flag)
    {
        b_WarpWallFlag = _flag;

        if (b_WarpWallFlag)
        {
            foreach (GameObject go in go_OnActiveGameObjects)
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
            }

            foreach (GameObject go in go_OnDeactiveGameObjects)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }
        else
        {
            foreach (GameObject go in go_OnActiveGameObjects)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }

            foreach (GameObject go in go_OnDeactiveGameObjects)
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// ワープPrefabを生成する位置を返します。
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

    /// <summary>
    /// ワープ後にプレイヤーが出現する位置を返します。
    /// </summary>
    public Transform GetWarpAfterPositionTransform()
    {
        if (go_WarpAfterPosition == null)
        {
            Debug.LogWarning(
                "[CS_WarpWallSwitch] go_WarpAfterPosition が設定されていません : "
                + gameObject.name,
                this
            );

            // 未設定の場合はワープPrefabの生成位置を使用する
            return GetWarpPointTransform();
        }

        return go_WarpAfterPosition.transform;
    }
}
