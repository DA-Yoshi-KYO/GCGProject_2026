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
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private int initSoul = 5;//初期のソウルの数
    [HideInInspector] public int currentSoul { private set; get; } = 0;//現在のソウルの数
    [HideInInspector] public int currentGimmickIndex { private set; get; } = 0;//現在選択しているギミック

    public List<GameObject> gimmickKind;//所持しているギミックの種類
    
    private PlayerData playerData;
    private GameObject currentRoom;
    GameObject interactObject = null;

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

        //デバッグ：ギミックの設置位置描画
        DebugDrawGimmickSet();
    }

    private void SettingAction()
    {
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
        if (currentRoom == null)
        {
            Debug.Log("この部屋にトラップは配置できません");
            return;    // 設置可能な部屋のみ設置する
        }

        var roomGrid = currentRoom.GetComponent<RoomGrid>();

        // グリッド配置
        if (!roomGrid.SetGimmickInGrid(CalculateGimmickSetPosition(), gimmick)) return;

        //ソウルの消費
        currentSoul -= gimmick.requiredSoul;
    }

    //ギミックの設置位置を補正する計算：大瀧
    private Vector3 CalculateGimmickSetPosition()
    {
        var roomGrid = currentRoom.GetComponent<RoomGrid>();
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

        // X成分とZ成分の絶対値を比較して、どちらを優先するか決める
        if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
        {
            // X軸
            if (forward.x >= 0f)
                settingPos += new Vector3(offsetX, 0f, 0f);
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
        float sizeX = gimmick.gimmickSizeX;
        float sizeY = gimmick.gimmickSizeY;

        float gridSizeX = gridSize.x;
        float gridSizeY = gridSize.y;

        if ((int)sizeX % 2 == 0)
            if (settingPos.x <= spawnPos.x)
                offsetX -= 1f * gridSizeX; // 左に1マス

        if ((int)sizeY % 2 == 0)
            if (settingPos.z <= spawnPos.z)
                offsetZ -= 1f * gridSizeY; // 奥に1マス

        spawnPos.x += offsetX;
        spawnPos.z += offsetZ;

        // 各種数値を取得
        grid = roomGrid.GetGridFromPos(settingPos);
        spawnPos = roomGrid.GetWorldPosFromGrid(grid);
        sizeX = gimmick.gimmickSizeX;
        sizeY = gimmick.gimmickSizeY;

        // グリッドサイズ
        gridSizeX = gridSize.x;
        gridSizeY = gridSize.y;

        // 半分オフセット
        offsetX = sizeX * 0.5f;
        offsetZ = sizeY * 0.5f;

        // 偶数サイズ補正（囲碁）
        if ((int)sizeX % 2 == 0)
        {
            if (settingPos.x <= spawnPos.x)
                offsetX -= 1f * gridSizeX;   // 左に1マス 
        }
        if ((int)sizeY % 2 == 0)
        {
            if (settingPos.z <= spawnPos.z)
                offsetZ -= 1f * gridSizeY;   // 下に1マス
        }
        // ワールド座標に変換
        offsetX *= gridSizeX;
        offsetZ *= gridSizeY;

        // 中心が grid に来るように補正
        spawnPos.x += offsetX - (gridSizeX * 0.5f); // 中心が grid に来るように補正
        spawnPos.z += offsetZ - (gridSizeY * 0.5f);

        return spawnPos;
    }

    //ギミックの設置予定位置の取得とデバッグ用ボックス描画
    private void DebugDrawGimmickSet()
    {
        // デバッグ用のギミック設置位置描画
        if (currentRoom == null) { Debug.Log("currentRoomError_DDGS"); return; }
        if (gimmickKind.Count == 0) { Debug.Log("gimmickKind.Count_DDGS"); return; }

        var roomGrid = currentRoom.GetComponent<RoomGrid>();
        GimmickBase gimmick = gimmickKind[currentGimmickIndex].GetComponent<GimmickBase>();

        // ギミックの設置位置を計算
        Vector3 spawnPos = CalculateGimmickSetPosition();
        if (spawnPos.magnitude == float.PositiveInfinity) return;

        // グリッドサイズ取得
        Vector3 gridSize = roomGrid.gridSize;

        // ギミックのサイズ取得
        float sizeX = gimmick.gimmickSizeX;
        float sizeY = gimmick.gimmickSizeY;

        // ===============================
        // LineRenderer生成
        GameObject lineObj = new GameObject("GimmickGridRenderer_Temp");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = 0.03f;
        line.useWorldSpace = true;

        // 色設定
        bool canPlace = true;
        line.startColor = canPlace ? Color.green : Color.red;
        line.endColor = canPlace ? Color.green : Color.red;

        Object.Destroy(lineObj, 0.02f);

        // ライン描画用のポイントを計算
        List<Vector3> points = new List<Vector3>();

        float width = sizeX * gridSize.x;
        float depth = sizeY * gridSize.y;

        float lineY = spawnPos.y + 0.1f;

        Vector3 p1 = new Vector3(spawnPos.x - width / 2f, lineY, spawnPos.z - depth / 2f);
        Vector3 p2 = new Vector3(spawnPos.x - width / 2f, lineY, spawnPos.z + depth / 2f);
        Vector3 p3 = new Vector3(spawnPos.x + width / 2f, lineY, spawnPos.z + depth / 2f);
        Vector3 p4 = new Vector3(spawnPos.x + width / 2f, lineY, spawnPos.z - depth / 2f);

        // 外枠
        points.Add(p1); points.Add(p2);
        points.Add(p2); points.Add(p3);
        points.Add(p3); points.Add(p4);
        points.Add(p4); points.Add(p1);

        // 中心
        points.Add(spawnPos);
        points.Add(spawnPos + Vector3.up * 1.0f);

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
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

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            currentRoom = collision.gameObject;
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
