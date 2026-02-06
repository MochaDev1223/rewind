using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class AlertTextUI : MonoBehaviour
{
    public Text alertText;

    public float typingDuration = 2f; // 타이핑 총 시간
    public float stayTime = 0.5f;     // 다 찍고 나서 잠깐 유지

    [Header("Boss Battle Start")]
    public UnityEvent OnAlertComplete; // 경고 완료 시 실행할 이벤트

    void Start()
    {
        PlayAlertSequence();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.BossAlert); // 보스용 사운드
    }

    public void PlayAlertSequence()
    {
        Sequence seq = DOTween.Sequence();

        alertText.gameObject.SetActive(true);

        // 1️⃣ 경보! 침입자 감지!
        seq.AppendCallback(() =>
        {
            alertText.color = Color.white;
            alertText.text = "";
        });

        seq.Append(
            alertText.DOText("경보! 침입자 감지!", typingDuration)
                     .SetEase(Ease.Linear)
        );

        seq.AppendInterval(stayTime);

        // 살짝 페이드 아웃 (선택)
        seq.Append(alertText.DOFade(0f, 0.3f));

        // 2️⃣ 침입자 섬멸모드 가동 (빨간색)
        seq.AppendCallback(() =>
        {
            alertText.color = Color.red;
            alertText.text = "";
        });

        seq.Append(
            alertText.DOText("침입자 섬멸모드 가동", typingDuration)
                     .SetEase(Ease.Linear)
        );

        seq.AppendInterval(stayTime);
        seq.Append(alertText.DOFade(0f, 0.3f));

        // 종료 - 보스 전투 시작!
        seq.OnComplete(() =>
        {
            // 이벤트 실행 (보스 활성화)
            OnAlertComplete?.Invoke();

            alertText.gameObject.SetActive(false);
            gameObject.SetActive(false);
        });
    }
}