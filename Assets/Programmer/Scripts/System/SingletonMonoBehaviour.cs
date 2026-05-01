/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    MonoBehaviourを継承したシングルトンの基底クラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-20 | 初回作成
 * 
 */
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    // シングルトンインスタンス
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        // すでにインスタンスが存在する場合は警告を出して削除する
        if (Instance != null)
        {
            Debug.LogWarning($"You are about to create more than one instance of {typeof(T)}!" +
                             $" It will be removed from {gameObject.name}.");
            Destroy(this);
        }
        else
        {
            // インスタンスを設定
            Instance = this as T;
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
