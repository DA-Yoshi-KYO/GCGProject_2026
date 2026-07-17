/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    チュートリアルの画像の変更処理（キーボードかコントローラーかで）
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-07-16 | 初回作成
 */
using UnityEngine;

public class CS_ImageSwitch : MonoBehaviour
{
    [Header("Manualの画像を格納（0:コントローラー 1:キーボード）")][SerializeField] private Texture2D[] manualImage;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material.mainTexture = manualImage[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (CS_CustomInputActionManager.instance.currentInputType == CS_CustomInputActionManager.InputType.Gamepad)
        {
            GetComponent<MeshRenderer>().material.mainTexture = manualImage[0];
        }
        else
        {
            GetComponent<MeshRenderer>().material.mainTexture = manualImage[1];
        }
    }
}
