/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    部屋のグリッド生成、管理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 *    大瀧蓮
 * ----------------------------------------------------------
 * 2026-04-21 | 初回作成
 * 2026-05-06 | 偶数サイズギミックにおけるグリッド位置の補正を追加：大瀧
 * 2026-05-08 | リファクタリング(大瀧)
 * 
 */
using EffekseerTool.Data.Value;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomGrid : MonoBehaviour
{
    [SerializeField] private Vector2Int gridDivision;
    public Vector2 gridSize { get; private set; } = new Vector2(1, 1);   // グリッド1マスの大きさ
    List<List<GameObject>> gridGimmicks;
    GameObject[,] gridObjects;
    public enum GridOrigin
    {
        [Header("北西")] NorthWest,  // 北西
        [Header("北東")] NorthEast,  // 北東
        [Header("南西")] SouthWest,  // 南西
        [Header("南東")] SouthEast,  // 南東
    }
    [Header("グリッドの原点(ここを[0,0]とし、溢れ判定を行う)")][SerializeField] private GridOrigin gridOrigin = GridOrigin.NorthWest;
    [Header("床に使用するマテリアルの候補")][SerializeField] private Material[] floorMaterials;
    Vector3 gridCenter = Vector3.zero;

    void Start()
    {
        // グリッド上のギミック情報を初期化
        gridGimmicks = new List<List<GameObject>>();
        for (int i = 0 ; i < gridDivision.y ; i++)  // グリッド[y][x]として保存
        {
            gridGimmicks.Add(new List<GameObject>());
            for (int j = 0 ; j < gridDivision.x ; j++)
            {
                gridGimmicks[i].Add(null);
            }
        }
        List<GameObject> floors = new List<GameObject>();

        int floorCount = 0;
        Vector3 sum = Vector3.zero;
        foreach (Transform child in transform)
        {
            if (!child.gameObject.name.Contains("Floor")) continue;
            floors.Add(child.gameObject);

            // 床のマテリアルをランダムに変更する
            if (floorMaterials.Length > 0)
            {
                Material randomMaterial = floorMaterials[UnityEngine.Random.Range(0, floorMaterials.Length)];
                child.GetComponent<Renderer>().material = randomMaterial;
            }

            sum += child.transform.position;
            floorCount++;
        }
        gridCenter = sum / floorCount;

        // グリッドから溢れているかチェックする
        const int gridCellNumX = 3;
        const int gridCellNumY = 3;
        if (gridDivision.x % gridCellNumX == 0 && gridDivision.y % gridCellNumY == 0) return;

        // グリッドから溢れたマスをカリングする
        Vector2Int gridObjectLength = new Vector2Int(Mathf.CeilToInt((float)gridDivision.x / (float)gridCellNumX), Mathf.CeilToInt((float)gridDivision.y / (float)gridCellNumY));   // マス目は1グリッドあたり3つ;
        gridObjects = new GameObject[gridObjectLength.y, gridObjectLength.x];
        int overflowX = Mathf.CeilToInt((float)gridDivision.x / gridCellNumX) * gridCellNumX - gridDivision.x;
        int overflowZ = Mathf.CeilToInt((float)gridDivision.y / gridCellNumY) * gridCellNumY - gridDivision.y;

        switch (gridOrigin)
        {
            case GridOrigin.NorthWest:
                gridCenter += new Vector3(-overflowX * 0.5f, 0, overflowZ * 0.5f);
                break;
            case GridOrigin.NorthEast:
                gridCenter += new Vector3(overflowX * 0.5f, 0, overflowZ * 0.5f);
                break;
            case GridOrigin.SouthWest:
                gridCenter += new Vector3(-overflowX * 0.5f, 0, -overflowZ * 0.5f);
                break;
            case GridOrigin.SouthEast:
                gridCenter += new Vector3(overflowX * 0.5f, 0, -overflowZ * 0.5f);
                break;
        }

        // Floorの座標から3*3グリッドに変換
        foreach (GameObject child in floors)
        {
            Vector3 childPos = child.transform.position; // 床から見た相対座標

            // 左右前後の座標を計算
            float left = gridCenter.x - gridDivision.x / 2.0f;
            float right = gridCenter.x + gridDivision.x / 2.0f; 
            float top = gridCenter.z + gridDivision.y / 2.0f;
            float bottom = gridCenter.z - gridDivision.y / 2.0f;

            // グリッド座標(float)
            float fX = 0.0f;
            float fZ = 0.0f;

            // 基準点を元にグリッド座標に変換
            switch (gridOrigin)
            {
                case GridOrigin.NorthWest:
                    fX = childPos.x - left;
                    fZ = top - childPos.z;
                    break;
                case GridOrigin.NorthEast:
                    fX = right - childPos.x;
                    fZ = top - childPos.z;
                    break;
                case GridOrigin.SouthWest:
                    fX = childPos.x - left;
                    fZ = childPos.z - bottom;
                    break;
                case GridOrigin.SouthEast:
                    fX = right - childPos.x;
                    fZ = childPos.z - bottom;
                    break;
            }

            // グリッド座標をintに変換(切り捨て)
            int x = Mathf.FloorToInt(fX / gridCellNumX);
            int z = Mathf.FloorToInt(fZ / gridCellNumY);

            gridObjects[z, x] = child;
        }

        // それぞれどれだけ溢れているかチェックする
        Vector2Int overflowObjectNum = new Vector2Int(gridObjects.GetLength(1), gridObjects.GetLength(0));
        Vector2Int overflow = new Vector2Int(overflowObjectNum.x * gridCellNumX - gridDivision.x, overflowObjectNum.y * gridCellNumY - gridDivision.y);
        Vector2Int overflowNum = overflow;
        Vector2Int overflowObjectIndex = new Vector2Int(overflowObjectNum.x - 1, overflowObjectNum.y - 1);
        int forcedQuitCount = 0;
        while (overflowNum.x >= 0 && overflowNum.y >= 0)
        {
            if (overflowNum.x > 0)
            {
                float ratio = overflowNum.x >= gridCellNumX ? gridCellNumX : overflowNum.x;
                switch (gridOrigin)
                {
                    case GridOrigin.NorthEast:
                    case GridOrigin.SouthEast:
                        ratio = -ratio;
                        break;
                }
                float curringUVX = ratio / (float)gridCellNumX;
                for (int i = 0 ; i < overflowObjectNum.y ; i++)
                {
                    gridObjects[i, overflowObjectIndex.x].GetComponent<Renderer>().material.SetFloat("_CurringUVX", curringUVX);
                }
                overflowObjectIndex.x--;
                overflowNum.x -= gridCellNumX;
            }
            if (overflowNum.y > 0)
            {
                float ratio = overflowNum.y >= gridCellNumY ? gridCellNumY : overflowNum.y;
                switch (gridOrigin)
                {
                    case GridOrigin.NorthWest:
                    case GridOrigin.NorthEast:
                        ratio = -ratio;
                        break;
                }
                float curringUVY = ratio / (float)gridCellNumY;
                for (int i = 0 ; i < overflowObjectNum.x ; i++)
                {
                    gridObjects[overflowObjectIndex.y, i].GetComponent<Renderer>().material.SetFloat("_CurringUVY", curringUVY);
                }
                overflowObjectIndex.y--;
                overflowNum.y -= gridCellNumY;
            }

            forcedQuitCount ++;
            if (forcedQuitCount > 50)
            {
                Debug.LogError("RoomGrid.cs Start() was forced to quited!!");
                break;
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

        return gridGimmicks[grid.y][grid.x] != null;
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
        if (spawnPos.magnitude == float.PositiveInfinity) return false; 

        //偶数補正
        spawnPos = GimmickEvenNumberCorrection(spawnPos, grid, gimmick);

        Ray ray = new Ray();
        ray.direction = Vector3.down;
        const float rayOriginY = 255f;
        ray.origin = new Vector3(spawnPos.x, rayOriginY, spawnPos.z);
        // 床を確実に取れるようマージンを大きめに取りレイを飛ばす
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Abs(rayOriginY - (gameObject.transform.position.y - 10.0f)), ~0,
            QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hitItem in hits)
        {
            if (hitItem.transform.CompareTag("Player")) continue;
            if (hitItem.transform.CompareTag("Thief")) continue;

            spawnPos.y = hitItem.point.y;
            break;
        }

        GameObject gimmickObject = Instantiate(gimmick.gameObject, spawnPos, Quaternion.identity);
        GimmickBase spawnGimmick = gimmickObject.GetComponent<GimmickBase>();
        spawnGimmick.roomGrid = this;
        gridGimmicks[grid.y][grid.x] = gimmickObject;
        spawnGimmick.SetGimmickPos(grid);
        spawnGimmick.AdjustScaleToGrid();

        return true;
    }

    // ギミックが偶数サイズだった場合の補正用計算関数
    // 概要：奇数はチェス型、偶数は囲碁型に配置します。
    // 引数：Vector3 / オブジェクトの設置する位置※偶数補正前の値
    // 戻値：Vector3 / 補正した値を返します。
    public Vector3 GimmickEvenNumberCorrection(Vector3 setPos, Vector2Int setGrid, GimmickBase gimmick)
    {
        Vector2Int grid = setGrid;
        Vector3 spawnPos = setPos;

        float sizeX = gimmick.GetGimmickSize().x;
        float sizeY = gimmick.GetGimmickSize().y;

        // グリッドサイズ
        float gridSizeX = gridSize.x;
        float gridSizeY = gridSize.y;

        // 半分オフセット
        float offsetX = sizeX * 0.5f;
        float offsetY = sizeY * 0.5f;

        // 偶数サイズ補正（囲碁）
        if ((int)sizeX % 2 == 0)
        {
            if (setPos.x <= spawnPos.x)
                offsetX -= gridSizeX;   // 左に1マス 
        }
        if ((int)sizeY % 2 == 0)
        {
            if (setPos.z <= spawnPos.z)
                offsetY -= gridSizeY;   // 下に1マス
        }
        // ワールド座標に変換
        offsetX *= gridSizeX;
        offsetY *= gridSizeY;

        // 中心が grid に来るように補正
        spawnPos.x += offsetX - (gridSizeX * 0.5f);
        spawnPos.z += offsetY - (gridSizeY * 0.5f);

        return spawnPos;
    }

    public Vector2Int GetGridFromPos(Vector3 pos)
    {
        // posとgridCenterの差分をローカル座標に変換
        Vector3 localPos = transform.InverseTransformVector(pos - gridCenter);

        Vector2 relativePos = new Vector2(
            localPos.x + (gridDivision.x * gridSize.x) * 0.5f,
            (gridDivision.y * gridSize.y) * 0.5f - localPos.z);

        Vector2Int gridPos = new Vector2Int(
            Mathf.FloorToInt(relativePos.x),
            Mathf.FloorToInt(relativePos.y)
        );

        if (gridPos.x < 0 || gridPos.x >= gridDivision.x) gridPos.x = -1;
        if (gridPos.y < 0 || gridPos.y >= gridDivision.y) gridPos.y = -1;

        return gridPos;
    }

    public Vector3 GetWorldPosFromGrid(Vector2Int gridPos)
    {
    if (gridPos.x < 0 || gridPos.x >= gridDivision.x ||
        gridPos.y < 0 || gridPos.y >= gridDivision.y)
    {
        return Vector3.positiveInfinity;
    }

    // gridCenterからの相対インデックス（セル単位）
    Vector3 localOffset = new Vector3(
        (gridPos.x - (gridDivision.x - 1) * 0.5f) * gridSize.x,
        0f,
        -(gridPos.y - (gridDivision.y - 1) * 0.5f) * gridSize.y
    );

    Vector3 worldPos = gridCenter + transform.TransformVector(localOffset);
    worldPos.y = gridCenter.y;
    return worldPos;
    }
}
