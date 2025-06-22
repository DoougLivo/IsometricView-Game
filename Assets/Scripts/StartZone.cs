using UnityEngine;

public class StartZone : MonoBehaviour
{
    public GameManager manager; // 게임매니저 오브젝트 변수

    private void OnTriggerEnter(Collider other)
    {
        // 스테이지 존과 접촉된 대상의 태그가 Player면 스테이지 시작
        if (other.gameObject.tag == "Player")
            manager.StageStart();
    }
}
