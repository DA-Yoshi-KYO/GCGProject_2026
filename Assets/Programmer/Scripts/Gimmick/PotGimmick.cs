// == PotGimmick.cs ==
// 作者 :秋野翔太
// 更新 :2026/04/22 作成開始
//
// ポットギミック仕様
// ・Active状態のとき、命中範囲に当たり判定を
// ・当たり判定内に、敵がいた場合、攻撃力を与える
// ・Broken状態のとき、当たり判定を消す
// ・当たり判定は、ギミックの向きに応じて、
//   ギミックの前方に設置する
// ・当たり判定の大きさは、HitRangeX, HitRangeY

using UnityEngine;
using System.Collections;

public class PotGimmick : GimmickBase
{
    [Header("アクティブ時間")]
    [Tooltip("アクティブ状態の時間"), Min(0)]
    public float activeTime;

    [Header("重力値")]
    [Tooltip("主に落下時に使用"), Min(0)]
    [SerializeField]
    private float gravity;

    [Header("DangerZone")]
    [SerializeField, Tooltip("破壊時に生成する DangerZone")]
    private CS_DangerZone dangerZone;

    [SerializeField, Tooltip("泥棒のLayer。未設定なら全レイヤー")]
    private LayerMask thiefLayer;

    [Header("壺破壊VAT")]
    [SerializeField]
    private CS_EffectVAT cs_PotVatEffect;

    private bool isVatPlayed = false;

    private bool isFall = false;
    private float initPositionY;
    private float initOffsetY = 0.2f;
    private bool isFirstUpdate = true;
    private bool isFirstBroken = true;
    private bool isDangerZoneSpawned;
    private float downRayLength = 0.5f;

    private Vector3 activePos; // アクティブ状態のときの移動先座標
    private float activeMoveTime = 0.06f; // アクティブ初期状態までの移動時間
    private Coroutine moveCoroutine;
    private bool isMoving;

    protected override void IdleUpdate()
    {
    }

    protected override void SearchUpdate()
    {
        Debug.Log("PotGimmick: SearchUpdate called", this);
        base.SearchUpdate();
    }   

    protected override void ActiveUpdate()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Vector2Int directionVec = GetDirectionVec();
            Vector2Int hitCheckerGridPos = new Vector2Int(gimmickGridPos.x + directionVec.x, gimmickGridPos.y + directionVec.y);
            SetHitChecker(hitCheckerGridPos.x, hitCheckerGridPos.y);
            activePos = transform.position;
            Vector3 smokePosition = activePos;
            smokePosition.y += initOffsetY;

