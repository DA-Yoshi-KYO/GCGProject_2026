using UnityEngine;

[ExecuteAlways]
public class RoomGridMaterial : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().material.SetVector("_GridNum", new Vector4(gameObject.transform.localScale.x, gameObject.transform.localScale.z, 0.0f, 0.0f));
    }
}
