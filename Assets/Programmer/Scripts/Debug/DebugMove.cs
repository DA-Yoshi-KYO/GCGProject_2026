using System;
using UnityEngine;

public class DebugMove : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 5f;
    [Header("デバッグしたいオブジェクト")]
    [SerializeField] GameObject debugObject; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))     transform.position += transform.forward * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))   transform.position -= transform.forward * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.RightArrow))  transform.position -= transform.right * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow))   transform.position += transform.right * moveSpeed * Time.deltaTime;
    }

    void Update()
    {
        if (debugObject != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Position:" + transform.position);

                var roomGrid = debugObject.GetComponent<RoomGrid>();
                Vector2Int grid = roomGrid.GetGridFromPos(transform.position);
                Debug.Log("Grid:" + grid);

                Vector3 gridPos = roomGrid.GetWorldPosFromGrid(grid);
                Debug.Log("GridPosition:" + gridPos);
            }
        }
    }
}
