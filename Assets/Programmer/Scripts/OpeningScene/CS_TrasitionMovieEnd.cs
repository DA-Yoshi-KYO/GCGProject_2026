/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    オープニングシーンの処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-22 | 初回作成
 */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class CS_TrasitionMovieEnd : MonoBehaviour
{
    [Header("遷移するシーンの名前")][SerializeField] private string sceneName;
    [Header("FadeCanvasのPrefab格納")][SerializeField] private GameObject fadeCanvas;
    [Header("VideoPlayerのコンポーネントが入ったゲームオブジェクト")][SerializeField] private VideoPlayer videoPlayer;
    [Header("長押しする時間")][SerializeField] private float holdTime;

    [Header("映像を格納")][SerializeField] private VideoClip[] videoclip;

    private CustomInputAction custoomInputAction;
    private bool Holding = false;
    private float time;

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーの入力アクションの初期化と有効化
        custoomInputAction = new CustomInputAction();
        custoomInputAction.Openning.Enable();
        custoomInputAction.Openning.SkipButton.started += OnHoldStart;
        custoomInputAction.Openning.SkipButton.canceled += OnHoldEnd;

        if (CS_InputType.currentInputType == CS_InputType.InputType.Gamepad)
        {
            videoPlayer.clip = videoclip[0];
        }
        else
        {
            videoPlayer.clip = videoclip[1];
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

    // Update is called once per frame
    void Update()
    {
        if (!Holding)
            return;

        time += Time.deltaTime;

        if (time >= holdTime)
        {
            Holding = false;
            if (videoPlayer != null)
                videoPlayer.Stop();

            StartTransition();
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

    private void OnDestroy()
    {
        // プレイヤーの入力アクションの無効化
        if (custoomInputAction != null)
        {
            custoomInputAction.Openning.SkipButton.started -= OnHoldStart;
            custoomInputAction.Openning.SkipButton.canceled -= OnHoldEnd;
            custoomInputAction.Openning.Disable();
        }
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
