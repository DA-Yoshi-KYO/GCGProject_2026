/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    SE再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-17 | 初回作成
 * 2026-05-22 | オプションでの音量調整追加
 */
using CriWare;
using UnityEngine;

public class CS_BackGroundPlaySE : MonoBehaviour
{
    [SerializeField]private SO_BackGroundSEDataBase dataBase;//データベース

    private CriAtomExPlayer playerInfo;//Player生成

    private float currentVolume = 1.0f;//現在の音量

    // Start is called before the first frame update
    void Start()
    {
        ////初期化
        playerInfo = new CriAtomExPlayer();

        if (Option.Instance != null)
        {
            currentVolume = Option.Instance.GetSEVolume() / 100.0f;
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
                SEOption();
            }
        }
    }

    //SE設定
    private void SettingSE(string currentSituation)
    {
        BackGroundSEData data = dataBase.seData[currentSituation];

        playerInfo.SetCue(CriAtom.GetCueSheet("BackGround_SE").acb, data.cueName.ToString());
        playerInfo.Loop(false);
        playerInfo.SetVoicePriority(50);
        playerInfo.Prepare();
    }

    public void PlaySE(string currentSituation)
    {
        //SEの設定
        SettingSE(currentSituation);

        playerInfo.Start();
    }

    //SEのオプションでの音量調整
    public void SEOption()
    {
        currentVolume = Option.Instance.GetSEVolume() / 100.0f;
        CriAtom.SetCategoryVolume("CategorySE", currentVolume);
    }
}
