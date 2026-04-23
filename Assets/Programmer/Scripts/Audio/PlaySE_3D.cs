/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    3DSE再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-22 | 初回作成
 * 
 */

using CriWare;
using UnityEngine;

public class PlaySE_3D : MonoBehaviour
{
    private CriAtomEx3dSource source;//音源
    private CriAtomEx3dListener listener;//オーディエンス

    public GameObject speaker;//音源発生のオブジェクト
    public GameObject player;//プレイヤー

    private CriAtomExPlayer playerInfo;//AtomExPlayer
    private CriAtomEx.CueInfo[] cueInfoList;//CueName格納
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet格納

    private float[] volumeList;//音量

    // Start is called before the first frame update
    void Start()
    {
        ////初期化
        playerInfo = new CriAtomExPlayer();
        cueInfoList = new CriAtomEx.CueInfo[1];
        cueInfoList[0] = new CriAtomEx.CueInfo();
        criAtomExAcbsList = new CriAtomExAcb[1];
        volumeList = new float[1];
        source = new CriAtomEx3dSource();
        listener = new CriAtomEx3dListener();

        playerInfo.Set3dSource(source);
        playerInfo.Set3dListener(listener);

        playerInfo.SetCue(criAtomExAcbsList[0], "CatMeow");
        volumeList[0] = 1.0f;
        playerInfo.SetVolume(volumeList[0]);
        playerInfo.Loop(true);//本来ならfalseだけど確認のためtrueにしている
        playerInfo.Prepare();
        playerInfo.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //音源の座標更新
        source.SetPosition(speaker.transform.position.x, speaker.transform.position.y, speaker.transform.position.z);
        source.Update();
        
        //オーディエンスの座標更新
        listener.SetPosition(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        listener.Update();
    }
}
