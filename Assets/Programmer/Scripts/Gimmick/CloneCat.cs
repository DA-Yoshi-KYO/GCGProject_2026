using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CloneCat : GimmickBase
{
    [Header("効果時間")]
    [SerializeField] private float ActiveTime = 20.0f;//秒数
    [SerializeField] private GameObject CloneCatActor;

    [Header("Player透明度")]
    [SerializeField] private float IdleAlpha = 0.5f;
    [SerializeField] private float BrokenAlpha = 1.0f;

    GameObject player;
    GameObject cloneCatActor;
    CS_RoomPlayerPosition roomPlayerPosition;
    float activeTimer = 0.0f;
    bool bFirstBroken = true;
    private SkinnedMeshRenderer[] mesh;

    protected override void IdleUpdate()
    {
        base.IdleUpdate();
        gimmickState = GimmickState.Active;
        player = GameObject.Find("Player(Clone)");


        roomPlayerPosition = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();

        if (player == null) return;
        CS_PlayerMove move = player.GetComponent<CS_PlayerMove>();



        if (move == null) return;
        move.SetInvincibleFlag(true);

        CS_PlayerAction action = player.GetComponent<CS_PlayerAction>();
        if (action == null) return;
        action.SetViewPreview(false);
        action.SetSelectGimmickActive(false);


        cloneCatActor = GameObject.Find("CloneCatActor(Clone)");
        if (cloneCatActor != null)
        {
            player.transform.position = cloneCatActor.transform.position;
            ChengePosProcess();
        }

        ViewGimmickUI(false);

        SetPlayerAlpha(IdleAlpha);
    }

    protected override void ActiveUpdate()
    {
        // アクティブ状態の時間をカウント
        activeTimer += Time.deltaTime;
        if (activeTimer >= ActiveTime)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();

        if (!bFirstBroken) return;
        bFirstBroken = false;

        ViewGimmickUI(true);

        if (player != null)
        {
            cloneCatActor = GameObject.Find("CloneCatActor(Clone)");
            if (cloneCatActor == null)
            {
                cloneCatActor = Instantiate(CloneCatActor, this.transform.position, Quaternion.identity);
            }
            cloneCatActor.transform.position = player.transform.position;


            Vector3 WarpPosition = this.transform.position;
            WarpPosition.y = 5.0f;
            player.transform.position = WarpPosition;
            ChengePosProcess();

            CS_PlayerMove move = player.GetComponent<CS_PlayerMove>();
            if (move != null)
            {
                move.SetInvincibleFlag(false);
            }
            CS_PlayerAction action = player.GetComponent<CS_PlayerAction>();
            if (action != null)
            {
                action.SetViewPreview(true);
                action.SetSelectGimmickActive(true);
            }

            SetPlayerAlpha(BrokenAlpha);
        }
    }

    void ChengePosProcess()
    {
        roomPlayerPosition.RefreshPlayerRoomData();
        if (player != null)
        {
            CS_PlayerData playerdata = player.GetComponent<CS_PlayerData>();
            if (playerdata != null)
            {
                playerdata.ChangePlayerRoomData();
            }
            CS_PlayerCamera camera = player.GetComponent<CS_PlayerCamera>();
            if (camera != null)
            {
                camera.ChangeCamera();
            }

        }
    }

    void SetPlayerAlpha(float alpha)
    {
        if (player == null) return;

        if (mesh == null || mesh.Length == 0)
        {
            mesh = player.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        foreach (var renderer in mesh)
        {
            if (renderer == null) continue;

            Material[] mats = renderer.materials;
            foreach (var mat in mats)
            {
                if (mat == null || !mat.HasProperty("_Alpha")) continue;

                mat.SetFloat("_Alpha", alpha);
            }
        }
    }

    void ViewGimmickUI(bool bView)
    {
        GameObject Canvase = GameObject.Find("Canvases");
        if (Canvase == null)
        {
            Debug.Log("Canvase not found");
            return;
        }
        GameObject gimmickUI = GameObject.Find("GameUICavas");
        if (gimmickUI == null)
        {
            Debug.Log("gimmickUI not found");
            return;
        }
        GameObject gimmikF = gimmickUI.transform.Find("GimmickF").gameObject;
        if (gimmikF != null) gimmikF.SetActive(bView);
        GameObject ctMask1 = gimmickUI.transform.Find("CTMask1").gameObject;
        if (ctMask1 != null) ctMask1.SetActive(bView);
        GameObject ctMask2 = gimmickUI.transform.Find("CTMask2").gameObject;
        if (ctMask2 != null) ctMask2.SetActive(bView);
        GameObject gimmick = gimmickUI.transform.Find("Gimmick").gameObject;
        if (gimmick != null) gimmick.SetActive(bView);
        GameObject gimmickSelectUI = gimmickUI.transform.Find("GimmickSelectUI").gameObject;
        if (gimmickSelectUI != null) gimmickSelectUI.SetActive(bView);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gimmickState = GimmickState.Broken;
        }
    }
}
