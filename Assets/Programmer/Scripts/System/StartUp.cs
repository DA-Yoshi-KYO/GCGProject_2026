/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    実行時のロードを行うスクリプト
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-20 | 初回作成
 * 
 */
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUp : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Additive);
    }
}
