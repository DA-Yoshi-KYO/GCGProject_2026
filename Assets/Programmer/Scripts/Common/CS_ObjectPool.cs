/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    オブジェクトプールの作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 * 2026-07-09 | 継承して使えるようにvirtual処理を追加(ヨシモト)
 * 2026-07-15 | フェーズ用のObjectをPoolで戻す処理追加
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObject用のObjectPool基底クラスです。
/// 継承先で取得時・返却時の処理を変更できます。
/// </summary>
public class CS_ObjectPool
{
    /// <summary>
    /// 複製元Prefabです。
    /// </summary>
    protected GameObject prefab;

    /// <summary>
    /// 初期生成数です。
    /// </summary>
    protected int initialSize = 10;

    /// <summary>
    /// Pool最大数です。
    /// 0以下の場合は上限なしです。
    /// </summary>
    protected int maxPoolSize = 0;

    /// <summary>
    /// Pool管理中のObject一覧です。
    /// </summary>
    protected List<GameObject> poolList = new List<GameObject>();

    /// <summary>
    /// Pool待機中Objectの親です。
    /// </summary>
    protected GameObject objectParent;

    /// <summary>
    /// ObjectPoolを作成します。
    /// </summary>
    public CS_ObjectPool(GameObject gameObject, GameObject poolParent)
    {
        prefab = gameObject;
        objectParent = poolParent;

        for (int i = 0 ; i < initialSize ; ++i)
        {
            CreatePoolObject();
        }
    }

    /// <summary>
    /// ObjectPoolを作成します。
    /// </summary>
    public CS_ObjectPool(
        GameObject gameObject,
        GameObject poolParent,
        int poolSize)
    {
        prefab = gameObject;
        objectParent = poolParent;

        initialSize = Mathf.Max(0, poolSize);
        maxPoolSize = Mathf.Max(0, poolSize);

        for (int i = 0 ; i < initialSize ; ++i)
        {
            CreatePoolObject();
        }
    }

    /// <summary>
    /// Pool最大数を設定します。
    /// </summary>
    public void SetMaxPoolSize(int poolSize)
    {
        maxPoolSize = Mathf.Max(0, poolSize);
    }

    /// <summary>
    /// PoolからObjectを取得します。
    /// </summary>
    public virtual GameObject GetObject()
    {
        for (int i = 0 ; i < poolList.Count ; i++)
        {
            GameObject obj = poolList[i];

            if (obj == null)
            {
                continue;
            }

            if (obj.activeSelf)
            {
                continue;
            }

            obj.SetActive(true);

            OnGetObject(obj);

            return obj;
        }

        if (maxPoolSize > 0 &&
            poolList.Count >= maxPoolSize)
        {
            return null;
        }

        GameObject newObject = CreatePoolObject();

        if (newObject == null)
        {
            return null;
        }

        newObject.SetActive(true);

        OnGetObject(newObject);

        return newObject;
    }

    /// <summary>
    /// ObjectをPoolへ戻します。
    /// </summary>
    public virtual void ReturnObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        OnReturnObject(gameObject);

        if (objectParent != null)
        {
            gameObject.transform.SetParent(
                objectParent.transform,
                true);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 指定時間後に非アクティブにします。
    /// </summary>
    public IEnumerator DisableAfterTime(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);

        ReturnObject(gameObject);
    }

    /// <summary>
    /// ObjectをPoolへ戻します。（フェード用）
    /// </summary>
    public IEnumerator FadeReturnObject(GameObject gameObject, float fadetime)
    {
        var material = gameObject.GetComponent<MeshRenderer>().material;
        if (material == null)
            yield break;

        float startAlpha = material.GetFloat("_AlphaFloat");
        float time = 0.0f;

        while(time < fadetime)
        {
            time += Time.deltaTime;
            material.SetFloat("_AlphaFloat", Mathf.Lerp(startAlpha, 0.0f, time / fadetime));

            yield return null;
        }

        //完全に透明になったらプールに戻す
        ReturnObject(gameObject);

        material.SetFloat("_AlphaFloat", 1.0f);
    }


    /// <summary>
    /// Pool用Objectを作成します。
    /// </summary>
    protected virtual GameObject CreatePoolObject()
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject obj = GameObject.Instantiate(prefab);

        obj.name = prefab.name + "(Pool)";

        if (objectParent != null)
        {
            obj.transform.SetParent(
                objectParent.transform,
                true);
        }

        obj.SetActive(false);

        poolList.Add(obj);

        return obj;
    }

    /// <summary>
    /// Object取得時の追加処理です。
    /// 継承先で上書きします。
    /// </summary>
    protected virtual void OnGetObject(GameObject gameObject)
    {

    }

    /// <summary>
    /// Object返却時の追加処理です。
    /// 継承先で上書きします。
    /// </summary>
    protected virtual void OnReturnObject(GameObject gameObject)
    {

    }
}
