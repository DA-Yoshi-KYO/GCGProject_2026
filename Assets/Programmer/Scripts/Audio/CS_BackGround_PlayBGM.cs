/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    BGM再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-20 | 初回作成
 */

using CriWare;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CS_BackGround_PlayBGM : MonoBehaviour
{
    [SerializeField]private SO_BackGround_BGM_DataBase dataBase;//データベース
    private BackGround_BGM_Data[] dataList;//データのリスト

    private CriAtomExPlayer playerInfo;//Player生成
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet

    private string currentScene;//現在のシーン

    private void Awake()
    {
        //全てのデータ受け取る
        dataList = dataBase.bgmDatas;

        //現在のシーン更新
        currentScene = SceneManager.GetActiveScene().name;

        ////初期化
        playerInfo = new CriAtomExPlayer();
        criAtomExAcbsList = new CriAtomExAcb[dataList.Length];

        //BGMの設定と再生
        SettingBGM();
    }

    public void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    //BGM設定
    private void SettingBGM()
    {
        for (int i = 0 ; i < dataList.Length; ++i)
        {
            if (currentScene == dataList[i].sceneName.ToString())
            {
                playerInfo.SetCue(criAtomExAcbsList[0], dataList[i].cueName.ToString());
                playerInfo.SetVolume(dataList[i].volume);
                playerInfo.Loop(true);
                playerInfo.Prepare();
                playerInfo.Start();
            }
        }
    }
}
