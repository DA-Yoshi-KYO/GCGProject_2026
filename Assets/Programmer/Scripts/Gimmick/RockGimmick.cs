/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    大岩ギミック
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    大瀧蓮
 * ----------------------------------------------------------
 * 2026-04-26 | 初回作成(大瀧)
 * 2026-05-08 | リファクタリング(大瀧)
 * 2026-06-18 | リファクタリング(大瀧)
 */

using UnityEngine;

public class RockGimmick : GimmickBase
{
    private bool isFirstActive = true;

    private float slopeAngleLimit;   //破壊判定がおこる斜面の角度限度値
    private float initPositionY;     //初期位置Y

    private Vector3 velocity = Vector3.zero;
    private GameObject checker;
    [Header("下方向へのレイの距離")]
    [SerializeField]
    private float rayDownLength;     //下方へのレイ
    [Header("前後左右へのレイの距離")]
    [SerializeField]
    private float raySideLength;     //前後左右へのレイ
    [Header("滑り係数")]
    [SerializeField]
    private float slideSpeed;        // 滑る強さ
    [Header("重力値")]
    [SerializeField]
    private float gravity;           // 重力
    [Header("平面の転がり速度")]
    [SerializeField]
    private float rollSpeed;         // 平面の転がり速度
    [Header("無視する段差の高さ")]
    [SerializeField]
    private float stepHeight = 0.2f;

    [Header("DangerZone")]
    [SerializeField, Tooltip("破壊時に生成する DangerZone")]
    private CS_DangerZone dangerZone;

    [SerializeField, Tooltip("泥棒のLayer。未設定なら全レイヤー")]
    private LayerMask thiefLayer;

    private bool isDangerZoneSpawned;

    //サウンドにストップができたら使う
    private float activeTimer = 0f;
    private bool soundPlayed = false;

    Vector3 startPos;

    bool isStart = false;
    private float debugIdleOffset = 0.0f;
    private float debugUpdateOffset = 0.4f;
    private HitChecker hitting;
    private Transform visualRoot;

    private bool isBrokenFirst = false;
    int cicleHit;
    protected override void IdleUpdate()
    {
        //！！デバッグ用応急処置！！//
        if(!isStart)
        {
            isStart = true;
            startPos = transform.position;
        }
        transform.position = new Vector3(startPos.x, startPos.y + gimmickSize.y * 0.5f, startPos.z);
    }

