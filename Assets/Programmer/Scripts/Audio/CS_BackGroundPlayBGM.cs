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

public class CS_BackGroundPlayBGM : MonoBehaviour
{
    [SerializeField]private SO_BackGroundBGMDataBase dataBase;//データベース

    private CriAtomExPlayer playerInfo;//Player生成
    private CriAtomExAcb[] criAtomExAcbsList;//CueSheet

    private string currentScene;//現在のシーン

    private void Awake()
    {
        //現在のシーン更新
        currentScene = SceneManager.GetActiveScene().name;

        ////初期化
        playerInfo = new CriAtomExPlayer();
        criAtomExAcbsList = new CriAtomExAcb[dataBase.bgmDatas.Length];

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
        for (int i = 0 ; i < dataBase.bgmDatas.Length; ++i)
        {
            if (currentScene == dataBase.bgmDatas[i].sceneName.ToString())
            {
                playerInfo.SetCue(criAtomExAcbsList[0], dataBase.bgmDatas[i].cueName.ToString());
                playerInfo.SetVolume(dataBase.bgmDatas[i].volume);
                playerInfo.Loop(true);
                playerInfo.Prepare();
                playerInfo.Start();
            }
        }
    }
}
