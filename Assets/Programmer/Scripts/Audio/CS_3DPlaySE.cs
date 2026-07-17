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

    [SerializeField] private GameObject objectPrent;//生成した3DSEオブジェクトをまとめる親オブジェクト

    public enum SEMode
    {
        Normal,
        Reverb,
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Option.Instance != null)
        {
            currentVolume = Option.Instance.GetSEVolume() / 100.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //オプション画面開いたら音源調整の関数呼び出す
        if (Option.Instance != null)
        {
            if (Option.Instance.GetIsOptionUIActive())
            {
                SE3DOption();
            }
        }
    }


    public GameObject PlayOneShotSE(string currentSituation, Vector3 pos, string gameObjectName, SEMode seMode = SEMode.Normal)
    {
        if (!dataBase.seData.TryGetValue(currentSituation, out SE3DData data) || data == null)
            return null;

        //Snapshot切り替え
        ChangeSnapshot(currentSituation, seMode.ToString(), 0.5f);

        // 再生処理
        GameObject obj = new GameObject(gameObjectName.ToString());
        obj.transform.SetParent(objectPrent.transform);
        obj.transform.position = pos;

        AudioSource audioSource = obj.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = currentVolume;
        audioSource.maxDistance = 30.0f;

        ChangeAudioMixerGroup(currentSituation, audioSource, seMode.ToString());

        audioSource.PlayOneShot(data.audioClip);
        Destroy(obj, data.audioClip.length);

        return obj;
    }

    private void ChangeSnapshot(string currentSituation, string snapshotName, float time = 1.0f)
    {
        if (!dataBase.seData.TryGetValue(currentSituation, out SE3DData data) || data == null) return;
        foreach (var item in data.audioMixerSnapshot)
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
        if (!dataBase.seData.TryGetValue(currentSituation, out SE3DData data) || data == null) return;
        foreach (var item in data.audioMixerGroup)
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
        if (!dataBase.seData.TryGetValue(currentSituation, out SE3DData data) || data == null || data.audioClip == null) return 0.0f;
        return data.audioClip.length;
    }


    //SE3Dのオプションでの音量調整
    public void SE3DOption()
    {
        currentVolume = Option.Instance.GetSEVolume() / 100.0f;
    }
}
