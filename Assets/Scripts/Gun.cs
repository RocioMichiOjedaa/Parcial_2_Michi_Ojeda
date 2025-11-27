using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 10f;
    public float range = 50f;
    public float fireRate = 0.4f;

    [Header("Ammo")]
    public int magSize = 15;
    public int currentAmmo;
    public bool isReloading = false;

    public TMP_Text bulletsText;

    float nextTimeToShoot;
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
        currentAmmo = magSize;

        bulletsText.text = "Balas: " + currentAmmo + " / " + magSize;
    }

    public void Shoot()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            Debug.Log("SIN BALAS");
            return;
        }

        if (Time.time < nextTimeToShoot) return;
        nextTimeToShoot = Time.time + fireRate;

        currentAmmo--;
        bulletsText.text = "Balas: " + currentAmmo + " / " + magSize;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
        {
            if (hit.collider.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(damage);
            }
            else if (hit.collider.GetComponentInParent<EnemyAI>())
            {
                EnemyAI enemyGO = hit.collider.GetComponentInParent<EnemyAI>();
                enemyGO.TakeDamage(damage);
            }
            else if (hit.collider.TryGetComponent(out SurveillanceCamera sc))
            {
                sc.TakeDamage(damage);
            }
            else if (hit.collider.GetComponentInParent<SurveillanceCamera>())
            {
                SurveillanceCamera scParent = hit.collider.GetComponentInParent<SurveillanceCamera>();
                scParent.TakeDamage(damage);
            }
        }

        Debug.Log("Balas restantes: " + currentAmmo);
    }

    public void Reload()
    {
        if (currentAmmo == magSize)
        {
            Debug.Log("Cargador completo, no recargo");
            return;
        }

        Debug.Log("Recargando...");
        isReloading = true;

        currentAmmo = magSize;
        isReloading = false;

        bulletsText.text = "Balas: " + currentAmmo + " / " + magSize;
    }
}
