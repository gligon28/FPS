using UnityEngine;
using System.Collections;

public class EnemyGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public bool isAutomatic = true;
    public float fireRate = 1f;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public int bulletsPerShot = 1;
    public float bulletSpread = 2f;
    public float enemyAccuracy = 3f;
    public float attackDamage = 5;

    [Header("Ammo Settings")]
    public int maxAmmo = 10;
    private int currentAmmo;
    public float reloadTime = 2f;
    //private bool isReloading = false;
    private float nextFireTime = 0f;

    private EnemyMovement enemyMovementScript;
    public PlayerMovement playerMovementScript;

    void Start()
    {
        currentAmmo = maxAmmo;
        enemyMovementScript = GetComponentInParent<EnemyMovement>(); // Get reference to the enemy movement script
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        playerMovementScript = playerObject.GetComponent<PlayerMovement>();
    }

    void Update()
    {

        // If the enemy is dead, don't shoot
        if (enemyMovementScript != null && enemyMovementScript.isDead)
            return;

        if (Time.time >= nextFireTime && enemyMovementScript.isFacingPlayer && enemyMovementScript.isPlayerDetected)
        {
            if (!enemyMovementScript.meleeMode)
            {
                Debug.Log("Calling Shoot function");
                Shoot();
            }
            else
            {
                Debug.Log("Calling Stab function");
                Stab();
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            Vector3 shootDirection = (enemyMovementScript.transform.forward).normalized;

            float spreadX = Random.Range(-bulletSpread, bulletSpread);
            float spreadY = Random.Range(-bulletSpread, bulletSpread);
            Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
            shootDirection = spreadRotation * shootDirection;

            float accuracyX = Random.Range(-enemyAccuracy, enemyAccuracy);
            float accuracyY = Random.Range(-enemyAccuracy, enemyAccuracy);
            Quaternion accuracyRotation = Quaternion.Euler(accuracyY, accuracyX, 0);
            shootDirection = accuracyRotation * shootDirection;

            // Instantiate the bullet
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

            // Get the bullet's script and assign the attackDamage to myDamage
            BulletScript bulletScript = bullet.GetComponent<BulletScript>(); // Assuming the bullet script is named Bullet
            if (bulletScript != null)
            {
                bulletScript.myDamage = attackDamage; // Assign attackDamage to bullet's myDamage
            }

            // Apply the velocity to the bullet
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = shootDirection * bulletSpeed; // Apply the shoot direction and bullet speed
            }
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
    }
    void Stab()
    {
        playerMovementScript.playerHealth -= attackDamage;

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
    }
}
