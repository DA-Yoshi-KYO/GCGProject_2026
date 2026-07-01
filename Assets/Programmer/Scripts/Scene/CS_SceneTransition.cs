/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    シーン遷移とフェード処理の作成
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-15 | 初回作成
 */
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CS_SceneTransition : MonoBehaviour
{
    enum FadeKind
    { 
        BlackFade,
        CatFade,
    }

    [Header("フェードの種類")][SerializeField] private FadeKind fadeKind;//フェードの種類
    [Header("ブラックフェードの画像")][SerializeField]private Image blackFadeImage;//フェードの画像
    [Header("猫フェードの画像")][SerializeField]private Image catFadeImage;//フェードの画像
    [Header("フェードにかける時間")][SerializeField] private float fadeDuration = 1.0f;//フェードにかける時間
    private bool transition = false;//遷移したかどうか

    private bool fadeOut = false;//フェードアウトしたかどうか

    private CS_BackGroundPlayBGM backGroundPlayBGM;

    // Start is called before the first frame update
    void Start()
    {
        backGroundPlayBGM = GameObject.Find("BGM").GetComponent<CS_BackGroundPlayBGM>();

        switch(fadeKind)
        {
            case FadeKind.BlackFade:
                blackFadeImage.color = new Color(blackFadeImage.color.r, blackFadeImage.color.g, blackFadeImage.color.b, 1.0f);
                catFadeImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                blackFadeImage.raycastTarget = false;

                //フェードイン
                fadeOut = false;
                StartCoroutine(BlackFadeProcessing(0.0f));
                break;
            case FadeKind.CatFade:
                break;
        }
    }

    void Update()
    {

    }

    //遷移開始
    public void StartSceneTransition(string sceneName)
    {
        if (!transition)
        {
            StartCoroutine(SwitchScene(sceneName));
        }
    }

    //フェードアウトしてシーン切り替え
    private IEnumerator SwitchScene(string sceneName)
    {
        transition = true;

        //フェードアウト
        fadeOut = true;

        yield return StartCoroutine(BlackFadeProcessing(1.0f));      

        //シーンの切り替え
        SceneManager.LoadScene(sceneName);

        //シーンが切り替わるまで待つ
        yield return null;
    }

    //フェードの処理
    private IEnumerator BlackFadeProcessing(float targetAlpha)
    {
        float startAlpha = blackFadeImage.color.a;
        float time = 0.0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            blackFadeImage.color = new Color(blackFadeImage.color.r, blackFadeImage.color.g, blackFadeImage.color.b, alpha);

            if (fadeOut)
            {
                backGroundPlayBGM.BGMFadeOut(time, fadeDuration);
            }
            else
            {
                backGroundPlayBGM.BGMFadeIn(time, fadeDuration);
            }

            yield return null;
        }

        blackFadeImage.color = new Color(blackFadeImage.color.r, blackFadeImage.color.g, blackFadeImage.color.b, targetAlpha);
    }
}
