/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    大岩ギミック
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    大瀧蓮
 * ----------------------------------------------------------
 * 2026-04-26 | 初回作成(大瀧)
 * 2026-05-08 | リファクタリング(大瀧)
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
    private float rayDownLength = 1.2f;  //下方へのレイ
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
    [SerializeField, Tooltip("破壊時に生成する DangerZone prefab")]
    private DangerZone dangerZonePrefab;

    [SerializeField, Tooltip("泥棒のLayer。未設定なら全レイヤー")]
    private LayerMask thiefLayer;

    private bool isDangerZoneSpawned;

    //デバッグ用！！！！
    Vector3 startPos;
    bool isStart = false;
    private float debugIdleOffset = 0.9f;
    private float debugUpdateOffset = 0.4f;

    protected override void IdleUpdate()
    {
        //！！デバッグ用応急処置！！//
        if(!isStart)
        {
            isStart = true;
            startPos = transform.position;
        }
        transform.position = new Vector3(startPos.x, startPos.y + debugIdleOffset, startPos.z);
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

            initPositionY = transform.position.y + debugIdleOffset;
            velocity = Vector3.zero;

            CS_PlayerAction playerAction = GameObject.FindObjectOfType<CS_PlayerAction>();
            playerAction.SettingGimmickDirection(this);
        }

        // =========================
        // 斜面滑り
        // =========================
        RaycastHit hit;
        RaycastHit check;

        // レイ起点を少し上にずらして自身コライダへの衝突を回避する
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        bool hasValidHit = false;   //ヒットが有効かどうか

        // 下方向へのレイキャスト　※落下時の判定用
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDownLength, ~0, QueryTriggerInteraction.Ignore))
        {
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

            //インタラクト時転がす
            if (gimmickDirection == GimmickDirection.Up)
            {//Z+
                velocity = Vector3.back * rollSpeed;
            }
            else if (gimmickDirection == GimmickDirection.Down)
            {//Z-
                velocity = Vector3.forward * rollSpeed;
            }
            else if (gimmickDirection == GimmickDirection.Left)
            {//X-
                velocity = Vector3.right * rollSpeed;
            }
            else if (gimmickDirection == GimmickDirection.Right)
            {//X+
                velocity = Vector3.left * rollSpeed;
            }
            if (hit.collider.CompareTag("Plane") || hit.collider.CompareTag("Untagged"))
            {
                transform.position += velocity * Time.deltaTime;
                //！！デバッグ用応急処置！！//
                transform.position = new Vector3(transform.position.x, transform.position.y + debugIdleOffset, transform.position.z);
            }
            //---------------
            // 壁判定
            // XZ方向にレイを飛ばす
            // 大岩自体が大きいため前後左右レイを少し下に調整
            Vector3 rayXYOrigin = new Vector3(transform.position.x, transform.position.y - 1.3f, transform.position.z);
            // レイデバッグ
            Debug.DrawRay(rayXYOrigin, velocity * raySideLength, Color.yellow);
            //Debug.Log(rayXYOrigin);
            //レイ判定
            if (Physics.Raycast(rayXYOrigin, velocity, out check, raySideLength))
            {//レイが当たったら角度をチェック
                if (HitBrokeAngle(check, velocity, slopeAngleLimit))
                {//当たった面が一定値以上の斜面なら
                    if (hit.collider.CompareTag("Plane") || hit.collider.CompareTag("Untagged"))
                    {
                        gimmickState = GimmickState.Broken;
                    }
                }
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
            if (angle < slopeAngleLimit && transform.position.y < initPositionY - 0.1f/*落下判定の距離*/)
            {
                //接地判定
                //接地(滑らない床)は破壊※一定以上落下している場合のみ
                gimmickState = GimmickState.Broken;
            }
            else if (hit.collider.CompareTag("Plane") || hit.collider.CompareTag("Untagged"))
            {
                // 滑り
                pos += slopeDir * speed * Time.deltaTime;

                // Yだけ補正
                // 斜面の角度から補正値を計算
                float angleCorrection;
                angleCorrection = gravity / 3.141592f + angle / (3.141592f * 2f);
                pos.y = hit.point.y + 0.4f;
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

        //破壊時に1回だけ生成
        if (!isDangerZoneSpawned)
        {
            isDangerZoneSpawned = true;

            if (dangerZonePrefab != null)
            {
                CSS_ThiefCommonStatusData common = null;
                var thiefManager = GameObject.FindObjectOfType<ThiefManager>();
                if (thiefManager != null) common = thiefManager.GetThiefCommonDB();

                DangerZoneSpawner.SpawnAndRegisterFromGimmick(dangerZonePrefab, transform.position, this, common, thiefLayer);
            }
            else
            {
                Debug.LogWarning("RockGimmick: dangerZonePrefab が未設定です。", this);
            }
        }

        if (checker != null)
            Destroy(checker);

        Destroy(gameObject);
    }
}
