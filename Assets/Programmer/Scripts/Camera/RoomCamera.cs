/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    部屋のカメラ作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-07 | 初回作成
 */
using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    [HideInInspector] public Vector3 initPos = Vector3.zero;//初期値のカメラの位置
    [HideInInspector] public Quaternion initRotate;//初期値のカメラの回転
    [Header("移動量の制限値")]public Vector3 moveAmountLimit = Vector3.zero;//移動量制限値

    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;
        initRotate = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
