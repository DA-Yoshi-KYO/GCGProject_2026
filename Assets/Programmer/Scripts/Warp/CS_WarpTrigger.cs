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

    private CS_Mask mask;

    private GameObject g_SEDataObject;
    private CS_3DPlaySE cs_3DPlaySE;
    private string s_seName = "Cat_RoomMove";
    private string s_seObjectName = "Cat_Warp";


    private void Start()
    {
        selfWarpPoint = GetComponent<CS_WarpPoint>();

        roomPlayerPosition = GameObject.Find("RoomManager").GetComponent<CS_RoomPlayerPosition>();
        if (roomPlayerPosition == null)
        {
            Debug.Log("RoomManagerが見つかりませんでした");
            return;
        }

        mask = GameObject.Find("MaskCanvas").GetComponent<CS_Mask>();

        // SE取得
        g_SEDataObject = GameObject.Find("3DSE");
        cs_3DPlaySE = g_SEDataObject.GetComponent<CS_3DPlaySE>();
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

        CS_PlayerCamera playerCamera = other.GetComponent<CS_PlayerCamera>();



        // SEを鳴らす
        if (cs_3DPlaySE != null)
        {
            cs_3DPlaySE.PlayOneShotSE(s_seName, other.transform.position, s_seObjectName, CS_3DPlaySE.SEMode.Normal);
        }



        mask.StartInMask(playerCamera.ChangeCamera);

        //プレイヤーの座標更新
        controller.enabled = false;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
        {
            rb = other.GetComponentInParent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Transform exitPosition = wp.targetPoint.warpExitPosition;

        if (exitPosition == null)
        {
            exitPosition = wp.targetPoint.transform;
        }

        other.transform.position = exitPosition.position;

        if (rb != null)
        {
            rb.isKinematic = false;
        }
        controller.enabled = true;

        //カメラ更新
        roomPlayerPosition.RefreshPlayerRoomData();
       if (playerCamera == null)
        {
            Debug.Log("CS＿PlayerCameraが見つかりませんでした");
            return;
        }
        //playerCamera.ChangeCamera();

        selfWarpPoint.warping = true;
        wp.targetPoint.warping = true;
        selfWarpPoint.warpTimeCount = warpCoolTime;
        wp.targetPoint.warpTimeCount = warpCoolTime;

        CS_PlayerData playerdata = other.GetComponent<CS_PlayerData>();
        playerdata.ChangePlayerRoomData();
    }
}
