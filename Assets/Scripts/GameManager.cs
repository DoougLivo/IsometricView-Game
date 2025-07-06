using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement; // Scene 관련 함수 사용 시 필요함

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform[] enemyZones; // 몬스터 리스폰 존
    public GameObject[] enemies; // 몬스터 변수
    public List<int> enemyList; // 스테이지 별 몬스터 리스트

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;
    public Text maxScoreTxt;
    public Text scoreTxt;
    public Text stageTxt;
    public Text playTimeTxt;
    public Text playerHealthTxt;
    public Text playerAmmoTxt;
    public Text playerCoinTxt;
    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;
    public Text enemyATxt;
    public Text enemyBTxt;
    public Text enemyCTxt;
    public RectTransform bossHealthGroup; // 보스 체력 UI / RectTransform - UI 위치 변경
    public RectTransform bossHealthBar; // 보스 체력 게이지바
    public Text curScoreTxt;
    public Text bestTxt;
    public Text savedMaxScoreTxt;

    public void Awake()
    {
        enemyList = new List<int>(); // 리스트 초기화
        
        //PlayerPrefs.SetInt("MaxScore", 0); // 간편한 저장 함수
        //maxScoreTxt.alignment = TextAnchor.LowerCenter; // 실행하면 텍스트의 위치가 바뀌어서 다시 정렬함 
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore")); // {0:n0} - 천 단위 콤마 표시

        // key가 있는지 확인 후 없으면 0으로 저장
        if (!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);
    }

    // 게임 시작
    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }

    // 게임 오버
    public void GameOver()
    {
        gamePanel.SetActive(false); // 게임 판넬 끄고
        overPanel.SetActive(true); // 게임오버 판넬 활성화
        curScoreTxt.text = string.Format("{0:n0}", scoreTxt.text); // 게임중의 점수를 게임오버 판넬의 점수로 나오게 함
        //curScoreTxt.alignment = TextAnchor.LowerCenter; // 정렬

        int maxScore = PlayerPrefs.GetInt("MaxScore"); // PlayerPrefs에 저장되어 있는 최고점수를 불러옴
        
        if (player.score > maxScore) // 플레이어의 점수가 저장된 최고 점수보다 크면
        {
            bestTxt.gameObject.SetActive(true); // Best 오브젝트 활성화
            PlayerPrefs.SetInt("MaxScore", player.score); // 플레이어의 점수를 최고 점수로 저장
        }

        savedMaxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore")); // 저장된 최고 점수 표시
        //savedMaxScoreTxt.alignment = TextAnchor.LowerCenter; // 정렬
    }

    // 재 시작
    public void ReStart()
    {
        SceneManager.LoadScene(0); // 장면을 다시 불러 재 시작함
    }

    // 스테이지 시작
    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        // 스테이지 시작하면 몬스터 소환 존 모두 활성화
        foreach (Transform zone in enemyZones)
            zone.gameObject.SetActive(true);

        isBattle = true;
        StartCoroutine(InBattle());
    }

    // 코루틴으로 전투상태 구현
    IEnumerator InBattle()
    {
        if (stage % 10 == 0) // 10 스테이지 마다 보스 소환
        {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation);
            // 프리펩에 있는 오브젝트는 씬에 있는 오브젝트에 접근이 불가능함 - 예) 몬스터의 타겟이 플레이어로 지정 안돼있음
            Enemy enemy = instantEnemy.GetComponent<Enemy>(); // 적 스크립트를 가져옴
            enemy.target = player.transform; // 플레이어의 위치를 타겟으로 지정함
            enemy.manager = this; // GameManager를 Enemy 스크립트에 넣음
            boss = instantEnemy.GetComponent<Boss>(); // 보스 변수에 채우기
        } 
        else // 일반 스테이지
        {
            // 몬스터 소환 리스트 데이터 채우기
            for (int i = 0; i < stage; i++) // 스테이지 수 만큼 몬스터 생성함
            {
                int ran = Random.Range(0, 3); // A,B,C 몬스터 중 랜덤 뽑음
                enemyList.Add(ran);

                // 몬스터 마릿수 계산
                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
            }

            // 지속적인 몬스터 소환
            while (enemyList.Count > 0) // 소환 리스트가 끝날 때 까지 반복
            {
                int ranZone = Random.Range(0, 4); // 소환 위치는 랜덤으로 
                // 생성 시, 적 소환 리스트의 '첫번째 데이터'를 사용
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
                // 프리펩에 있는 오브젝트는 씬에 있는 오브젝트에 접근이 불가능함 - 예) 몬스터의 타겟이 플레이어로 지정 안돼있음
                Enemy enemy = instantEnemy.GetComponent<Enemy>(); // 적 스크립트를 가져옴
                enemy.target = player.transform; // 플레이어의 위치를 타겟으로 지정함
                enemy.manager = this; // GameManager를 Enemy 스크립트에 넣음

                // 적 생성 후, 사용된 첫번째 데이터(인덱스 0)는 RemoveAt() 함수로 삭제함
                enemyList.RemoveAt(0);

                // 안전하게 while문을 돌리기 위해선 꼭 yield return 을 포함 하기
                yield return new WaitForSeconds(4f); // 4초 뒤 몬스터 소환
            }
        }

        // 남은 몬스터 갯수 검사
        while (enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0) // 몬스터가 남아 있는 경우
        {
            yield return null; // 밑의 코드를 실행하지 않고 기다림 // 1 프레임임. while문에는 이걸 넣어야 안전함
        }

        // 몬스터가 다 죽으면 밑에 코드 실행됨

        yield return new WaitForSeconds(4f); // 4초 후
        boss = null; // 보스 변수 값을 null로 만듬 (보스가 있는 경우에 보스 체력 UI를 사라지게 하기 위함)
        StageEnd(); // 스테이지 종료
    }

    // 스테이지 종료
    public void StageEnd()
    {
        player.transform.position = Vector3.up * 0.7f; // 스테이지 종료 시 플레이어 원 위치

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        // 스테이지 끝나면 몬스터 소환 존 모두 비활성화
        foreach (Transform zone in enemyZones)
            zone.gameObject.SetActive(false);

        isBattle = false;
        stage++; // 스테이지 값 1 증가
    }

    void Update()
    {
        if (isBattle)
            playTime += Time.deltaTime;
    }

    void LateUpdate() // Update()가 끝난 후 호출되는 생명주기, 데이터 처리 후에 호출할 때 사용함
    {
        // 상단 UI
        // 최고 점수
        scoreTxt.text = string.Format("{0:n0}", player.score);
        // 스테이지
        stageTxt.text = "STAGE " + stage;
        // 플레이타임
        int hour = (int)(playTime / 3600); // 초단위 시간을 3600, 60으로 나누어 시분초로 계산
        int min = (int)((playTime - hour * 3600) / 60); // 먼저 시간을 계산하고 남은 시간에 60을 나눔
        int second = (int)(playTime % 60); // 시,분 계산 후 60으로 나눈 나머지가 초

        playTimeTxt.text = string.Format("{0:00}", hour) + ":" 
                            + string.Format("{0:00}", min) + ":" 
                            + string.Format("{0:00}", second);

        // 플레이어 UI
        playerHealthTxt.text = player.health + " / " + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);
        if (player.equipWeapon == null) // 장착 무기가 없을 때
            playerAmmoTxt.text = "- / " + player.ammo;
        else if (player.equipWeapon.type == Weapon.Type.Melee) // 장착 무기가 해머일 때
            playerAmmoTxt.text = "- / " + player.ammo;
        else // 장착 무기가 총일 때
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;

        // 무기 UI - 무기 아이콘은 보유 상황에 따라 알파값만 변경
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0); // 해머
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0); // 핸드건
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0); // 서브머신건
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0); // 수류탄

        // 몬스터 숫자 UI
        enemyATxt.text = enemyCntA.ToString();
        enemyBTxt.text = enemyCntB.ToString();
        enemyCTxt.text = enemyCntC.ToString();

        // 보스 체력 UI - 보스 체력 이미지의 Scale을 남은 체력 비율에 따라 변경
        if (boss != null) // 보스가 있을 때만
        {
            bossHealthGroup.anchoredPosition = Vector3.down * 30; // 보스가 있을 때 보스 체력 UI 표시
            if (boss.curHealth <= 0) // 0 미만이면 0으로 고정
                bossHealthBar.localScale = new Vector3(0, 1, 1);
            else
                bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }
        else // 보스가 없을 때
        {
            bossHealthGroup.anchoredPosition = Vector3.up * 200; // 보스 체력 UI 안보이게 함
        }
    }
}
