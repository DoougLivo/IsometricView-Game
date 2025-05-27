using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] RectTransform uiGroup; // 상점 UI 가져오기
    [SerializeField] Animator anim; // 캐릭터 애니메이션

    public Player enterPlayer; // 플레이어 정보 가져오기 위함

    public void Enter(Player player) // 상점 존에 들어갔을 때
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero; // UI를 화면 정중앙에 오게 함
    }

    public void Exit() // 상점 존에서 나갔을 때
    {
        anim.SetTrigger("doHello"); // 상점 나가면 인사 애니메이션 발동
        uiGroup.anchoredPosition = Vector3.down * 1000; // UI를 화면 아래로 내림
    }
}
