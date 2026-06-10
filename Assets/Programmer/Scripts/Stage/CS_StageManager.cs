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

    [Tooltip("登録処理を実行したかどうか")]
    private bool isRegistered = false;

    /// <summary>
    /// 初期化。ウェーブ数を1にリセットする。
    /// </summary>
    private void Awake()
    {
        waveCount = 1;
        stageCount = 1;

    }

    /// <summary>
    /// ウェーブ数を1増やす
    /// </summary>
    public void WaveCountUp()
    {
        waveCount++;
    }

    /// <summary>
    /// ステージ数を1増やす
    /// </summary>
    public void StageCountUp()
    {
        stageCount++;
        isRegistered = false; // ステージ数が変わったので、登録処理を再度実行できるようにする
    }

    /// <summary>
    /// 現在のウェーブ数が最大ウェーブ数に達しているかどうかを返す
    /// </summary>
    /// <returns>現在のウェーブ数が最大ウェーブ数に達している場合はtrue、そうでない場合はfalse</returns>
    public bool IsMaxWave()
    {
        return waveCount >= maxWaveCount;
    }

    /// <summary>
    /// 敵の出現データをステージ数に応じてCS_RoomCreatePointに設定する
    /// </summary>
    public void SetStageEnemyEntryData()
    {
        // 登録処理がすでに実行されている場合は、再度実行しないようにする
        if (isRegistered) return;

        if (enemyDatabase == null)
        {
            Debug.LogError("敵のデータベースが設定されていません。");
            return;
        }

        var EnemyDatas = enemyDatabase.thiefData[stageCount - 1].enemtEntryDatas;

        // データベースから取得した敵の出現データをもとに、各部屋のCS_RoomCreatePointに敵の出現データを設定する
        foreach (var data in EnemyDatas)
        {
            // 部屋の名前から部屋のGameObjectを取得
            GameObject room = GameObject.Find(data.roomName);

            if (room == null)
            {
                Debug.LogError($"部屋のGameObjectが見つかりません。部屋の名前: {data.roomName}");
                continue;
            }

            // 取得した部屋のGameObjectからCS_RoomCreatePointコンポーネントを取得
            CS_RoomCreatePoint createPoint = room.GetComponent<CS_RoomCreatePoint>();

            if (createPoint == null)
            {
                Debug.LogError("部屋のGameObjectにCS_RoomCreatePointコンポーネントがアタッチされていません。");
                continue;
            }

            // データベースから取得した敵の情報をCS_RoomCreatePointに設定
            foreach (var doorDirInfo in data.thiefEntryDoorDirInfos)
            {
                createPoint.SetEnemyData(doorDirInfo.enemyDoorDir, doorDirInfo.waveDataBase);
            }
        }

        isRegistered = true; // 登録処理が完了したことを示すフラグを立てる
    }
}
