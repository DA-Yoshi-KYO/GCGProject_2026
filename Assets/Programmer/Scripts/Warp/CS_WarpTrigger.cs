/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-08 | 初回作成
 */
using UnityEngine;

public class CS_WarpTrigger : MonoBehaviour
{
    [Header("ワープのクールタイム")][SerializeField] public float warpCoolTime = 1.0f;

    private CS_WarpPoint selfWarpPoint;

    private CS_RoomPlayerPosition roomPlayerPosition;

    private void Start()
    {
        selfWarpPoint = GetComponent<CS_WarpPoint>();

        roomPlayerPosition = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();
        if (roomPlayerPosition == null)
        {
            Debug.Log("RoomManagerが見つかりませんでした");
            return;
        }
    }


    void Update()
    {
        if (selfWarpPoint.warping)
        {
            //ワープのクールタイム
            selfWarpPoint.warpTimeCount -= Time.deltaTime;
            if (selfWarpPoint.warpTimeCount <= 0.0f)
            {
                selfWarpPoint.warping = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (selfWarpPoint.warping)
            return;

        CS_WarpPoint wp = GetComponent<CS_WarpPoint>();
        CharacterController controller = other.GetComponent<CharacterController>();

        if (wp == null || wp.targetPoint == null)
            return;

        //プレイヤーの座標更新
        Time.timeScale = 0.0f;
        controller.enabled = false;

        var rb = other.attachedRigidbody;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Transform exitPosition = wp.targetPoint.warpExitPosition;

        if (exitPosition == null)
        {
            exitPosition = wp.targetPoint.transform;
        }

        other.transform.position = exitPosition.position;

        Time.timeScale = 1.0f;
        rb.isKinematic = false;
        controller.enabled = true;

        //カメラ更新
        roomPlayerPosition.RefreshPlayerRoomData();
        CS_PlayerCamera playerCamera = other.GetComponent<CS_PlayerCamera>();
        if (playerCamera == null)
        {
            Debug.Log("CS＿PlayerCameraが見つかりませんでした");
            return;
        }
        playerCamera.OnRoomMove();

        selfWarpPoint.warping = true;
        wp.targetPoint.warping = true;
        selfWarpPoint.warpTimeCount = warpCoolTime;
        wp.targetPoint.warpTimeCount = warpCoolTime;
    }
}
