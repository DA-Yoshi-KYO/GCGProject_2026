/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    BGM再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-20 | 初回作成
 * 
 */

using CriWare;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayBGM : MonoBehaviour
{
    public BGMDataBase bgmDataBase;

    private CriAtomEx.CueInfo[] cueInfoList;//CueName格納
    private CriAtomExPlayer[] playerInfoList;//Player生成
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet
    private string[] sceneList;//シーン

    private string currentScene;//現在のシーン

    private bool endBGM = false;//BGM終了判定
                                //private double time = 0;//時間

    private float[] volumeList;//音量

    private void Awake()
    {
        //全てのデータ受け取る
        BGMData[] data = bgmDataBase.bgmDatas;

        //現在のシーン更新
        currentScene = SceneManager.GetActiveScene().name;

        ////初期化
        playerInfoList = new CriAtomExPlayer[1];
        playerInfoList[0] = new CriAtomExPlayer();
        cueInfoList = new CriAtomEx.CueInfo[1];
        cueInfoList[0] = new CriAtomEx.CueInfo();
        criAtomExAcbsList = new CriAtomExAcb[1];
        sceneList = new string[1];
        volumeList = new float[1];

        //ループ設定
        playerInfoList[0].Loop(true);

        //終了判定
        endBGM = false;

        //シーン更新
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //時間
        //time = Time.fixedDeltaTime;
        //Debug.Log(time);

        Debug.Log(playerInfoList[0].GetStatus());
    }

    //シーン更新
    void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        //再生終了
        if (playerInfoList[0].GetStatus() == CriAtomExPlayer.Status.Playing)
        {
            playerInfoList[0].Stop();
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
        //シーン情報別設定
        switch (currentScene)
        {
            case "CriWare":
                playerInfoList[0].SetCue(criAtomExAcbsList[0], "CatMeow");
                volumeList[0] = 1.0f;
                playerInfoList[0].SetVolume(volumeList[0]);
                playerInfoList[0].Loop(true);
                playerInfoList[0].Prepare();
                playerInfoList[0].Start();
                ////CueSheet情報
                //criAtomExAcbsList[0] = CriAtom.GetAcb("WorkingTitle");
                //cueInfoList = criAtomExAcbsList[0].GetCueInfoList();
                ////CueName情報
                //playerInfoList[0].SetCue(criAtomExAcbsList[0], "CatMeow(WorkingTitle)");
                ////音量
                //volumeList[0] = 1.0f;
                //playerInfoList[0].SetVolume(volumeList[0]);
                ////再生
                //playerInfoList[0].Start();
                break;
            default:
                break;
        }
    }
}
