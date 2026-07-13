using UnityEngine;

public class TeleportGimmick : GimmickBase
{
    private const float TeleportCooldown = 2.0f;
    private const float TeleportHeightOffset = 2.0f;

    // Teleport先のオブジェクト
    private GameObject destination;
    private GameObject player;
    private bool isCooldown = false;
    private bool isTeleporting = false;
    private CS_RoomPlayerPosition roomPlayerPosition;

    private static float sharedCooldown;

    protected override void SpawnUpdate()
    {
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

        if (isTeleporting) OnTeleport();

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
        base.BrokenUpdate();
        sharedCooldown = 0.0f;
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

    private void OnTeleport()
    {
        isTeleporting = false;

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

    /// <summary>
    /// テレポート可能な状態かどうかを判定する処理
    /// </summary>
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

    // Teleportに触れた場合の処理
    private void OnTriggerEnter(Collider other)
    {
        // Player以外のオブジェクトは処理を行わない
        if (!other.CompareTag("Player"))
        {
            Debug.Log("TeleportGimmick: Player以外のオブジェクトが触れました。");
            return;
        }
        isTeleporting = true;
    }
}
