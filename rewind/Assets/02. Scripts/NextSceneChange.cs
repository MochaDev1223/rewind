using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Cinemachine;

public class NextSceneChange : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] string nextSceneName;

    [Header("Fade")]
    [SerializeField] Image fadeImage;
    [SerializeField] float fadeDuration = 1f;

    [Header("Zoom")]
    [SerializeField] CinemachineCamera vcam;
    [SerializeField] float zoomSize = 2.5f;
    [SerializeField] float zoomDuration = 1f;

    bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading) return;
        
        // 플레이어만 감지
        if (other.CompareTag("Player"))
        {
            isLoading = true;
            PlayTransition();
        }
    }

    void PlayTransition()
    {
        // Confiner 비활성화
        var confiner = vcam.GetComponent<CinemachineConfiner2D>();
        if (confiner != null)
        {
            confiner.enabled = false;
        }

        Sequence seq = DOTween.Sequence();

        // 줌인
        seq.Append(
            DOTween.To(
                () => vcam.Lens.OrthographicSize,
                x => vcam.Lens.OrthographicSize = x,
                zoomSize,
                zoomDuration
            ).SetEase(Ease.InOutSine)
        );

        // 페이드 아웃 (줌인이랑 같이)
        seq.Join(
            fadeImage.DOFade(1f, fadeDuration)
        );

        seq.OnComplete(() =>
        {
            SceneManager.LoadScene(nextSceneName);
        });
    }
}