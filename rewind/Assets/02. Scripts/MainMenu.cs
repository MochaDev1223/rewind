using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject btnGameStart;

    void Awake()
    {
        AudioManager.instance.PlayBgm(true);
    }

    public void OnClickGameStart()
    {
        // 시간 흐름 정상화 (일시정지 상태에서 재시작할 경우를 대비)
        Time.timeScale = 1f;
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        // 스테이지 1 시작
        SceneManager.LoadScene("Stage1");
        AudioManager.instance.PlayBgm(false);
    }
}
