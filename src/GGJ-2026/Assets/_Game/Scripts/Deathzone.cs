using UnityEngine;
using UnityEngine.SceneManagement;

public class Deathzone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
        {
            if (player != null)
            {
                bool res = player.HandleDeath();
                if (res)
                {
                    SceneManager.LoadScene("Death");
                }
            }
        }
    }
}
