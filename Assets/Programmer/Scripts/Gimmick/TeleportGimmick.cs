using System.Collections.Generic;
using UnityEngine;

public class TeleportGimmick : GimmickBase
{
    private const float TeleportCooldown = 2.0f;
    private const float TeleportHeightOffset = 2.0f;
    private const float FadeOutDuration = 0.5f;
    private const float FadeInDuration = 0.5f;

    private enum TeleportPhase
    {
        None,
        FadingOut,
        FadingIn,
    }

    // Teleport先のオブジェクト
    private GameObject destination;
    private GameObject player;

    private bool isCooldown = false;

    private TeleportPhase teleportPhase = TeleportPhase.None;
    private float phaseTimer = 0f;

    private CS_RoomPlayerPosition roomPlayerPosition;
    private SkinnedMeshRenderer[] smesh;

    private GimmickSelectUI gimmickSelectUI;
    private static float sharedCooldown;

    protected override void SpawnUpdate()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.0f, gameObject.transform.position.z);
        gimmickState = GimmickState.Idle;
    }

    protected override void IdleUpdate()
    {
        base.IdleUpdate();

        // 自分以外のTeleportGimmickを探す
        SearchOfDestination();

        if (roomPlayerPosition == null)
        {
            GameObject roomManager = GameObject.Find("RoomManager");
            if (roomManager != null)
            {
                roomPlayerPosition = roomManager.GetComponent<CS_RoomPlayerPosition>();
            }
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (teleportPhase != TeleportPhase.None)
        {
            UpdateTeleportAnimation();
        }

        if (gimmickSelectUI == null)
        {
            // シーンからGimmickSelectUIを取得
            gimmickSelectUI = FindObjectOfType<GimmickSelectUI>();
            if (gimmickSelectUI != null)
            {
                if(destination != null)
                {
                    gimmickSelectUI.ResetUIActive = true;
                }
                else
                {
                    gimmickSelectUI.ResetUIActive = false;
                }
            }
        }



        if (isCooldown)
        {
            sharedCooldown -= Time.deltaTime;
        }
    }

    protected override void ActiveUpdate()
    {
        base.ActiveUpdate();
        // インタラクトされた際に破棄
        gimmickState = GimmickState.Broken;
    }

    protected override void BrokenUpdate()
    {
        sharedCooldown = 0.0f;
        base.BrokenUpdate();
    }

    private void BeginTeleport()
    {
        teleportPhase = TeleportPhase.FadingOut;
        phaseTimer = 0f;
        SetPlayerAlpha(1f);
    }

    private void UpdateTeleportAnimation()
    {
        // TimeScaleを0にしている間も進行させるため、unscaledDeltaTimeを使用する
        phaseTimer += Time.unscaledDeltaTime;

        switch (teleportPhase)
        {
            case TeleportPhase.FadingOut:
                {
                    float t = Mathf.Clamp01(phaseTimer / FadeOutDuration);
                    SetPlayerAlpha(1f - t);

                    if (t >= 1f)
                    {
                        OnTeleport();
                        teleportPhase = TeleportPhase.FadingIn;
                        phaseTimer = 0f;
                    }
                    break;
                }
            case TeleportPhase.FadingIn:
                {
                    float t = Mathf.Clamp01(phaseTimer / FadeInDuration);
                    SetPlayerAlpha(t);

                    if (t >= 1f)
                    {
                        teleportPhase = TeleportPhase.None;
                    }
                    break;
                }
        }
    }

    private void OnTeleport()
    {
        if (!CanTeleport(out CS_PlayerData playerData, out CS_PlayerCamera playerCamera)) return;

        Vector3 pos = destination.transform.position;
        pos.y += TeleportHeightOffset;
        player.transform.position = pos;

        isCooldown = true;
        sharedCooldown = TeleportCooldown;

        playerData.ChangePlayerRoomData();
        roomPlayerPosition.RefreshPlayerRoomData();
        playerCamera.ChangeCamera();
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

    private bool CanTeleport(out CS_PlayerData playerData, out CS_PlayerCamera playerCamera)
    {
        playerData = null;
        playerCamera = null;

        if (player == null)
        {
            Debug.LogWarning("TeleportGimmick: Playerが見つかりません。");
            return false;
        }
        // 移動先がない場合は処理を行わない
        if (destination == null)
        {
            Debug.LogWarning("TeleportGimmick: 移動先が設定されていません。");
            return false;
        }
        // クールタイム中は処理を行わない
        if (sharedCooldown > 0)
        {
            Debug.Log("TeleportGimmick: クールタイム中のため、テレポートできません。");
            return false;
        }
        if (roomPlayerPosition == null)
        {
            Debug.LogWarning("TeleportGimmick: RoomPlayerPositionが見つかりません。");
            return false;
        }

        playerData = player.GetComponent<CS_PlayerData>();
        if (!playerData)
        {
            Debug.LogWarning("TeleportGimmick: PlayerDataが見つかりません。");
            return false;
        }

        playerCamera = player.GetComponent<CS_PlayerCamera>();
        if (!playerCamera)
        {
            Debug.LogWarning("TeleportGimmick: PlayerCameraが見つかりません。");
            return false;
        }

        return true;
    }

    private void SearchOfDestination()
    {
        TeleportGimmick[] teleportGimmicks = FindObjectsOfType<TeleportGimmick>();
        foreach (TeleportGimmick gimmick in teleportGimmicks)
        {
            if (gimmick != this)
            {
                destination = gimmick.gameObject;
                return;
            }
        }
        destination = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player以外のオブジェクトは処理を行わない
        if (!other.CompareTag("Player"))
        {
            Debug.Log("TeleportGimmick: Player以外のオブジェクトが触れました。");
            return;
        }

        // 既にテレポート演出中の場合は二重に開始しない
        if (teleportPhase != TeleportPhase.None) return;

        // テレポートできない状態であれば、演出（TimeScale停止・フェード）自体を始めない
        if (!CanTeleport(out _, out _)) return;

        BeginTeleport();
    }
}
