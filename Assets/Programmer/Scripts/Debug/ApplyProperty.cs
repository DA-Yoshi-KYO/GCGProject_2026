using UnityEngine;


public class ApplyProperty : MonoBehaviour
{
    float time;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time += Time.deltaTime;
    }
}
