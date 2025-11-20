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

    float nextTimeToShoot;
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
        currentAmmo = magSize;
    }

    public void Shoot()
    {
        // no dispara si está recargando
        if (isReloading) return;

        // no dispara si no hay balas
        if (currentAmmo <= 0)
        {
            Debug.Log("SIN BALAS");
            return;
        }

        if (Time.time < nextTimeToShoot) return;
        nextTimeToShoot = Time.time + fireRate;

        currentAmmo--;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
        {
            if (hit.collider.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(damage);
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

        // no hay tiempo animado, así que recarga instantáneo
        currentAmmo = magSize;
        isReloading = false;
    }
}
