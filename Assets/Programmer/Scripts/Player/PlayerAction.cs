/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    プレイヤーアクション作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 *    秋野翔太
 * ----------------------------------------------------------
 * 2026-04-24 | 初回作成
 * 2026-04-27 | ソウル消費およびギミックの初期化の実装
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
        }
        else if(playerData.playerInput.Player.GimmickChangeLeft.triggered)
        {
            currentGimmickIndex--;

            if (currentGimmickIndex < 0)
                currentGimmickIndex = gimmickKind.Count - 1;
        }

        
        //モードの切り替え
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

    private void SettingAction()
    {
        //ギミックの生成位置
        Vector3 settingPos = Vector3.zero;
        settingPos = transform.position + 
            new Vector3(transform.forward.x * transform.localScale.x,
                    0.0f,
                    transform.forward.z * transform.localScale.z);

        //グリッドの位置を取得
        GameObject currentRoom = playerData.currentRoomData.GetPlayerRoomData();
        var roomGrid = currentRoom.transform.GetComponentInChildren<RoomGrid>();

        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);

        Vector3 gridPos = roomGrid.GetWorldPosFromGrid(grid);

        //生成
        GimmickBase gimmick = Instantiate(gimmickKind[currentGimmickIndex], gridPos, Quaternion.identity).GetComponent<GimmickBase>();

        //ソウルの消費
        currentSoul -= gimmick.requiredSoul;

        // ソウルが0未満になる場合は召喚を行わずソウル数を戻す
        if (currentSoul < 0)
        {
            currentSoul += gimmick.requiredSoul;
            Destroy(gimmick.gameObject);
        }

        //gimmickBase.roomGrid = roomGrid.csを設定 
        //gimmick.SetGimmickPos(grid);// 位置の設定
        //gimmick.AdjustScaleToGrid();// グリッドに合わせてサイズを調整


    }

    //ソウルの数を加算する関数
    public void AddSoul(int addnum)
    {
        currentSoul += addnum;
    }

    private void OnCollisionStay(Collision collision)
    {
        //接触している
        if (collision.gameObject.CompareTag("Gimmick"))
        {
            if (playerData.playerInput.Player.Interact.triggered)
            {
                //ギミックの情報を取得
                GimmickBase gimmick = collision.gameObject.GetComponent<GimmickBase>();
                if ((gimmick.gimmickState != GimmickState.Idle)) return;
                gimmick.ActivateGimmick();
            }
        }
    }
}