            //インタラクトされた方向に地面があるか確認
            //原点をギミックの方向にずらして、下方にレイを飛ばす
            //インタラクト時転がす
            float interactVecX = 0f;
            float interactVecZ = 0f;
            switch (gimmickDirection)
            {
                case GimmickDirection.Up:
                    interactVecZ = -roomGrid.gridSize.x;
                    break;
                case GimmickDirection.Down:
                    interactVecZ = roomGrid.gridSize.x;
                    break;
                case GimmickDirection.Left:
                    interactVecX = roomGrid.gridSize.x;
                    break;
                case GimmickDirection.Right:
                    interactVecX = -roomGrid.gridSize.x;
                    break;
            }
            //ちょっと上にずらす
            activePos += new Vector3(interactVecX, initOffsetY, interactVecZ);
            initPositionY = activePos.y;
            isFall = true;
            isMoving = true;
            PlayEffectPlayer(smokePosition);
            if (gimmickSound != null)
            {
                gimmickSound.PlayOneShotSE("Gimmick_PotFall", activePos, "PotSound");
            }
        }
        if (activePos != transform.position && isMoving)
        {
            MoveToPosition(activePos, activeMoveTime);
            return;
        }
        else
        {
            isMoving = false;
        }

        //落下
        transform.position -= new Vector3(0, gravity, 0);
        SetHitChecker(transform.position);

        Ray rayDown = new Ray(transform.position, Vector3.down);
        Debug.DrawRay(rayDown.origin, rayDown.direction * downRayLength, Color.red, 0.1f);
        if (Physics.Raycast(rayDown, out RaycastHit hit, downRayLength))
        {
            gimmickState = GimmickState.Broken;
        }

        //ゅかより下に行ったら破壊
        if (transform.position.y < 0.0f)
        {
            gimmickState = GimmickState.Broken;
        }

        //壊れなかったとき用の時間破壊
        activeTime -= Time.deltaTime;
        if (activeTime <= 0)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();

        if (isFirstBroken)
        {
            //破壊時に1回だけ生成
            PlayPotVatEffect();

            if (!isDangerZoneSpawned)
            {
                isDangerZoneSpawned = true;

                if (dangerZone != null)
                {
                    // ThiefCommonDBから残存時間を取得
                    CO_ThiefCommonStatusData common = null;
                    var thiefManager = GameObject.FindObjectOfType<CS_ThiefManager>();
                    if (thiefManager != null) common = thiefManager.GetThiefCommonDB();
                    CS_DangerZoneSpawner.SpawnAndRegisterFromGimmick(dangerZone, transform.position, this, common, thiefLayer);
                }
                else
                {
                    Debug.LogWarning("PotGimmick: dangerZone が未設定です。", this);
                }
            }
            if (gimmickSound != null)
            {
                gimmickSound.PlayOneShotSE("Gimmick_PotBreak", gameObject.transform.position, "PotSound");
            }
            isFirstBroken = false;
            DeleteHitChecker();
        }
    }

    private void PlayPotVatEffect()
    {
        if (isVatPlayed)
        {
            return;
        }

        isVatPlayed = true;

        if (cs_PotVatEffect == null)
        {
            cs_PotVatEffect = GetComponentInChildren<CS_EffectVAT>(true);
        }

        if (cs_PotVatEffect == null)
        {
            return;
        }

        CSST_EffectPlayData csst_EffectPlayData = new CSST_EffectPlayData();
        csst_EffectPlayData.CSST_EffectPlayData_Init();

        csst_EffectPlayData.SetPosition(cs_PotVatEffect.transform.position);
        csst_EffectPlayData.SetRotation(cs_PotVatEffect.transform.rotation);

        // VAT再生時はアウトラインをOFFにする
        CS_OutlineTarget[] outlineTargets =
            cs_PotVatEffect.GetComponentsInChildren<CS_OutlineTarget>(true);

        for (int i = 0 ; i < outlineTargets.Length ; i++)
        {
            if (outlineTargets[i] == null)
            {
                continue;
            }

            outlineTargets[i].enabled = false;
        }

        cs_PotVatEffect.PlayEffect(csst_EffectPlayData);
    }

    public void MoveToPosition(Vector3 targetPos, float moveTime)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveToPositionCoroutine(targetPos, moveTime));
    }

    private IEnumerator MoveToPositionCoroutine(Vector3 targetPos, float moveTime)
    {
        Vector3 startPos = transform.position;
        float timer = 0.0f;

        while (timer < moveTime)
        {
            timer += Time.deltaTime;

            float t = timer / moveTime;
            t = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        // 誤差防止で最後にぴったり合わせる
        transform.position = targetPos;

        moveCoroutine = null;
    }

    protected override bool GetGimmickSettingsArea()
    {
        //設置該当位置地点が１マス以上上でないと設置は不可能とする
        //また、斜面に設置される場合も不可能とする
        //１マス以上上でないといけないが
        //レイキャストを壺から左右四方向から下に飛ばして
        //どちらかの方向に落ちられる※設置位置から下にいける
        //部分がない場合も設置不可能とする
        if (roomGrid == null)
            return false;

        Vector2Int placementGrid = roomGrid.GetGridFromPos(placementCheckPosition);
        if (placementGrid.x < 0 || placementGrid.y < 0)
            return false;

        Vector3 roomFloorPosition = roomGrid.GetWorldPosFromGrid(placementGrid);
        if (float.IsInfinity(roomFloorPosition.x))
            return false;

        float oneGridHeight = roomGrid.gridSize.y;
        const float surfaceRayOffset = 0.1f;
        const float surfaceHeightTolerance = 0.05f;
        const float maxSlopeAngle = 5.0f;

        // 部屋の地面より高ければ設置可能とし、地面と同じ高さは設置不可とする
        bool isAboveRoomFloor =
            placementCheckPosition.y - roomFloorPosition.y > surfaceHeightTolerance;

        Vector3 surfaceRayOrigin =
            placementCheckPosition + Vector3.up * surfaceRayOffset;

        // 壺の直下にある設置面は、四方向の落下判定とは分けて確認する
        float surfaceRayLength = surfaceRayOffset + surfaceHeightTolerance;
        bool hasSurface = Physics.Raycast(
            surfaceRayOrigin,
            Vector3.down,
            out RaycastHit surfaceHit,
            surfaceRayLength,
            ~0,
            QueryTriggerInteraction.Ignore);

        bool isFlatSurface =
            hasSurface && Vector3.Angle(surfaceHit.normal, Vector3.up) <= maxSlopeAngle;
        Debug.DrawRay(
            surfaceRayOrigin,
            Vector3.down * surfaceRayLength,
            isAboveRoomFloor && isFlatSurface ? Color.green : Color.red,
            0.0f,
            false);

        if (!isAboveRoomFloor || !hasSurface)
        {
            return false;
        }

        if (!isFlatSurface)
            return false;

        // 支えている設置面を落下可能方向と誤判定しないよう、
        // 壺の接地範囲より少し外側からレイを飛ばす
        float outsideOffset = Mathf.Min(roomGrid.gridSize.x, roomGrid.gridSize.y) * 0.05f;
        float halfX = roomGrid.gridSize.x * gimmickSize.x + outsideOffset;
        float halfZ = roomGrid.gridSize.y * gimmickSize.y + outsideOffset;
        float dropRayLength = oneGridHeight + surfaceRayOffset + surfaceHeightTolerance;
        Vector3[] offsets =
        {
            new Vector3(halfX, 0.0f, 0.0f),
            new Vector3(-halfX, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, halfZ),
            new Vector3(0.0f, 0.0f, -halfZ),
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 dropRayOrigin = surfaceRayOrigin + offset;
            bool hasDropHit = Physics.Raycast(
                dropRayOrigin,
                Vector3.down,
                out RaycastHit dropHit,
                dropRayLength,
                ~0,
                QueryTriggerInteraction.Ignore);
            bool canDrop =
                !hasDropHit ||
                placementCheckPosition.y - dropHit.point.y >=
                oneGridHeight - surfaceHeightTolerance;

            Debug.DrawRay(
                dropRayOrigin,
                Vector3.down * dropRayLength,
                canDrop ? Color.green : Color.red,
                0.0f,
                false);

            if (canDrop)
            {
                return true;
            }
        }

        return false;
    }
}
