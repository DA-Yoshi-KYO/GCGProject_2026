/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    チュートリアル用マネージャー
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-07-04 | 初回作成
 * 2026-07-05 | チュートリアル用の泥棒を出現させる処理を追加
 *  
 */
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// チュートリアル用のマネージャークラスです。
/// </summary>
public class CS_TutorialManager : MonoBehaviour
{
    [Serializable]
    public class ThiefSpawnData
    {
        [Header("泥棒の共通ステータスデータ")]
        public CO_ThiefCommonStatusData thiefCommonData;

        [Header("泥棒の種類別ステータスデータ")]
        public CO_ThiefStatusData thiefTypeData;

        [Header("泥棒の出現位置")]
        public Vector3 spawnPoint;
    }

    [SerializeField, Header("泥棒の出現データリスト")]
    private List<ThiefSpawnData> thiefSpawnDataList;


    private void Start()
    {
        GameObject thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent == null)
        {
            // ThiefParentが存在しない場合は新しく作成
            thiefParent = new GameObject("ThiefParent");
        }

        // チュートリアル用の泥棒を出現させる処理
        foreach (var spawnData in thiefSpawnDataList)
        {
            // 泥棒のプレハブを生成
            GameObject thief = Instantiate(
                spawnData.thiefTypeData.thiefPrefab,
                spawnData.spawnPoint,
                Quaternion.identity,
                thiefParent.transform
                );

            thief.name = "Thief_" + thiefParent.transform.childCount;

            // 基準となるプレイヤーの速度を取得
            float playerSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<CS_PlayerMove>().GetBasePlayerSpeed();

            // 行動AIの設定
            CS_ThiefAI thiefAI = thief.GetComponent<CS_ThiefAI>();
            thiefAI.Setting(Instantiate(spawnData.thiefTypeData), Instantiate(spawnData.thiefCommonData), playerSpeed, null, null);
        }
    }
}
