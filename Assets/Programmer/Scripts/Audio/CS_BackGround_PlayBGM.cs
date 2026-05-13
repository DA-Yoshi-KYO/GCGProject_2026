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

public class PlayBGM : MonoBehaviour
{
    public BackGround_BGM_DataBase dataBase;//データベース
    private BackGround_BGM_Data[] dataList;//データのリスト

    private CriAtomExPlayer playerInfo;//Player生成
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet

    private string currentScene;//現在のシーン

    private bool endBGM = false;//BGM終了判定

    private void Awake()
    {
        //全てのデータ受け取る
        dataList = dataBase.bgmDatas;

        //現在のシーン更新
        currentScene = SceneManager.GetActiveScene().name;

        ////初期化
        playerInfo = new CriAtomExPlayer();
        criAtomExAcbsList = new CriAtomExAcb[dataList.Length];

        //終了判定
        endBGM = false;

        //シーン更新
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    public void Start()
    {
        playerInfo.Prepare();
        playerInfo.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    //シーン更新
    void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        //再生終了
        if (playerInfo.GetStatus() == CriAtomExPlayer.Status.Playing)
        {
            playerInfo.Stop();
        }

        //現在のシーンと次のシーンが違うとき
        if (currentScene != nextScene.name)
        {
            currentScene = nextScene.name;
        }

        //終了判定
        if (endBGM)
        {
            endBGM = true;
        }

        //BGM設定
        SettingBGM();
    }

    //BGM設定
    void SettingBGM()
    {
        for (int i = 0 ; i < dataList.Length; ++i)
        {
            if (currentScene == dataList[i].sceneName.ToString())
            {
                playerInfo.SetCue(criAtomExAcbsList[0], dataList[i].cueName.ToString());
                playerInfo.SetVolume(dataList[i].volume);
                playerInfo.Loop(true);
            }
        }
    }
}
