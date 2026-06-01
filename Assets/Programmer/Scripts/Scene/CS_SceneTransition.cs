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
    [Header("フェードの画像")][SerializeField]private Image fadeImage;//フェードの画像
    [Header("フェードにかける時間")][SerializeField] private float fadeDuration = 1.0f;//フェードにかける時間
    private bool transition = false;//遷移したかどうか

    private bool fadeOut = false;//フェードアウトしたかどうか

    private CS_BackGroundPlayBGM backGroundPlayBGM;

    // Start is called before the first frame update
    void Start()
    {
        fadeImage = GameObject.Find("Fade").GetComponent<Image>();
        backGroundPlayBGM = GameObject.Find("BGM").GetComponent<CS_BackGroundPlayBGM>();

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1.0f);
        fadeImage.raycastTarget = false;

        //フェードイン
        fadeOut = false;
        StartCoroutine(FadeProcessing(0.0f));
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
        yield return StartCoroutine(FadeProcessing(1.0f));

        //シーンの切り替え
        SceneManager.LoadScene(sceneName);

        //シーンが切り替わるまで待つ
        yield return null;
    }

    //フェードの処理
    private IEnumerator FadeProcessing(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0.0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);

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

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
    }
}
