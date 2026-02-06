using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class NextSceneTime : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] string nextSceneName;

    [Header("Fade")]
    [SerializeField] Image fadeImage;
    [SerializeField] float fadeDuration = 1f;

    [Header("Delay")]
    [SerializeField] float waitTime = 2f;   // 페이크 로딩 시간


    void Start()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Loading);
        PlayLoadingSequence();
    }

    void PlayLoadingSequence()
    {
        Sequence seq = DOTween.Sequence();

        // 가짜 로딩 시간
        seq.AppendInterval(waitTime);

        // 페이드 아웃
        seq.Append(
            fadeImage.DOFade(1f, fadeDuration)
                .SetEase(Ease.Linear)
        );

        // 씬 로드
        seq.OnComplete(() =>
        {
            SceneManager.LoadScene(nextSceneName);
        });
    }
}
