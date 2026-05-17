/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    SE再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-17 | 初回作成
 */
using CriWare;
using UnityEngine;

public class CS_BackGround_PlaySE : MonoBehaviour
{
    [SerializeField]private SO_BackGround_SE_DataBase dataBase;//データベース
    private BackGround_SE_Data[] dataList;//データのリスト

    private CriAtomExPlayer playerInfo;//Player生成
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet

    // Start is called before the first frame update
    void Start()
    {
        //全てのデータ受け取る
        dataList = dataBase.seDatas;

        ////初期化
        playerInfo = new CriAtomExPlayer();
        criAtomExAcbsList = new CriAtomExAcb[dataList.Length];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //SE設定
    private void SettingSE(string currentSituation)
    {
        for (int i = 0 ; i < dataList.Length ; ++i)
        {
            if (currentSituation == dataList[i].situation.ToString())
            {
                playerInfo.SetCue(criAtomExAcbsList[0], dataList[i].cueName.ToString());
                playerInfo.SetVolume(dataList[i].volume);
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
