/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒を管理するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-19 | 泥棒のパラメーター設定処理の記載(行動AIの設定、視界システムの設定)
 * 2026-04-23 | 移動速度の設定処理の記載(プレイヤーの速度を仮で用意して、そこから泥棒の速度を計算するように変更)
 * 2026-04-26 | ファイル名・クラス名をThiefManagerに変更
 * 2026-05-07 | CS_RoomEnemyEntryPointDataを用いた泥棒の生成処理の記載
 *            | 生成タイムと生成数の管理の記載
 *            | 生成位置の選定の記載
 * 2026-05-22 | ファイル名を変更（ThiefManager.cs → CS_ThiefManager.cs）
 *            | クラス名を変更（ThiefManager → CS_ThiefManager）
 *            | データベースから生成間隔の値を取得して設定する処理の記載
 * 2026-05-24 | 生成情報のスタックをリストに変更し、複数の部屋から同時に泥棒を生成できるように変更
 *            | 生成情報のスタックを辞書に変更し、部屋ごとに泥棒を生成できるように変更
 * 
 */
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 泥棒を管理するクラス
/// </summary>
public class CS_ThiefManager : MonoBehaviour
{
    [SerializeField, Tooltip("泥棒の種類共通パラメーターのデータベース")]
    private CO_ThiefCommonStatusData thiefCommonDB;
    public CO_ThiefCommonStatusData GetThiefCommonDB() { return GameObject.Instantiate(thiefCommonDB); }

    [Tooltip("初回生成が完了しているかどうか")]
    private bool isFirstGenerationComplete = false;
    public bool read_IsFirstGenerationComplete => isFirstGenerationComplete;

    [Tooltip("生成が完了しているかどうか")]
    private bool isGenerationComplete = false;
    public bool read_IsGenerationComplete => isGenerationComplete;

    [Tooltip("ウェーブ進行後のリセットを行うかどうか")]
    private bool isResetAfterWaveProgress = true;
    public bool read_IsResetAfterWaveProgress => isResetAfterWaveProgress;

    [Tooltip("ゲーム開始から一体目を生成するまでの時間")]
    private float firstSpawnDelay = 5.0f;
    [Tooltip("ウェーブ進行後に一体目を生成するまでの時間")]
    private float waveProgressSpawnDelay = 5.0f;
    [Tooltip("泥棒を生成する間隔の時間")]
    private float spawnInterval = 5.0f;


    [Tooltip("生成する泥棒スタック情報")]
    public class ThiefSpawnInfo
    {
        [Tooltip("入ってくる部屋の情報")]
        public class ThiefEntryInfo
        {
            [Tooltip("部屋の名前")]
            public string roomName;
            [Tooltip("出入口の方向")]
            public CSE_RoomDoorDirection entryDirection;
        }

        [Tooltip("入ってくる部屋の情報")]
        public ThiefEntryInfo entryInfo;

        [Tooltip("生成する泥棒のタイプ")]
        public CO_ThiefStatusData thiefTypeData;

        [Tooltip("生成されるまでの秒数")]
        public float spawnDelay;

        public ThiefSpawnInfo(string roomName, CSE_RoomDoorDirection entryDirection, CO_ThiefStatusData thiefTypeData, float spawnDelay)
        {
            this.entryInfo = new ThiefEntryInfo
            {
                roomName = roomName,
                entryDirection = entryDirection
            };
            this.thiefTypeData = thiefTypeData;
            this.spawnDelay = spawnDelay;
        }
    }

    [Tooltip("敵の生成情報のスタック")]
    private List<Stack<ThiefSpawnInfo>> thiefWaveStack = new List<Stack<ThiefSpawnInfo>>();


    private void Awake()
    {
        // 泥棒の親オブジェクトを生成
        new GameObject("ThiefParent");

        CO_ThiefCommonStatusData commonStatusData = GetThiefCommonDB();
        firstSpawnDelay = commonStatusData.firstCreateInterval;
        waveProgressSpawnDelay = commonStatusData.nextWaveCreateInterval;
        spawnInterval = commonStatusData.createInterval;
    }

