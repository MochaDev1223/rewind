// using UnityEngine;

// public class BackGround : MonoBehaviour
// {
//     public float scrollSpeed = 0.5f;
//     private Material myMaterial;


//     void Start()
//     {
//         // 머터리얼 가져오기
//         myMaterial = GetComponent<Renderer>().material;
//     }

//     void Update()
//     {
//         // offset material 에서 가져오기
//         Vector2 newOffset = myMaterial.mainTextureOffset;
//         newOffset.Set(newOffset.x + (scrollSpeed * Time.deltaTime), 0);

//         // metarial에 offset 값을 넣어준다
//         myMaterial.mainTextureOffset = newOffset;
//     }
// }

using UnityEngine;

public class BackGround : MonoBehaviour
{
    public Transform player;          // 플레이어 Transform
    public float parallaxFactor = 0.1f; // 배경이 얼마나 느리게 움직일지

    private Material myMaterial;
    private Vector3 lastPlayerPos;

    void Start()
    {
        myMaterial = GetComponent<Renderer>().material;
        lastPlayerPos = player.position;
    }

    void Update()
    {
        // 플레이어가 이번 프레임에 얼마나 움직였는지
        float deltaX = player.position.x - lastPlayerPos.x;

        // 현재 머터리얼 offset 가져오기
        Vector2 offset = myMaterial.mainTextureOffset;

        // 플레이어 이동 방향에 따라 offset 변경
        offset.x += deltaX * parallaxFactor;

        // 적용
        myMaterial.mainTextureOffset = offset;

        // 위치 갱신
        lastPlayerPos = player.position;
    }
}
