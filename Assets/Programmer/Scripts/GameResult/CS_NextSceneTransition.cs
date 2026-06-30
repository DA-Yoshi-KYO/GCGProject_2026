using UnityEngine;
using UnityEngine.SceneManagement;

public class CS_NextSceneTransition : MonoBehaviour
{
    [SerializeField] string sceneName;

    void Update()
    {
        if (!gameObject.activeSelf) return;

        GameObject fadeCanvas = GameObject.Find("FadeCanvas");
        if (fadeCanvas == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        CS_SceneTransition transition = fadeCanvas.GetComponent<CS_SceneTransition>();
        if (transition == null)
            SceneManager.LoadScene(sceneName);
        else
            transition.StartSceneTransition(sceneName);
    }
}
