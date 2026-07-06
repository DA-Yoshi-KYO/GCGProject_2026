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

    private float rotateSpeedOffset = 0.5f; //転がる速度(見た目)の調整用
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


        //-----------------------------------------
        // 進行方向決定
        //-----------------------------------------

        Vector3 moveDir = Vector3.zero;
        Vector3 rotateAxis = Vector3.zero;
        float rotateSign = 1.0f;

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
                rotateSign = 1.0f;
                break;

            case GimmickDirection.Right:
                moveDir = Vector3.left;
                rotateAxis = Vector3.forward;
                rotateSign = -1.0f;
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

                float normalY = Mathf.Max(hit.normal.y, 0.2f);
                pos.y = hit.point.y + hit.distance;

                transform.position = pos;
            }
            //------------------------------------------------
            // 坂情報
            //------------------------------------------------

            float angle =
                Vector3.Angle(hit.normal, Vector3.up);

            // 指定方向を坂面に沿わせた方向
            Vector3 moveOnGround =
                Vector3.ProjectOnPlane(moveDir, hit.normal);

            if (moveOnGround.sqrMagnitude > 0.0001f)
            {
                moveOnGround.Normalize();
            }
            else
            {
                moveOnGround = moveDir;
            }

            // 上方向へ進もうとしていたら反転する
            if (angle > 1.0f && moveOnGround.y > 0.01f)
            {
                ReverseGimmickDirection();

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
                        moveDir = Vector3.left;
                        rotateAxis = Vector3.forward;
                        rotateSign = 1.0f;
                        break;

                    case GimmickDirection.Right:
                        moveDir = Vector3.right;
                        rotateAxis = Vector3.forward;
                        rotateSign = -1.0f;
                        break;
                }

                moveOnGround =
                    Vector3.ProjectOnPlane(moveDir, hit.normal);

                if (moveOnGround.sqrMagnitude > 0.0001f)
                {
                    moveOnGround.Normalize();
                }
                else
                {
                    moveOnGround = moveDir;
                }
            }

            // 坂の方向 = Dirの方向
            Vector3 rollDir = moveOnGround;
            
            //------------------------------------------------
            // 速度計算
            //------------------------------------------------

            float slopePower =
                Mathf.Sin(angle * Mathf.Deg2Rad);

            float speed =
                rollSpeed + slopePower * slideSpeed;

            Vector3 frameMove =
                rollDir * speed * Time.deltaTime;

            //------------------------------------------------
            // 移動
            //------------------------------------------------

            transform.position += frameMove;

            //------------------------------------------------
            // 回転
            //------------------------------------------------

            float rotateAmount =
                frameMove.magnitude *
                360.0f;

            transform.Rotate(
                rotateAxis,
                rotateAmount * rotateSign * rotateSpeedOffset,
                Space.Self);


            //-----------------------------------------
            // 壁判定
            //-----------------------------------------
            RaycastHit wallHit;

            //三本レイをサイクルで管理
            Vector3 rayPos = transform.position;
            if (cicleHit > 1)
                cicleHit = -1;

            switch (gimmickDirection)
            {
                case GimmickDirection.Up:
                    rayPos = new Vector3(
                        transform.position.x
                            + cicleHit * radius * 0.75f,
                        transform.position.y,
                        transform.position.z
                        );
                    break;
                case GimmickDirection.Down:
                    rayPos = new Vector3(
                        transform.position.x
                            + cicleHit * radius * 0.75f,
                        transform.position.y,
                        transform.position.z
                        );
                    break;
                case GimmickDirection.Left:
                    rayPos = new Vector3(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z
                            + cicleHit * radius * 0.75f
                        );
                    break;
                case GimmickDirection.Right:
                    rayPos = new Vector3(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z
                            + cicleHit * radius * 0.75f
                        );
                    break;
            }
            Debug.DrawRay(rayPos, rollDir, Color.yellow);
            if (Physics.Raycast(
                rayPos,
                rollDir,
                out wallHit,
                raySideLength))
            {
                if (HitBrokeAngle(wallHit, rollDir, 85f))
                {
                    bool isHit = false;
                    //親オブジェクトの当たり判定のみを対象とする
                    foreach (var h in Physics.RaycastAll(rayPos, rollDir, raySideLength))
                    {
                        var root = h.collider.transform.root;

                        //自身に当たっていたら
                        if (root == transform.root)
                            continue;
                        //親が存在しなかったら

                        //タグ指定※playerやthiefに当たらないようにする
                        if (root.CompareTag("Plane") ||
                                 root.CompareTag("Untagged"))
                        {
                            Debug.Log(root.name);
                            wallHit = h;
                            isHit = true;
                            break;
                        }
                    }
                    if (isHit)
                    {
                        gimmickState = GimmickState.Broken;

                    }
                }
            }
            cicleHit++;
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
                gimmickSound.PlayOneShotSE("Gimmick_RockHitBreak", gameObject.transform.position, "RockSound");
                Destroy(gimmickSound);
            }
            if (checker != null)
                Destroy(checker);
        }
    }
    private void ReverseGimmickDirection()
    {
        switch (gimmickDirection)
        {
            case GimmickDirection.Up:
                gimmickDirection = GimmickDirection.Down;
                break;

            case GimmickDirection.Down:
                gimmickDirection = GimmickDirection.Up;
                break;

            case GimmickDirection.Left:
                gimmickDirection = GimmickDirection.Right;
                break;

            case GimmickDirection.Right:
                gimmickDirection = GimmickDirection.Left;
                break;
        }
    }
}
