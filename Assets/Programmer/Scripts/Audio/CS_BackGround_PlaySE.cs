/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    SE再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-17 | 初回作成
 */
using CriWare;
using UnityEngine;

public class CS_BackGroundPlaySE : MonoBehaviour
{
    [SerializeField]private SO_BackGroundSEDataBase dataBase;//データベース

    private CriAtomExPlayer playerInfo;//Player生成
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet

    // Start is called before the first frame update
    void Start()
    {
        ////初期化
        playerInfo = new CriAtomExPlayer();
        criAtomExAcbsList = new CriAtomExAcb[dataBase.seDatas.Length];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //SE設定
    private void SettingSE(string currentSituation)
    {
        for (int i = 0 ; i < dataBase.seDatas.Length ; ++i)
        {
            if (currentSituation == dataBase.seDatas[i].situation.ToString())
            {
                playerInfo.SetCue(criAtomExAcbsList[0], dataBase.seDatas[i].cueName.ToString());
                playerInfo.SetVolume(dataBase.seDatas[i].volume);
                playerInfo.Loop(false);
                playerInfo.Prepare();
            }
        }
    }

    public void PlaySE(string currentSituation)
    {
        //SEの設定
        SettingSE(currentSituation);

        playerInfo.Start();
    }
}
