using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickTest : MonoBehaviour
{
    [Header("テスト用ギミック召喚")]
    [Tooltip("召喚するギミック"), SerializeField]
    public GameObject gimmick;

    [Header("グリッド座標")]
    [Tooltip("グリッド座標X"), SerializeField]
    public int gridX;
    [Tooltip("グリッド座標Y"), SerializeField]
    public int gridY;

    [Header("RoomGrid")]
    [Tooltip("RoomGrid"), SerializeField]
    public RoomGrid roomGrid;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GameObject T = Instantiate(gimmick, transform.position, Quaternion.identity);
            GimmickBase gimmickBase = T.GetComponent<GimmickBase>();
            gimmickBase.roomGrid = roomGrid;
            gimmickBase.AdjustScaleToGrid();
            gimmickBase.SetGimmickPos(new Vector2Int(gridX, gridY));
        }
    }
}
