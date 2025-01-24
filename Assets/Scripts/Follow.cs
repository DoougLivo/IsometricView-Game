using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;

    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
