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

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private int initSoul = 5;//初期のソウルの数
    [HideInInspector] public int currentSoul { private set; get; } = 0;//現在のソウルの数
    [HideInInspector] public int currentGimmickIndex { private set; get; } = 0;//現在選択しているギミック

    public List<GameObject> gimmickKind;//所持しているギミックの種類
    
    private PlayerData playerData;
    GameObject interactObject = null;

    Vector3 settingPos = Vector3.zero;  // 設置予定場所

    // Start is called before the first frame update
    void Start()
    {
        //現在のソウルの数
        currentSoul = initSoul;
    }

    // Update is called once per frame
    void Update()
    {
        playerData = GetComponent<PlayerData>();
        settingPos = CalculateGimmickSetPosition();

        //キー操作でUIのギミックの選択
        if (playerData.playerInput.Player.GimmickChangeRight.triggered)
        {
            currentGimmickIndex++;

            if (currentGimmickIndex >= gimmickKind.Count)
                currentGimmickIndex = 0;

            Debug.Log("現在選択中のギミック：" + gimmickKind[currentGimmickIndex].name);
        }
        else if(playerData.playerInput.Player.GimmickChangeLeft.triggered)
        {
            currentGimmickIndex--;

            if (currentGimmickIndex < 0)
                currentGimmickIndex = gimmickKind.Count - 1;

            Debug.Log("現在選択中のギミック：" + gimmickKind[currentGimmickIndex].name);
        }


        //モードの切り替え
        if (interactObject == null)
        {
            if (playerData.playerInput.Player.Interact.triggered)
            {
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

            //設置モードのキャンセル
            if (playerData.playerInput.Player.InteractCancel.triggered)
            {
                playerData.currentMode = PlayerData.PlayerMode.Normal;
            }
        }
        else
        {
            if (playerData.playerInput.Player.Interact.triggered)
            {
                //ギミックの情報を取得
                GimmickBase gimmick = interactObject.GetComponent<GimmickBase>();
                if ((gimmick.gimmickState != GimmickState.Idle)) return;
                Debug.Log($"ギミック：" + interactObject.name + "がアクティブになりました");
                gimmick.ActivateGimmick();
            }
        }

#if UNITY_EDITOR
        //デバッグ：ギミックの設置位置描画
        DebugDrawGimmickSet();
        // ソウル数などのデバッグコマンド
        DebugCommand();
#endif
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

        Vector3 settingPos = transform.position;

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

        // プレイヤの前面にギミックを置くために行う補正
        // X成分とZ成分の絶対値を比較して、どちらを優先するか決める
        if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
        {
            // X軸
            if (forward.x >= 0f)
                settingPos += new Vector3(offsetX, 0f, 0f); //
            else
                settingPos -= new Vector3(offsetX, 0f, 0f);
        }
        else
        {
            // Z軸（同値なら Z を優先）
            if (forward.z >= 0f)
                settingPos += new Vector3(0f, 0f, offsetZ);
            else
                settingPos -= new Vector3(0f, 0f, offsetZ);
        }

        // ===============================
        // グリッド変換
        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);
        if (grid.x == -1 || grid.y == -1) return Vector3.positiveInfinity;

        Vector3 spawnPos = roomGrid.GetWorldPosFromGrid(grid);
        if (spawnPos.magnitude == float.PositiveInfinity) return Vector3.positiveInfinity;

        // ===============================
        // 偶数補正（SetGimmickInGridに合わせる）※囲碁型配置
        // 各種数値を取得
        spawnPos = roomGrid.GetWorldPosFromGrid(grid);

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

    private void OnTriggerStay(Collider other)
    {
        //接触している
        if (other.gameObject.CompareTag("Gimmick"))
        {
            GimmickBase gimmick = other.gameObject.GetComponent<GimmickBase>();
            if ((gimmick.gimmickState != GimmickState.Idle)) return;

            interactObject = other.gameObject;
        }
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

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Gimmick"))
        {
            interactObject = null;
        }
    }
}
