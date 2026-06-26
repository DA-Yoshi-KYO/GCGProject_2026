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

    private float slopeAngleLimit = 10f;    //破壊判定がおこる斜面の角度限度値
    private float initPositionY;  //初期位置Y

    private Vector3 velocity = Vector3.zero;
    private GameObject checker;
    [Header("下方向へのレイの距離")]
    [SerializeField]
    private float rayDownLength = 1.0f;  //下方へのレイ
    [Header("前後左右へのレイの距離")]
    [SerializeField]
    private float raySideLength = 0.6f;  //前後左右へのレイ
    [Header("滑り係数")]
    [SerializeField]
    private float slideSpeed = 1f;       // 滑る強さ
    [Header("重力値")]
    [SerializeField]
    private float gravity = 2f;          // 重力
    [Header("平面の転がり速度")]
    [SerializeField]
    private float rollSpeed = 0.6f;       // 平面の転がり速度

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
        // =========================
        // 初期化
        // =========================
        if (isFirstActive)
        {
            isFirstActive = false;
            Vector2Int directionVec = GetDirectionVec();

            if (gimmickSound != null)
            {
                gimmickSound.PlayOneShotSE("Gimmick_RockRoll", gameObject.transform.position, "RockRolling");
                activeTimer = gimmickSound.GetAudioLength("Gimmick_RockRoll");
            }
            soundPlayed = true;

            initPositionY = transform.position.y + debugIdleOffset;
            velocity = Vector3.zero;

            SetHitChecker(transform.position);
        }

        // =========================
        // 斜面滑り
        // =========================
        RaycastHit hit;
        RaycastHit check;

        // レイ起点を少し上にずらして自身コライダへの衝突を回避する
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Vector3 rayDirection = Vector3.zero;
        bool hasValidHit = false;   //ヒットが有効かどうか


        //！！！ここら辺のサウンド周りは、ストップ関数ができたら使います！！！//

        //if (soundPlayed)
        //{
        //    activeTimer -= Time.deltaTime;
        //    if (activeTimer <= 0f)
        //    {
        //        soundPlayed = false;
        //    }
        //}

        // 下方向へのレイキャスト　※落下時の判定用
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDownLength, ~0, QueryTriggerInteraction.Ignore))
        {
            //サウンドループ用
            //if(!soundPlayed)
            //{
            //    if (gimmickSound != null) gimmickSound.PlayOneShotSE("RockRolling", gameObject.transform.position, "RockSound");
            //    activeTimer = gimmickSound.GetAudioLength("RockRolling");
            //    soundPlayed = true;
            //}

            // 自身または子に当たっているなら RaycastAll で次のヒットを探す
            if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
            {
                var hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayDownLength);
                foreach (var h in hits)
                {
                    //例外用処理
                    if (h.collider == null) continue;
                    if (h.collider.gameObject == gameObject) continue;
                    if (h.collider.transform.IsChildOf(transform)) continue;
                    hit = h;
                    hasValidHit = true;
                    break;
                }
            }
            else
            {
                hasValidHit = true;
            }

            //---------------
            // 壁判定
            // XZ方向にレイを飛ばす
            // 大岩自体が大きいため前後左右レイを少し下に調整
            Vector3 rayXYOrigin = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            Debug.DrawRay(rayXYOrigin, rayDirection * raySideLength, Color.yellow);

            // レイが当たったかチェック
            if (Physics.Raycast(rayXYOrigin, rayDirection, out check, raySideLength))
            {
                // 自身や子オブジェクトに当たっていないか確認
                if (check.collider.gameObject != gameObject && !check.collider.transform.IsChildOf(transform))
                {
                    if (HitBrokeAngle(check, rayDirection, 85f))
                    {
                        if (check.collider.CompareTag("Plane") || check.collider.CompareTag("Untagged"))
                        {
                            gimmickState = GimmickState.Broken;
                        }
                    }
                }
            }
            Debug.Log("レイの長さ" + hit.distance);
            if (rayDownLength - 0.1f < hit.distance || true)
            {
                hasValidHit = false;
                Debug.Log("段差無視");
                switch (gimmickDirection)
                {
                    case GimmickDirection.Up:
                        rayDirection = Vector3.back;
                        velocity = Vector3.back * rollSpeed;
                        transform.Rotate(Vector3.right, rollSpeed * Time.deltaTime * 360f, Space.Self);
                        break;

                    case GimmickDirection.Down:
                        rayDirection = Vector3.forward;
                        velocity = Vector3.forward * rollSpeed;
                        transform.Rotate(Vector3.right, -rollSpeed * Time.deltaTime * 360f, Space.Self);
                        break;

                    case GimmickDirection.Left:
                        rayDirection = Vector3.right;
                        velocity = Vector3.right * rollSpeed;
                        transform.Rotate(Vector3.forward, -rollSpeed * Time.deltaTime * 360f, Space.Self);
                        break;

                    case GimmickDirection.Right:
                        rayDirection = Vector3.left;
                        velocity = Vector3.left * rollSpeed;
                        transform.Rotate(Vector3.forward, rollSpeed * Time.deltaTime * 360f, Space.Self);
                        break;
                }
                //ベロシティ移動
                transform.position += velocity * Time.deltaTime;

                //！！デバッグ用応急処置！！//
                transform.position = new Vector3(transform.position.x, transform.position.y + debugIdleOffset, transform.position.z);

            }
        }

        if (hasValidHit)
        {//地面接触時
            Vector3 normal = hit.normal;
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, normal);
            Vector3 pos = transform.position;

            //---------------
            // 地面判定
            float angle = Vector3.Angle(normal, Vector3.up);
            float speed = Mathf.Sin(angle * Mathf.Deg2Rad) * slideSpeed;
            if (angle < slopeAngleLimit && transform.position.y < initPositionY - 1f / 2f/*落下判定の距離*/)
            {
                //接地判定
                //接地(滑らない床)は破壊※一定以上落下している場合のみ
                gimmickState = GimmickState.Broken;
            }
            else
            {
                Debug.Log("滑り");
                // 滑り
                pos += slopeDir * speed * Time.deltaTime;

                // Yだけ補正
                // 斜面の角度から補正値を計算
                if (angle < 5f)
                {
                    Debug.Log("平面");
                    pos.y = hit.point.y + 0.5f;
                }
                else
                {
                    while (hasValidHit)
                    {
                        Debug.Log("斜面");
                        pos.y -= 0.01f;
                        if (pos.y <= hit.point.y)
                        {
                            hasValidHit = false;
                        }
                    }
                    initPositionY = transform.position.y;
                }
                transform.position = new Vector3(pos.x, pos.y + debugUpdateOffset, pos.z);
            }
        }
        else
        {
            // =========================
            // 落下
            // =========================
            velocity.y -= gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
        }
        Hit();  //ヒットチェック
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
        DeleteHitChecker();
        base.BrokenUpdate();
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
