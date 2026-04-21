/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    部屋のグリッド生成、管理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-21 | 初回作成
 * 
 */
using UnityEngine;

public class RoomGrid : MonoBehaviour
{
    [Header("グリッドの分割数(X:横方向(X)、Y:奥方向(Z))")]
    [SerializeField] private Vector2Int gridDivision;

    public Vector2 gridSize { get; private set; }   // グリッド1マスの大きさ
    private Renderer rendererMaterial; // グリッドのマテリアル

    void Start()
    {
        // グリッド1マスの大きさを計算
        gridSize = new Vector2(transform.localScale.x / gridDivision.x, transform.localScale.z / gridDivision.x);
        rendererMaterial = GetComponent<Renderer>();
        if (rendererMaterial != null)
        {
            rendererMaterial.material.SetVector("_GridNum", new Vector4(gridDivision.x, gridDivision.y, 0, 0));
        }
        else        
        {
            Debug.LogWarning("RoomGrid: Material not found on the GameObject.");
        }
    }

    /// <summary>
    /// 引数のワールド座標から、グリッド上の座標を返す
    /// グリッドの範囲外の場合、-1を返す
    /// </summary>
    /// <param name="pos">座標</param>
    /// <returns>0 ~ グリッドの分割数-1で表されるグリッド位置(範囲外の場合-1)</returns>
    public Vector2Int GetGridFromPos(Vector3 pos)
    {
        // ワールド座標を床から見たローカル座標に変換
        Vector3 localPos = transform.InverseTransformPoint(pos);

        // 北西を原点(0.0f,0.0f)とした相対座標に変換
        Vector2 relativePos = new Vector2(localPos.x + 0.5f, 0.5f - localPos.z);

        // グリッドの分割数に基づいてグリッド位置を計算
        Vector2Int gridPos = new Vector2Int(
            Mathf.FloorToInt(relativePos.x / 1.0f * gridDivision.x),
            Mathf.FloorToInt(relativePos.y / 1.0f * gridDivision.y)
        );

        // グリッドの範囲外の場合は-1を返す
        if (gridPos.x < 0 || gridPos.x >= gridDivision.x) gridPos.x = -1;
        if (gridPos.y < 0 || gridPos.y >= gridDivision.y) gridPos.y = -1;
        
        return gridPos;
    }
}
