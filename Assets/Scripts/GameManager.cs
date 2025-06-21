using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;

    public GameObject menuPanel;
    public GameObject gamePanel;
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

    public void Awake()
    {
        // PlayerPrefs.SetInt("MaxScore", 999999); // 간편한 저장 함수
        maxScoreTxt.alignment = TextAnchor.LowerCenter; // 실행하면 텍스트의 위치가 바뀌어서 다시 정렬함 
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore")); // {0:n0} - 천 단위 콤마 표시
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
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
        if (boss.curHealth <= 0) // 0 미만이면 0으로 고정
            bossHealthBar.localScale = new Vector3(0, 1, 1);
        else
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
    }
}
