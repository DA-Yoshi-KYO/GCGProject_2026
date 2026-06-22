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
    [Header("動画再生中に遷移するキー")][SerializeField] private KeyCode pressKey;
    [Header("VideoPlayerのコンポーネントが入ったゲームオブジェクト")][SerializeField] private VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.loopPointReached += OnMovieFinished;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(pressKey))
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
}
