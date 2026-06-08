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
    private float warpDisableTime = 5.0f;

    private CS_WarpPoint selfWarpPoint;

    private void Start()
    {
        selfWarpPoint = GetComponent<CS_WarpPoint>();
    }


    void Update()
    {
        if (selfWarpPoint.warping)
        {
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

        if (wp == null || wp.targetPoint == null)
            return;

        Debug.Log("BeforePos" + other.transform.position);

        Vector3 offset = other.transform.forward * 1.0f;
        other.transform.position = wp.targetPoint.transform.position + offset;

        Debug.Log("AfterPos" + other.transform.position);

        selfWarpPoint.warping = true;
        wp.targetPoint.warping = true;
        selfWarpPoint.warpTimeCount = warpDisableTime;
        wp.targetPoint.warpTimeCount = warpDisableTime;
    }
}