    /// <summary>
    /// 毎フレーム、敵の出入口から泥棒を生成する処理を行う
    /// </summary>
    private void Update()
    {
        // スタックされた生成情報が存在する場合、生成処理を行う
        if (thiefWaveStack.Count <= 0) return;

        // スタック情報をスナップショット
        var thiefWaveStackSnapshot = new List<Stack<ThiefSpawnInfo>>(thiefWaveStack);

        foreach (var item in thiefWaveStackSnapshot)
        {
            if (item.Count <= 0)
            {
                thiefWaveStack.Remove(item);
                continue;
            }

            // 生成までの時間を減らす
            item.Peek().spawnDelay -= Time.deltaTime;

            if (item.Peek().spawnDelay <= 0)
            {
                // 泥棒を生成する処理
                Create(item.Peek());
                // スタックから生成情報を削除
                item.Pop();
            }
        }
    }

    /// <summary>
    /// 指定された生成情報に基づいて泥棒を生成する処理
    /// </summary>
    /// <param name="Info">生成情報</param>
    private void Create(ThiefSpawnInfo Info)
    {
        // 生成する泥棒の親オブジェクトを取得、存在しない場合は生成
        GameObject thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent == null)
        {
            thiefParent = new GameObject("ThiefParent");
        }

        GameObject roomObject = GameObject.Find(Info.entryInfo.roomName);

        CS_RoomEnemyEntryPointCollector collector = GameObject.Find("RoomCreatePoints").GetComponent<CS_RoomEnemyEntryPointCollector>();

        // 生成位置の取得
        Transform entryPoint = roomObject.transform.GetComponent<CS_RoomCreatePoint>().GetRoomDoorPosition(Info.entryInfo.entryDirection);

        // 生成される初期部屋の取得
        CS_RoomNode entryRoom = roomObject.transform.GetComponentInChildren<CS_RoomNode>();

        // 泥棒のタイプに応じたデータを取得
        CO_ThiefStatusData typeData = Info.thiefTypeData;

        // ============================================ 応急処置
        //泥棒の生成
        GameObject thief = GameObject.Instantiate(
            typeData.thiefPrefab,
            entryPoint.position,
            entryPoint.rotation,
            thiefParent.transform
        );

        thief.name = "Thief_" + thiefParent.transform.childCount;

        // 近くのNavMesh上の位置を検索して、泥棒をそこにワープさせる
        UnityEngine.AI.NavMeshAgent agent = thief.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null && UnityEngine.AI.NavMesh.SamplePosition(entryPoint.position, out UnityEngine.AI.NavMeshHit hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // 基準となるプレイヤーの速度を取得
        float playerSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<CS_PlayerMove>().GetBasePlayerSpeed();

        // 行動AIの設定
        CS_ThiefAI thiefAI = thief.GetComponent<CS_ThiefAI>();
        thiefAI.Setting(GameObject.Instantiate(typeData), GetThiefCommonDB(), playerSpeed, entryRoom, Info.entryInfo.entryDirection, entryPoint);
    }

    /// <summary>
    /// 指定されたウェーブ泥棒データを元に、生成情報を登録する処理
    /// </summary>
    /// <param name="waveThiefData">ウェーブ泥棒データ</param>
    /// <param name="isFirstWave">最初のウェーブかどうか</param>
    public void RegistGenerationInfo(CO_StageThiefDB.StageThiefData.WaveThiefData waveThiefData, bool isFirstWave)
    {
        // 新しいスタックを作成し、生成情報を追加
        Dictionary<string, Stack<ThiefSpawnInfo>> newStacks = new Dictionary<string, Stack<ThiefSpawnInfo>>();

        foreach (var entryData in waveThiefData.enemtEntryDatas)
        {
            foreach (var thief in entryData.thiefEntryDoorDirInfos[0].thiefStatusDatas)
            {
                ThiefSpawnInfo spawnInfo;

                if (newStacks.ContainsKey(entryData.roomName))
                {
                    spawnInfo = new ThiefSpawnInfo(
                        entryData.roomName,                                         // 部屋の名前
                        entryData.thiefEntryDoorDirInfos[0].enemyDoorDir,           // 出入口の方向
                        thief,                                                      // 泥棒のタイプデータ
                        spawnInterval                                               // 生成されるまでの秒数
                    );

                    newStacks[entryData.roomName].Push(spawnInfo);

                }
                else
                {
                    spawnInfo = new ThiefSpawnInfo(
                        entryData.roomName,                                         // 部屋の名前
                        entryData.thiefEntryDoorDirInfos[0].enemyDoorDir,           // 出入口の方向
                        thief,                                                      // 泥棒のタイプデータ
                        isFirstWave ? firstSpawnDelay : waveProgressSpawnDelay      // 生成されるまでの秒数
                        );

                    Stack<ThiefSpawnInfo> newStack = new Stack<ThiefSpawnInfo>();
                    newStack.Push(spawnInfo);
                    newStacks.Add(entryData.roomName, newStack);
                }
            }
        }

        // 新しいスタックをリストに追加
        foreach (var stack in newStacks.Values)
        {
            thiefWaveStack.Add(stack);
        }
    }

