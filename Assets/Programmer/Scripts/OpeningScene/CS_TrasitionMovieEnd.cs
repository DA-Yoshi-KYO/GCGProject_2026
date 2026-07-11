/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    オープニングシーンの処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-22 | 初回作成
 */
using UnityEngine;
using UnityEngine.Video;

public class CS_TrasitionMovieEnd : MonoBehaviour
{
    [Header("遷移するシーンの名前")][SerializeField] private string sceneName;
    [Header("FadeCanvasのPrefab格納")][SerializeField] private GameObject fadeCanvas;
    [Header("VideoPlayerのコンポーネントが入ったゲームオブジェクト")][SerializeField] private VideoPlayer videoPlayer;

    private CustomInputAction custoomInputAction;

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーの入力アクションの初期化と有効化
        custoomInputAction = new CustomInputAction();
        custoomInputAction.Openning.Enable();

        if (videoPlayer == null)
        {
            Debug.LogError("CS_TrasitionMovieEnd: videoPlayerが設定されていません。", this);
            return;
        }
        videoPlayer.loopPointReached += OnMovieFinished;
    }

    // Update is called once per frame
    void Update()
    {
        if (custoomInputAction.Openning.SkipButton.triggered)
        {
            if (videoPlayer != null) videoPlayer.Stop();
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
            custoomInputAction.Openning.Disable();
    }
}
