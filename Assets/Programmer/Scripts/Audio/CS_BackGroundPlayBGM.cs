/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    BGM再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-04-20 | 初回作成
 * 2026-05-22 | オプションでの音量調整追加
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

    private float currentVolume;//現在の音量
    private float maxVolume = 1.0f;//最大の音量

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
        if (Option.Instance != null)
        {
            maxVolume = Option.Instance.GetBGMVolume() / 100.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //オプション画面開いたら音源調整の関数呼び出す
        if (Option.Instance != null)
        {
            if (Option.Instance.GetIsOptionUIActive())
            {
                BGMOption();
            }
        }
    }

    //BGM設定
    private void SettingBGM()
    {
        for (int i = 0 ; i < dataBase.bgmDatas.Length; ++i)
        {
            if (currentScene == dataBase.bgmDatas[i].sceneName.ToString())
            {
                playerInfo.SetCue(criAtomExAcbsList[0], dataBase.bgmDatas[i].cueName.ToString());
                playerInfo.Loop(true);
                playerInfo.SetVoicePriority(255);
                playerInfo.Prepare();
                playerInfo.Start();
            }
        }
    }

    //BGMのフェードアウト
    public void BGMFadeOut(float time, float fadeDuration)
    {
        currentVolume = Mathf.Lerp(maxVolume, 0.0f, time / fadeDuration);
        CriAtom.SetCategoryVolume("CategoryBGM", currentVolume);
    }

    //BGMのフェードイン
    public void BGMFadeIn(float time, float fadeDuration)
    {
        currentVolume = Mathf.Lerp(0.0f, maxVolume, time / fadeDuration);
        CriAtom.SetCategoryVolume("CategoryBGM", currentVolume);
    }

    //BGMのオプションでの音量調整
    public void BGMOption()
    {
        currentVolume = Option.Instance.GetBGMVolume() / 100.0f;
        maxVolume = currentVolume;
        CriAtom.SetCategoryVolume("CategoryBGM", currentVolume);
    }

}
