/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ステージ情報を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17   | 初回作成
 * 2026-05-22   | ファイル名を変更（WaveManager.cs → CS_WaveManager.cs）
 *              | クラス名を変更（WaveManager → CS_WaveManager）
 * 2026-06-10   | 敵の出現データをステージ数に応じてCS_RoomCreatePointに設定する処理を追加
 *              | ファイル名を変更（CS_WaveManager.cs → CS_StageManager.cs）
 *              | クラス名を変更（CS_WaveManager → CS_StageManager）
 * 
 */
using UnityEngine;

/// <summary>
/// ステージに関する情報を管理するシステム
/// </summary>
public class CS_StageManager : MonoBehaviour
{
    [Tooltip("ウェーブ数"), Min(1)]
    private int waveCount;
    public int read_WaveNumber => waveCount;

    [Tooltip("ステージ数"), Min(1)]
    private int stageCount = 1;
    public int read_StageCount => stageCount;

    [Tooltip("最大ウェーブ数"), Min(1)]
    private int maxWaveCount = 1;
    public int read_MaxWaveCount => maxWaveCount;

    [Header("使用する敵のデータベース"), SerializeField]
    private CO_StageThiefDB enemyDatabase;

    [Tooltip("泥棒の親オブジェクト")]
    private GameObject thiefParent;

    [Tooltip("ThiefManagerの参照")]
    private CS_ThiefManager thiefManager;


    /// <summary>
    /// 初期化。ウェーブ数を1にリセットする。
    /// </summary>
    private void Start()
    {
        waveCount = 1;
        stageCount = 1;

        if (enemyDatabase == null)
        {
            Debug.LogError("CS_StageManager: enemyDatabaseがInspectorで設定されていません。", this);
            return;
        }

        maxWaveCount = enemyDatabase.stageThiefDataList[stageCount - 1].waveData.Count; // データベースから現在のステージの最大ウェーブ数を取得して設定する

        Time.timeScale = 1.0f; // 時間のスケールをリセットする

        // 生成情報を登録
        thiefManager = GameObject.FindAnyObjectByType<CS_ThiefManager>();
        if (thiefManager == null)
        {
            Debug.LogError("CS_StageManager: CS_ThiefManagerが見つかりません。", this);
            return;
        }
        thiefManager.RegistGenerationInfo(
            enemyDatabase.stageThiefDataList[stageCount - 1].waveData[waveCount - 1],
            waveCount == 1
            );
    }

    /// <summary>
    /// 生成後敵が全員倒された場合、ウェーブ数を増やす処理を呼び出す
    /// </summary>
    private void Update()
    {
        // ThiefParentがnullの場合は、シーン内からThiefParentを探して設定する
        if (thiefParent == null) thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent == null) return;

        // ThiefManagerがnullの場合は、シーン内からThiefManagerを探して設定する
        if (thiefManager == null)thiefManager = GameObject.FindAnyObjectByType<CS_ThiefManager>();
        if (thiefManager == null) return;

    }

    /// <summary>
    /// ウェーブ数を1増やす
    /// </summary>
    public void WaveCountUp()
    {
        if (IsMaxWave()) return;
        waveCount++;

        // 生成情報を登録
        thiefManager.RegistGenerationInfo(
            enemyDatabase.stageThiefDataList[stageCount - 1].waveData[waveCount - 1],
            waveCount == 1
            );
    }

    /// <summary>
    /// ステージ数を1増やす
    /// </summary>
    public void StageCountUp()
    {
        stageCount++;

        waveCount = 1; // ステージが変わるとウェーブ数はリセットされる
        maxWaveCount = enemyDatabase.stageThiefDataList[stageCount - 1].waveData.Count; // データベースから現在のステージの最大ウェーブ数を取得して設定する

        // 生成情報を登録
        thiefManager.RegistGenerationInfo(
            enemyDatabase.stageThiefDataList[stageCount - 1].waveData[waveCount - 1],
            waveCount == 1
            );
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
