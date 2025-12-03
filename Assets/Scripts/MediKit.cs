using UnityEngine;

public class MediKit : MonoBehaviour
{
    public float healAmount = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null)
            return;

        bool healed = stats.Heal(healAmount);

        if (healed)
        {
            Destroy(gameObject);
        }
    }
}
