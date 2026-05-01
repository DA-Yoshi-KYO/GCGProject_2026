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
        if (currentSoul - gimmick.requiredSoul <= 0) return;    // ソウルが足りない場合召喚しない

        var roomGrid = playerData.currentRoomData.GetPlayerRoomData().transform.GetChild(0).Find("Floors").Find("Plane").GetComponent<RoomGrid>();

        //ギミックの生成位置
        Vector3 settingPos = Vector3.zero;
        settingPos = transform.position +
            new Vector3(
                transform.forward.x * roomGrid.gridSize.x,
                0.0f,
                transform.forward.z * roomGrid.gridSize.y);

        //グリッドの位置に召喚を試みる
        if (!roomGrid.SetGimmickInGrid(settingPos, gimmick)) return;    // 召喚に失敗

        //ソウルの消費
        currentSoul -= gimmick.requiredSoul;

        // ソウルが0未満になる場合は召喚を行わずソウル数を戻す
        if (currentSoul < 0)
        {
            currentSoul += gimmick.requiredSoul;
            Destroy(gimmick.gameObject);
        }
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

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Gimmick"))
        {
            interactObject = null;
        }
    }
}
