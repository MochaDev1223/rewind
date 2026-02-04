using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform player;
    [Header("Follow")]
    [SerializeField] float ySmoothSpeed = 5f;   // Y축 따라오는 속도
    [SerializeField] float yOffset = 1f;         // 캐릭터를 화면에서 살짝 아래에 두기

    [Header("Boundary")]
    [SerializeField] Vector2 minCameraBoundary;
    [SerializeField] Vector2 maxCameraBoundary;

    void LateUpdate() // 카메라는 LateUpdate가 정석
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        // transform.position = new Vector3(player.position.x, transform.position.y, transform.position.z);

        // 목표 위치 계산
        float targetX = player.position.x;
        float targetY = player.position.y + yOffset;

        // 현재 카메라 위치
        Vector3 camPos = transform.position;

        // X는 즉시 따라감 (플랫포머 조작감 유지)
        camPos.x = targetX;

        // Y는 부드럽게 따라감 (점프 높이감)
        camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * ySmoothSpeed);

        // 카메라 범위 제한
        // camPos.x = Mathf.Clamp(camPos.x, minCameraBoundary.x, maxCameraBoundary.x);
        // camPos.y = Mathf.Clamp(camPos.y, minCameraBoundary.y, maxCameraBoundary.y);

        transform.position = camPos;
    }
}
