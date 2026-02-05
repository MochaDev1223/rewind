using UnityEngine;

public class Enemy_Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어 데미지 처리
            Player player = collision.GetComponent<Player>();
            Animator playerAnim = player.GetComponent<Animator>();
            AudioManager.instance.PlaySfx(AudioManager.Sfx.PlayerHit);
            if (player != null)
            {
                player.currentHealth -= damage; // 플레이어 체력 1 감소
                if (player.currentHealth >= 1)
                {
                    playerAnim.SetTrigger("Hit");
                }
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
