/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーアクション作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 *    秋野翔太
 *    大瀧蓮
 *    吉田京志郎
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-04-27 | ソウル消費およびギミックの初期化の実装
 * 2026-05-06 | ギミックの設置位置をプレイヤーの前へ修正：大瀧
 * 2026-05-08 | リファクタリング(大瀧)
 * 2026-05-11 | ギミックの設置方向の設定を追加：大瀧
 * 2026-05-24 | インタラクトの範囲を円柱化：吉田
 * 2026-05-25 | SEを追加：吉田
 * 2026-05-25 | インタラクトの範囲に入った泥棒に通知処理：吉田
 * 2026-06-11 | ギミック設置時のEffect再生処理を追加：吉本
 * 2026-06-22 | ギミック設置時のプレビュー表示を追加：大瀧
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerAction : MonoBehaviour
{
    [Header("ギミックの軌道に切り替わる為に必要な長押しの時間")][SerializeField] private float switchInteract = 1f;         // ギミックの起動へ切り替わる為に必要な長押しの時間
    [Header("インタラクト範囲用のオブジェクト")][SerializeField] private GameObject interactField = null;   // インタラクトの範囲を示すフィールド
    [Header("ギミックの大きさが最大になるために必要な秒数")][SerializeField] private float interactSpeed = 1f;          // インタラクトの速度(interactSpeed秒で範囲が1になる)
    [System.Serializable]
    public struct InteractSyllinder // インタラクトの範囲を構成する要素をまとめた構造体
    {
        public float radius;    // 円柱の直径
        public float height;    // 円柱の高さ
    }
    [SerializeField] public InteractSyllinder interactMin = new InteractSyllinder { radius = 3f, height = 3f };//インタラクトの範囲の最小値
    [SerializeField] public InteractSyllinder interactMax = new InteractSyllinder { radius = 5f, height = 5f };//インタラクトの範囲の最大値
    [HideInInspector] public int currentGimmickIndex { private set; get; } = 0;//現在選択しているギミック

    private GameObject previewBase;

    private CS_OutlineTarget outlineTarget; // プレイヤーのアウトラインを制御

    private CS_PlayerData playerData;
    float interactTime = 0.0f;
    Vector3 interactScale = Vector3.zero;
    bool isInteracting = false;

    Vector3 settingPos = Vector3.zero;  // 設置予定場所

    private GimmickList gimmickManager;
    private CS_3DPlaySE playSE;
    List<Collider> hitList = new List<Collider>();

    // ギミック設置時のEffect再生クラスへの参照
    private CS_GimmickSetEffectPlayer cs_GimmickSetEffectPlayer;

    // インタラクト範囲Effect再生クラスへの参照
    private CS_PlayerInteractRangeEffectPlayer cs_PlayerInteractRangeEffectPlayer;

    private int saveGimmickIndex;// 現在のギミックインデックスを保存する変数

    private bool isShowGimmickPreview = false;

    private bool isViewPreview = true;

    private bool isSelectGimmickActive = true;

    private bool b_IsPlayingAnkhStandEffect = false;

    private const float PlacementBoundsInsetRate = 0.01f;
    [Header("ワープUI表示のオブジェクト格納")] [SerializeField] private GameObject warpUIGameObject;
    [Header("ワープのUI")][SerializeField] private Sprite[] warpUISprite;
    [HideInInspector]public bool warpUIView;
    [HideInInspector]public bool doWarp;

    // Start is called before the first frame update
    void Start()
    {
        //現在のソウルの数
        playerData = GetComponent<CS_PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("CS_PlayerDataコンポーネントが見つかりませんでした。");
            return;
        }

        gimmickManager = GetComponent<GimmickList>();

        // インプットアクションの登録
        playerData.customInputAction.Player.GimmickChange.started += OnSelect;

        playerData.customInputAction.Player.Interact.started += OnInteract;
        playerData.customInputAction.Player.Interact.performed += OnInteract;
        playerData.customInputAction.Player.Interact.canceled += OnInteract;

        playerData.customInputAction.Player.InteractCancel.started += OnCancel;

        if (interactField != null)
        {
            Renderer interactRenderer = interactField.GetComponent<Renderer>();
            if (interactRenderer != null) interactRenderer.enabled = false;
        }

        // アウトラインコントローラーの初期化
        outlineTarget = GetComponentInChildren<CS_OutlineTarget>();
        if (outlineTarget != null) outlineTarget.SetOutlineColor(Color.gray);

        GameObject se3DObject = GameObject.Find("3DSE");
        playSE = se3DObject != null ? se3DObject.GetComponent<CS_3DPlaySE>() : null;

        cs_GimmickSetEffectPlayer = GetComponent<CS_GimmickSetEffectPlayer>();

        cs_PlayerInteractRangeEffectPlayer = GetComponent<CS_PlayerInteractRangeEffectPlayer>();

        if (warpUIGameObject == null)
        {
            Debug.LogWarning("warpUIGameObjectが設定されていません。");
        }
        else 
        {
            if (CS_CustomInputActionManager.instance.currentInputType == CS_CustomInputActionManager.InputType.Gamepad)
            {
                warpUIGameObject.GetComponent<SpriteRenderer>().sprite = warpUISprite[0];
            }
            else
            {
                warpUIGameObject.GetComponent<SpriteRenderer>().sprite = warpUISprite[1];
            }
            warpUIGameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        settingPos = CalculateGimmickSetPosition();

        if (playerData.currentMode == CS_PlayerData.PlayerMode.Normal)
        {
            outlineTarget.SetOutlineColor(Color.gray);
        }
        else
        {
            outlineTarget.SetOutlineColor(Color.yellow);
        }

        if (isInteracting)
        {
            interactTime += Time.deltaTime * interactSpeed;
            // インタラクト範囲の拡大
            if (interactTime >= switchInteract)
            {
                if (IsCurrentGimmickMagicAnkh())
                {
                    PlayAllAnkhStandEffect();
                }


                if (hitList != null)
                {
                    foreach (var item in hitList)
                    {
                        if (item == null) continue;
                        GimmickBase gimmick = item.GetComponent<GimmickBase>();
                        if (gimmick != null)
                        {
                            gimmick.SetOutLineColor(Color.gray);
                        }
                    }
                }

                outlineTarget.SetOutlineColor(Color.green);

                // インタラクト範囲を拡大
                interactScale.x = Mathf.Max(interactTime - switchInteract, 0f) + interactMin.radius;
                interactScale.y = Mathf.Max(interactTime - switchInteract, 0f) + interactMin.height;
                interactScale.z = Mathf.Max(interactTime - switchInteract, 0f) + interactMin.radius;
                interactScale.x = Mathf.Min(interactScale.x, interactMax.radius);
                interactScale.y = Mathf.Min(interactScale.y, interactMax.height);
                interactScale.z = Mathf.Min(interactScale.z, interactMax.radius);
                interactField.transform.localScale = interactScale;
                Vector3 interactPos = transform.position;
                interactPos.y += interactScale.y * 0.5f; // フィールドが地面に接するように位置を調整
                interactField.transform.position = interactPos;

                // Effectのサイズを変更
                if (cs_PlayerInteractRangeEffectPlayer != null)
                {
                    cs_PlayerInteractRangeEffectPlayer.UpdateInteractRangeEffect(
                        interactField.transform);
                }

                // 円柱で判定を取るために、カプセルでオーバーラップを取った後に上下の半球を除外する
                Collider[] hits = Physics.OverlapCapsule(
                    interactField.transform.position + interactField.transform.up * interactScale.y * 0.5f,
                    interactField.transform.position - interactField.transform.up * interactScale.y * 0.5f,
                    interactScale.x * 0.5f,
                    LayerMask.GetMask("Gimmick", "Thief")
                    );

                float minHeight = -interactScale.y * 0.5f;
                float maxHeight = interactScale.y * 0.5f;
                for (int i = 0 ; i < hits.Length ; i++)
                {
                    if (hits[i] == null) continue;
                    //ギミックの情報を取得
                    Vector3 hitPoint = hits[i].ClosestPoint(interactField.transform.position);

                    // interactField基準高さ
                    float height =
                        Vector3.Dot(
                            hitPoint - interactField.transform.position,
                            interactField.transform.up
                        );

                    // 高さ制限
                    if (height < minHeight ||
                        height > maxHeight)
                    {
                        continue;
                    }

                    // ギミックがすでに起動している場合は緑のアウトラインをつけない
                    var gimmick = hits[i].GetComponent<GimmickBase>();
                    if (gimmick != null)
                    {
                        if (gimmick.gimmickState != GimmickState.Idle) continue;
                        gimmick.SetOutLineColor(Color.green);
                    }

                    hitList.Add(hits[i]);
                }
            }
        }
        if (playerData.currentMode == CS_PlayerData.PlayerMode.Setting && isViewPreview)
        {
            ShowGimmickPreview();
        }
        else
        {
            // 残ったプレビューオブジェクトの削除
            if (previewBase != null)
            {
                ClearGimmickPreview();
            }
        }

        WarpUIView();
    }
    private IEnumerator ExecuteSettingSequence()
    {
        playerData.currentMode = CS_PlayerData.PlayerMode.Normal;
        //一フレーム遅らせる処理
        yield return null;
        SettingAction();
        int interactSEIndex = UnityEngine.Random.Range(1, 4);
        playSE.PlayOneShotSE("Cat_Interact" + interactSEIndex.ToString(), gameObject.transform.position, "InteractSE");
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        if (!isSelectGimmickActive) return;
        float contextValue = context.ReadValue<float>();

        //キー操作でUIのギミックの選択
        if (contextValue == 1) currentGimmickIndex++;
        else if (contextValue == -1) currentGimmickIndex--;
        currentGimmickIndex = (currentGimmickIndex % gimmickManager.GetCurrentGimmick().Count + gimmickManager.GetCurrentGimmick().Count) % gimmickManager.GetCurrentGimmick().Count;
        Debug.Log("ギミックの数：" + gimmickManager.GetCurrentGimmick().Count);
        Debug.Log("現在選択中のギミック：" + gimmickManager.GetCurrentGimmick()[currentGimmickIndex].gimmickPrefab.name);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (warpUIView)
        {
            doWarp = true;
        }
        else 
        {
            if (context.started)
            {
                interactTime = 0.0f;
                interactField.GetComponent<Renderer>().enabled = true;

                interactField.transform.localScale = Vector3.zero;
                isInteracting = true;
                if (cs_PlayerInteractRangeEffectPlayer != null)
                {
                    cs_PlayerInteractRangeEffectPlayer.PlayInteractRangeEffect(
                        interactField.transform);
                }
            }
            else if (context.canceled)
            {
                isInteracting = false;

                EndAllAnkhStandEffect();

                // Effectの停止
                if (cs_PlayerInteractRangeEffectPlayer != null)
                {
                    cs_PlayerInteractRangeEffectPlayer.EndInteractRangeEffect();
                }
                if (interactTime < switchInteract)
                {
                    // 短押しは設置の処理を行う
                    switch (playerData.currentMode)
                    {
                        case CS_PlayerData.PlayerMode.Normal:
                            playerData.currentMode = CS_PlayerData.PlayerMode.Setting;
                            break;
                        case CS_PlayerData.PlayerMode.Setting:
                            StartCoroutine(ExecuteSettingSequence());
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // 長押しはギミックの起動を行う
                    int interactSEIndex = UnityEngine.Random.Range(1, 4);
                    interactField.GetComponent<Renderer>().enabled = false;
                    playSE.PlayOneShotSE("Cat_Interact" + interactSEIndex.ToString(), gameObject.transform.position, "InteractSE");

                    //アンク用動作
                    if (gimmickManager.GetCurrentGimmick()[currentGimmickIndex].gimmickTag ==
                        Gimmick.MagicAnkh)
                    {//カーソルがアンクの時インタラクト発動
                        foreach (var GM in gimmickManager.GetGimmickList())
                        {
                            //タグでアンク判定→アクティブへ
                            if (GM.gimmick.GetGimmickTag() == Gimmick.MagicAnkh)
                            {
                                GM.gimmick.gimmickState = GimmickState.Active;
                            }
                        }
                    }

                    foreach (Collider hit in hitList)
                    {
                        var renderers = hit.GetComponentsInChildren<Renderer>();
                        foreach (var renderer in renderers)
                        {
                            if (renderer.materials.Length < 2) continue;
                            Material material = renderer.materials[1];
                            if (material != null) material.SetVector("_OutlineColor", Color.gray);
                        }

                        GimmickBase gimmick = hit.GetComponent<GimmickBase>();
                        if (gimmick != null)
                        {
                            if (gimmick.gimmickState != GimmickState.Idle) continue;

                            SettingGimmickDirection(gimmick);

                            //ギミックをアクティブにする
                            Debug.Log($"ギミック：" + hit.name + "がアクティブになりました" + gimmick.GetDirectionVec());
                            gimmick.ActivateGimmick();
                            continue;
                        }

                        CS_ThiefAI thief = hit.GetComponent<CS_ThiefAI>();
                        if (thief != null)
                        {
                            thief.read_HearingSystem.InvestigateSound(gameObject.transform.position, CS_HearingSystem.AttractSoundType.CatVoice);
                        }
                    }

                    hitList.Clear();
                }
            }
        }
    }

    public Gimmick GetSelectCurrentGimmickTag()
    {
        if (currentGimmickIndex < 0)
            return Gimmick.None;

        return gimmickManager.GetCurrentGimmick()[currentGimmickIndex].gimmickTag;
    }
    public GimmickList.CurrentGimmickData GetCurrentGimmick(Gimmick gimmickTag)
    {
        if(gimmickTag == Gimmick.None)
            return null;

        GimmickList.CurrentGimmickData currentGimmick = gimmickManager.GetCurrentGimmick().Find(g => g.gimmickTag == gimmickTag);

        return currentGimmick;
    }
    public GimmickList.GimmickInfoData GetGimmickInfoData(Gimmick gimmickTag)
    {
        if (gimmickTag == Gimmick.None)
            return null;
        GimmickList.GimmickInfoData gimmickInfo = gimmickManager.GetGimmickInfoDataList().Find(g => g.gimmickTag == gimmickTag);
        return gimmickInfo;
    }

    /// <summary>
    /// 現在選択中のギミックがアンクか確認します。
    /// </summary>
    private bool IsCurrentGimmickMagicAnkh()
    {
        if (gimmickManager == null)
        {
            return false;
        }

        if (gimmickManager.GetCurrentGimmick() == null)
        {
            return false;
        }

        if (currentGimmickIndex < 0 ||
            currentGimmickIndex >= gimmickManager.GetCurrentGimmick().Count)
        {
            return false;
        }

        return gimmickManager.GetCurrentGimmick()[currentGimmickIndex].gimmickTag ==
               Gimmick.MagicAnkh;
    }

    /// <summary>
    /// 配置済みの全アンクから待機Effectを再生します。
    /// </summary>
    private void PlayAllAnkhStandEffect()
    {
        if (b_IsPlayingAnkhStandEffect)
        {
            return;
        }

        b_IsPlayingAnkhStandEffect = true;

        foreach (var GM in gimmickManager.GetGimmickList())
        {
            if (GM.gimmick == null)
            {
                continue;
            }

            if (GM.gimmick.GetGimmickTag() != Gimmick.MagicAnkh)
            {
                continue;
            }

            MagicAnkhGimmick magicAnkhGimmick =
                GM.gimmick.GetComponent<MagicAnkhGimmick>();

            if (magicAnkhGimmick == null)
            {
                continue;
            }

            magicAnkhGimmick.PlayAnkhStandEffect();
        }
    }

    /// <summary>
    /// 配置済みの全アンクの待機Effectを終了します。
    /// </summary>
    private void EndAllAnkhStandEffect()
    {
        if (b_IsPlayingAnkhStandEffect == false)
        {
            return;
        }

        b_IsPlayingAnkhStandEffect = false;

        foreach (var GM in gimmickManager.GetGimmickList())
        {
            if (GM.gimmick == null)
            {
                continue;
            }

            if (GM.gimmick.GetGimmickTag() != Gimmick.MagicAnkh)
            {
                continue;
            }

            MagicAnkhGimmick magicAnkhGimmick =
                GM.gimmick.GetComponent<MagicAnkhGimmick>();

            if (magicAnkhGimmick == null)
            {
                continue;
            }

            magicAnkhGimmick.EndAnkhStandEffect();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        // キャンセル操作があった場合、現在のモードをノーマルに戻す
        playerData.currentMode = CS_PlayerData.PlayerMode.Normal;
    }

    private void SettingAction()
    {
        if (IsInfinityPosition(settingPos)) return;

        if (gimmickManager.GetCurrentGimmick()[currentGimmickIndex] == null)
        {
            Debug.LogError("選択されたギミックが見つかりません");
            return;
        }
        GimmickBase gimmick = 
            gimmickManager.GetCurrentGimmick()[currentGimmickIndex].gimmickPrefab.GetComponent<GimmickBase>();
        if (gimmick == null)
        {
            Debug.LogError("選択されたギミックにGimmickBaseコンポーネントが付いていません"); return;
        }
        if (!gimmickManager.IsSetting(gimmick.gimmick))
        {
            Debug.Log("ギミックの設置失敗: IsSetting");
            return;
        }
        GameObject playerRoomData = playerData.currentRoomData.GetPlayerRoomData();
        if (playerRoomData == null)
        {
            Debug.Log("現在の部屋データが取得できません");
            return;
        }
        GameObject currentRoom = playerRoomData.transform.GetChild(0).gameObject;
        string roomName = currentRoom.name;
        bool isNotSettingRoom = roomName.Contains("Treasure");
        Debug.Log(roomName);
        if (currentRoom == null || isNotSettingRoom)
        {
            Debug.Log("この部屋にトラップは配置できません");
            return;    // 設置可能な部屋のみ設置する
        }

        GameObject currentFloor = playerData.currentRoomData.GetPlayerFloorData();
        if (currentFloor == null)
        {
            Debug.LogError("現在の部屋の床データが取得できません");
            return;
        }
        var roomGrid = currentFloor.GetComponent<RoomGrid>();

        // グリッド配置
        if (roomGrid == null)
            Debug.LogError("この部屋の床にRoomGridがついていません");

        Vector3 setPos = CalculateGimmickSetPosition();
        if (IsInfinityPosition(setPos)) return;

        gimmick.gimmickState = GimmickState.Spawn;

        // 設置処理 //
        if (!roomGrid.SetGimmickInGrid(setPos, gimmick))
            return;

        // =========================
        // 実際に生成されたインスタンス取得
        // =========================
        Vector3 center = setPos;          // 中心位置
        Vector3 halfExtents = new Vector3(roomGrid.gridSize.x * 0.5f, 5f, roomGrid.gridSize.y * 0.5f); // 半径ではなく「半サイズ」

        Collider[] hits = Physics.OverlapBox(center, halfExtents);
        GimmickBase instance = null;

        foreach (var hit in hits)
        {
            GimmickBase gimmickBase = hit.GetComponent<GimmickBase>();

            if (gimmickBase == null)
                continue;

            // 同じ種類のみ
            if (gimmickBase.GetGimmickTag() != gimmick.GetGimmickTag())
                continue;

            instance = gimmickBase;
            break;
        }

        if (instance == null)
        {
            Debug.LogError("配置後のGimmick取得失敗");
            return;
        }

        bool isEffect = true;

        //ギミックごとに魔法陣の位置を調整可能に
        //ギミックの場所も変わる
        switch(instance.gimmick)
        {
            case Gimmick.Nyaki:
                instance.transform.position = transform.position;
                break;
        }

        // ギミック直下に魔法陣Effectを生成して再生
        // ギミック設置時Effectを再生
        if (cs_GimmickSetEffectPlayer != null && isEffect)
        {
            cs_GimmickSetEffectPlayer.PlayGimmickSetEffect(
                instance.transform.position,
                instance);
        }

        // Managerへ実体を登録
        gimmickManager.SettingStart(instance);
    }

    public void SettingGimmickDirection(GimmickBase gimmick)
    {
        // インタラクト方向を設定※ギミックとの位置関係で判定（対角線で四分割：三角形×4）
        Vector3 gimmickPos = CalculateGimmickSetPosition();
        Vector3 toPlayer = transform.position - gimmickPos;
        float dx = toPlayer.x;
        float dz = toPlayer.z;

        // 三角形境界は z = ±x なので絶対値で比較する
        float adx = Mathf.Abs(dx);
        float adz = Mathf.Abs(dz);
        const float eps = 1e-5f; // 同値判定の小さな余裕

        if (dz > adx + eps)
        {
            // プレイヤーがギミックの「前（+Z）側の三角形」：Up
            gimmick.SetGimmickDirection(GimmickDirection.Up);
        }
        else if (-dz > adx + eps)
        {
            // プレイヤーがギミックの「後（-Z）側の三角形」：Down
            gimmick.SetGimmickDirection(GimmickDirection.Down);
        }
        else if (dx > adz + eps)
        {
            // プレイヤーがギミックの「右（+X）側の三角形」：Right
            gimmick.SetGimmickDirection(GimmickDirection.Right);
        }
        else if (-dx > adz + eps)
        {
            // プレイヤーがギミックの「左（-X）側の三角形」：Left
            gimmick.SetGimmickDirection(GimmickDirection.Left);
        }
        else
        {
            // 厳密な境界上（対角線上）に居る場合のフォールバック：
            // X/Z の絶対値で優勢側を使う（斜め真正面は Z 優先）
            if (adz >= adx) gimmick.SetGimmickDirection(dz >= 0f ? GimmickDirection.Up : GimmickDirection.Down);
            else gimmick.SetGimmickDirection(dx >= 0f ? GimmickDirection.Right : GimmickDirection.Left);
        }
    }

    //ギミックの設置位置を補正する計算：大瀧
    private Vector3 CalculateGimmickSetPosition()
    {
        GameObject currentRoom = playerData.currentRoomData.GetPlayerFloorData();

        var roomGrid = currentRoom.GetComponent<RoomGrid>();
        if (roomGrid == null) return Vector3.positiveInfinity;
        if (gimmickManager.GetCurrentGimmick().Count == 0) return Vector3.positiveInfinity;
        GimmickBase gimmick = gimmickManager.GetCurrentGimmick()[currentGimmickIndex].gimmickPrefab.GetComponent<GimmickBase>();

        settingPos = transform.position;

        //ギミックのサイズ
        Vector2Int size = gimmick.GetGimmickSize();
        // グリッドサイズ
        Vector3 gridSize = roomGrid.gridSize;

        // 各方向のオフセット量を計算
        float offsetX = size.x;
        float offsetZ = size.y;

        // ワールド座標に変換
        offsetX *= gridSize.x;
        offsetZ *= gridSize.y;

        // 向きで分岐
        Vector3 forward = transform.forward;
        bool forwardZ = false;

        // プレイヤの前面にギミックを置くために行う補正
        // X成分とZ成分の絶対値を比較して、どちらを優先するか決める
        if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
        {
            forwardZ = false;
            // X軸
            if (forward.x >= 0f)
                settingPos += new Vector3(offsetX, 0f, 0f);
            else
            {
                //設置位置の補正※偶数時
                if (size.x % 2 == 0)
                    settingPos -= new Vector3(offsetX * 0.5f, 0f, 0f);  //設置予定位置の中核部分なため半マスのみ動かす。
                else
                    settingPos -= new Vector3(offsetX, 0f, 0f);
            }
        }
        else
        {
            forwardZ = true;
            // Z軸（同値なら Z を優先）
            if (forward.z >= 0f)
                settingPos += new Vector3(0f, 0f, offsetZ);
            else
            {
                //設置位置の補正※偶数時
                if (size.x % 2 == 0)
                    settingPos -= new Vector3(0f, 0f, offsetZ * 0.5f);  //設置予定位置の中核部分なため半マスのみ動かす。
                else
                    settingPos -= new Vector3(0f, 0f, offsetZ);
            }
        }
        // ===============================
        // グリッド変換
        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);
        if (grid.x == -1 || grid.y == -1) return Vector3.positiveInfinity;

        //グリッド座標からワールドへ逆変換
        Vector3 spawnPos = roomGrid.GetWorldPosFromGrid(grid);
        if (IsInfinityPosition(spawnPos)) return Vector3.positiveInfinity;

        ////グリッド補正
        ////グリッドを４分割して考えより細かく
        ////設置位置の調整をできるようにする。
        if (gimmick.GetGimmickSize().x % 2 == 0 && forwardZ)
        {//サイズが偶数の時 && Z軸を向いているとき
            if (spawnPos.x < transform.position.x &&
                spawnPos.x + (size.x / 2f) > transform.position.x)
            {//グリッド設置予定位置より右よりにいたら
             //グリッド設置予定位置+ギミックサイズより左にいたら
                spawnPos.x += gridSize.x;
            }
        }
        else if (gimmick.GetGimmickSize().y % 2 == 0 && !forwardZ)
        {
            if (spawnPos.z < transform.position.z &&
                spawnPos.z + (size.y / 2f) > transform.position.z)
            {//グリッド設置予定位置より右よりにいたら
             //グリッド設置予定位置+ギミックサイズより左にいたら
                spawnPos.z += gridSize.y;
            }
        }

        Vector2Int fixedGrid = roomGrid.GetGridFromPos(spawnPos);
        if (fixedGrid.x == -1 || fixedGrid.y == -1) return Vector3.positiveInfinity;

        Vector3 correctedSpawnPos =
            roomGrid.GimmickEvenNumberCorrection(spawnPos, fixedGrid, gimmick);

        if (!IsGimmickInsideRoom(roomGrid, correctedSpawnPos, size))
            return Vector3.positiveInfinity;

        settingPos = spawnPos;
        return spawnPos;
    }

    private static bool IsInfinityPosition(Vector3 position)
    {
        return float.IsInfinity(position.x) ||
               float.IsInfinity(position.y) ||
               float.IsInfinity(position.z);
    }

    private bool IsGimmickInsideRoom(
        RoomGrid roomGrid,
        Vector3 centerPos,
        Vector2Int size)
    {
        if (roomGrid == null) return false;
        if (IsInfinityPosition(centerPos)) return false;

        Vector2 gridSize = roomGrid.gridSize;
        float inset = Mathf.Min(gridSize.x, gridSize.y) * PlacementBoundsInsetRate;
        float halfX = Mathf.Max(size.x * gridSize.x * 0.5f - inset, 0f);
        float halfZ = Mathf.Max(size.y * gridSize.y * 0.5f - inset, 0f);

        Vector3[] checkOffsets =
        {
            Vector3.zero,
            new Vector3(halfX, 0f, halfZ),
            new Vector3(halfX, 0f, -halfZ),
            new Vector3(-halfX, 0f, halfZ),
            new Vector3(-halfX, 0f, -halfZ),
        };

        foreach (Vector3 offset in checkOffsets)
        {
            Vector3 checkPos =
                centerPos + roomGrid.transform.TransformVector(offset);
            Vector2Int grid = roomGrid.GetGridFromPos(checkPos);

            if (grid.x == -1 || grid.y == -1)
                return false;
        }

        return true;
    }

    private Vector2Int GetPreviewCheckSize(GimmickBase gimmick)
    {
        Vector2Int size = gimmick.GetGimmickSize();

        if (gimmickManager == null ||
            gimmickManager.GetCurrentGimmick() == null ||
            currentGimmickIndex < 0 ||
            currentGimmickIndex >= gimmickManager.GetCurrentGimmick().Count)
        {
            return size;
        }

        GameObject previewPrefab =
            gimmickManager.GetCurrentGimmick()[currentGimmickIndex].previewPrefab;
        if (previewPrefab == null) return size;

        PreviewBase preview = previewPrefab.GetComponent<PreviewBase>();
        if (preview == null) return size;

        Vector3Int previewSize = preview.GetPreviewSize();
        size.x = Mathf.Max(size.x, previewSize.x);
        size.y = Mathf.Max(size.y, previewSize.z);
        return size;
    }

    private void ClearGimmickPreview()
    {
        if (previewBase != null)
        {
            Destroy(previewBase.gameObject);
            previewBase = null;
        }

        isShowGimmickPreview = false;
    }

    // ギミックのプレビュー表示
    private void ShowGimmickPreview()
    {
        if (gimmickManager.GetCurrentGimmick().Count == 0)
            return;

        GameObject currentRoom =
            playerData.currentRoomData.GetPlayerFloorData();

        if (currentRoom == null)
            return;

        var roomGrid =
            currentRoom.GetComponent<RoomGrid>();

        if (roomGrid == null)
            return;

        if (gimmickManager.GetCurrentGimmick()[currentGimmickIndex] == null)
        {
            Debug.LogError("選択されたギミックが見つかりません");
            return;
        }
        //選択しているギミックのタグを選択
        GimmickBase gimmick = gimmickManager.GetCurrentGimmick()[currentGimmickIndex].gimmickPrefab.GetComponent<GimmickBase>();

        //現在の部屋
        gimmick.roomGrid = roomGrid;

        if (gimmick == null)
            return;

        // 設置位置の計算_______________________________________
        settingPos = CalculateGimmickSetPosition();
        if (IsInfinityPosition(settingPos))
        {
            ClearGimmickPreview();
            return;
        }

        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);
        if (grid.x == -1 || grid.y == -1)
        {
            ClearGimmickPreview();
            return;
        }

        settingPos = roomGrid.GetWorldPosFromGrid(grid);
        if (IsInfinityPosition(settingPos))
        {
            ClearGimmickPreview();
            return;
        }

        settingPos = roomGrid.GimmickEvenNumberCorrection(settingPos, grid, gimmick);
        if (!IsGimmickInsideRoom(roomGrid, settingPos, GetPreviewCheckSize(gimmick)))
        {
            ClearGimmickPreview();
            return;
        }

        // レイでギミックの設置位置を計算_______________________
        Ray ray = new Ray();
        ray.direction = Vector3.down;
        const float rayOriginY = 255f;
        ray.origin = new Vector3(settingPos.x, rayOriginY, settingPos.z);
        // 床を確実に取れるようマージンを大きめに取りレイを飛ばす
        RaycastHit[] rayHits = Physics.RaycastAll(ray, Mathf.Abs(rayOriginY - (gameObject.transform.position.y - 10.0f)), ~0,
            QueryTriggerInteraction.Ignore);
        Array.Sort(rayHits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hitItem in rayHits)
        {
            if (hitItem.transform.CompareTag("Player")) continue;
            if (hitItem.transform.CompareTag("Thief")) continue;

            settingPos.y = hitItem.point.y;
            break;
        }
        // 設置位置補正_______________________________________
        gimmick.SetGimmickPos(settingPos);
        MeshFilter meshFilter = gimmick.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
            gimmick.AdjustScaleToGrid();

        // infinity例外
        if (IsInfinityPosition(settingPos)) return;

        //----------------------------------
        // 初回のみ生成
        //----------------------------------
        if (!isShowGimmickPreview || saveGimmickIndex != currentGimmickIndex)
        {
            saveGimmickIndex = currentGimmickIndex;

            gimmick.gimmickState =
                GimmickState.Preview;
            if (gimmickManager.GetCurrentGimmick()[currentGimmickIndex] == null)
            {
                Debug.Log("previewInstance取得失敗");
                return;
            }

            // 設置前破壊
            if(previewBase != null)
            {
                ClearGimmickPreview();
            }
            if(gimmickManager.GetCurrentGimmick()[currentGimmickIndex].previewPrefab == null)
            {
                return;
            }
            //ギミックじゃなかった場合　※発動系だった場合は無視
            if (!gimmickManager.GetCurrentGimmick()[currentGimmickIndex].previewPrefab.GetComponent<PreviewBase>().GetIsGimmick())
            {
                Debug.Log("発動式なため描画を無視");
                return;
            }

            previewBase = Instantiate(gimmickManager.GetCurrentGimmick()[currentGimmickIndex].previewPrefab);
            isShowGimmickPreview = true;

            // =========================
            // 実際に生成されたインスタンス取得
            // =========================
            Vector3 center = new Vector3(settingPos.x, settingPos.y, settingPos.z);// 中心位置
            Vector3 halfExtents = new Vector3(2f, 10f, 2f); // 半径ではなく「半サイズ」

            Collider[] hits = Physics.OverlapBox(center, halfExtents);

            foreach (var hit in hits)
            {
                PreviewBase prevBase = hit.GetComponent<PreviewBase>();

                if (prevBase == null)
                    continue;

                // 同じ種類のみ
                if (prevBase.GetGimmickTag() != gimmick.GetGimmickTag())
                    continue;

                previewBase = prevBase.gameObject;
                break;
            }

            if (previewBase == null)
            {
                Debug.LogError("配置後のPreview取得失敗");
                return;
            }
            Debug.Log("新しいプレビューの表示" + previewBase.name);
        }
        //----------------------------------
        // 生成済なら移動だけ
        //----------------------------------
        else
        {
            if (previewBase != null)
            {
                GameObject prevObj;
                prevObj = previewBase.gameObject;
                if (prevObj.GetComponent<PreviewBase>().GetPreviewSize().y % 2 == 0)
                {
                    settingPos.y += prevObj.GetComponent<PreviewBase>().GetPreviewSize().y * 0.5f;
                }

                previewBase.transform.position =
                    settingPos;
                //プレイヤーとギミックとの位置でギミックの向きを設定
                //SettingGimmickDirection(previewInstance);
            }
        }
    }
    public void SetViewPreview(bool isView)
    {
        isViewPreview = isView;
    }

    public void SetSelectGimmickActive(bool isActive)
    {
        isSelectGimmickActive = isActive;
    }


    //ワープUIの非表示処理
    private void WarpUIView()
    {
        if (!warpUIView) return;

        if(warpUIView)
        {
            warpUIGameObject.SetActive(true);
            Vector3 camPos = Camera.main.transform.position;
            // UI の高さを固定（上下に傾かない）
            camPos.y = warpUIGameObject.transform.position.y;
            // カメラ方向を向く
            warpUIGameObject.transform.LookAt(camPos);
            // 必要なら反転（Sprite が逆向きなら）
            warpUIGameObject.transform.Rotate(0, 180, 0);
        }
        else
        {
            warpUIGameObject.SetActive(false);
        }
    }
}
