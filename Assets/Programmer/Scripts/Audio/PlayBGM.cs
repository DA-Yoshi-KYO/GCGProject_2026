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
    public BGMDataBase bgmDataBase;//データベース
    private BGMData[] dataList;//データのリスト

    private CriAtomExPlayer playerInfo;//Player生成
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet
    private CriAtomEx.CueInfo[] cueInfoList;//CueName格納

    //private string[] cueSheetList;//キューシートリスト
    //private string[] cueNameList;//キューネームリスト
    //private string[] sceneList;//シーンリスト


    private string currentScene;//現在のシーン

    private bool endBGM = false;//BGM終了判定

    //private double time = 0;//時間


    private void Awake()
    {
        //全てのデータ受け取る
        dataList = bgmDataBase.bgmDatas;

        //現在のシーン更新
        currentScene = SceneManager.GetActiveScene().name;

        ////初期化
        playerInfo = new CriAtomExPlayer();
        criAtomExAcbsList = new CriAtomExAcb[dataList.Length];
        cueInfoList = new CriAtomEx.CueInfo[dataList.Length];

        //cueSheetList = new string[System.Enum.GetValues(typeof(CueSheetName)).Length];
        //for (int i = 0 ; i < System.Enum.GetValues(typeof(CueSheetName)).Length ; ++i)
        //{
        //    cueSheetList = System.Enum.GetNames(typeof(CueSheetName));
        //}
        //cueNameList = new string[System.Enum.GetValues(typeof(CueName)).Length];
        //for (int i = 0 ; i < System.Enum.GetValues(typeof(CueName)).Length ; ++i)
        //{
        //    cueNameList = System.Enum.GetNames(typeof(CueName));
        //}
        //sceneList = new string[System.Enum.GetValues(typeof(SceneName)).Length];
        //for (int i = 0 ; i < System.Enum.GetValues(typeof(SceneName)).Length ; ++i)
        //{
        //    sceneList = System.Enum.GetNames(typeof(SceneName));
        //}

        //ループ設定
        playerInfo.Loop(true);

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

        //Debug.Log(criAtomExAcbsList[0].ToString());
        //Debug.Log(sceneList[1]);
        //Debug.Log(playerInfo.GetStatus());
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
        ////シーン情報別設定
        //switch (currentScene)
        //{
        //    case "CriWare":
        //        playerInfo.SetCue(criAtomExAcbsList[0], "CatMeow");
        //        volumeList[0] = 1.0f;
        //        playerInfo.SetVolume(volumeList[0]);
        //        playerInfo.Loop(true);
        //        playerInfo.Prepare();
        //        playerInfo.Start();
        //        ////CueSheet情報
        //        //criAtomExAcbsList[0] = CriAtom.GetAcb("WorkingTitle");
        //        //cueInfoList = criAtomExAcbsList[0].GetCueInfoList();
        //        ////CueName情報
        //        //playerInfoList[0].SetCue(criAtomExAcbsList[0], "CatMeow(WorkingTitle)");
        //        ////音量
        //        //volumeList[0] = 1.0f;
        //        //playerInfoList[0].SetVolume(volumeList[0]);
        //        ////再生
        //        //playerInfoList[0].Start();
        //        break;
        //    default:
        //        break;
        //}


        for (int i = 0 ; i < dataList.Length; ++i)
        {
            if (currentScene == dataList[i].sceneName.ToString())
            {
                //for (int j = 0 ; j < sceneList.Length ; ++j)
                //{
                //    switch (j)
                //    {
                //        case sceneList:
                //            playerInfo.SetCue(criAtomExAcbsList[0], "CatMeow");
                //            playerInfo.SetVolume(dataList[i].volume);
                //            break;
                //        case "Shading":
                //            break;
                //        case "CreateRoom":
                //            break;
                //        case "CreateThiefScene":
                //            break;
                //        case "MainScene":
                //            break;
                //        case "BootStrap":
                //            break;
                //        default: break;
                //    }
                //}

                if (criAtomExAcbsList[i] != null)
                {
                    criAtomExAcbsList[i].Dispose();
                    criAtomExAcbsList[i] = null;
                }

                //criAtomExAcbsList[i] = CriAtomExAcb.LoadAcbFile(null, dataList[i].cueSheet.ToString() + ".acb", null);
                playerInfo.SetCue(criAtomExAcbsList[0], dataList[i].cueName.ToString());
                playerInfo.SetVolume(dataList[i].volume);
                playerInfo.Loop(true);
                playerInfo.Prepare();
                playerInfo.Start();
            }
        }
    }
}
