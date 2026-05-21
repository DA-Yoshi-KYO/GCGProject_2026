/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    音源が聞こえたどうかの判定処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-21 | 初回作成
 */
using UnityEngine;

public class CS_SoundHear : MonoBehaviour
{
    private AudioSource speaker;
    private SphereCollider sphereCollider;
    [HideInInspector]public　bool hear = false;//聞こえているかどうか
    private float hearingRange = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        speaker = GetComponent<AudioSource>();
        sphereCollider = GetComponent<SphereCollider>();

        //聞こえる範囲
        hearingRange = speaker.maxDistance;
        sphereCollider.radius = hearingRange;
    }

    // Update is called once per frame
    void Update()
    {
        if (speaker == null) return;
    }

    
    private void OnDrawGizmos()
    {
        if(hear)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, hearingRange);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, hearingRange);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Thief"))
        {
            hear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Thief"))
        {
            hear = false;
        }
    }
}
