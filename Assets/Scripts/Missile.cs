using UnityEngine;

public class Missile : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.right * 80 * Time.deltaTime); // 미사일 회전
    }
}
