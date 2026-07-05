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

        videoPlayer.loopPointReached += OnMovieFinished;
    }

    // Update is called once per frame
    void Update()
    {
        if (custoomInputAction.Openning.SkipButton.triggered)
        {
            videoPlayer.Stop();
            fadeCanvas.GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);
        }
    }

    private void OnMovieFinished(VideoPlayer vp)
    {
        if (vp != videoPlayer)
            return;

        videoPlayer.Stop();
        fadeCanvas.GetComponent<CS_SceneTransition>().StartSceneTransition(sceneName);

    }

    private void OnDestroy()
    {
        // プレイヤーの入力アクションの無効化
        if (custoomInputAction != null)
            custoomInputAction.Openning.Disable();
    }
}
