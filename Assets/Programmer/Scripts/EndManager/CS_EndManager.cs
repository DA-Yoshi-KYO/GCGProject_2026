/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    勝敗を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-06-01 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ウェーブに関する情報を管理するシステム
/// </summary>
public class CS_EndManager : MonoBehaviour
{
    [Tooltip("勝敗フラグ")]
    private bool isWin = false;
    public bool read_IsWin => isWin;

    [Tooltip("終了しているかどうか")]
    private bool isEnd = false;
    public bool read_IsEnd => isEnd;

    [Tooltip("宝物のリスト")]
    private List<CS_VisionTarget> treasureList = new List<CS_VisionTarget>();
    public List<CS_VisionTarget> read_TreasureList => treasureList;

    [Tooltip("宝物の総数の最大値")]
    private int maxTotalTreasureCount = 0;
    public int read_MaxTotalTreasureCount => maxTotalTreasureCount;


    private void Update()
    {
        //--- 勝敗判定 ---//


        // 宝物が残っている場合
        if (treasureList.Count > 0)
        {
            // 泥棒が残っているかどうかを確認
            GameObject thiefParent = GameObject.Find("ThiefParent");
            if (thiefParent == null) return;
            int thiefCount = thiefParent.transform.childCount;

            // CS_ThiefManagerを取得
            CS_ThiefManager thiefManager = GameObject.FindObjectOfType<CS_ThiefManager>();
            if (thiefManager == null) return;

            // まだ生成する予定がある場合は勝敗判定を行わない
            if (thiefManager.read_IsThiefWaveStackExist) return;

            // WaveManagerを取得
            CS_StageManager stageManager = GameObject.FindObjectOfType< CS_StageManager>();
            if (stageManager == null) return;

            // 泥棒がいなくなった場合
            if (thiefCount == 0 && stageManager.IsMaxWave())
            {
                // 勝利
                isWin = true;
                isEnd = true;
                Time.timeScale = 0f; // ゲームを停止
            }
        }
        // 宝物が残っていない場合
        else
        {
            // 敗北

            isWin = false;
            isEnd = true;
            Time.timeScale = 0f; // ゲームを停止
        }
    }

    /// <summary>
    /// 宝物を追加
    /// </summary>
    public void AddTreasure(CS_VisionTarget treasure)
    {
        treasureList.Add(treasure);
        maxTotalTreasureCount = treasureList.Count; // 最大値を更新
    }

    /// <summary>
    /// 宝物が盗まれた
    /// </summary>
    public void StolenTreasure(CS_VisionTarget treasure)
    {
        treasureList.Remove(treasure);
    }
}
