/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ウェーブ情報を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Tooltip("ウェーブ数"), Min(1)]
    private int waveCount;
    public int waveNumber => waveCount;

    private void Awake()
    {
        Reset();
    }

    // ウェーブ数をカウントアップする
    public void CountUp()
    {
        waveCount++;
    }

    // ウェーブ数をリセットする
    public void Reset()
    {
        waveCount = 1;
    }
}
