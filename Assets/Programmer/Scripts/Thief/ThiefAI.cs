/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のAIシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 2026-04-20 | 探索対象の決定ロジックを追加
 *            | 探索対象の優先順位を追加
 * 2026-04-22 | 耐久値を減少させる処理を追加
 *            | NavMeshAgentを利用して移動する処理を追加
 * 2026-04-23 | 泥棒のデータベースの項目変更に合わせて、Settingメソッドの内容を変更
 *            | 走り状態になる標的オブジェクトのタイプに応じて、移動速度を切り替える処理を追加
 * 2026-04-24 | 探索対象を強制的に変更する処理を追加
 *            | 探索対象の決定ロジックを一つにまとめる(複数個所に分散していたものを、DecideTargetメソッドにまとめる)
 * 2026-04-25 | 次に設定する移動ポイントを決定するロジックの不具合を修正
 * 2026-04-26 | 気絶後の退場処理を仮作成
 *            | 宝物を持って移動する処理を仮作成
 * 2026-04-27 | 部屋移動の閾値に達していたら次の部屋に移動する処理を追加
 * 2026-04-28 | 次の部屋に移動するための移動ポイントを決定するロジックを追加
 * 2026-05-01 | 帰宅ルートを構築するロジックを追加
 * 2026-05-07 | CS_RoomEnemyEntryPointDataを用いた初期部屋の設定の記載
 * 2026-05-08 | 初期部屋の入ってきたドアの位置を保存する処理の記載
 * 2026-05-15 | 同じ部屋の中で、他者が探索しているオブジェクトを探索対象にしないようにする処理を追加
 * 2026-05-17 | DecideTarget内のキャストエラーの不具合を修正
 * 2026-05-18 | A*アルゴリズムを用いて帰宅ルートを構築するロジックを追加
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 泥棒のAIシステム
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(VisionSensor))]
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
        [Tooltip("気絶状態")]
        Stunned
    }

    [SerializeField]
    [Tooltip("現在の行動状態")]
    private ThiefState currentState;

    [SerializeField, Header("泥棒のリアクションスプライト(仮)")]
    private List<Sprite> reactionSprites;
    enum ReactionSpriteType
    {
        Normal, // 通常
        Search, // 探索
        Stun    // 気絶
    }

    [Tooltip("泥棒のマテリアル")]
    private Material thiefMaterial;
    [Tooltip("泥棒のマテリアルのフェードアウトにかかる時間")]
    private float fadeAfterStunTime;

    [Tooltip("現在いる部屋の情報")]
    private RoomNode currentRoom;
    private GameObject currentRoomObject;
    [Tooltip("部屋に関する記憶")]
    private Dictionary<RoomNode, RoomMemory> roomMemories;

    /// <summary>
    /// デバッグ表示用：部屋の記憶(全ての部屋分)を取得する
    /// </summary>
    public IReadOnlyDictionary<RoomNode, RoomMemory> RoomMemories => roomMemories;

    [Tooltip("視認オブジェクトの記憶")]
    private Dictionary<VisionTarget, VisionTargetMemory> visionTargetMemories;

    [Tooltip("探索対象")]
    private ThiefTarget currentTarget;
    public ThiefTarget CurrentTarget => currentTarget;

    [Tooltip("次の部屋に移動するための移動ポイント")]
    private Transform nextRoomMovePoint;
    [Tooltip("次の部屋に移動するための移動ポイントを決定したかどうかを判定するフラグ")]
    private bool isNextRoomMovePointDecided;

    [Tooltip("持っている宝物オジェクト")]// 見つけたら設定する
    private GameObject heldTreasure;

    [SerializeField, Tooltip("泥棒の耐久力")]
    private int durability;
    [SerializeField, Tooltip("泥棒の移動速度")]
    private float walkSpeed;
    private float runSpeed;

    [Tooltip("泥棒が探索するのにかかる秒数")]
    private int searchTime;

    [Tooltip("攻撃を受けた後の気絶時間")]
    private float damageStunTime;

    [Tooltip("攻撃を受けた後の無敵時間")]
    private float invincibleTime;
    [Tooltip("無敵時間の現在残り時間")]
    private float remainingInvincibleTime;

    [Tooltip("気絶後に退場するまでの時間")]
    private int exitAfterStunTime;
    [Tooltip("気絶後の経過時間")]
    private float elapsedTimeAfterStun;

    [Tooltip("ドロップするソウルの数")]
    private int soulDropCount;

    [Tooltip("走り状態になる標的オブジェクトのタイプリスト")]
    private List<VisionTarget.TargetType> runTargetTypes;

    [Tooltip("次の部屋探索に切り替える探索度の閾値")]
    private int nextRoomSearchThreshold;

    [Tooltip("ナビメッシュエージェント")]
    private NavMeshAgent navMeshAgent;
    [Tooltip("泥棒のリアクションを管理するコンポーネント")]
    private ThiefReaction thiefReaction;

    [Tooltip("最初の部屋のオブジェクト")]
    private RoomNode firstRoom;
    private Transform firstEntryPoint; // 最初に入ってきたドアの位置(逃走ルートの最終目的位置)

    [Tooltip("移動ルート")]
    private List<Transform> moveRoute;

    [Header("DangerZone Avoidance")]
    [SerializeField, Tooltip("DangerZone を考慮して移動するためのコンポーネント。未設定なら同一GameObjectから取得")]
    private SmartNavAgent smartNavAgent;

    [SerializeField, Tooltip("この泥棒が回避する DangerZone の zoneID 一覧")]
    private List<int> avoidZoneIDs = new List<int>();


    // 泥棒の耐久力と移動速度を設定するメソッド
    public void Setting(CSS_ThiefStatusData typedata, CSS_ThiefCommonStatusData data, float playerSpeed, RoomNode entryRoom, Transform entryPoint)
    {
        /*未実装、未設定　*///data.jumpHeight;
        /*未設定、未設定　*///data.alertTime;

        durability = typedata.durability;
        walkSpeed = playerSpeed * typedata.walkSpeedMultiplier;
        runSpeed = playerSpeed * typedata.runSpeedMultiplier;
        nextRoomSearchThreshold = typedata.nextRoomSearchThreshold;
        runTargetTypes = typedata.runTargetTypes;
        soulDropCount = typedata.soulDropCount;
        searchTime = typedata.searchTime;

        exitAfterStunTime = data.exitAfterStunTime;
        damageStunTime = data.stunTime;
        invincibleTime = data.invincibleTime;
        remainingInvincibleTime = 0.0f;

        // 初期状態を探索に設定
        currentState = ThiefState.Explore;

        // 初期部屋を設定（仮）
        currentRoom = entryRoom;

        // 部屋の記憶を初期化
        roomMemories = new Dictionary<RoomNode, RoomMemory>();

        // 視認オブジェクトの記憶を初期化
        visionTargetMemories = new Dictionary<VisionTarget, VisionTargetMemory>();

        // 初期部屋の記憶を作成
        roomMemories[currentRoom] = new RoomMemory();
        roomMemories[currentRoom].FirstSetting();
        roomMemories[currentRoom].explorationLevel = currentRoom.initialExplorationLevel;

        // ナビメッシュエージェントの速度を設定
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.baseOffset =1.0f; // キャラクターの高さに合わせてオフセットを設定
        navMeshAgent.speed = this.walkSpeed;

        // SmartNavAgent を初期化（存在すれば DangerZone 回避を有効化）
        if (smartNavAgent == null) smartNavAgent = GetComponent<SmartNavAgent>();
        if (smartNavAgent != null)
        {
            smartNavAgent.SetAvoidZoneIDs(avoidZoneIDs);
        }

        // リジットボディの設定
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // ナビメッシュエージェントで移動させるため、リジットボディをキネマティックに設定
        rb.useGravity = false; // 重力の影響を受けないようにする

        // 泥棒のリアクションを管理するコンポーネントを取得
        thiefReaction = GetComponent<ThiefReaction>();
        thiefReaction.RegisterReaction(data.reactionUISprites);

        reactionSprites = data.reactionSprites;

        // 最初の部屋と入ってきたドアの位置を保存
        firstRoom = entryRoom;
        firstEntryPoint = entryPoint;
    }

    private void Start()
    {
        fadeAfterStunTime = GameObject.FindObjectOfType<ThiefManager>().GetThiefCommonDB().fadeAfterStunTime;

        thiefMaterial = GetComponent<Renderer>().material;
        if (thiefMaterial == null)
        {
            Debug.LogError("ThiefAI: 泥棒のマテリアルが見つかりません。");
        }
        thiefMaterial.SetFloat("_DisappearTime", fadeAfterStunTime);
        thiefMaterial.SetFloat("_Timer", fadeAfterStunTime);

        FindNowRoomNode();
    }

    private void Update()
    {
        // 無敵時間の経過を管理
        // 気絶状態のときは無敵時間の経過を管理しない（気絶状態のときは攻撃を受けない想定のため）
        if (remainingInvincibleTime > 0)
        {
            if (currentState != ThiefState.Stunned)
            {
                remainingInvincibleTime -= Time.deltaTime;
                if (remainingInvincibleTime <= 0)
                {
                    remainingInvincibleTime = 0;
                }
            }
        }

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
            case ThiefState.Stunned:
                Stunned();
                break;
        }
    }

    // 探索状態の行動
    private void Explore()
    {
        // 探索対象を決定
        RecognizeObjects();

        ChangeFace(ReactionSpriteType.Normal);

        // 現在の探索対象がプレイヤーである場合は、距離判定をして、一定距離以内であればプレイヤーに向かって移動する処理を追加する
        if (currentTarget != null && currentTarget is PlayerTarget)
        {
            // 距離判定
            VisionSensor visionSensor = GetComponent<VisionSensor>();
            int distanceToPlayer = (int)Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToPlayer <= visionSensor.viewDistance)
            {
                MoveTo(currentTarget.transform.position);
            }
            else
            {
                currentTarget = null;
            }
            return;
        }

        // moveRouteが設定されている場合は、moveRouteに沿って移動する処理を追加する
        if (moveRoute != null && moveRoute.Count > 0)
        {
            // プレイヤーや宝物、標的にする罠などが視認できている場合は、moveRouteをクリアして、そちらに向かう処理を追加する
            if (currentTarget is PlayerTarget || currentTarget is TrapTarget || currentTarget is VisionTarget)
            {
                if (currentTarget is VisionTarget)
                {
                    if (((VisionTarget)currentTarget).targetType == VisionTarget.TargetType.Treasure)
                    {
                        moveRoute.Clear();
                        return;
                    }
                }
                else
                {
                    moveRoute.Clear();
                    return;
                }
            }

            Transform nextPoint = moveRoute[0];
            if (nextPoint == null)
            {
                // 参照切れ対策：無効な要素を捨てて次へ
                moveRoute.RemoveAt(0);
                return;
            }
            navMeshAgent.SetDestination(nextPoint.position);
            // 次のポイントに十分近づいたら、次のポイントへ
            if (Vector3.Distance(transform.position, nextPoint.position) < 1.0f)
            {
                moveRoute.RemoveAt(0);
            }
            return;
        }

        // 探索対象がない場合や、探索度が次の部屋に移動するための閾値を超えている場合は、次の部屋に移動するための処理を追加する
        if (isNextRoomMovePointDecided || roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
        {
            if (nextRoomMovePoint == null)
            {
                // 次の部屋に移動するための移動ポイントが決まっていない場合は、次の部屋に移動するための移動ポイントを決定するロジックを実行する
                NextDoorElection();
                // 再取得してもnullの場合
                if (nextRoomMovePoint == null) return;

                navMeshAgent.SetDestination(nextRoomMovePoint.position);
            }
            return;
        }

        // 探索対象がない場合は、部屋の移動ポイントに沿って移動する処理を追加する
        if (currentTarget == null)
        {
            DecideTarget();
            return;
        }
        // 現在の探索対象が視認オブジェクト(VisionTarget)かどうかを判定
        if (currentTarget is VisionTarget)
        {
            // 探索対象が既に探索済みの場合
            if (visionTargetMemories[((VisionTarget)currentTarget)].isExplored)
            {
                DecideTarget();
            }
            // 探索対象が未探索の場合
            else
            {
                // 現在の探索対象が他者にも探索されているオブジェクトである場合は、探索対象をリセットして、次の探索対象を決定する
                if (visionTargetMemories[((VisionTarget)currentTarget)].searchThief != null && visionTargetMemories[((VisionTarget)currentTarget)].searchThief != this.gameObject)
                {
                    //Debug.Log("【" + this.gameObject.name + "】Explore: 探索対象 " + currentTarget.name + " は" + ((VisionTarget)currentTarget).searchThief.gameObject.name + "が探索しているため、探索対象をリセットします。");


                    currentTarget = null;
                    return;
                }

                // 探索対象に十分近づいたら、探索度を進める
                if (Vector3.Distance(transform.position, currentTarget.transform.position) < ((VisionTarget)currentTarget).exploredDistanceThreshold)
                {
                    ChangeFace(ReactionSpriteType.Search); // 探索完了の表情に変更する処理を追加する

                    ((VisionTarget)currentTarget).searchThief = this.gameObject; // 探索対象に対して、探索している人を設定する

                    if (ProgressTargetSearchTime())
                    {
                        // 宝物を探索にしていて、完了した場合は、発見状態に切り替える
                        if (((VisionTarget)currentTarget).targetType == VisionTarget.TargetType.Treasure)
                        {
                            // 発見状態に切り替える
                            currentState = ThiefState.Found;
                        }
                        // それ以外のオブジェクトを探索して完了した場合は、次の探索対象を決定する
                        else
                        {
                            // 探索対象を探索済みに設定
                            visionTargetMemories[((VisionTarget)currentTarget)].isExplored = true;
                            // 探索度を加算
                            roomMemories[currentRoom].explorationLevel += ((VisionTarget)currentTarget).explorationValue;

                            // 探索度が閾値を超えた場合は、次の部屋に移動するための処理を追加する
                            if (roomMemories[currentRoom].explorationLevel >= nextRoomSearchThreshold)
                            {
                                // 探索対象をリセット
                                ((VisionTarget)currentTarget).searchThief = null; // 探索対象の探索している人をリセットする
                                currentTarget = null;

                                isNextRoomMovePointDecided = true;
                            }
                            // それ以外の場合は、次の探索対象を決定する
                            else
                            {
                                ((VisionTarget)currentTarget).searchThief = null; // 探索対象の探索している人をリセットする
                                currentTarget = null;
                                DecideTarget();
                            }
                        }
                    }
                }
                else
                {
                    visionTargetMemories[((VisionTarget)currentTarget)].explorationProgress = 0; // 探索対象から離れた場合は、探索の進行度をリセットする
                }
            }
        }
        else
        {
            // 探索対象に向かって移動
            MoveTo(currentTarget.transform.position);

            // 探索対象に十分近づいたら、次の探索対象を決定
            if (Vector3.Distance(transform.position, currentTarget.transform.position) < 2.0f)
            {
                DecideTarget();
            }

            // 未探索のオブジェクトを視認した場合はそっちの探索に切り替える
            if (HasUnexploredTargets())
            {
                DecideTarget();
            }
        }
    }

    // 発見状態の行動
    // ----------------
    // TODO: 宝物発見バフの適応処理を追加する
    private void Found()
    {
        // 宝物を持つ
        heldTreasure = currentTarget.gameObject;
        currentTarget.gameObject.transform.parent = this.transform; // 泥棒の子オブジェクトにする
        currentTarget.GetComponent<Collider>().enabled = false; // 宝物のコライダーを無効にする
        currentTarget.gameObject.transform.localPosition = new Vector3(0.0f, this.transform.position.y, 0.0f); // 宝物の位置を泥棒の位置に合わせる

        // 状態を逃走に変更
        currentState = ThiefState.Escape;

        // 取得した宝物を他の泥棒の記憶から消去する
        GameObject.FindObjectOfType<ThiefManager>().EraseTheMemoryToAllThief(currentTarget);
        // 探索対象をリセット
        currentTarget = null;
    }

    // 逃走状態の行動
    private void Escape()
    {
        // ルートが未構築なら構築する
        if (moveRoute == null || moveRoute.Count == 0)
        {
            ConstructionRoute(firstEntryPoint); // ここで moveRoute が埋まる想定
        }

        // それでも無いなら、何らかの理由でルートが作れなかった
        if (moveRoute == null || moveRoute.Count == 0)
        {
            Debug.LogWarning("【泥棒】Escape: moveRoute が空のため移動できません。");
            return;
        }

        // 次に向かうドア
        Transform door = moveRoute[0];
        if (door == null)
        {
            // 参照切れ対策：無効な要素を捨てて次へ
            moveRoute.RemoveAt(0);
            isNextRoomMovePointDecided = false; // 次の部屋に移動するためのポイントを決定していない状態に戻す
            return;
        }

        // ドアに十分近づいたら、次のドアへ
        if (Vector3.Distance(transform.position, door.position) < 1.0f)
        {
            moveRoute.RemoveAt(0);
            isNextRoomMovePointDecided = false; // 次の部屋に移動するためのポイントを決定していない状態に戻す

            // ルート上のドアを全て通過した場合は、脱出完了で削除する
            if (moveRoute.Count == 0)
            {
                Debug.Log("【泥棒】Escape: 脱出ルート上のドアを全て通過し、脱出された");
                Destroy(this.gameObject);
                return;
            }

            return;
        }

        // ドアへ移動
        if (!isNextRoomMovePointDecided)
        {
            navMeshAgent.isStopped = false;
            MoveTo(door.position);
        }
    }

    // 気絶状態の行動
    // ----------------
    // TODO: その場で動けなくなる処理を追加する
    private void Stunned()
    {
        // ナビメッシュエージェントを停止させる
        navMeshAgent.isStopped = true;

        // 経過時間を加算
        elapsedTimeAfterStun += Time.deltaTime;

        // 耐久値が残っている場合は、気絶時間が経過したら無敵時間を付与して、状態を探索に戻す
        if (durability > 0)
        {
            // 経過時間が気絶時間を超えた場合は、耐久力を減少させて、状態を探索に戻す
            if (elapsedTimeAfterStun >= damageStunTime)
            {
                currentState = ThiefState.Explore; // 状態を探索に戻す
                navMeshAgent.isStopped = false; // ナビメッシュエージェントを再開させる
            }
        }
        // 耐久力が0以下の場合は、時間経過後に退場する
        else
        {
            // 経過時間が退場するまでの時間を超えた場合は、退場する処理を追加する
            if (elapsedTimeAfterStun >= exitAfterStunTime)
            {
                thiefMaterial.SetFloat("_Timer", fadeAfterStunTime - (elapsedTimeAfterStun - exitAfterStunTime));

                Transform faceTransform = this.transform.GetChild(0);
                Vector3 facePos = faceTransform.position;
                faceTransform.position = new Vector3(facePos.x, facePos.y + 0.01f * (elapsedTimeAfterStun - exitAfterStunTime), facePos.z);

                if (thiefMaterial.GetFloat("_Timer") <= 0.0f)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 部屋のオブジェクトを視認して記憶に保存する処理
    /// </summary>
    private void RecognizeObjects()
    {
        // 視界内オブジェクトを取得
        List<ThiefTarget> visionTargets = this.GetComponent<VisionSensor>().Scan();

        // 現在の部屋の記憶がない場合は新たに作成
        if (roomMemories[currentRoom] == null)
        {
            roomMemories[currentRoom] = new RoomMemory();
            roomMemories[currentRoom].FirstSetting();
        }

        bool isNewObjectRecognized = false; // 新たに視認したオブジェクトがあるかどうかを判定するフラグ
        // 視認したオブジェクトを記憶に保存
        foreach (ThiefTarget target in visionTargets)
        {

            // 現在の部屋の記憶に認識しているオブジェクトのリストがない場合は新たに作成
            if (roomMemories[currentRoom].recognizedObjects == null) roomMemories[currentRoom].recognizedObjects = new List<ThiefTarget>();

            bool isAlreadyRecognized = false; // 既に記憶しているオブジェクトかどうかを判定するフラグ
            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に記憶しているオブジェクトの場合はスキップ
                if (entry == target) isAlreadyRecognized = true;
            }
            if (isAlreadyRecognized)
            {
                if (target is VisionTarget)
                {
                    // 既に記憶しているオブジェクトが視認オブジェクト(VisionTarget)の場合は、探索している人がいるかどうかの情報を更新する
                    if (visionTargetMemories.ContainsKey((VisionTarget)target))
                    {
                        visionTargetMemories[((VisionTarget)target)].searchThief = ((VisionTarget)target).searchThief;
                    }
                }

                continue;
            }


            if (target is PlayerTarget)
            {
                // 現在の探索対象が宝物である場合は、プレイヤーを探索対象に設定しない
                if (currentTarget is VisionTarget && ((VisionTarget)currentTarget).targetType == VisionTarget.TargetType.Treasure)
                {
                    continue;
                }
                // 現在の探索対象が宝物でない場合は、プレイヤーを探索対象に設定する
                else
                {
                    currentTarget = target;
                }

                // 次の部屋に移動するためのポイントが設定されている場合は、削除する
                if (nextRoomMovePoint != null)
                {
                    nextRoomMovePoint = null;
                    isNextRoomMovePointDecided = false;
                }

                continue;
            }

            // 新しいオブジェクトを記憶に追加
            roomMemories[currentRoom].recognizedObjects.Add(target);
            // 記憶領域の作成
            if (target is VisionTarget) visionTargetMemories[((VisionTarget)target)] = new VisionTargetMemory();

            isNewObjectRecognized = true; // 新たに視認したオブジェクトがある場合はフラグを立てる
        }

        // 新たに視認したオブジェクトを記憶に保存した後、探索対象を決定する処理を追加する
        if (isNewObjectRecognized) DecideTarget();
    }

    /// <summary>
    /// 視認しているオブジェクトの中に未探索のものがあるかどうかを判定する処理
    /// </summary>
    /// <returns>
    /// true:未探索のオブジェクトがある | false:認識している全てのオブジェクトが探索済み
    /// </returns>
    private bool HasUnexploredTargets()
    {
        // 現在の部屋の記憶がない場合や、認識しているオブジェクトがない場合は、未探索のオブジェクトがないと判定してfalseを返す
        if (roomMemories[currentRoom] == null || roomMemories[currentRoom].recognizedObjects == null) return false;

        // 視認しているオブジェクトの中に未探索のものがあるかどうかを判定
        foreach (var entry in roomMemories[currentRoom].recognizedObjects)
        {
            // 未探索のオブジェクトで、かつ他者にも探索されているオブジェクトがある場合は、未探索のオブジェクトがあると判定してtrueを返す
            if (!visionTargetMemories[((VisionTarget)entry)].isExplored && (visionTargetMemories[((VisionTarget)entry)].searchThief == null)) return true;
        }

        // 全てのオブジェクトが探索済みの場合はfalseを返す
        return false;
    }

    /// <summary>
    /// 探索対象を決める処理
    /// </summary>
    private void DecideTarget()
    {
        // 探索対象との距離
        float distanceToTarget = Mathf.Infinity;

        // 未探索のオブジェクトがある場合は、未探索のオブジェクトを優先して探索対象に設定
        if (HasUnexploredTargets())
        {
            if (currentTarget is VisionTarget)
            {
                if (currentTarget != null && (VisionTarget)currentTarget)
                {
                    ((VisionTarget)currentTarget).searchThief = null; // 現在の探索対象の探索している人をリセットする
                }
            }

            foreach (var entry in roomMemories[currentRoom].recognizedObjects)
            {
                // 既に探索済みのオブジェクトはスキップ
                if (visionTargetMemories[((VisionTarget)entry)].isExplored) continue;

                // 他者にも探索されているオブジェクトはスキップ
                if (visionTargetMemories[((VisionTarget)entry)].searchThief != null) continue;

                // 既に探索対象に設定しているオブジェクトはスキップ
                if (entry == currentTarget) continue;

                // 現在の探索対象が視認オブジェクト(VisionTarget)かどうか
                if (entry is VisionTarget)
                {
                    // 探索対象の優先順位を決めるロジック
                    switch (((VisionTarget)entry).targetType)
                    {
                        case VisionTarget.TargetType.Treasure:
                            {
                                if (currentTarget is VisionTarget)
                                {
                                    // 現在の探索対象が宝物でない場合は、問答無用で宝物を探索対象に設定
                                    if (((VisionTarget)currentTarget).targetType != VisionTarget.TargetType.Treasure)
                                    {
                                        currentTarget = entry;
                                        break;
                                    }
                                    // 現在の探索対象も宝物の場合は、距離が近い方を探索対象に設定する
                                    else
                                    {
                                        // オブジェクトとの距離を計算
                                        float distance = Vector3.Distance(transform.position, entry.transform.position);

                                        // より近いオブジェクトを探索対象に設定
                                        if (distance < distanceToTarget)
                                        {
                                            distanceToTarget = distance;
                                            currentTarget = entry;
                                        }
                                        else continue;
                                    }
                                }
                                else if (currentTarget is TrapTarget)
                                {

                                    // 空の宝箱型の罠の場合ではない場合は、スキップ
                                    if (entry is TrapTarget tt && tt.gimmickScript.gimmick != Gimmick.EmptyChest) continue;

                                    // 宝物罠の場合は、距離判定で探索対象を切り替える
                                    // オブジェクトとの距離を計算
                                    float distance = Vector3.Distance(transform.position, entry.transform.position);

                                    // より近いオブジェクトを探索対象に設定
                                    if (distance < distanceToTarget)
                                    {
                                        distanceToTarget = distance;
                                        currentTarget = entry;
                                    }
                                    else continue;
                                }
                                else
                                {
                                    // プレイヤーを探索対象にしている場合は、問答無用で宝物を探索対象に設定
                                    currentTarget = entry;
                                }
                            }
                            break;
                        case VisionTarget.TargetType.RoomObject:
                            {
                                // 現在の探索対象が宝物の場合は、スキップ
                                if (currentTarget is VisionTarget vt && vt.targetType == VisionTarget.TargetType.Treasure) continue;
                                // 現在の探索対象が空の宝箱型の罠の場合は、スキップ
                                if (currentTarget is TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest) continue;

                                // オブジェクトとの距離を計算
                                float distance = Vector3.Distance(transform.position, entry.transform.position);

                                // より近いオブジェクトを探索対象に設定
                                if (distance < distanceToTarget)
                                {
                                    distanceToTarget = distance;
                                    currentTarget = entry;
                                }
                                else continue;
                            }
                            break;
                    }
                }
                else if (entry is TrapTarget)
                {
                    // 宝物を探索対象にしている場合は、スキップ
                    if (currentTarget is VisionTarget vt && vt.targetType == VisionTarget.TargetType.Treasure) continue;
                    // 宝物の罠を探索対象にしている場合は、スキップ
                    if (currentTarget is TrapTarget tt && tt.gimmickScript.gimmick == Gimmick.EmptyChest) continue;

                    // オブジェクトとの距離を計算
                    float distance = Vector3.Distance(transform.position, entry.transform.position);
                    // より近いオブジェクトを探索対象に設定
                    if (distance < distanceToTarget)
                    {
                        distanceToTarget = distance;
                        currentTarget = entry;
                    }
                    else continue;
                }
            }

        }
        // 未探索のオブジェクトがない場合は、部屋の移動ルートに沿って移動する処理を追加する
        else
        {
            // 前回の探索対象がThiefTargetの派生クラスかどうか(前回が移動ポイントでない場合)
            if (currentTarget == null || currentTarget is VisionTarget || currentTarget is TrapTarget || currentTarget is PlayerTarget)
            {
                // 前回の探索対象が視認オブジェクト(VisionTarget)の場合は、探索対象をリセットする
                if (currentTarget != null)
                {
                    if ((VisionTarget)currentTarget)
                    {
                        ((VisionTarget)currentTarget).searchThief = null;
                        currentTarget = null;
                    }
                }
                // 視認オブジェクトから移動ポイントにする場合は一番近いものを探索対象に設定
                foreach (ThiefTarget target in currentRoom.movePoints)
                {
                    if (target == null) continue;

                    // オブジェクトとの距離を計算
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    // より近いオブジェクトを探索対象に設定
                    if (distance < distanceToTarget)
                    {
                        distanceToTarget = distance;
                        currentTarget = target;
                    }
                    else continue;
                }
            }
            // 移動ポイントから移動ポイントにする場合は、右回りの場合リストを加算、左回りの場合リストを減算して設定
            else
            {
                // 現在の移動ポイントがリストのどこにあるかを判定
                for (int i = 0 ; i < currentRoom.movePoints.Count ; i++)
                {
                    // 現在の移動ポイントがリストのどこにあるかを判定
                    if (currentRoom.movePoints[i] == currentTarget)
                    {
                        int nextIndex = 0;

                        // 右回りの場合
                        if (currentRoom.isListDown)
                        {
                            // 次のインデックスを計算
                            nextIndex = i + 1;

                            // インデックスがリストの範囲を超える場合は、リストの先頭に戻す
                            if (nextIndex >= currentRoom.movePoints.Count) nextIndex = 0;

                            // リストを加算して次の移動ポイントを探索対象に設定
                            currentTarget = currentRoom.movePoints[nextIndex];
                            break;
                        }
                        // 左回りの場合
                        else
                        {
                            // 次のインデックスを計算
                            nextIndex = i - 1;

                            // インデックスがリストの範囲を超える場合は、リストの末尾に戻す
                            if (nextIndex < 0) nextIndex = currentRoom.movePoints.Count - 1;

                            // リストを減算して次の移動ポイントを探索対象に設定
                            currentTarget = currentRoom.movePoints[nextIndex];
                            break;
                        }
                    }
                }
            }
        }

        if (currentTarget is VisionTarget)
        {
            // 探索対象が走り状態になる標的オブジェクトのタイプリストに含まれている場合は、走り状態に切り替える
            if (runTargetTypes.Contains(((VisionTarget)currentTarget).targetType))
            {
                navMeshAgent.speed = runSpeed;
            }
            else
            {
                navMeshAgent.speed = walkSpeed;
            }
        }
        else
        {
            navMeshAgent.speed = walkSpeed;
        }

        // 探索対象に向かって移動
        MoveTo(currentTarget.transform.position);
    }

    /// <summary>
    /// 耐久値を減らす処理
    /// </summary>
    /// <param name="damage">与える減少値</param>
    public void TakeDamage(int damage, Gimmick type)
    {
        if (remainingInvincibleTime > 0) return;

        durability -= damage;

        switch (type)
        {
            case Gimmick.Pot:
                thiefReaction.SetReactionUI(ThiefReaction.ThiefReactionType.Pot);
                break;
            case Gimmick.IronBall:
                thiefReaction.SetReactionUI(ThiefReaction.ThiefReactionType.IronBall);
                break;
            case Gimmick.EmptyChest:
            case Gimmick.None:
            default:
                break;
        }

        currentState = ThiefState.Stunned; // 状態を気絶に変更
        elapsedTimeAfterStun = 0.0f; // 気絶時間の経過時間をリセット

        remainingInvincibleTime = invincibleTime; // 無敵時間を付与

        // 泥棒の表情を気絶の表情に変更する処理を追加する
        ChangeFace(ReactionSpriteType.Stun);


        // 耐久力が0以下になった場合は、耐久力を0に補正して気絶状態にする
        if (durability <= 0)
        {
            durability = 0;

            // プレイヤーにソウルを入手させる
            CS_PlayerAction playerAction = GameObject.FindObjectOfType<CS_PlayerAction>();

            // playerActionが見つかった場合は、ソウルを加算する処理を実行する。見つからない場合は、エラーログを出力する。
            if (playerAction != null) playerAction.AddSoul(soulDropCount);
            else Debug.LogError("PlayerActionが見つかりませんでした。ThiefAIのTakeDamageメソッドで、プレイヤーにソウルを入手させる処理が正常に動作しない可能性があります。");
        }
    }

    /// <summary>
    /// 現在いる部屋に関するオブジェクトをRaycastで取得して、currentRoomに設定する処理
    /// </summary>
    private void FindNowRoomNode()
    {
        GameObject currentobject = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(this.gameObject);
        if (currentobject == null)
        {
            Debug.LogWarning("【泥棒】現在いる部屋に関するオブジェクトの取得に失敗しました");
            return;
        }

        currentRoom = currentobject.transform.GetComponentInChildren<RoomNode>();
        currentRoomObject = currentobject;

        // 現在いる部屋の記憶がない場合は新たに作成
        if (!roomMemories.ContainsKey(currentRoom))
        {
            roomMemories[currentRoom] = new RoomMemory();
            roomMemories[currentRoom].FirstSetting();
            roomMemories[currentRoom].explorationLevel = currentRoom.initialExplorationLevel;
        }
    }

    /// <summary>
    /// 現在いる部屋の接続している方向を取得して、次に探索する部屋に行くための移動ポイントを決定する処理
    /// </summary>
    private void NextDoorElection()
    {
        if (currentRoomObject == null)
        {
            FindNowRoomNode();
            Debug.LogError("【泥棒】現在いる部屋のオブジェクトが見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
            return;
        }

        CS_RoomCreatePoint roomCreatePoint = currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>();
        if (roomCreatePoint == null)
        {
            Debug.LogError("【泥棒】現在いる部屋のRoomCreatePointが見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
            return;
        }

        // 現在いる部屋の接続している方向を取得
        List<CSE_RoomDoorDirection> connectDirs = roomCreatePoint.GetConnectDirections();
        if (connectDirs.Count == 0)
        {
            Debug.LogWarning("【泥棒】現在いる部屋の接続方向が見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
            return;
        }

        // 入ってきたドアをリストから除外
        // もし行ったことのない部屋がある場合は行ったことのある方向をリストから除外
        bool hasUnvisitedNextRooms = HasUnvisitedNextRooms(); // 次の部屋候補の中に行ったことのない部屋があるかどうかを判定するフラグ

        // 次の部屋候補の中に行ったことのない部屋がある場合
        if (hasUnvisitedNextRooms)
        {
            for (int i = 0 ; i < connectDirs.Count ; i++)
            {
                // 入ってきたドアの方向と同じ方向がある場合は、リストから除外
                if (connectDirs[i] == roomMemories[currentRoom].enteredDoorDirection)
                {
                    connectDirs.RemoveAt(i);
                    i--;
                    continue;
                }
                // 行ったことのある方向をリストから除外
                CS_RoomMoveConnection nextRoom;
                roomCreatePoint.TryGetConnection(connectDirs[i], out nextRoom);
                if (roomMemories.ContainsKey(nextRoom.TargetCreatePoint.GetComponentInChildren<RoomNode>()))
                {
                    connectDirs.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        // 次の部屋候補の中に行ったことのない部屋がない場合は、今までに行ったことのあるすべての部屋で選ばなかった方向をリストに追加
        else
        {
            connectDirs.Clear();
            // 今までに行ったことのあるすべての部屋で選ばなかった方向をリストに追加
            foreach (var room in roomMemories)
            {
                foreach (var dir in room.Value.unchosenDoors)
                {
                    if (!connectDirs.Contains(dir)) connectDirs.Add(dir);
                }
            }
        }

        // 宝部屋判定
        bool hasTreasureRoom = false;
        foreach (var dir in connectDirs)
        {
            CS_RoomMoveConnection nextRoom;
            roomCreatePoint.TryGetConnection(dir, out nextRoom);

            if (nextRoom.TargetCreatePoint.GetComponentInChildren<RoomNode>().transform.tag == "TreasureRoom")
            {
                hasTreasureRoom = true;
                break;
            }
        }
        // 宝部屋がある場合は、宝部屋以外の方向をリストから除外
        if (hasTreasureRoom)
        {
            for (int i = 0 ; i < connectDirs.Count ; i++)
            {
                CS_RoomMoveConnection nextRoom;
                roomCreatePoint.TryGetConnection(connectDirs[i], out nextRoom);

                if (nextRoom.TargetCreatePoint.GetComponentInChildren<RoomNode>().transform.tag != "TreasureRoom")
                {
                    connectDirs.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }

        // 接続している部屋の方向をランダムに選択
        int randomIndex = Random.Range(0, connectDirs.Count);

        if (hasUnvisitedNextRooms)
        {
            // 選択しなかった方向のドアを記憶
            for (int i = 0 ; i < connectDirs.Count ; i++)
            {
                if (i == randomIndex) continue;

                // 重複確認
                foreach (var unchosenDoor in roomMemories[currentRoom].unchosenDoors)
                {
                    if (connectDirs[i] == unchosenDoor)
                    {
                        // すでに記憶している選択しなかった方向のドアの場合は、重複して記憶しないようにスキップする
                        continue;
                    }
                }

                roomMemories[currentRoom].unchosenDoors.Add(connectDirs[i]);
            }

            // 選択した方向にあるドアの位置を次の移動ポイントに設定
            nextRoomMovePoint = currentRoom.GetDirectionWallToDoor(connectDirs[randomIndex]);
        }
        else
        {
            // 選択した方向のドアを記憶から削除
            bool isRemoved = false; // 選択した方向のドアを記憶から削除したかどうかを判定するフラグ
            RoomNode targetRoomNode = null;
            foreach (var room in roomMemories)
            {
                foreach (var dir in room.Value.unchosenDoors)
                {
                    if (dir == connectDirs[randomIndex])
                    {
                        // どの部屋のドアかを記憶
                        targetRoomNode = room.Key;
                        // 記憶から選択した方向のドアを削除
                        room.Value.unchosenDoors.Remove(dir);
                        // 選択した方向のドアを記憶から削除したフラグを立てる
                        isRemoved = true;
                        break;
                    }
                }
                if (isRemoved) break;
            }

            // 選択したドアの位置を取得
            Transform targetDoorPos = targetRoomNode.GetDirectionWallToDoor(connectDirs[randomIndex]);

            if (targetDoorPos == null)
            {
                Debug.LogError("【泥棒】選択したドアの位置が見つかりませんでした。ThiefAIのNextDoorElectionメソッドで、次に設定する移動ポイントを決定するロジックが正常に動作しない可能性があります。");
                return;
            }

            // ドアの位置を最終目的位置としてルートを構築
            ConstructionRoute(targetDoorPos);
        }
    }

    /// <summary>
    /// 次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理
    /// </summary>
    /// <returns>
    /// true:次の部屋候補の中に行ったことのない部屋がある | false:次の部屋候補の中に行ったことのない部屋がない
    /// </returns>
    private bool HasUnvisitedNextRooms()
    {
        if (currentRoomObject == null)
        {
            FindNowRoomNode();
            Debug.LogError("【泥棒】現在いる部屋のオブジェクトが見つかりませんでした。ThiefAIのHasUnvisitedNextRoomsメソッドで、次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理が正常に動作しない可能性があります。");
            return false;
        }
        CS_RoomCreatePoint roomCreatePoint = currentRoomObject.transform.GetComponent<CS_RoomCreatePoint>();
        if (roomCreatePoint == null)
        {
            Debug.LogError("【泥棒】現在いる部屋のRoomCreatePointが見つかりませんでした。ThiefAIのHasUnvisitedNextRoomsメソッドで、次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理が正常に動作しない可能性があります。");
            return false;
        }
        // 現在いる部屋の接続している方向を取得
        List<CSE_RoomDoorDirection> connectDirs = roomCreatePoint.GetConnectDirections();
        if (connectDirs.Count == 0)
        {
            Debug.LogWarning("【泥棒】現在いる部屋の接続方向が見つかりませんでした。ThiefAIのHasUnvisitedNextRoomsメソッドで、次の部屋候補の中に行ったことのない部屋があるかどうかを判定する処理が正常に動作しない可能性があります。");
            return false;
        }
        // 接続している部屋の中に行ったことのない部屋があるかどうかを判定
        foreach (var dir in connectDirs)
        {
            CS_RoomMoveConnection nextRoom;
            roomCreatePoint.TryGetConnection(dir, out nextRoom);
            if (!roomMemories.ContainsKey(nextRoom.TargetCreatePoint.GetComponentInChildren<RoomNode>())) return true;
        }
        return false;
    }

    /// <summary>
    /// 探索対象を強制的に変更する処理
    /// (対象：プレイヤーが攻撃してきたときや、ミミックの罠にかかったときなど)
    /// </summary>
    /// <param name="target">新しく設定する探索対象</param>
    public void SetTarget(ThiefTarget target)
    {
        currentTarget = target;
    }

    /// <summary>
    /// 指定のオブジェクトに関する記憶を消去する処理
    /// </summary>
    /// <param name="obj">指定オブジェクト</param>
    public void EraseTheMemory(ThiefTarget obj)
    {
        foreach (var room in roomMemories)
        {
            // 指定のオブジェクトに関する記憶がない場合はスキップ
            if (room.Value.recognizedObjects == null) continue;

            foreach (var entry in room.Value.recognizedObjects)
            {
                // 指定のオブジェクトに関する記憶がある場合は、記憶から削除する
                if (entry == obj)
                {
                    room.Value.recognizedObjects.Remove(entry);
                    break;
                }
            }
        }

        if (currentTarget == obj)
        {
            currentTarget = null;
        }
    }

    /// <summary>
    /// 探索対象の探索にかかる時間を経過させる処理
    /// </summary>
    /// <returns>探索が終了しているかどうか</returns>
    private bool ProgressTargetSearchTime()
    {
        // 探索対象がない場合は、falseを返す
        if (currentTarget == null) return false;

        // 現在の探索対象が視認オブジェクト(VisionTarget)でない場合は、falseを返す
        if (!(currentTarget is VisionTarget)) return false;

        // 探索対象の探索にかかる時間を経過させる
        //　((VisionTarget)currentTarget).explorationProgress　: 対象の探索度(MAX : 100.0f)
        // searchTime : 探索対象の探索にかかる時間
        visionTargetMemories[((VisionTarget)currentTarget)].explorationProgress += (100.0f / searchTime) * Time.deltaTime;

        // 探索対象の探索にかかる時間が経過した場合は、trueを返す
        if (visionTargetMemories[((VisionTarget)currentTarget)].explorationProgress >= 100.0f) return true;

        return false;
    }

    /// <summary>
    /// 泥棒の表情を変更する処理
    /// </summary>
    /// <param name="reaction">変更するタイプ</param>
    private void ChangeFace(ReactionSpriteType reaction)
    {
        // 子オブジェクトを取得
        GameObject child = transform.GetChild(0).gameObject;
        // 取得できなかった場合は、エラーログを出力して処理を終了する
        if (child == null) Debug.LogError("子オブジェクトが見つかりませんでした。ThiefAIのChangeFaceメソッドで、泥棒の表情を変更する処理が正常に動作しない可能性があります。");

        // 子オブジェクトからMeshRendererを取得
        MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
        // 取得できなかった場合は、エラーログを出力して処理を終了する
        if (meshRenderer == null) Debug.LogError(" MeshRendererが見つかりませんでした。ThiefAIのChangeFaceメソッドで、泥棒の表情を変更する処理が正常に動作しない可能性があります。");

        // Materialの取得
        Material material = meshRenderer.material;
        // 取得できなかった場合は、エラーログを出力して処理を終了する
        if (material == null) Debug.LogError(" Materialが見つかりませんでした。ThiefAIのChangeFaceメソッドで、泥棒の表情を変更する処理が正常に動作しない可能性があります。");

        // 表情のスプライトを変更する
        material.mainTexture = reactionSprites[(int)reaction].texture;
    }

    /// <summary>
    /// 指定した位置にワープする処理
    /// </summary>
    /// <param name="targetPos">指定位置</param>
    /// <param name="entryDoorDir">入ってきたドアの方向</param>
    public void WarpAction(Vector3 targetPos, CSE_RoomDoorDirection entryDoorDir)
    {
        // 現在の経路をリセットして、ワープ後に新しい経路を計算させる
        navMeshAgent.ResetPath();
        // NavMeshAgentのWarpメソッドを使用して、指定した位置にワープする
        navMeshAgent.Warp(targetPos);

        transform.position = targetPos;
        FindNowRoomNode();

        roomMemories[currentRoom].enteredDoorDirection = entryDoorDir;

        isNextRoomMovePointDecided = false;
        nextRoomMovePoint = null;
        currentTarget = null;
    }

    /// <summary>
    /// ルートを構築する処理
    /// </summary>
    /// <param name="end">ルートの終点</param>
    private void ConstructionRoute(Transform end)
    {
        // -------- 前提チェック --------
        // currentRoom が取れていない場合はルート構築できない
        if (currentRoom == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute: currentRoom が nullです。");
            return;
        }
        // 終点が無い場合もルート構築できない
        if (end == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute: end が nullです。");
            return;
        }

        // --------ルートリストの初期化 --------
        //既存のルートが残っていると誤動作するので毎回クリアする
        if (moveRoute == null) moveRoute = new List<Transform>();
        moveRoute.Clear();

        // -------- 終点が属する部屋(endRoom)を特定 --------
        // end は Transformなので、その Transform が属する RoomCreatePoint を Raycast 等で取得し、
        //そこから RoomNode を引き当てる
        RoomNode endRoom = null;
        try
        {
            GameObject endRoomObj = CS_RoomCreatePointRaycast.GetRayRoomCreatePoint(end.gameObject);
            if (endRoomObj != null)
            {
                endRoom = endRoomObj.GetComponentInChildren<RoomNode>();
            }
        }
        catch
        {
            // Raycast実装やシーン状態によって例外が起きる可能性があるため、ここでは握りつぶす
            // （取得できない場合はフォールバックを行う）
        }

        //取得できなかった場合のフォールバック：firstRoom を終点部屋とみなす
        // （通常は firstEntryPoint が firstRoom にある想定のため、ここで破綻しにくくする）
        if (endRoom == null)
        {
            endRoom = firstRoom;
        }

        //それでも取れない場合は中断
        if (endRoom == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute: 終点部屋(endRoom)を特定できません。");
            return;
        }

        // -------- A*探索の準備 --------
        // open : 探索候補ノード（まだ確定していない）
        // closed : 確定済みノード（これ以上更新しない）
        // bestG : 各部屋(RoomNode)に到達する最小コスト(G)の記録
        var open = new List<RouteNode>();
        var closed = new HashSet<RoomNode>();
        var bestG = new Dictionary<RoomNode, float>();

        // 開始ノード：現在部屋
        // - G:開始なので0
        // - H:ヒューリスティック（ここでは部屋座標間距離）
        RouteNode start = new RouteNode(currentRoom, null, null, 0f, Heuristic(currentRoom, endRoom));
        open.Add(start);
        bestG[currentRoom] = 0f;

        // 終点到達時にここへ入る
        RouteNode goal = null;

        // -------- A*探索 --------
        // open が空になるまで探索（=到達不可）
        while (open.Count > 0)
        {
            // open の中から、評価値 F = G + H が最小のノードを選ぶ
            // ※優先度付きキューではなく線形探索（部屋数が少ない前提）
            int bestIndex = 0;
            float bestF = open[0].F;
            for (int i = 1 ; i < open.Count ; i++)
            {
                float f = open[i].F;
                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }

            // 最小Fのノードを current として取り出す
            RouteNode current = open[bestIndex];
            open.RemoveAt(bestIndex);

            // 終点部屋に到達したら探索終了
            if (current.Room == endRoom)
            {
                goal = current;
                break;
            }

            // 確定済みに追加
            closed.Add(current.Room);

            // 現在部屋から行ける隣接部屋を列挙
            foreach (var edge in GetNeighbors(current.Room))
            {
                // 隣接が取得できない場合はスキップ
                if (edge.NextRoom == null) continue;

                // 「行ったことのある部屋のみ」を通る（記憶に無い部屋は通らない）
                if (!roomMemories.ContainsKey(edge.NextRoom)) continue;

                // closed に入っている部屋は再評価しない
                if (closed.Contains(edge.NextRoom)) continue;

                // Gコスト更新：現在までのコスト + 隣接へ行くコスト
                float tentativeG = current.G + edge.Cost;

                //既に「より良い経路(Gが小さい)」が記録されているなら更新しない
                float recordedG;
                if (bestG.TryGetValue(edge.NextRoom, out recordedG) && tentativeG >= recordedG)
                {
                    continue;
                }

                // この部屋への最良Gを更新
                bestG[edge.NextRoom] = tentativeG;

                // open 内に同じ部屋が存在するか確認（あれば親などを更新、なければ追加）
                RouteNode exist = null;
                for (int i = 0 ; i < open.Count ; i++)
                {
                    if (open[i].Room == edge.NextRoom)
                    {
                        exist = open[i];
                        break;
                    }
                }

                // ヒューリスティック（終点までの推定距離）
                float h = Heuristic(edge.NextRoom, endRoom);

                if (exist != null)
                {
                    //既存ノード更新
                    // Parent :どの部屋から来たか
                    // ViaDoor: Parent -> exist.Room に行くために通るドア(Parent側)
                    exist.Parent = current;
                    exist.ViaDoor = edge.Door;
                    exist.G = tentativeG;
                    exist.H = h;
                }
                else
                {
                    // 新規ノード追加
                    open.Add(new RouteNode(edge.NextRoom, current, edge.Door, tentativeG, h));
                }
            }
        }

        // -------- 探索結果の判定 --------
        // goal が null のままなら到達できなかった（もしくは訪問済み制限で経路が作れなかった）
        if (goal == null)
        {
            Debug.LogWarning("【泥棒】ConstructionRoute:ルート構築に失敗（到達不可 or 訪問済み部屋が不足） 現在:" + currentRoom.name + " 終点:" + endRoom.name);
            return;
        }

        // --------ルート復元（goalから startへ親を辿る） --------
        // goalから Parent を辿る形で復元すると「逆順」になるため、いったんreversedDoors に積んでから反転します。
        // moveRoute に入れるのは「各部屋から次の部屋へ行くために通るドア(Transform)」です。
        var reversedDoors = new List<Transform>();
        RouteNode node = goal;
        while (node != null && node.Parent != null)
        {
            // node.ViaDoor は「node.Parent.Roomから node.Room に行くためのドア(Parent側)」
            if (node.ViaDoor != null)
            {
                reversedDoors.Add(node.ViaDoor);
            }
            node = node.Parent;
        }

        // start -> goal の順になるように reverseして moveRouteへ設定
        for (int i = reversedDoors.Count - 1 ; i >= 0 ; i--)
        {
            moveRoute.Add(reversedDoors[i]);
        }

        // -------- 最終目的地(end)をルート末尾に追加 --------
        if (end != null)
        {
            // 二重追加を避ける（既に末尾がendなら追加しない）
            if (moveRoute.Count == 0 || moveRoute[moveRoute.Count - 1] != end)
            {
                moveRoute.Add(end);
            }
        }
    }

    // --- ConstructionRoute 用の補助 ---

    /// <summary>
    /// A* 探索で扱う「部屋ノード」情報。
    ///
    /// - Room : このノードが表す部屋
    /// - Parent :ひとつ前に辿ってきたノード（経路復元用）
    /// - ViaDoor: Parent.Room -> Room に入るために通過する「Parent側のドア」
    /// - G/H/F : A* のコスト（G=実コスト, H=推定, F=評価値）
    ///
    /// NOTE:
    /// - RoomNode 自体はシーン上の参照なので、探索の都合に合わせた情報はここに保持する。
    /// - sealed にすることで継承拡張を防ぎ、用途を固定しておく。
    /// </summary>
    private sealed class RouteNode
    {
        /// <summary>この探索ノードが表す部屋</summary>
        public RoomNode Room;

        /// <summary>ひとつ前のノード（経路復元に使用）</summary>
        public RouteNode Parent;

        /// <summary>
        /// Parent.Room -> Room に行く時に通過する「Parent側」のドア。
        /// moveRouteにはこのドアTransformを並べる。
        /// </summary>
        public Transform ViaDoor;

        /// <summary>開始から現在部屋までの実コスト</summary>
        public float G;

        /// <summary>現在部屋から終点部屋までの推定コスト</summary>
        public float H;

        /// <summary>評価値（小さいほど優先度が高い）</summary>
        public float F => G + H;

        public RouteNode(RoomNode room, RouteNode parent, Transform viaDoor, float g, float h)
        {
            Room = room;
            Parent = parent;
            ViaDoor = viaDoor;
            G = g;
            H = h;
        }
    }

    /// <summary>
    /// 「ある部屋から隣接部屋へ移動する」ための辺情報。
    ///
    /// - NextRoom : 接続先の部屋
    /// - Door : from側（現在部屋側）に存在するドア Transform
    /// - Cost : 隣接部屋へ移動するコスト（ここでは部屋中心間距離）
    /// </summary>
    private struct NeighborEdge
    {
        public RoomNode NextRoom;
        public Transform Door;
        public float Cost;

        public NeighborEdge(RoomNode nextRoom, Transform door, float cost)
        {
            NextRoom = nextRoom;
            Door = door;
            Cost = cost;
        }
    }

    /// <summary>
    /// 指定した部屋から、接続している隣接部屋の一覧（辺）を列挙する。
    ///
    ///取得元：
    /// - `RoomNode` が属する `CS_RoomCreatePoint`から接続方向を取得
    /// - `TryGetConnection`で相手側 `RoomNode` を取得
    /// - `RoomNode.GetDirectionWallToDoor`で from側のドアTransformを取得
    ///
    /// NOTE:
    /// -ここでは「行ったことがある部屋」制限は掛けない（ConstructionRoute側で判定）。
    /// - 接続が壊れている・参照が取れないケースは `yield break/continue`で安全側に倒す。
    /// </summary>
    private IEnumerable<NeighborEdge> GetNeighbors(RoomNode from)
    {
        if (from == null) yield break;

        // RoomNode は RoomCreatePoint の子として配置されている前提
        var createPoint = from.transform.parent != null ? from.transform.parent.GetComponent<CS_RoomCreatePoint>() : null;
        if (createPoint == null) yield break;

        // 接続しているドア方向（Right/Left/Front/Back）
        List<CSE_RoomDoorDirection> dirs = createPoint.GetConnectDirections();
        if (dirs == null) yield break;

        foreach (var dir in dirs)
        {
            // from側のドアTransform（後で moveRoute に積むのはこの Transform）
            Transform door = from.GetDirectionWallToDoor(dir);

            // 接続先 RoomNode
            CS_RoomMoveConnection connection;
            if (!createPoint.TryGetConnection(dir, out connection) || connection == null || connection.TargetCreatePoint == null) continue;

            RoomNode nextRoom = connection.TargetCreatePoint.GetComponentInChildren<RoomNode>();
            if (nextRoom == null) continue;

            // コスト：部屋中心間の距離（取得できなければ1 とする）
            float cost = 1f;
            try
            {
                cost = Vector3.Distance(from.transform.position, nextRoom.transform.position);
            }
            catch
            {
                cost = 1f;
            }

            yield return new NeighborEdge(nextRoom, door, cost);
        }
    }

    /// <summary>
    /// A* のヒューリスティック関数。
    ///
    ///ここでは「部屋同士の直線距離」を推定コストとして使用する。
    /// - ドア位置ではなく部屋の Transform 座標を使うため、厳密な移動距離とは一致しない。
    /// -ただし部屋移動の大まかな近さを示せるため、探索効率は上がる。
    /// </summary>
    private float Heuristic(RoomNode a, RoomNode b)
    {
        if (a == null || b == null) return 0f;
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    /// <summary>
    /// 移動要求を統一する。SmartNavAgent がある場合は DangerZone を考慮して移動する。
    /// </summary>
    private void MoveTo(Vector3 destination)
    {
        if (smartNavAgent != null)
        {
            smartNavAgent.MoveTo(destination);
        }
        else if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(destination);
        }
    }

    /// <summary>
    ///罠発動などで「この泥棒が回避する DangerZone」を動的に追加する。
    /// </summary>
    public void AddAvoidZoneID(int zoneID)
    {
        if (avoidZoneIDs == null) avoidZoneIDs = new List<int>();
        if (!avoidZoneIDs.Contains(zoneID)) avoidZoneIDs.Add(zoneID);

        // SmartNavAgent がある場合は即時反映
        if (smartNavAgent == null) smartNavAgent = GetComponent<SmartNavAgent>();
        if (smartNavAgent != null)
        {
            smartNavAgent.SetAvoidZoneIDs(avoidZoneIDs);
        }
    }
}
