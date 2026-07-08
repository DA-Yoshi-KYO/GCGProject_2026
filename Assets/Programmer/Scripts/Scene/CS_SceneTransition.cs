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
    //フェードの種類
    public enum FadeKind
    { 
        BlackFade,
        CatFade,
    }

    [Header("フェードの種類")][SerializeField] private FadeKind fadeKind;//フェードの種類
    [Header("ブラックフェードの画像")][SerializeField]private Image blackFadeImage;//フェードの画像
    [Header("猫フェードの画像")][SerializeField]private Image catInFadeImage;//フェードの画像
    [Header("猫フェードの画像")][SerializeField]private Image catOutFadeImage;//フェードの画像
    [Header("フェードにかける時間")][SerializeField] private float fadeDuration = 1.0f;//フェードにかける時間

    [Header("チュートリアル確認キャンバス")] [SerializeField] private GameObject tutorialConfirmationCanvas;
    private bool isTutorialConfirmation = false;//チュートリアル確認キャンバスが表示されたかどうか

    private bool transition = false;//遷移したかどうか

    private bool fadeOut = false;//フェードアウトしたかどうか

    private CS_BackGroundPlayBGM backGroundPlayBGM;

    // Start is called before the first frame update
    void Start()
    {
        backGroundPlayBGM = GameObject.Find("BGM").GetComponent<CS_BackGroundPlayBGM>();

        InitSet();

        if (tutorialConfirmationCanvas != null)
        {
            //チュートリアル確認キャンバスを非表示
            tutorialConfirmationCanvas.SetActive(false);
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

    public void StartSceneTransition(string sceneName, FadeKind kind)
    {
        fadeKind = kind;
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

        switch (fadeKind)
        {
            case FadeKind.BlackFade:
                yield return StartCoroutine(BlackFadeProcessing(1.0f));
                break;
            case FadeKind.CatFade:
                yield return StartCoroutine(CatFadeProcessing(1.0f, 0.0f, 0.0f, 18.0f));
                break;
        }

        //チュートリアル確認キャンバス表示されている
        if (isTutorialConfirmation)
        {
            //シーンの切り替え
            SceneManager.LoadScene(sceneName);
            //シーンが切り替わるまで待つ
            yield return null;
        }

        //チュートリアル確認キャンバスがない場合通常
        if (tutorialConfirmationCanvas == null)
        {
            //シーンの切り替え
            SceneManager.LoadScene(sceneName);

            //シーンが切り替わるまで待つ
            yield return null;
        }
        else
        {
            //チュートリアル確認キャンバスを表示
            tutorialConfirmationCanvas.SetActive(true);
            transition = false;
            isTutorialConfirmation = true;
        }
    }

    private void InitSet()
    {
        blackFadeImage.raycastTarget = false;
        catInFadeImage.raycastTarget = false;
        catOutFadeImage.raycastTarget = false;

        switch (fadeKind)
        {
            case FadeKind.BlackFade:
                blackFadeImage.color = new Color(blackFadeImage.color.r, blackFadeImage.color.g, blackFadeImage.color.b, 1.0f);
                catInFadeImage.material.SetFloat("_AlphaScaleFloat", 1.0f);
                catInFadeImage.material.SetFloat("_CurrentScaleFloat", 0.0f);

                catOutFadeImage.material.SetFloat("_AlphaScaleFloat", 1.0f);
                catOutFadeImage.material.SetFloat("_CurrentScaleFloat", 0.0f);

                //フェードイン
                fadeOut = false;
                StartCoroutine(BlackFadeProcessing(0.0f));
                break;
            case FadeKind.CatFade:
                blackFadeImage.color = new Color(blackFadeImage.color.r, blackFadeImage.color.g, blackFadeImage.color.b, 0.0f);
                catInFadeImage.material.SetFloat("_CurrentScaleFloat", 18.0f);
                catInFadeImage.material.SetFloat("_AlphaScaleFloat", 0.0f);

                catOutFadeImage.material.SetFloat("_CurrentScaleFloat", 18.0f);
                catOutFadeImage.material.SetFloat("_AlphaScaleFloat", 0.0f);

                //フェードイン
                fadeOut = false;
                StartCoroutine(CatFadeProcessing(0.0f, 18.0f, 1.0f, 0.0f));
                break;
        }
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

    private IEnumerator CatFadeProcessing(float startAlpha, float startScale, float endAlpha, float endScale)
    {
        float time = 0.0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;

            float scale = Mathf.Lerp(startScale, endScale, time / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);

            catInFadeImage.material.SetFloat("_CurrentScaleFloat", scale);
            catOutFadeImage.material.SetFloat("_CurrentScaleFloat", scale);

            catInFadeImage.material.SetFloat("_AlphaScaleFloat", alpha);
            catOutFadeImage.material.SetFloat("_AlphaScaleFloat", alpha);

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

        catInFadeImage.material.SetFloat("_CurrentScaleFloat", endScale);
        catOutFadeImage.material.SetFloat("_CurrentScaleFloat", endScale);

        catInFadeImage.material.SetFloat("_AlphaScaleFloat", endAlpha);
        catOutFadeImage.material.SetFloat("_AlphaScaleFloat", endAlpha);
    }
}
