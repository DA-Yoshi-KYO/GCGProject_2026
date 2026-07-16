/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ワープの作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-08 | 初回作成
 */
using System.Collections;
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

        GameObject roomManager = GameObject.Find("RoomManager");
        roomPlayerPosition = roomManager != null ? roomManager.GetComponent<CS_RoomPlayerPosition>() : null;
        if (roomPlayerPosition == null)
        {
            Debug.Log("RoomManagerが見つかりませんでした");
            return;
        }

        GameObject maskCanvas = GameObject.Find("MaskCanvas");
        mask = maskCanvas != null ? maskCanvas.GetComponent<CS_Mask>() : null;

        // SE取得
        g_SEDataObject = GameObject.Find("3DSE");
        cs_3DPlaySE = g_SEDataObject != null ? g_SEDataObject.GetComponent<CS_3DPlaySE>() : null;
    }


    void Update()
    {
        if (selfWarpPoint == null) return;

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

        //UI表示
        CS_PlayerAction playerAction = other.GetComponent<CS_PlayerAction>();
        if (playerAction == null)
        {
            Debug.LogError("ワープさせるプレイヤーのコンポーネントにCS_PlayerActionがありませんでした");
            return;
        }

        playerAction.warpUIView = true;

        //ワープする
        if (!playerAction.doWarp)
            return;

        CS_PlayerMove playerMove = other.GetComponent<CS_PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("ワープさせるプレイヤーのコンポーネントにCS_PlayerMoveがありませんでした");
            return;
        }

        // プレイヤーをワープの正面に配置し、ワープの方向へ向かせる
        PlacePlayerInFrontOfWarp(other, playerMove);

        playerMove.animator.SetTrigger("WarpInTrigger");

        StartCoroutine(WarpCoroutine(other, playerMove.animator));

        playerAction.warpUIView = false;
        playerAction.doWarp = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        CS_PlayerAction playerAction = other.GetComponent<CS_PlayerAction>();
        playerAction.warpUIView = false;
        playerAction.doWarp = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (selfWarpPoint.warping)
            return;

        CS_PlayerAction playerAction = other.GetComponent<CS_PlayerAction>();

        //ワープする
        if (!playerAction.doWarp)
            return;

        CS_PlayerMove playerMove = other.GetComponent<CS_PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("ワープさせるプレイヤーのコンポーネントにCS_PlayerMoveがありませんでした");
            return;
        }

        // プレイヤーをワープの正面に配置し、ワープの方向へ向かせる
        PlacePlayerInFrontOfWarp(other, playerMove);

        playerMove.animator.SetTrigger("WarpInTrigger");

        StartCoroutine(WarpCoroutine(other, playerMove.animator));

        playerAction.warpUIView = false;
        playerAction.doWarp = false;
    }

    /// <summary>
    /// プレイヤーをワープの正面に配置し、ワープの方向へ向かせる
    /// </summary>
    private void PlacePlayerInFrontOfWarp(Collider other, CS_PlayerMove playerMove)
    {
        Vector2 dirWarpToPlayer = new Vector2(playerMove.transform.position.x - transform.position.x, playerMove.transform.position.z - transform.position.z).normalized;

        Vector3 playerStartPoint = other.transform.position;
        playerStartPoint.x += dirWarpToPlayer.x;
        playerStartPoint.z += dirWarpToPlayer.y;

        Quaternion playerRotate = Quaternion.LookRotation(Vector3.Normalize(transform.position - playerStartPoint));

        // CharacterControllerが有効なまま座標を書き換えると位置がずれるため、一旦無効化してから書き換える
        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        other.transform.SetPositionAndRotation(playerStartPoint, playerRotate);

        if (controller != null) controller.enabled = true;

        // rb.rotation等を同期しないとFixedUpdateで回転が古い値に戻され、がたつきが発生する
        playerMove.SyncTransform(playerStartPoint, playerRotate);
    }

    IEnumerator WarpCoroutine(Collider other, Animator animator)
    {
        // WarpInアニメーションの再生開始を待つ
        yield return null;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("WarpIn"))
        {
            yield return null;
        }

        // WarpInアニメーションの再生完了を待つ
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        WarpDo(other);
    }

    private void WarpDo(Collider other)
    {
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

        Transform exitPosition = wp.targetPoint.warpExitPosition;

        if (exitPosition == null)
        {
            exitPosition = wp.targetPoint.transform;
        }

        // ワープ先を向かないよう、ワープ先とは逆方向（外向き）に回転させる
        Vector3 dirAwayFromWarp = exitPosition.position - wp.targetPoint.transform.position;
        dirAwayFromWarp.y = 0f;
        Quaternion exitRotation = dirAwayFromWarp.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(dirAwayFromWarp.normalized)
            : exitPosition.rotation;

        other.transform.SetPositionAndRotation(exitPosition.position, exitRotation);

        CS_PlayerMove playerMove = other.GetComponent<CS_PlayerMove>();
        if (playerMove != null)
        {
            playerMove.SyncTransform(other.transform.position, other.transform.rotation);
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
