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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

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
    [SerializeField] private InteractSyllinder interactMin = new InteractSyllinder { radius = 3f, height = 3f };//インタラクトの範囲の最小値
    [SerializeField] private InteractSyllinder interactMax = new InteractSyllinder { radius = 5f, height = 5f };//インタラクトの範囲の最大値
    [HideInInspector] public int currentSoul { private set; get; } = 0;//現在のソウルの数
    [HideInInspector] public int currentGimmickIndex { private set; get; } = 0;//現在選択しているギミック

    public List<GameObject> gimmickKind;//所持しているギミックの種類
    private CS_OutlineController outlineController; // プレイヤーのアウトラインを制御

    private CS_PlayerData playerData;
    float interactTime = 0.0f;
    Vector3 interactScale = Vector3.zero;
    bool isInteracting = false;

    Vector3 settingPos = Vector3.zero;  // 設置予定場所

    private GimmickManager gimmickManager;
    private CS_3DPlaySE playSE;
    List<Collider> hitList = new List<Collider>();

    // ギミック設置時のEffect再生クラスへの参照
    private CS_GimmickSetEffectPlayer cs_GimmickSetEffectPlayer;

    // インタラクト範囲Effect再生クラスへの参照
    private CS_PlayerInteractRangeEffectPlayer cs_PlayerInteractRangeEffectPlayer;

    private int saveGimmickIndex;// 現在のギミックインデックスを保存する変数

    private bool isShowGimmickPreview = false;
    private GimmickBase previewInstance;
    // Start is called before the first frame update
    void Start()
    {
        //現在のソウルの数
        playerData = GetComponent<CS_PlayerData>();

        gimmickManager = GetComponent<GimmickManager>();

        // インプットアクションの登録
        playerData.customInputAction.Player.GimmickChange.started += OnSelect;

        playerData.customInputAction.Player.Interact.started += OnInteract;
        playerData.customInputAction.Player.Interact.performed += OnInteract;
        playerData.customInputAction.Player.Interact.canceled += OnInteract;

        playerData.customInputAction.Player.InteractCancel.started += OnCancel;

        interactField.GetComponent<Renderer>().enabled = false;

        // アウトラインコントローラーの初期化
        outlineController = new CS_OutlineController(GetComponentInChildren<SkinnedMeshRenderer>());
        outlineController.SetOutlineColor(Color.gray);

        playSE = GameObject.Find("3DSE").GetComponent<CS_3DPlaySE>();

        cs_GimmickSetEffectPlayer = GetComponent<CS_GimmickSetEffectPlayer>();

        cs_PlayerInteractRangeEffectPlayer = GetComponent<CS_PlayerInteractRangeEffectPlayer>();

    }

    // Update is called once per frame
    void Update()
    {
        settingPos = CalculateGimmickSetPosition();

        if (playerData.currentMode == CS_PlayerData.PlayerMode.Normal)
        {
            outlineController.SetOutlineColor(Color.gray);
        }
        else
        {
            outlineController.SetOutlineColor(Color.yellow);
        }

        if (isInteracting)
        {
            interactTime += Time.deltaTime * interactSpeed;
            // インタラクト範囲の拡大
            if (interactTime >= switchInteract)
            {
                if (hitList != null)
                {
                    foreach (var item in hitList)
                    {
                        if (item == null) continue;
                        var renderers = item.GetComponentsInChildren<Renderer>();
                        foreach (var renderer in renderers)
                        {
                            if (renderer.materials.Length < 2) continue;

                            Material material = renderer.materials[1];
                            if (material != null) material.SetVector("_OutlineColor", Color.gray);
                        }
                    }
                }

                outlineController.SetOutlineColor(Color.green);

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
                    }

                    // アウトラインの色付け
                    var renderers = hits[i].GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        if (renderer.materials.Length < 2) continue;

                        Material material = renderer.materials[1];
                        if (material != null) material.SetVector("_OutlineColor", Color.green);
                    }

                    hitList.Add(hits[i]);
                }
            }

        }

        if (playerData.currentMode == CS_PlayerData.PlayerMode.Setting)
        {
            ShowGimmickPreview();
        }
        else
        {
            // プレビューオブジェクトの削除
            if (previewInstance != null)
            {
                Destroy(previewInstance.gameObject);
                previewInstance = null;
            }
            isShowGimmickPreview = false;
        }

