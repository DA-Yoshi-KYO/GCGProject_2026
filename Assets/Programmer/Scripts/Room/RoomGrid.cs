/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    部屋のグリッド生成、管理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-21 | 初回作成
 * 
 */
using UnityEngine;
using System.Collections.Generic;

public class RoomGrid : MonoBehaviour
{
    [Header("グリッドの分割数(X:横方向(X)、Y:奥方向(Z))")]
    [SerializeField] private Vector2Int gridDivision;

    public Vector2 gridSize { get; private set; }   // グリッド1マスの大きさ
    private Renderer rendererMaterial; // グリッドのマテリアル
    private GameObject gridObject; // グリッドの大きさを正確に取得する為の子オブジェクト

    List<List<GameObject>> gridGimmicks;

    void Start()
    {
        // グリッド1マスの大きさを計算
        gridObject = gameObject.transform.GetChild(0).gameObject;
        gridSize = new Vector2(gridObject.transform.lossyScale.x / gridDivision.x, gridObject.transform.lossyScale.z / gridDivision.y);
        rendererMaterial = GetComponent<Renderer>();
        if (rendererMaterial != null)
        {
            rendererMaterial.material.SetVector("_GridNum", new Vector4(gridDivision.x, gridDivision.y, 0, 0));
        }
        else        
        {
            Debug.LogWarning("RoomGrid: Material not found on the GameObject.");
        }

        gridGimmicks = new List<List<GameObject>>();
        for (int i = 0 ; i < gridDivision.y ; i++)
        {
            gridGimmicks.Add(new List<GameObject>());
            for (int j = 0 ; j < gridDivision.x ; j++)
            {
                gridGimmicks[i].Add(new GameObject());
            }
        }
    }

    /// <summary>
    /// グリッド位置にギミックが存在するかを取得する
    /// </summary>
    /// <param name="grid">確認するグリッド位置</param>
    /// <returns>true:存在する false:存在しない</returns>
    public bool IsGridOnGimmick(Vector2Int grid)
    {
        if (grid.x == -1 || grid.y == -1) return false;

        return gridGimmicks[grid.y][grid.x] != true;
    }

    /// <summary>
    /// 引数のワールド座標から変換されるグリッド位置にギミックを召喚する
    /// </summary>
    /// <param name="pos">座標</param>
    /// <param name="gimmick">召喚するgimmickのベースクラス</param>
    /// <returns>true:召喚成功 false:召喚失敗</returns>
    public bool SetGimmickInGrid(Vector3 pos, GimmickBase gimmick)
    {
        if (gimmick == null) return false;
        
        Vector2Int grid = GetGridFromPos(pos);
        if (grid.x == -1 || grid.y == -1) return false;
        if (IsGridOnGimmick(grid)) return false;

        Vector3 spawnPos = GetWorldPosFromGrid(grid);
        if (spawnPos == Vector3.positiveInfinity) return false;

        GameObject gimmickObject = Instantiate(gimmick.gameObject, spawnPos, Quaternion.identity);
        GimmickBase spawnGimmick = gimmickObject.GetComponent<GimmickBase>();
        spawnGimmick.roomGrid = this;
        // spawnGimmick.AdjustScaleToGrid();

        return true;
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
        Vector2 relativePos = new Vector2(
            localPos.x + gridObject.transform.lossyScale.x * 0.5f,
            gridObject.transform.lossyScale.z * 0.5f - localPos.z);
        
        // グリッドの分割数に基づいてグリッド位置を計算
        Vector2Int gridPos = new Vector2Int(
            Mathf.FloorToInt(relativePos.x),
            Mathf.FloorToInt(relativePos.y)
        );

        // グリッドの範囲外の場合は-1を返す
        if (gridPos.x < 0 || gridPos.x >= gridDivision.x) gridPos.x = -1;
        if (gridPos.y < 0 || gridPos.y >= gridDivision.y) gridPos.y = -1;
        
        return gridPos;
    }

    /// <summary>
    /// 引数のグリッド位置から、ワールド座標を返す
    /// グリッドの範囲外の場合、無限数を返す
    /// </summary>
    /// <param name="gridPos">グリッド番号</param>
    /// <returns>引数のグリッドが存在するワールド座標(範囲外の場合無限数)</returns>
    public Vector3 GetWorldPosFromGrid(Vector2Int gridPos)
    {
        // 範囲外チェック
        if (gridPos.x < 0 || gridPos.x >= gridDivision.x ||
            gridPos.y < 0 || gridPos.y >= gridDivision.y)
        {
            return Vector3.positiveInfinity;
        }

        // グリッドから相対座標に変換
        Vector2 relativePos = new Vector2(
            gridPos.x * gridSize.x - gridObject.transform.lossyScale.x * 0.5f + gridSize.x * 0.5f,
            gridObject.transform.lossyScale.z * 0.5f - gridPos.y * gridSize.y - gridSize.x * 0.5f
        );
        
        Debug.Log($"relativePos: {relativePos}");
        Vector3 localPos = new Vector3(relativePos.x, transform.position.y, relativePos.y);
        Vector3 worldPos = transform.TransformPoint(localPos);
        worldPos.y = transform.position.y;
        return worldPos;
    }
}
