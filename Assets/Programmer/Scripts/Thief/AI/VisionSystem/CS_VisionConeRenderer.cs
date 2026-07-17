/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の視野角を床に可視化するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 * ----------------------------------------------------------
 * 2026-07-10 | 初回作成
 */
using UnityEngine;

// CS_VisionSensorが持つ視界距離・視野角・障害物レイヤーを使って、
// 床に投影する半透明の扇形メッシュをGame view上に描画する
[RequireComponent(typeof(CS_VisionSensor))]
public class CS_VisionConeRenderer : MonoBehaviour
{
    [Header("表示設定")]
    [Tooltip("視野角の可視化を表示するかどうか")]
    [SerializeField] private bool isVisible = true;
    [Tooltip("扇形の色（アルファ値が透明度）")]
    [SerializeField] private Color coneColor = new Color(1f, 0.9f, 0.2f, 0.35f);
    [Tooltip("扇形の分割数（多いほど滑らかになる）")]
    [SerializeField, Range(4, 64)] private int segments = 24;
    [Tooltip("床からの浮かせる高さ（Zファイティング防止）")]
    [SerializeField] private float floorOffset = 0.03f;
    [Tooltip("障害物判定に使うレイの足元からの高さ")]
    [SerializeField] private float rayHeight = 1.0f;

    private CS_VisionSensor visionSensor;
    private Transform thiefTransform;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private int floorRaycastMask;

    // 床のY座標が1フレームだけ検出できなかった場合に、Coneが一瞬他の高さへ飛ぶのを防ぐためのキャッシュ
    private float cachedFloorY;
    private bool hasCachedFloorY;

    // Physics.RaycastNonAllocの結果格納用（毎フレームのGCAllocを避けるため使い回す）
    private static readonly RaycastHit[] floorHitBuffer = new RaycastHit[16];

    CS_ThiefAI thiefAI;

    private void Awake()
    {
        visionSensor = GetComponent<CS_VisionSensor>();

        // 視野角の判定はCS_ThiefAIのtransformを基準にしているため、可視化も同じ基準に合わせる
        CS_ThiefAI thiefAI = GetComponentInParent<CS_ThiefAI>();
        thiefTransform = thiefAI != null ? thiefAI.transform : transform.parent;

        // 床の高さを調べる際、プレイヤーや泥棒本体など動くキャラクターの当たり判定を
        // 誤って床として拾わないように除外する（例：プレイヤーが泥棒の真上に乗った場合など）
        floorRaycastMask = ~LayerMask.GetMask(
        "Effect", "Player", "Thief", "CommandBlock_Player", "CommandBlock_Enmy", "Gimmick", "VisionObstacle", "OutLineModel", "VisionTarget");

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        mesh = new Mesh { name = "VisionConeMesh" };
        meshFilter.mesh = mesh;

        // Cull Offのため、メッシュの巻き順に関わらず上から見て表示される
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = coneColor;
        meshRenderer.sharedMaterial = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        thiefAI = transform.GetComponentInParent<CS_ThiefAI>();
    }

    private void LateUpdate()
    {
        if (meshRenderer.enabled != isVisible) meshRenderer.enabled = isVisible;
        if (!isVisible) return; 

        UpdateColor();
        BuildConeMesh();
    }

    /// <summary>
    /// メッシュの色を更新する
    /// </summary>
    private void UpdateColor()
    {
        if (thiefAI == null)
        {
            thiefAI = transform.GetComponentInParent<CS_ThiefAI>();
            Debug.Log("CS_ThiefAIが見つかりません。視野角の色を更新できません。");
            return;
        }

        if(thiefAI.read_MemorySystem.read_CurrentTarget == null)
        {
            meshRenderer.sharedMaterial.color = new Color(1f, 0.9f, 0.2f, 0.35f); // 黄色
        }
        else
        {
            if (thiefAI.read_MemorySystem.read_CurrentTarget is CS_PlayerTarget)
            {
                meshRenderer.sharedMaterial.color = new Color(1f, 0.2f, 0.2f, 0.35f); // 赤色
            }
            else
            {
                meshRenderer.sharedMaterial.color = new Color(1f, 0.9f, 0.2f, 0.35f); // 黄色
            }
        }
    }

    /// <summary>
    /// 視野角・視界距離・障害物に合わせた扇形メッシュを毎フレーム再構築する
    /// </summary>
    private void BuildConeMesh()
    {
        if (thiefTransform == null || visionSensor == null) return;

        float viewDistance = visionSensor.viewDistance;
        float viewAngle = visionSensor.viewAngle;
        LayerMask obstacleLayer = visionSensor.obstacleLayer;

        if (viewDistance <= 0f || viewAngle <= 0f) return;

        Vector3 origin = thiefTransform.position;

        // 泥棒が棚の上などにいる場合、transform.positionが必ずしも足元の床の高さとは限らないため、
        // 下方向にレイを飛ばして実際に立っている面の高さを求める
        float floorY = FindFloorY(origin);

        Vector3 rayOrigin = new Vector3(origin.x, floorY + rayHeight, origin.z);
        floorY += floorOffset;

        Vector3 forward = thiefTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        // MeshFilterの頂点はこのGameObject(Vision)のローカル座標系で解釈されるため、
        // ワールド座標で計算した点は必ずInverseTransformPointでローカル座標に変換してから格納する
        vertices[0] = transform.InverseTransformPoint(new Vector3(origin.x, floorY, origin.z));

        float startAngle = -viewAngle * 0.5f;
        float angleStep = viewAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward;

            float distance = viewDistance;
            if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, viewDistance, obstacleLayer))
            {
                distance = hit.distance;
            }

            Vector3 point = origin + dir * distance;
            Vector3 worldPoint = new Vector3(point.x, floorY, point.z);
            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);
        }

        for (int i = 0; i < segments; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    /// <summary>
    /// 泥棒の足元にある床面のY座標を求める
    /// </summary>
    /// <remarks>
    /// ギミック（壺など）はモデルの当たり判定が床と同じDefaultレイヤーに置かれているものがあり、
    /// floorRaycastMaskだけでは除外しきれない。そのため1回のRaycastで最初に当たった面を
    /// そのまま床とはせず、複数のヒットを近い順に調べてギミック自身のコライダーは読み飛ばし、
    /// 純粋な床（階段や2階部分も含む）だけを採用することで、ギミック召喚時にConeの高さが
    /// 上下にブレるのを防ぐ。
    /// </remarks>
    private float FindFloorY(Vector3 origin)
    {
        int hitCount = Physics.RaycastNonAlloc(
            origin + Vector3.up * 2f, Vector3.down, floorHitBuffer, 10f, floorRaycastMask);

        float nearestDistance = float.MaxValue;
        bool foundFloor = false;
        float floorY = hasCachedFloorY ? cachedFloorY : origin.y;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = floorHitBuffer[i];

            // ギミック本体に属するコライダー（Defaultレイヤーに置かれているモデル当たり判定など）は
            // 床ではないため無視する
            if (hit.collider.GetComponentInParent<GimmickBase>() != null) continue;

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                floorY = hit.point.y;
                foundFloor = true;
            }
        }

        if (foundFloor)
        {
            cachedFloorY = floorY;
            hasCachedFloorY = true;
        }

        return floorY;
    }

    /// <summary>
    /// 視野角の可視化の表示・非表示を切り替える
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;
    }
}
