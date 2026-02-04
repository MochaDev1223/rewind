using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AlertIcon : MonoBehaviour
{
    public RectTransform rect;
    public Image image;

    private Vector2 originPos;
    private Sequence seq;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        // 최초 위치 저장
        originPos = rect.anchoredPosition;
    }

    public void Play()
    {
        // 이전 트윈 정리
        seq?.Kill();
        rect.DOKill();
        image.DOKill();

        // 위치 초기화
        rect.anchoredPosition = originPos;

        // 알파 초기화
        Color c = image.color;
        c.a = 1f;
        image.color = c;

        seq = DOTween.Sequence();

        // 위로 살짝 튀어오르기
        seq.Append(
            rect.DOAnchorPosY(originPos.y + 50f, 0.25f)
                .SetEase(Ease.OutBack)
        );
        seq.AppendInterval(0.25f);

        // 페이드 아웃
        seq.Join(
            image.DOFade(0f, 0.3f)
                .SetDelay(0.15f)
        );

        seq.OnComplete(() =>
        {
            rect.anchoredPosition = originPos;
        });
    }
}
