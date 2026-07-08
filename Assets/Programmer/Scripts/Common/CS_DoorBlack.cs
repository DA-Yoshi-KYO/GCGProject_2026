/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ドアの黒くなる部分のシェーダの処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-07-07 | 初回作成
 */
using UnityEngine;

public class CS_DoorBlack : MonoBehaviour
{
    private Transform playerPos;
    private Material doorMaterial;
    [SerializeField] private float fadeRange;

    // Start is called before the first frame update
    void Start()
    {
        doorMaterial = GetComponent<Renderer>().material;

        doorMaterial.SetFloat("_FadeRangeFloat", fadeRange);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPos == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player == null)
                return;

            playerPos = player.transform;
        }

        if (playerPos == null)
            return;

        float dist = Vector3.Distance(playerPos.position, transform.position);
        doorMaterial.SetFloat("_PlayerPosFloat", dist);
    }
}
