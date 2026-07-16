/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    オープニングシーンの処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-22 | 初回作成
 * 2026-07-14 | UIの追加
 */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class CS_TrasitionMovieEnd : MonoBehaviour
{
    [Header("遷移するシーンの名前")][SerializeField] private string sceneName;
    [Header("FadeCanvasのPrefab格納")][SerializeField] private GameObject fadeCanvas;
    [Header("VideoPlayerのコンポーネントが入ったゲームオブジェクト")][SerializeField] private VideoPlayer videoPlayer;
    [Header("長押しする時間")][SerializeField] private float holdTime;

    [Header("Manualの画像を格納")][SerializeField] private GameObject[] manualImage;
    [Header("Manualの画像の背景(黒)")][SerializeField] private GameObject[] backBlackManualImage;
    [Header("Manualの画像の背景(緑)")][SerializeField] private GameObject[] backGreenManualImage;

    private CustomInputAction customInputAction;
    private bool Holding = false;
    private float time;

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーの入力アクションの初期化と有効化
        customInputAction = CS_CustomInputActionManager.instance.customInputAction;
        customInputAction.Openning.SkipButton.started += OnHoldStart;
        customInputAction.Openning.SkipButton.canceled += OnHoldEnd;

        if (manualImage != null && manualImage.Length >= 2)
        {
            manualImage[0].SetActive(true);
            manualImage[1].SetActive(false);

            backGreenManualImage[0].GetComponent<Image>().material.SetFloat("_MaxTimeFloat", holdTime);
            backGreenManualImage[1].GetComponent<Image>().material.SetFloat("_MaxTimeFloat", holdTime);
        }


        if (videoPlayer == null)
        {
            Debug.LogError("CS_TrasitionMovieEnd: videoPlayerが設定されていません。", this);
            return;
        }
        videoPlayer.loopPointReached += OnMovieFinished;

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void OnDestroy()
    {
        customInputAction.Openning.SkipButton.started -= OnHoldStart;
        customInputAction.Openning.SkipButton.canceled -= OnHoldEnd;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnMovieFinished;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Holding)
        {
            time += Time.deltaTime;

            if (time >= holdTime)
            {
                Holding = false;
                if (videoPlayer != null)
                    videoPlayer.Stop();

                StartTransition();
            }
        }
        else
        {
            time = 0.0f;
        }

        if (CS_CustomInputActionManager.instance.currentInputType == CS_CustomInputActionManager.InputType.Gamepad)
        {
            manualImage[0].SetActive(true);
            manualImage[1].SetActive(false);

            backBlackManualImage[0].SetActive(true);
            backBlackManualImage[1].SetActive(false);

            backGreenManualImage[0].SetActive(true);
            backGreenManualImage[1].SetActive(false);

            backGreenManualImage[0].GetComponent<Image>().material.SetFloat("_TimeFloat", time);
        }
        else
        {
            manualImage[0].SetActive(false);
            manualImage[1].SetActive(true);

            backBlackManualImage[0].SetActive(false);
            backBlackManualImage[1].SetActive(true);

            backGreenManualImage[0].SetActive(false);
            backGreenManualImage[1].SetActive(true);

            backGreenManualImage[1].GetComponent<Image>().material.SetFloat("_TimeFloat", time);
        }
    }

    private void OnMovieFinished(VideoPlayer vp)
    {
        if (vp != videoPlayer)
            return;

        videoPlayer.Stop();
        StartTransition();
    }

    private void StartTransition()
    {
        if (fadeCanvas == null)
        {
            Debug.LogError("CS_TrasitionMovieEnd: fadeCanvasが設定されていません。", this);
            return;
        }
        CS_SceneTransition sceneTransition = fadeCanvas.GetComponent<CS_SceneTransition>();
        if (sceneTransition == null)
        {
            Debug.LogError("CS_TrasitionMovieEnd: fadeCanvasにCS_SceneTransitionが見つかりません。", this);
            return;
        }
        sceneTransition.StartSceneTransition(sceneName);
    }

    private void OnHoldStart(InputAction.CallbackContext ctx)
    {
        Holding = true;
        time = 0.0f;
    }

    private void OnHoldEnd(InputAction.CallbackContext ctx)
    {
        Holding = false;
        time = 0.0f;
    }
}
