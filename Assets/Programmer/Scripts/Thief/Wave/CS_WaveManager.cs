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

    /// <summary>
    /// 初期化。ウェーブ数を1にリセットする。
    /// </summary>
    private void Awake()
    {
        Reset();
    }

    /// <summary>
    /// ウェーブ数を1増やす
    /// </summary>
    public void CountUp()
    {
        waveCount++;
    }

    /// <summary>
    /// ウェーブ数をリセットする
    /// </summary>
    public void Reset()
    {
        waveCount = 1;
    }
}
