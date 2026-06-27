using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneCat : GimmickBase
{
    [SerializeField] private float ActiveTime = 20.0f;//秒数

    GameObject player;
    CS_RoomPlayerPosition roomPlayerPosition;
    float activeTimer = 0.0f;
    bool bFirstBroken = true;

    protected override void IdleUpdate()
    {
        base.IdleUpdate();
        gimmickState = GimmickState.Active;
        player = GameObject.Find("Player(Clone)");
        if(player != null)
        {
            Debug.Log("CloneCat: Player found");
        }
        roomPlayerPosition = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();

    }

    protected override void ActiveUpdate()
    {
        base.ActiveUpdate();
        if (player == null) return;
        CS_PlayerMove move = player.GetComponent<CS_PlayerMove>();
        if (move == null) return;
        move.SetInvincibleFlag(true);
        CS_PlayerAction action = player.GetComponent<CS_PlayerAction>();
        if (action == null) return;
        action.SetViewPreview(false);



        // アクティブ状態の時間をカウント
        activeTimer += Time.deltaTime;
        if(activeTimer >= ActiveTime)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();

        if (!bFirstBroken) return;
        bFirstBroken = false;
        if (player != null)
        {
            Vector3 WarpPosition = this.transform.position;
            WarpPosition.y = 5.0f;
            player.transform.position = WarpPosition;
            roomPlayerPosition.RefreshPlayerRoomData();
            CS_PlayerData playerdata = player.GetComponent<CS_PlayerData>();
            playerdata.ChangePlayerRoomData();

            CS_PlayerMove move = player.GetComponent<CS_PlayerMove>();
            if (move != null)
            {
                move.SetInvincibleFlag(false);
            }
            CS_PlayerAction action = player.GetComponent<CS_PlayerAction>();
            if (action != null)
            {
                action.SetViewPreview(true);
            }
            CS_PlayerCamera camera = player.GetComponent<CS_PlayerCamera>();
            if (camera == null) return;
            camera.ChangeCamera();
        }
    }

}
