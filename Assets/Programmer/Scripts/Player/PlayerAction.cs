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
using static PlayerAction;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private int initSoul = 5;//初期のソウルの数
    private int currentSoul;//現在のソウルの数

    [SerializeField] private  GameObject[] gimmickKind;//所持しているギミックの種類
    private List<KeyValuePair<GameObject, int>> gimmickList;//所持しているギミックの数

    private int currentGimmickIndex = 0;//現在選択しているギミック

    //プレイヤーのモード
    public enum PlayerMode
    {
        Normal,//通常
        Setting,//設置フェーズ
    };

    public PlayerMode currentMode { private set; get; }

    // Start is called before the first frame update
    void Start()
    {
        //現在のソウルの数
        currentSoul = initSoul;

        //現在のモード
        currentMode = PlayerMode.Normal;

        //ギミックごとに持っている数を初期化
        gimmickList = new List<KeyValuePair<GameObject, int>>();

        for (int i = 0 ; i < gimmickKind.Length ; ++i)
        {
            gimmickList.Add(new KeyValuePair<GameObject, int>(gimmickKind[i], 0));
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PlayerMove playerMove = GetComponent<PlayerMove>();

        //キー操作でUIのギミックの選択
        if (playerMove.playerInput.Player.GimmickChangeRight.triggered)
        {
            currentGimmickIndex++;

            if (currentGimmickIndex > gimmickList.Count)
                currentGimmickIndex = 0;
        }
        else if(playerMove.playerInput.Player.GimmickChangeLeft.triggered)
        {
            currentGimmickIndex--;

            if (currentGimmickIndex < 0)
                currentGimmickIndex = gimmickList.Count;
        }
        
        //モードの切り替え
        if(playerMove.playerInput.Player.Interact.triggered)
        {
            switch (currentMode)
            {
                case PlayerMode.Normal:
                    currentMode = PlayerMode.Setting;
                    break;
                case PlayerMode.Setting:
                    SettingAction();
                    currentMode = PlayerMode.Normal;
                    break;
                default:
                    break;
            }
        }

        //設置モードのキャンセル
        if (playerMove.playerInput.Player.InteractCancel.triggered)
        {
            currentMode = PlayerMode.Normal;
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
        var roomGrid = FindObjectOfType<RoomGrid>();

        Vector2Int grid = roomGrid.GetGridFromPos(settingPos);

        Vector3 gridPos = roomGrid.GetWorldPosFromGrid(grid);

        //生成
        GimmickBase gimmick = Instantiate(gimmickList[currentGimmickIndex].Key, gridPos, Quaternion.identity).GetComponent<GimmickBase>();

        //ソウルの消費
        currentSoul -= gimmick.requiredSoul;

        // ソウルが0未満になったらギミックを破壊
        if (currentSoul < 0)
        {
            currentSoul = 0;
            Destroy(gimmick.gameObject);
        }

        //gimmickBase.roomGrid = roomGrid.csを設定 
        gimmick.SetGimmickPos(grid);// 位置の設定
        gimmick.AdjustScaleToGrid();// グリッドに合わせてサイズを調整


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
            PlayerMove playerMove = GetComponent<PlayerMove>();
            if (playerMove.playerInput.Player.Interact.triggered)
            {
                //ギミックの情報を取得
                GimmickBase gimmick = collision.gameObject.GetComponent<GimmickBase>();
                if ((gimmick.gimmickState != GimmickState.Idle)) return;
                gimmick.ActivateGimmick();
            }
        }
    }
}
