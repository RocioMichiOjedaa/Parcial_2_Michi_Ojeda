using UnityEngine;

[CreateAssetMenu(fileName = "New Soldier", menuName = "Enemies/Soldier")]
public class Soldier : ScriptableObject
{
    [Header("Stats")]
    public float maxHealth = 50f;

    [Header("Vision")]
    public float visionAngle = 45f;
    public float visionRange = 7f;

    [Header("Chase")]
    public float chaseDistance = 5f;
    public float staminaDrainRate = 1f;

    [Header("Attack")]
    public bool hasWeapon = true;
    public float attackDamage = 10f;
    public float attackRange = 6f;
    public float attackCooldown = 1f;
}
