/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ウェーブ情報を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-05-22 | ファイル名を変更（WaveManager.cs → CS_WaveManager.cs）
 *            | クラス名を変更（WaveManager → CS_WaveManager）
 * 
 */
using UnityEngine;

/// <summary>
/// ウェーブに関する情報を管理するシステム
/// </summary>
public class CS_WaveManager : MonoBehaviour
{
    [Tooltip("ウェーブ数"), Min(1)]
    private int waveCount;
    public int waveNumber => waveCount;

    [Tooltip("最大ウェーブ数"), Min(1)]
    private int maxWaveCount = 1;
    public int read_MaxWaveCount => maxWaveCount;

    /// <summary>
    /// 初期化。ウェーブ数を1にリセットする。
    /// </summary>
    private void Awake()
    {
        waveCount = 1;
    }

    /// <summary>
    /// ウェーブ数を1増やす
    /// </summary>
    public void CountUp()
    {
        waveCount++;
    }

    /// <summary>
    /// 現在のウェーブ数が最大ウェーブ数に達しているかどうかを返す
    /// </summary>
    /// <returns>現在のウェーブ数が最大ウェーブ数に達している場合はtrue、そうでない場合はfalse</returns>
    public bool IsMaxWave()
    {
        return waveCount >= maxWaveCount;
    }
}
