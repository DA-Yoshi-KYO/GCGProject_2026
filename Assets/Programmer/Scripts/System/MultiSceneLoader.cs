/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    マルチシーンを管理・ロードするクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-20 | 初回作成
 * 
 */
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneLoader : MonoBehaviour
{
    [SerializeField]
    private SceneAsset[] sceneAssets;

    void Start()
    {
        foreach (var item in sceneAssets)
        {
            SceneManager.LoadScene(item.name, LoadSceneMode.Additive);
        }
    }

    // 他シーンのオブジェクトを参照したいときのメソッド(例)
    private void ExampleMethod()
    {
        // 例えば、"OtherScene"という名前のシーンにあるオブジェクトを参照したい場合
        Scene otherScene = SceneManager.GetSceneByName("OtherScene");
        if (otherScene.isLoaded)
        {
            GameObject[] rootObjects = otherScene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                // ここでobjを使って何か処理をする
                //ThiefAI thiefAI = obj.GetComponentInChildren<ThiefAI>();

                //thiefAI.Setting(100, 5.0f, 3);
            }
        }
    }
}
