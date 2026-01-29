using UnityEngine;

public class BackGround : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Material myMaterial;


    void Start()
    {
        // 머터리얼 가져오기
        myMaterial = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // offset material 에서 가져오기
        Vector2 newOffset = myMaterial.mainTextureOffset;
        newOffset.Set(newOffset.x + (scrollSpeed * Time.deltaTime), 0);

        // metarial에 offset 값을 넣어준다
        myMaterial.mainTextureOffset = newOffset;
    }
}
