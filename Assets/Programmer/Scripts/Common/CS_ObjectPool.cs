/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    オブジェクトプールの作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CS_ObjectPool
{
    private GameObject prefab;
    private int initialSize = 10;
    private List<GameObject> poolList = new List<GameObject>();

    private GameObject objectParent;

    // Start is called before the first frame update
    public CS_ObjectPool(GameObject gameObject, GameObject poolParent)
    {
        prefab = gameObject;
        objectParent = poolParent;
        for (int i = 0 ; i < initialSize ; ++i)
        {
            GameObject obj = GameObject.Instantiate(gameObject);
            obj.transform.SetParent(objectParent.transform);
            obj.SetActive(false);
            poolList.Add(obj);
        }
    }
    public GameObject GetObject()
    {
        //非アクティブを探す
        foreach (var obj in poolList)
        {
            if (obj.activeSelf) continue;
            obj.SetActive(true);
            return obj;
        }

        //全部アクティブなら新しく作って作成
        GameObject newObject = GameObject.Instantiate(prefab);
        newObject.transform.SetParent(objectParent.transform);
        poolList.Add(newObject);
        return newObject;
    }

    //非アクティブにする処理
    public IEnumerator DisableAfterTime(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
