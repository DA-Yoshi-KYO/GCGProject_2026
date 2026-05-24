/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    3DのSE再生用
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-18 | 初回作成
 */
using UnityEngine;

public class CS_3DPlaySE : MonoBehaviour
{
    [SerializeField] private SO_3DSEDataBase dataBase;//データベース

    private float currentVolume = 1.0f;//現在の音量

    public enum SEMode
    {
        Normal,
        Reverb,
    }


    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void PlayOneShotSE(string currentSituation, Vector3 pos, string gameObjectName, SEMode seMode = SEMode.Normal)
    {
        SE3DData data = dataBase.seData[currentSituation];

        if (data == null)
            return;

        //Snapshot切り替え
        ChangeSnapshot(currentSituation, seMode.ToString(), 0.5f);

        // 再生処理
        GameObject obj = new GameObject(gameObjectName.ToString());
        obj.transform.position = pos;

        AudioSource audioSource = obj.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = currentVolume;

        ChangeAudioMixerGroup(currentSituation, audioSource, seMode.ToString());

        audioSource.PlayOneShot(data.audioClip);
        Destroy(obj, data.audioClip.length);

    }


    private void ChangeSnapshot(string currentSituation, string snapshotName, float time = 1.0f)
    {
        foreach (var item in  dataBase.seData[currentSituation].audioMixerSnapshot)
        {
            if (item.name == snapshotName)
            {
                item.TransitionTo(time);
                return;
            }
        }
    }

    private void ChangeAudioMixerGroup(string currentSituation, AudioSource audioSource, string groupName)
    {
        foreach (var item in dataBase.seData[currentSituation].audioMixerGroup)
        {
            if (item.name == groupName)
            {
                audioSource.outputAudioMixerGroup = item;
                return;
            }
        }
    }


    public float GetAudioLength(string currentSituation)
    {
        return dataBase.seData[currentSituation].audioClip.length;
    }
}
