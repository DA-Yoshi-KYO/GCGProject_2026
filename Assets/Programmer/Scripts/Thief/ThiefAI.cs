/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のAIシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */
using System.Collections.Generic;
using UnityEngine;

// 泥棒のAIシステム
public class ThiefAI : MonoBehaviour
{
    [Tooltip("泥棒の行動状態を定義する列挙型")]
    private enum ThiefState
    {
        [Tooltip("探索状態")]
        Explore,
        [Tooltip("発見状態")]
        Found,
        [Tooltip("逃走状態")]
        Escape,
    }

    [Tooltip("現在の行動状態")]
    private ThiefState currentState;

    [Tooltip("現在いる部屋の情報")]
    private RoomNode currentRoom;
    [Tooltip("部屋に関する記憶")]
    private Dictionary<RoomNode, RoomMemory> roomMemories;
    [Tooltip("探索対象")]
    private VisionTarget currentTarget;

    [Tooltip("泥棒の耐久力")]
    private int durability;
    [Tooltip("泥棒の移動速度")]
    private float speed;
    [Tooltip("次の部屋探索に切り替える探索度の閾値")]
    private int nextRoomSearchThreshold;

    private void Start()
    {
        // 初期状態を探索に設定
        currentState = ThiefState.Explore;

        // 初期部屋を設定（仮）
        currentRoom = FindObjectOfType<RoomNode>();

        // 部屋の記憶を初期化
        roomMemories = new Dictionary<RoomNode, RoomMemory>();

        // 初期部屋の記憶を作成
        roomMemories[currentRoom] = new RoomMemory();
        roomMemories[currentRoom].FirstSetting();

        // 初期耐久力
        durability = 4;

        // 初期移動速度
        speed = 1f;
    }

    // 泥棒の耐久力と移動速度を設定するメソッド
    public void Setting(int durability, float speed, int nextRoomSearchThreshold)
    {
        this.durability = durability;
        this.speed = speed;
        this.nextRoomSearchThreshold = nextRoomSearchThreshold;
    }

    private void Update()
    {
        // 現在の状態に応じた行動を実行
        switch (currentState)
        {
            case ThiefState.Explore:
                Explore();
                break;
            case ThiefState.Found:
                Found();
                break;
            case ThiefState.Escape:
                Escape();
                break;
        }
    }



    // 探索状態の行動
    // TODO: 探索対象がない場合は部屋に設定されている移動ルートに沿って移動する処理を追加する
    //     : ? 歩くだけで探索度が上がるかも
    private void Explore()
    {
        // 探索対象を決定
        RecognizeObjects();

        if (currentTarget == null)
        {
            DecideTarget();
            return;
        }

        // 現在の探索対象の記憶情報
        VisionTargetMemory targetMemory = roomMemories[currentRoom].recognizedObjects[currentTarget];

        if (targetMemory.isExplored)
        {
            // 探索対象が既に探索済みの場合は、次の探索対象を決定
            DecideTarget();
        }
        else
        {
            // 探索対象に向かって移動
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // 探索対象の方に向く
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed * 0.5f);

            // 探索対象に十分近づいたら、探索完了とする
            if (Vector3.Distance(transform.position, currentTarget.transform.position) < targetMemory.exploredDistanceThreshold)
            {
                // 探索対象を探索済みに設定
                targetMemory.isExplored = true;
                // 探索度を加算
                roomMemories[currentRoom].explorationLevel += targetMemory.explorationValue;
                // 次の探索対象を決定
                DecideTarget();
            }
        }
    }

    // 発見状態の行動
    private void Found()
    {
        // TODO
        // 発見後のバフ処理


        // 状態を逃走に変更
        currentState = ThiefState.Escape;
    }

    // 逃走状態の行動
    private void Escape()
    { 
    }

    // 部屋のオブジェクトを視認して記憶に保存する処理
    private void RecognizeObjects()
    {
        // 視界内オブジェクトを取得
        List<VisionTarget> visionTargets = this.GetComponent<VisionSensor>().Scan();

        // 視認したオブジェクトを記憶に保存
        foreach (VisionTarget target in visionTargets)
        {
            // 現在の部屋の記憶がない場合は新たに作成
            if (roomMemories[currentRoom] == null)
            {
                roomMemories[currentRoom] = new RoomMemory();
                roomMemories[currentRoom].FirstSetting();
            }

            // オブジェクトが部屋の記憶にない場合は新たに追加
            if (roomMemories[currentRoom].recognizedObjects == null) roomMemories[currentRoom].recognizedObjects = new Dictionary<VisionTarget, VisionTargetMemory>();

            // 現在の部屋の記憶にオブジェクトを追加
            if (!roomMemories[currentRoom].recognizedObjects.ContainsKey(target))
            {
                // 新しいオブジェクトを記憶に追加
                VisionTargetMemory newMemory = new VisionTargetMemory();
                newMemory.FirstSetup();

                roomMemories[currentRoom].recognizedObjects.Add(target, newMemory);
            }
            // 既に記憶しているオブジェクトの場合はスキップ
            else continue;
        }
    }

    // 探索対象を決める処理
    // TODO: 探索対象の優先順位を決めるロジックを追加する（例： 宝物 > プレイヤー > トラップ = 部屋のオブジェクト）
    private void DecideTarget()
    {
        // 探索対象との距離
        float distanceToTarget = Mathf.Infinity;

        // 視認したオブジェクトの中から、未探索で近いものを優先して探索対象に設定
        foreach (KeyValuePair<VisionTarget, VisionTargetMemory> entry in roomMemories[currentRoom].recognizedObjects)
        {
            // 既に探索済みのオブジェクトはスキップ
            if (entry.Value.isExplored) continue;

            // オブジェクトとの距離を計算
            float distance = Vector3.Distance(transform.position, entry.Key.transform.position);

            // より近いオブジェクトを探索対象に設定
            if (distance < distanceToTarget)
            {
                distanceToTarget = distance;
                currentTarget = entry.Key;
            }
            else continue;
        }
    }
}

