using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool isPaused = false;

    public GameObject btnRestart;
    public GameObject btnPause;
    public GameObject btnPlay;
    public GameObject PauseMenu;
    public GameObject btnHome;
    public GameObject btnRetry;
    public GameObject btnSetting;
    public GameObject Notice;


    void Start()
    {
        Time.timeScale = 1f;
        
        AudioManager.instance.PlayBgm(true);
    }

    void Update()
    {
        // ESC 키를 누르면 일시정지/재생 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 현재 일시정지 상태인지 확인
            if (Time.timeScale == 0f)
            {
                OnClickPlay();
            }
            else
            {
                OnClickPause();
            }
        }
    }
    

    //restart 버튼을 누르면
    public void OnClickRestart()
    {
        // 시간 흐름 정상화 (일시정지 상태에서 재시작할 경우를 대비)
        Time.timeScale = 1f;
        isPaused = false;
        // 현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void OnClickPause()
    {
        // 게임이 멈춤
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        Time.timeScale = 0f;
        isPaused = true;
        PauseMenu.SetActive(true);
        btnPause.SetActive(false);
        btnPlay.SetActive(true);
    }

    public void OnClickPlay()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        // 게임이 다시 동작
        Time.timeScale = 1f;
        isPaused = false;
        PauseMenu.SetActive(false);
        btnPause.SetActive(true);
        btnPlay.SetActive(false);
    }

    public void OnClickNotice()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        StartCoroutine(NoticeRoutine());
    }

    IEnumerator NoticeRoutine()
    {
        Notice.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        Notice.SetActive(false);
    }

    // 홈 버튼 (메인 메뉴로 이동)
    public void OnClickHome()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        // 시간 흐름 정상화
        Time.timeScale = 1f;
        isPaused = false;

        // 메인 메뉴 씬으로 이동
        SceneManager.LoadScene("MainMenu");
        AudioManager.instance.PlayBgm(false);
    }

}
