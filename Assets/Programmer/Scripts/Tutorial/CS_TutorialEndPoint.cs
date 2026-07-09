using UnityEngine;

public class CS_TutorialEndPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject.Find("FadeCanvas").GetComponent<CS_SceneTransition>().StartSceneTransition("MainScene");
        }
    }
}