    protected override void ActiveUpdate()
    {
        //！！！ここら辺のサウンド周りは、ストップ関数ができたら使います！！！//

        //if (soundPlayed)
        //{
        //    activeTimer -= Time.deltaTime;
        //    if (activeTimer <= 0f)
        //    {
        //        soundPlayed = false;
        //    }
        //}
        //サウンドループ用
        //if(!soundPlayed)
        //{
        //    if (gimmickSound != null) gimmickSound.PlayOneShotSE("RockRolling", gameObject.transform.position, "RockSound");
        //    activeTimer = gimmickSound.GetAudioLength("RockRolling");
        //    soundPlayed = true;
        //}

        //=========================================
        // 初期化
        //=========================================
        if (isFirstActive)
        {
            isFirstActive = false;

            if (gimmickSound != null)
            {
                gimmickSound.PlayOneShotSE("Gimmick_RockRoll", transform.position, "RockRolling");
                activeTimer = gimmickSound.GetAudioLength("Gimmick_RockRoll");
            }

            velocity = Vector3.zero;

            SetHitChecker(transform.position);
        }

        //-----------------------------------------
        // 進行方向決定
        //-----------------------------------------

        Vector3 moveDir = Vector3.zero;
        Vector3 rotateAxis = Vector3.zero;
        float rotateSign = 1.0f;

        switch (gimmickDirection)
        {
            case GimmickDirection.Up:
                moveDir = Vector3.back;
                rotateAxis = Vector3.right;
                rotateSign = 1.0f;
                break;

            case GimmickDirection.Down:
                moveDir = Vector3.forward;
                rotateAxis = Vector3.right;
                rotateSign = -1.0f;
                break;

            case GimmickDirection.Left:
                moveDir = Vector3.right;
                rotateAxis = Vector3.forward;
                rotateSign = -1.0f;
                break;

            case GimmickDirection.Right:
                moveDir = Vector3.left;
                rotateAxis = Vector3.forward;
                rotateSign = 1.0f;
                break;
        }

        //-----------------------------------------
        // 地面判定
        //-----------------------------------------

        bool isRolling = true;
        float radius = gimmickSize.y * 0.5f;
        Vector3 rayOrigin =
            transform.position;

        RaycastHit hit;
        Debug.DrawRay(rayOrigin, Vector3.down, Color.yellow);
        bool isGround = false;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDownLength))
        {
            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform))
            {
                foreach (var h in Physics.RaycastAll(rayOrigin, Vector3.down, rayDownLength))
                {

                    if (h.collider.gameObject == gameObject) continue;
                    if (h.collider.transform.IsChildOf(transform)) continue;
                    isGround = true;
                    isRolling = true;
                    hit = h;
                    //地面とオブジェクト以外は無視
                    if (!h.collider.CompareTag("Plane") &&
                        !h.collider.CompareTag("Untagged"))
                    {
                        isRolling = false;
                    }
                    break;
                }
            }
            else
            {
                isGround = true;
                isRolling = true;
                //地面とオブジェクト以外は無視
                if (!hit.collider.CompareTag("Plane") &&
                    !hit.collider.CompareTag("Untagged"))
                {
                    isRolling = false;
                }
            }
        }

        //-----------------------------------------
        // 地面あり
        //-----------------------------------------
        if (isGround)
        {
            //------------------------------------------------
            // 接地位置へ吸着 ※異例物質無効処理
            //------------------------------------------------
            if (isRolling)
            {
                Vector3 pos = transform.position;
                pos.y = hit.point.y + radius;
                transform.position = pos;
            }
            //------------------------------------------------
            // 坂情報
            //------------------------------------------------

            float angle =
                Vector3.Angle(hit.normal, Vector3.up);

            Vector3 moveOnGround =
            Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;

            //------------------------------------------------
            // 基本速度
            //------------------------------------------------

            Vector3 moveVelocity =
                moveDir * rollSpeed;

            //------------------------------------------------
            // 合成
            //------------------------------------------------

            float speed =
                rollSpeed +
                Mathf.Sin(angle * Mathf.Deg2Rad) * slideSpeed;

            //逆に遅くなってたら元速に戻す
            if(speed <= rollSpeed)
                speed = rollSpeed;

            velocity =
                moveOnGround * speed;

            velocity *= Time.deltaTime;

            //------------------------------------------------
            // 移動
            //------------------------------------------------

            transform.position += velocity;

            //------------------------------------------------
            // 回転
            //------------------------------------------------

            float rotateAmount =
                velocity.magnitude *
                360.0f;

            transform.Rotate(
                rotateAxis,
                rotateAmount * rotateSign,
                Space.Self);
        }
        //-----------------------------------------
        // 落下
        //-----------------------------------------
        else
        {
            velocity.y -= gravity * Time.deltaTime;

            transform.position +=
                velocity * Time.deltaTime;
        }

        //-----------------------------------------
        // 壁判定
        //-----------------------------------------
        RaycastHit wallHit;

        //三本レイをサイクルで管理
        Vector3 rayPos = transform.position;
        if(cicleHit > 1)
            cicleHit = -1;

        switch (gimmickDirection)
        {
            case GimmickDirection.Up:
                rayPos = new Vector3(
                    transform.position.x + cicleHit * radius,
                    transform.position.y,
                    transform.position.z
                    );
                break;
            case GimmickDirection.Down:
                rayPos = new Vector3(
                    transform.position.x + cicleHit * radius,
                    transform.position.y,
                    transform.position.z
                    );
                break;
            case GimmickDirection.Left:
                rayPos = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    transform.position.z + cicleHit * radius
                    );
                break;
            case GimmickDirection.Right:
                rayPos = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    transform.position.z + cicleHit * radius
                    );
                break;
        }
        Debug.DrawRay(rayPos, moveDir, Color.yellow);
        if (Physics.Raycast(
            rayPos,
            moveDir,
            out wallHit,
            raySideLength))
        {
            if (wallHit.collider.gameObject != gameObject &&
                !wallHit.collider.transform.IsChildOf(transform))
            {
                if (HitBrokeAngle(wallHit, moveDir, 85f))
                {
                    if (wallHit.collider.CompareTag("Plane") ||
                        wallHit.collider.CompareTag("Untagged"))
                    {
                        gimmickState = GimmickState.Broken;
                    }
                }
            }
        }
        cicleHit++;
        //-----------------------------------------
        // ヒット判定
        //-----------------------------------------

        Hit();
    }


    // =========================
    // ヒット処理
    // =========================
    private void Hit()
    {
        SetHitChecker(transform.position);
    }
    // =========================
    // レイヒットオブジェクトの角度計算
    // =========================
    private bool HitBrokeAngle(RaycastHit hit, Vector3 rayDir, float breakAngle)
    {
        Vector3 normal = hit.normal;

        // Rayは「進行方向」なので反転させる
        float angle = Vector3.Angle(normal, -rayDir);

        return angle < breakAngle;
    }
    // =========================
    // 破壊処理
    // =========================
    protected override void BrokenUpdate()
    {
        base.BrokenUpdate();

        if (!isBrokenFirst)
        {
            isBrokenFirst = true;
            DeleteHitChecker();
            if (GetThiefGimmickAction() != null)
            {
                GetThiefGimmickAction().IronBallEnd(this);
            }
            //破壊時に1回だけ生成
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
                gimmickSound.PlayOneShotSE("Gimmick_RockHit", gameObject.transform.position, "RockSound");
                Destroy(gimmickSound);
            }
            if (checker != null)
                Destroy(checker);
        }
    }
}
