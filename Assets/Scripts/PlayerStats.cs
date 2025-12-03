using System;
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100;
    public float currentHealth;

    [Header("Estamina")]
    public float maxStamina = 10;
    public float currentStamina;
    public float staminaRegenRate = 1f;
    public bool canRegenStamina = true;

    private bool died = false;
    private Action onDied = null;

    [Header("UI")]
    [SerializeField] private TMP_Text lifeText;

    public bool Died => died;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        lifeText.text = "Vida: " + currentHealth;
    }

    private void Update()
    {
        if (canRegenStamina && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    public void Init(Action onDied)
    {
        this.onDied = onDied;
    }

    public void TakeDamage(float dmg)
    {
        if (died) return;

        currentHealth -= dmg;
        Debug.Log("Vida player: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("PLAYER DEAD!");
            currentHealth = 0;

            died = true;
            onDied?.Invoke();
        }

        lifeText.text = "Vida: " + currentHealth;
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

    public void ResetStats()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        canRegenStamina = true;
        died = false;
    }

    public bool Heal(float amount)
    {
        if (Died) return false;
        if (currentHealth >= maxHealth) return false;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        lifeText.text = "Vida: " + currentHealth;

        Debug.Log($"Player healed {amount}. HP: {currentHealth}");

        return true; // Sí se curó
    }

}
