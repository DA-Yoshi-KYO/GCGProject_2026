using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Unity.VisualScripting;

public class CloneCat : GimmickBase
{
    [Header("効果時間")]
    [SerializeField] private float activeTime = 30.0f;
    [FormerlySerializedAs("CloneCatActor")]
    [SerializeField] private GameObject cloneCatActorPrefab;

    [Header("秒数")]
    [SerializeField] private float CoolTime = 10.0f;

    [Header("倍率加算")]
    [SerializeField] private float addition_multiplier = 0.2f;

    [Header("Player透明度")]
    [SerializeField] private float activeAlpha = 0.5f;

    [Header("Broken時ワープY座標")]
    [SerializeField] private float warpPositionY = 5.0f;

    [Header("インタラクト範囲倍率")]
    [SerializeField] private float interactRangeMultiplier = 0.5f;

    [Header("削除回避時間")]
    [SerializeField] private float noBrokenTime = 2.0f;

    GameObject player;
    GameObject cloneCatActor;
    CS_RoomPlayerPosition roomPlayerPosition;
    CS_PlayerMove playerMove;
    CS_PlayerAction playerAction;
    GimmickList gimmickList;
    SkinnedMeshRenderer[] smesh;
    GameObject gimmickUI;

    CS_PlayerAction.InteractSyllinder defaultMaxSyllinder;
    CS_PlayerAction.InteractSyllinder defaultMinSyllinder;

    CustomInputAction inputAction;

    float activeTimer = 0.0f;
    bool bFirstActive = true;
    bool bFirstBroken = true;



    protected override void IdleUpdate()
    {
        base.IdleUpdate();
        gimmickState = GimmickState.Active;

        player = GameObject.Find("Player(Clone)");
        GameObject roomManager = GameObject.Find("RoomManager");
        roomPlayerPosition = roomManager != null ? roomManager.GetComponent<CS_RoomPlayerPosition>() : null;
        inputAction = new CustomInputAction();
        inputAction.Enable();

        if (player == null) return;

        CachePlayerComponents();

        SetPlayerControl(false);
        defaultMaxSyllinder = playerAction.interactMax;
        defaultMinSyllinder = playerAction.interactMin;
        playerAction.interactMax = new CS_PlayerAction.InteractSyllinder { radius = defaultMaxSyllinder.radius * interactRangeMultiplier, height = defaultMaxSyllinder.height };
        playerAction.interactMin = new CS_PlayerAction.InteractSyllinder { radius = defaultMinSyllinder.radius * interactRangeMultiplier, height = defaultMinSyllinder.height };

        cloneCatActor = GameObject.Find("CloneCatActor(Clone)");
        if (cloneCatActor != null)
        {
            player.transform.position = cloneCatActor.transform.position;
            ChangePosProcess();
        }

        ViewGimmickUI(true);
        SetPlayerAlpha(activeAlpha);
    }

    protected override void ActiveUpdate()
    {
        if (inputAction.Player.InteractCancel.IsPressed())
        {
            gimmickState = GimmickState.Broken;
        }

        activeTimer += Time.deltaTime;
        if (activeTimer >= activeTime)
        {
            gimmickState = GimmickState.Broken;
        }

        noBrokenTime -= Time.deltaTime;

        if (bFirstActive && gimmickSound != null)
        {
            gimmickSound.PlayOneShotSE(
            "Gimmick_Clone",
            transform.position,
            "GimmickCloneSound"
            );
            bFirstActive = false;
        }
    }

    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();
        if (!bFirstBroken) return;
        bFirstBroken = false;

        float Multiplier = 1.0f;
        int Count = (int)(activeTimer / CoolTime);
        Multiplier += Count * addition_multiplier;
        float CT = activeTimer * Multiplier;

        List<GimmickList.ActiveGimmick> activeGimmicks = gimmickList.GetGimmickList();
        int index = 0;
        for (int i = 0; i < activeGimmicks.Count; i++)
        {
            if (activeGimmicks[i].gimmickType == Gimmick.CloneCat)
            {
                index = i;
                break;
            }
        }
        gimmickList.SetActiveGimmickCoolTime(index, CT);

        Debug.Log($"CloneCat CoolTime: {CT} Multiplier: {Multiplier} Count: {Count}");

        ViewGimmickUI(true);

        if (player == null) return;

        cloneCatActor = GameObject.Find("CloneCatActor(Clone)");
        playerAction.interactMax = defaultMaxSyllinder;
        playerAction.interactMin = defaultMinSyllinder;
        if (cloneCatActor == null)
        {
            cloneCatActor = Instantiate(cloneCatActorPrefab, transform.position, Quaternion.identity);
        }
        cloneCatActor.transform.position = player.transform.position;

        Vector3 warpPosition = transform.position;
        warpPosition.y = warpPositionY;
        player.transform.position = warpPosition;
        ChangePosProcess();

        SetPlayerControl(true);
        SetPlayerAlpha(1.0f);
    }



    void CachePlayerComponents()
    {
        playerMove = player.GetComponent<CS_PlayerMove>();
        playerAction = player.GetComponent<CS_PlayerAction>();
        gimmickList = player.GetComponent<GimmickList>();
        smesh = null;
    }

    void SetPlayerControl(bool enabled)
    {
        if (playerMove != null) playerMove.SetInvincibleFlag(!enabled);
        if (playerAction != null)
        {
            playerAction.SetViewPreview(enabled);
            playerAction.SetSelectGimmickActive(enabled);
        }
        if (gimmickList != null) gimmickList.SetIsSetting(enabled);
    }

    void ChangePosProcess()
    {
        roomPlayerPosition.RefreshPlayerRoomData();
        if (player == null) return;

        player.GetComponent<CS_PlayerData>()?.ChangePlayerRoomData();
        player.GetComponent<CS_PlayerCamera>()?.ChangeCamera();
    }

    void SetPlayerAlpha(float alpha)
    {
        if (player == null) return;

        if (smesh == null || smesh.Length == 0)
        {
            smesh = player.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        foreach (var renderer in smesh)
        {
            if (renderer == null) continue;
            foreach (var mat in renderer.materials)
            {
                if (mat == null || !mat.HasProperty("_Alpha")) continue;
                mat.SetFloat("_Alpha", alpha);
            }
        }
    }

    void ViewGimmickUI(bool bView)
    {
        if (gimmickUI == null)
        {
            if (GameObject.Find("Canvases") == null)
            {
                Debug.Log("Canvase not found");
                return;
            }
            gimmickUI = GameObject.Find("GameUICavas");
            if (gimmickUI == null)
            {
                Debug.Log("gimmickUI not found");
                return;
            }
        }

        gimmickUI.transform.Find("GimmickF")?.gameObject.SetActive(bView);
        gimmickUI.transform.Find("CTMask1")?.gameObject.SetActive(bView);
        gimmickUI.transform.Find("CTMask2")?.gameObject.SetActive(bView);
        gimmickUI.transform.Find("Gimmick")?.gameObject.SetActive(bView);
        gimmickUI.transform.Find("GimmickSelectUI")?.gameObject.SetActive(bView);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(noBrokenTime > 0.0f) return;
        if (other.CompareTag("Player"))
        {
            gimmickState = GimmickState.Broken;
        }
    }
}
