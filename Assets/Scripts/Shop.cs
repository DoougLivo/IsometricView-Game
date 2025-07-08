using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI 사용시 필요함. 없으면 텍스트가 안나옴

public class Shop : MonoBehaviour
{
    [SerializeField] RectTransform uiGroup; // 상점 UI 가져오기
    [SerializeField] Animator anim; // 캐릭터 애니메이션

    public Player enterPlayer; // 플레이어 정보 가져오기 위함

    public GameObject[] itemObj; // 아이템 프리팹
    public int[] itemPrice; // 아이템 가격
    public Transform[] itemPos; // 아이템이 생성될 위치
    public Text talkText; // 금액 부족을 알리기 위한 대사 텍스트
    public string[] talkData;

    // 오디오 소스
    public AudioSource buySound; // 구매 소리
    public AudioSource deniedSound; // 구매 실패 소리

    public void Enter(Player player) // 상점 존에 들어갔을 때
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero; // UI를 화면 정중앙에 오게 함
    }

    public void Exit() // 상점 존에서 나갔을 때
    {
        anim.SetTrigger("doHello"); // 상점 나가면 인사 애니메이션 발동
        uiGroup.anchoredPosition = Vector3.down * 1100; // UI를 화면 아래로 내림
    }

    public void Buy(int index) // 물건 구매 함수 (어떤 물건인지 파악하기 위한 인덱스)
    {
        int price = itemPrice[index]; // 아이템 가격
        if (price > enterPlayer.coin) // 플레이어가 돈이 부족할 때
        {
            deniedSound.Play(); // 돈 부족 사운드
            StopCoroutine(Talk()); // 꾹 누르면 계속 실행되기 때문에 실행중인 코루틴을 꺼놓아야 함
            StartCoroutine(Talk()); // 다시 재시작되게 함
            return;
        }

        // 돈이 있을 때
        enterPlayer.coin -= price; // 플레이어의 지갑에서 물건가격을 뺌
        buySound.Play(); // 구매 소리 재생
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3); // 랜덤 위치 생성
        Instantiate(itemObj[index], itemPos[index].position + ranVec, itemPos[index].rotation); // 물건 생성
    }

    IEnumerator Talk()
    {
        talkText.text = talkData[1]; // 금액 부족 텍스트
        yield return new WaitForSeconds(2f); // 2초 뒤
        talkText.text = talkData[0]; // 원래 있던 대사
    }
}