#if UNITY_EDITOR
        //デバッグ：ギミックの設置位置描画
        //DebugDrawGimmickSet();
#endif
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        float contextValue = context.ReadValue<float>();

        //キー操作でUIのギミックの選択
        if (contextValue == 1) currentGimmickIndex++;
        else if (contextValue == -1) currentGimmickIndex--;
        currentGimmickIndex = (currentGimmickIndex % gimmickKind.Count + gimmickKind.Count) % gimmickKind.Count;
        Debug.Log("ギミックの数：" + gimmickKind.Count);
        Debug.Log("現在選択中のギミック：" + gimmickKind[currentGimmickIndex].name);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interact: " + context.phase);
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
                        SettingAction();
                        playSE.PlayOneShotSE("CatInteract", gameObject.transform.position, "InteractSE");
                        playerData.currentMode = CS_PlayerData.PlayerMode.Normal;
                        break;
                    default:
                        break;
                }
            }
            else
            {


                //! ここにギミック起動範囲のエフェクトを入れる。



                // 長押しはギミックの起動を行う
                interactField.GetComponent<Renderer>().enabled = false;
                playSE.PlayOneShotSE("CatInteract", gameObject.transform.position, "InteractSE");

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

                        //ギミックをアクティブにする
                        Debug.Log($"ギミック：" + hit.name + "がアクティブになりました");
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

    private void OnCancel(InputAction.CallbackContext context)
    {
        // キャンセル操作があった場合、現在のモードをノーマルに戻す
        playerData.currentMode = CS_PlayerData.PlayerMode.Normal;
    }

    private void SettingAction()
    {
        if (settingPos.magnitude == float.PositiveInfinity) return;

        if (gimmickKind[currentGimmickIndex] == null)
        {
            Debug.LogError("選択されたギミックが見つかりません");
            return;
        }
        GimmickBase gimmick = gimmickKind[currentGimmickIndex].GetComponent<GimmickBase>();
        if (gimmick == null)
        {
            Debug.LogError("選択されたギミックにGimmickBaseコンポーネントが付いていません"); return;
        }
        if (!gimmickManager.IsSetting(gimmick.gimmick))
        {
            Debug.Log("ギミックの設置失敗: IsSetting");
            return;
        }
        GameObject currentRoom = playerData.currentRoomData.GetPlayerRoomData().transform.GetChild(0).gameObject;
        string roomName = currentRoom.name;
        bool isNotSettingRoom = roomName.Contains("Treasure");
        Debug.Log(roomName);
        if (currentRoom == null || isNotSettingRoom)
        {
            Debug.Log("この部屋にトラップは配置できません");
            return;    // 設置可能な部屋のみ設置する
        }

        GameObject currentFloor = playerData.currentRoomData.GetPlayerFloorData();
        var roomGrid = currentFloor.GetComponent<RoomGrid>();

        // グリッド配置
        if (roomGrid == null)
            Debug.LogError("この部屋の床にRoomGridがついていません");

        Vector3 setPos = CalculateGimmickSetPosition();

        gimmick.gimmickState = GimmickState.Spawn;

        // 設置処理 //
        if (!roomGrid.SetGimmickInGrid(setPos, gimmick))
            return;

        // =========================
        // 実際に生成されたインスタンス取得
        // =========================
        Vector3 center = setPos;          // 中心位置
        Vector3 halfExtents = new Vector3(1f, 5f, 1f); // 半径ではなく「半サイズ」

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

        // ギミック直下に魔法陣Effectを生成して再生
        // ギミック設置時Effectを再生
        if (cs_GimmickSetEffectPlayer != null)
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
            Debug.Log("Up");
        }
        else if (-dz > adx + eps)
        {
            // プレイヤーがギミックの「後（-Z）側の三角形」：Down
            gimmick.SetGimmickDirection(GimmickDirection.Down);
            Debug.Log("Down");
        }
        else if (dx > adz + eps)
        {
            // プレイヤーがギミックの「右（+X）側の三角形」：Right
            gimmick.SetGimmickDirection(GimmickDirection.Right);
            Debug.Log("Right");
        }
        else if (-dx > adz + eps)
        {
            // プレイヤーがギミックの「左（-X）側の三角形」：Left
            gimmick.SetGimmickDirection(GimmickDirection.Left);
            Debug.Log("Left");
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
        GimmickBase gimmick = gimmickKind[currentGimmickIndex].GetComponent<GimmickBase>();

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
        if (spawnPos.magnitude == float.PositiveInfinity) return Vector3.positiveInfinity;

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

        settingPos = spawnPos;
        return spawnPos;
    }

    // ギミックのプレビュー表示
    private void ShowGimmickPreview()
    {
        if (gimmickKind.Count == 0)
            return;

        GameObject currentRoom =
            playerData.currentRoomData.GetPlayerFloorData();

        if (currentRoom == null)
            return;

        var roomGrid =
            currentRoom.GetComponent<RoomGrid>();

        if (roomGrid == null)
            return;

        if (gimmickKind[currentGimmickIndex] == null)
        {
            Debug.LogError("選択されたギミックが見つかりません");
            return;
        }
        GimmickBase gimmick = gimmickKind[currentGimmickIndex].GetComponent<GimmickBase>();

        gimmick.roomGrid = roomGrid;

        if (gimmick == null)
            return;
        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);
        settingPos = roomGrid.GetWorldPosFromGrid(grid);
        settingPos = CalculateGimmickSetPosition();
        settingPos = roomGrid.GimmickEvenNumberCorrection(settingPos, grid, gimmick);

        gimmick.SetGimmickPos(settingPos);
        gimmick.AdjustScaleToGrid();
        //----------------------------------
        // 初回のみ生成
        //----------------------------------
        if (!isShowGimmickPreview || saveGimmickIndex != currentGimmickIndex)
        {
            saveGimmickIndex = currentGimmickIndex;

            if(previewInstance != null)
            {
                Destroy(previewInstance.gameObject);
                previewInstance = null;
            }

            gimmick.gimmickState =
                GimmickState.Preview;

            if (!roomGrid.SetGimmickInGrid(
                    settingPos,
                    gimmick))
            {
                return;
            }

            isShowGimmickPreview = true;

            // =========================
            // 実際に生成されたインスタンス取得
            // =========================
            Vector3 center = new Vector3(settingPos.x, settingPos.y + 1.0f, settingPos.z);// 中心位置
            Vector3 halfExtents = new Vector3(1f, 5f, 1f); // 半径ではなく「半サイズ」

            Collider[] hits = Physics.OverlapBox(center, halfExtents);

            foreach (var hit in hits)
            {
                GimmickBase gimmickBase = hit.GetComponent<GimmickBase>();

                if (gimmickBase == null)
                    continue;

                // 同じ種類のみ
                if (gimmickBase.GetGimmickTag() != gimmick.GetGimmickTag())
                    continue;

                previewInstance = gimmickBase;
                break;
            }

            if (previewInstance == null)
            {
                Debug.LogError("配置後のGimmick取得失敗");
                return;
            }
        }
        //----------------------------------
        // 生成済なら移動だけ
        //----------------------------------
        else
        {
            if (previewInstance != null)
            {
                if (previewInstance.GetGimmickSize().y % 2 == 0)
                {
                    settingPos.y += previewInstance.GetGimmickSize().y * 0.5f;
                }

                previewInstance.transform.position =
                    settingPos;
                //プレイヤーとギミックとの位置でギミックの向きを設定
                SettingGimmickDirection(previewInstance);
            }
        }
    }

    //ソウルの数を加算する関数
    public void AddSoul(int addnum)
    {
        currentSoul += addnum;
    }
}