    /// <summary>
    /// 指定された泥棒データを元に、生成情報を登録する処理
    /// </summary>
    /// <param name="thiefData">泥棒データ</param>
    /// <param name="roomName">生成する部屋の名前</param>
    /// <param name="doorDir">生成する出入口の方向</param>
    public void RegistGenerationInfo(CO_ThiefStatusData thiefData, string roomName, CSE_RoomDoorDirection doorDir)
    {
        // 新しいスタックを作成し、生成情報を追加
        Stack<ThiefSpawnInfo> newStacks = new Stack<ThiefSpawnInfo>();
        // 生成情報を作成
        ThiefSpawnInfo spawnInfo = new ThiefSpawnInfo(
            roomName,                       // 部屋の名前
            doorDir,                        // 出入口の方向
            thiefData,                      // 泥棒のタイプデータ
            spawnInterval          // 生成されるまでの秒数
        );
        newStacks.Push(spawnInfo);

        // 新しいスタックをリストに追加
        thiefWaveStack.Add(newStacks);
    }

    /// <summary>
    /// 全ての泥棒の記憶から指定されたターゲットを消す処理
    /// </summary>
    /// <param name="obj">記憶から消すターゲット</param>
    public void EraseTheMemoryToAllThief(CS_ThiefTarget obj)
    {
        // 全泥棒を取得
        GameObject[] thieves = GameObject.FindAnyObjectByType<CS_ThiefAI>().gameObject.scene.GetRootGameObjects();
        foreach (var thief in thieves)
        {
            CS_ThiefAI thiefAI = thief.GetComponentInChildren<CS_ThiefAI>();
            if (thiefAI != null)
            {
                thiefAI.read_MemorySystem.EraseTheMemory(obj);
            }
        }

    }

    /// <summary>
    /// 逃走中の泥棒が存在するかを返す処理(宝物を持っていかれているかどうか)
    /// </summary>
    /// <returns>逃走中の泥棒が存在する場合はtrue、存在しない場合はfalse</returns>
    public bool IsEscapeThief()
    {
        // 泥棒の親オブジェクトを取得
        GameObject thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent == null)
        {
            //Debug.LogError("ThiefParentが存在しません。");
            return false;
        }

        // 全ての泥棒をチェックして、逃走中の泥棒が存在するかを確認
        for (int i = 0 ; i < thiefParent.transform.childCount ; i++)
        {
            GameObject thief = thiefParent.transform.GetChild(i).gameObject;
            CS_ThiefAI thiefAI = thief.GetComponent<CS_ThiefAI>();
            if (thiefAI == null) continue;

            if (thiefAI.read_CurrentState == CS_ThiefAI.ThiefState.Escape)
            {
                return true; // 逃走中の泥棒が存在する場合はtrueを返す
            }
        }

        return false;
    }

    /// <summary>
    /// 全ての泥棒の記憶から指定された危険地帯IDを消す処理
    /// </summary>
    /// <param name="zoneID">記憶から消す危険地帯ID</param>
    public void EraseTheAvoidZoneIDToAllThief(int zoneID)
    {
        // 泥棒の親オブジェクトを取得
        GameObject thiefParent = GameObject.Find("ThiefParent");
        if (thiefParent == null)
        {
            Debug.LogError("ThiefParentが存在しません。");
        }

        // 全ての泥棒をチェックして、指定の危険地帯IDを記憶から消す
        for (int i = 0; i < thiefParent.transform.childCount; i++)
        {
            GameObject thief = thiefParent.transform.GetChild(i).gameObject;
            CS_ThiefAI thiefAI = thief.GetComponent<CS_ThiefAI>();
            if (thiefAI != null)
            {
                thiefAI.read_MemorySystem.RemoveAvoidZoneID(zoneID);
            }
        }
    }
}
