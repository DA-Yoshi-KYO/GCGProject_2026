/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    カットシーンビデオの再生処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-03 | 初回作成
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CS_CutSceneVideo : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private SO_CutSceneVideo cutSceneVideoDataBase;

    [HideInInspector] public CutSceneData data;
    [HideInInspector] public CutSceneData nextData;
    private bool nextWait = false;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        //非表示
        rawImage.enabled = false;

        for (int i = 0 ; i < cutSceneVideoDataBase.cutSceneDatas.Length; ++i)
        {
            cutSceneVideoDataBase.cutSceneDatas[i].played = false;
        }
        videoPlayer.loopPointReached += OnMovieFinished;
    }

    // Update is called once per frame
    void Update()
    {
        //再生していた場合処理しない
        if (data.played)
            return;

        //再生
        if (rawImage.isActiveAndEnabled)
        {
            videoPlayer.Play();
            data.played = true;
        }
    }

    //ビデオの情報設定
    public void SetVideoInfo(string Situation)
    {
        if (!videoPlayer.isPlaying)
        {
            data = cutSceneVideoDataBase.cutSceneData[Situation];
        }
        else
        {
            nextWait = true;
            nextData = cutSceneVideoDataBase.cutSceneData[Situation];
        }
    }

    //ビデオの再生準備
    public void PlayVideo()
    {
        //再生していたら実行しない
        if (data.played)
            return;

        //ビデオのデータ設定
        videoPlayer.clip = data.videoClip;
        
        //表示
        rawImage.enabled = true;
    }

    //再生し終わったら停止
    private void OnMovieFinished(VideoPlayer vp)
    {
        rawImage.enabled = false;
        videoPlayer.Stop();

        if (nextWait)
            data = nextData;
            PlayVideo();
    }
}
