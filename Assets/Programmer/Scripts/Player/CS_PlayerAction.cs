/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーアクション作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 *    秋野翔太
 *    大瀧蓮
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-04-27 | ソウル消費およびギミックの初期化の実装
 * 2026-05-06 | ギミックの設置位置をプレイヤーの前へ修正：大瀧
 * 2026-05-08 | リファクタリング(大瀧)
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerAction : MonoBehaviour
{
    [SerializeField] private int initSoul = 5;//初期のソウルの数
    [SerializeField] private float switchInteract = 1f;//ギミックの起動へ切り替る為に必要な長押しの時間
    [SerializeField] private GameObject interactField = null;//インタラクトの範囲を示すフィールド
    [SerializeField] private float interactSpeed = 1f;//インタラクトの速度(interactSpeed秒で範囲が1になる)
    [System.Serializable]
    public struct InteractSyllinder
    {
        public float radius;
        public float height;
    }
    [SerializeField] private InteractSyllinder interactMin = new InteractSyllinder { radius = 3f, height = 3f };//インタラクトの範囲の最小値
    [SerializeField] private InteractSyllinder interactMax = new InteractSyllinder { radius = 5f, height = 5f };//インタラクトの範囲の最大値
    [HideInInspector] public int currentSoul { private set; get; } = 0;//現在のソウルの数
    [HideInInspector] public int currentGimmickIndex { private set; get; } = 0;//現在選択しているギミック

    public List<GameObject> gimmickKind;//所持しているギミックの種類
    
    private PlayerData playerData;
    float interactTime = 0.0f;
    Vector3 interactScale = Vector3.zero;
    bool isInteracting = false;

    Vector3 settingPos = Vector3.zero;  // 設置予定場所

    // Start is called before the first frame update
    void Start()
    {
        //現在のソウルの数
        currentSoul = initSoul;
        playerData = GetComponent<PlayerData>();

        // インプットアクションの登録
        playerData.customInputAction.Player.GimmickChange.started += OnSelect;

        playerData.customInputAction.Player.Interact.started += OnInteract;
        playerData.customInputAction.Player.Interact.performed += OnInteract;
        playerData.customInputAction.Player.Interact.canceled += OnInteract;

        playerData.customInputAction.Player.InteractCancel.started += OnCancel;

        interactField.GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        settingPos = CalculateGimmickSetPosition();

        if (isInteracting)
        {
            interactTime += Time.deltaTime * interactSpeed;
            // インタラクト範囲の拡大
            if (interactTime >= switchInteract)
            {
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
            }
        }

#if UNITY_EDITOR
        //デバッグ：ギミックの設置位置描画
        DebugDrawGimmickSet();
        // ソウル数などのデバッグコマンド
        DebugCommand();
#endif
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        float contextValue = context.ReadValue<float>();

        //キー操作でUIのギミックの選択
        if (contextValue == 1) currentGimmickIndex++;
        else if (contextValue == -1) currentGimmickIndex--;
        currentGimmickIndex = (currentGimmickIndex % gimmickKind.Count + gimmickKind.Count) % gimmickKind.Count;

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
        }
        else if (context.canceled)
        {
            isInteracting = false;
            if (interactTime < switchInteract)
            {
                // 短押しは設置の処理を行う
                switch (playerData.currentMode)
                {
                    case PlayerData.PlayerMode.Normal:
                        playerData.currentMode = PlayerData.PlayerMode.Setting;
                        break;
                    case PlayerData.PlayerMode.Setting:
                        SettingAction();
                        playerData.currentMode = PlayerData.PlayerMode.Normal;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // 長押しはギミックの起動を行う
                interactField.GetComponent<Renderer>().enabled = false;
                Collider[] hits = Physics.OverlapSphere(
                    interactField.transform.position,
                    interactScale.x * 0.5f
                );
                foreach (Collider hit in hits)
                {
                    //ギミックの情報を取得
                    Vector3 pos = hit.transform.position;

                    float y = Mathf.Abs(pos.y - interactField.transform.position.y);

                    // 球と高さを使うことで疑似的に円柱の当たり判定にする
                    if (y > interactScale.y * 0.5f)
                    {
                        continue;
                    }

                    GimmickBase gimmick = hit.GetComponent<GimmickBase>();
                    if (gimmick == null) continue;
                    if (gimmick.gimmickState != GimmickState.Idle) continue;

                    // インタラクト方向を設定※ギミックとの位置関係で判定（対角線で四分割：三角形×4）
                    Vector3 gimmickPos = gimmick.transform.position;
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
                    //ギミックをアクティブにする
                    Debug.Log($"ギミック：" + hit.name + "がアクティブになりました");
                    gimmick.ActivateGimmick();
                }
            }
        }

    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        // キャンセル操作があった場合、現在のモードをノーマルに戻す
        playerData.currentMode = PlayerData.PlayerMode.Normal;
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
        if (currentSoul - gimmick.requiredSoul < 0)
        {
            Debug.Log("ソウルが不足しています");
            return;    // ソウルが足りない場合召喚しない
        }
        GameObject currentRoom = playerData.currentRoomData.GetPlayerRoomData().transform.GetChild(0).gameObject;
        string roomName = currentRoom.name;
        bool isNotSettingRoom = roomName.Contains("Start") || roomName.Contains("Treasure");
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
        {
            Debug.LogError("この部屋の床にRoomGridがついていません");
        }
        if (!roomGrid.SetGimmickInGrid(CalculateGimmickSetPosition(), gimmick)) return;

        //ソウルの消費
        currentSoul -= gimmick.requiredSoul;
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
        else if(gimmick.GetGimmickSize().y % 2 == 0 && !forwardZ)
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

    // ===== staticで保持※デバッグ用=====
    private LineRenderer line = null;
    private Vector3[] points = new Vector3[10]; // 最大数

    //ギミックの設置予定位置の取得とデバッグ用ボックス描画
    private void DebugDrawGimmickSet()
    {
        if (settingPos.magnitude == float.PositiveInfinity) return;
        GameObject currentRoom = playerData.currentRoomData.GetPlayerFloorData();

        // デバッグ用のギミック設置位置描画
        if (currentRoom == null) { Debug.Log("currentRoomError_DDGS"); return; }
        if (gimmickKind.Count == 0) { Debug.Log("gimmickKind.Count_DDGS"); return; }

        var roomGrid = currentRoom.GetComponent<RoomGrid>();
        GimmickBase gimmick = gimmickKind[currentGimmickIndex].GetComponent<GimmickBase>();

        // ギミックの設置位置を計算
        if (settingPos.magnitude == float.PositiveInfinity) return;

        // グリッドサイズ取得
        Vector3 gridSize = roomGrid.gridSize;

        // ギミックのサイズ取得
        float sizeX = gimmick.gimmickSizeX;
        float sizeY = gimmick.gimmickSizeY;

        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);
        settingPos = roomGrid.GimmickEvenNumberCorrection(settingPos, grid, gimmick);

        // ===============================
        // LineRenderer生成※初回のみ
        if (line == null)
        {
            GameObject lineObj = new GameObject("GimmickGridRenderer_Debug");
            line = lineObj.AddComponent<LineRenderer>();

            line.material = new Material(Shader.Find("Sprites/Default"));
            line.widthMultiplier = 0.03f;
            line.useWorldSpace = true;
        }

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = 0.03f;
        line.useWorldSpace = true;

        bool canPlace = true;

        line.startColor = Color.green;
        line.endColor = Color.green;
        // 色設定
        // 縦横サイズに合わせて判定を拡大
        for (int sX = 0 ; sX < sizeX ; sX++)
        {
            for (int sY = 0 ; sY < sizeY ; sY++)
            {
                Vector3 checkPos;
                //偶数だけX軸がズレるから一マス修正
                if(sizeX % 2 == 0)
                {
                    checkPos.x = settingPos.x +(sX * gridSize.x) - (gridSize.x * sizeX / 2f);
                }
                else
                {
                    checkPos.x = settingPos.x + (sX * gridSize.x);
                }
                checkPos.y = 0;
                checkPos.z = settingPos.z + (sY * gridSize.y);
                canPlace = !roomGrid.IsGridOnGimmick(roomGrid.GetGridFromPos(checkPos));
                // どこか一箇所でも置けない場所があれば赤色にする
                if (!canPlace)
                {
                    line.startColor = Color.red;
                    line.endColor = Color.red;
                }
            }
        }

        float width = sizeX * gridSize.x;
        float depth = sizeY * gridSize.y;
        float lineY = settingPos.y + 0.1f;

        Vector3 p1 = new Vector3(settingPos.x - width / 2f, lineY, settingPos.z - depth / 2f);
        Vector3 p2 = new Vector3(settingPos.x - width / 2f, lineY, settingPos.z + depth / 2f);
        Vector3 p3 = new Vector3(settingPos.x + width / 2f, lineY, settingPos.z + depth / 2f);
        Vector3 p4 = new Vector3(settingPos.x + width / 2f, lineY, settingPos.z - depth / 2f);

        // 配列入れ込み
        int i = 0;
        points[i++] = p1; points[i++] = p2;
        points[i++] = p2; points[i++] = p3;
        points[i++] = p3; points[i++] = p4;
        points[i++] = p4; points[i++] = p1;

        points[i++] = settingPos;
        points[i++] = settingPos + Vector3.up * 1.0f;

        // 反映
        line.positionCount = i;
        line.SetPositions(points);
    }

    //ソウルの数を加算する関数
    public void AddSoul(int addnum)
    {
        currentSoul += addnum;
    }

    void DebugCommand()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if ((Input.GetKey(KeyCode.Space)))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    currentSoul = initSoul;
                }
            }
        }
    }
}
