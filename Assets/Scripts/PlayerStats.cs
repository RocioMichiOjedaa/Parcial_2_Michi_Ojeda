using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100;
    public float currentHealth;

    [Header("Estamina")]
    public float maxStamina = 10;
    public float currentStamina;
    public float staminaRegenRate = 1f; // +1 por segundo cuando no está en chase
    public bool canRegenStamina = true;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (canRegenStamina && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        Debug.Log("Vida player: " + currentHealth);

        if (currentHealth <= 0)
            Debug.Log("PLAYER DEAD!");
    }

    public void DrainStamina(float amountPerSecond)
    {
        canRegenStamina = false;
        currentStamina -= amountPerSecond * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    public void StopDraining()
    {
        canRegenStamina = true;
    }
}
