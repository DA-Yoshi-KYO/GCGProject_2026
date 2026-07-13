using UnityEngine;

public class TeleportGimmick : GimmickBase
{
    private const float TeleportCooldown = 2.0f;
    private const float TeleportHeightOffset = 0.0f;
    private const float FadeOutDuration = 0.5f;
    private const float FadeInDuration = 0.5f;

    private enum TeleportPhase
    {
        None,
        FadingOut,
        FadingIn,
    }

    private enum CirclePhase
    {
        Idle,
        Rising,
        Falling,
    }

    [SerializeField] private CS_EffectWarpShaderOnly warpEffect;
    [SerializeField] private Color noActive;
    [SerializeField] private Color active;

    [SerializeField] GameObject magicCircle;
    [SerializeField] private float magicCircleRiseHeight = 2f;

    // Teleport先のオブジェクト
    private GameObject destination;
    private GameObject player;
    private CS_PlayerAction playerAction;

    private bool isCooldown = false;

    private TeleportPhase teleportPhase = TeleportPhase.None;
    private float phaseTimer = 0f;

    private CirclePhase circlePhase = CirclePhase.Idle;
    private float circleTimer = 0f;
    private Vector3 magicCircleBasePosition;
    private bool magicCircleBaseCaptured = false;

    private CS_RoomPlayerPosition roomPlayerPosition;
    private SkinnedMeshRenderer[] smesh;

    private GimmickSelectUI gimmickSelectUI;
    private static float sharedCooldown;

    private CustomInputAction customInputAction;

    protected override void SpawnUpdate()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.0f, gameObject.transform.position.z);
        customInputAction = new CustomInputAction();
        customInputAction.Enable();
        gimmickState = GimmickState.Idle;
    }

    protected override void IdleUpdate()
    {
        base.IdleUpdate();

        // 自分以外のTeleportGimmickを探す
        HandleDiscardInput();
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

        if (playerAction == null && player != null)
        {
            playerAction = player.GetComponent<CS_PlayerAction>();
        }

        if (teleportPhase != TeleportPhase.None)
        {
            UpdateTeleportAnimation();
        }

        UpdateMagicCircle();
        UpdateGimmickSelectUI();
        UpdateWarpEffectColor();;

        if (isCooldown)
        {
            sharedCooldown -= Time.deltaTime;
        }
    }

    protected override void ActiveUpdate()
    {
        gimmickState = GimmickState.Idle;
    }

    protected override void BrokenUpdate()
    {
        sharedCooldown = 0.0f;
        gimmickSelectUI.ResetUIActive = false;
        base.BrokenUpdate();
    }

    /// <summary>
    /// GimmickSelectUIの参照解決と、リセットUIの表示状態（移動先の有無）を毎フレーム同期する処理
    /// </summary>
    private void UpdateGimmickSelectUI()
    {
        if (gimmickSelectUI == null)
        {
            gimmickSelectUI = FindObjectOfType<GimmickSelectUI>();
        }

        if (gimmickSelectUI != null)
        {
            gimmickSelectUI.ResetUIActive = destination != null;
        }
    }

    /// <summary>
    /// 移動先が無い（1個のみ召喚）場合、またはクールタイム中は非アクティブ色にする
    /// </summary>
    private void UpdateWarpEffectColor()
    {
        bool isTeleportUsable = destination != null && sharedCooldown <= 0f;
        warpEffect.SetEffectColor(isTeleportUsable ? active : noActive);
    }

    /// <summary>
    /// このギミックを選択中に破棄入力が行われた場合、テレポートギミックを手放す処理
    /// </summary>
    private void HandleDiscardInput()
    {
        if (destination == null || playerAction == null || gimmickSelectUI == null) return;
        if (GetGimmickTag() != playerAction.GetSelectCurrentGimmickTag()) return;
        if (!customInputAction.Player.Interact.triggered) return;

        gimmickSelectUI.ResetUIActive = false;
        gimmickState = GimmickState.Broken;
    }

    private void BeginTeleport()
    {
        teleportPhase = TeleportPhase.FadingOut;
        phaseTimer = 0f;
        SetPlayerAlpha(1f);

        // 魔法陣に乗って上に運ばれていく演出を開始する
        CaptureMagicCircleBase();
        circlePhase = CirclePhase.Rising;
        circleTimer = 0f;
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


    private void UpdateMagicCircle()
    {
        if (circlePhase == CirclePhase.Idle || magicCircle == null) return;

        circleTimer += Time.unscaledDeltaTime;
        float duration = (circlePhase == CirclePhase.Rising) ? FadeOutDuration : FadeInDuration;
        float t = Mathf.Clamp01(circleTimer / duration);

        Vector3 topPosition = magicCircleBasePosition + Vector3.up * magicCircleRiseHeight;

        if (circlePhase == CirclePhase.Rising)
        {
            magicCircle.transform.localPosition = Vector3.Lerp(magicCircleBasePosition, topPosition, t);
            if (t >= 1f)
            {
                circlePhase = CirclePhase.Falling;
                circleTimer = 0f;
            }
        }
        else
        {
            magicCircle.transform.localPosition = Vector3.Lerp(topPosition, magicCircleBasePosition, t);
            if (t >= 1f) circlePhase = CirclePhase.Idle;
        }
    }

    /// <summary>
    /// 到着側の魔法陣を、上から降りてくる状態にする処理（テレポート実行元から呼び出される）
    /// </summary>
    public void BeginArrivalCircle()
    {
        CaptureMagicCircleBase();

        if (magicCircle != null)
        {
            magicCircle.transform.localPosition = magicCircleBasePosition + Vector3.up * magicCircleRiseHeight;
        }

        circlePhase = CirclePhase.Falling;
        circleTimer = 0f;
    }

    private void CaptureMagicCircleBase()
    {
        if (magicCircleBaseCaptured || magicCircle == null) return;
        magicCircleBasePosition = magicCircle.transform.localPosition;
        magicCircleBaseCaptured = true;
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

        // 到着先の魔法陣を、上から降りてくる演出に連動させる
        TeleportGimmick destinationGimmick = destination.GetComponent<TeleportGimmick>();
        if (destinationGimmick != null)
        {
            destinationGimmick.BeginArrivalCircle();
        }
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

        // テレポートできない状態であれば、演出（フェード）自体を始めない
        if (!CanTeleport(out _, out _)) return;

        BeginTeleport();
    }
}
