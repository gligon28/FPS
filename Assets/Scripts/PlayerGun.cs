using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class GunScript : MonoBehaviour
{
    [Header("Gun Settings")]
    public bool isAutomatic = false; // Toggle for semi/auto fire mode
    public float fireRate = 0.2f; // Time between shots

    [Header("Bullet Settings")]
    public float attackDamage = 5;
    public GameObject bulletPrefab; // Bullet prefab to spawn
    public Transform firePoint; // Where the bullet spawns
    public float bulletSpeed = 20f; // Speed of the bullet
    public int bulletsPerShot = 1; // Number of bullets per shot
    public float bulletSpread = 0f; // Angle variation for guns with spread

    [Header("Ammo Settings")]
    public int maxAmmo = 10; // Max bullets in a magazine
    private int currentAmmo; // current ammo
    public float reloadTime = 2f; // Time to reload
    private bool isReloading = false; // Reload state
    private float nextFireTime = 0f; // Tracks when we can fire next

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText; // Reference to the TextMeshPro object on our canvas

    [Header("Customization")]
    public Color gunColor = Color.gray; // Default color, but you can change this in the inspector

    // New: A timer to ignore input for a short period after regaining focus
    private float ignoreInputUntil = 0f;
    // Adjust this delay (in seconds) as needed
    private float inputIgnoreDelay = 1f;

    void Start()
    {
        currentAmmo = maxAmmo; // Fill magazine at start
        GetComponent<Renderer>().material.color = gunColor; // Assign the chosen color
    }

    void Update()
    {
        // Ignore input for a brief delay after regaining focus.
        if (Time.time < ignoreInputUntil)
        {
            return;
        }

        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (isAutomatic)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            {
                Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (currentAmmo > 0 && bulletPrefab != null && firePoint != null)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                // Raycast from the center of the screen to determine the target point.
                Ray cameraRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;
                Vector3 targetPoint;

                if (Physics.Raycast(cameraRay, out hit))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = cameraRay.GetPoint(1000);
                }

                // Calculate bullet direction
                Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

                // Apply spread
                float spreadX = Random.Range(-bulletSpread, bulletSpread);
                float spreadY = Random.Range(-bulletSpread, bulletSpread);
                Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
                shootDirection = spreadRotation * shootDirection;

                // Instantiate bullet and assign its rotation
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

                // Set bullet damage
                BulletScript bulletScript = bullet.GetComponent<BulletScript>();
                if (bulletScript != null)
                {
                    bulletScript.myDamage = attackDamage;
                }

                // Set bullet velocity
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = shootDirection * bulletSpeed;
                }
            }

            currentAmmo--;
            nextFireTime = Time.time + fireRate;
            UpdateAmmoUI();
        }
        else
        {
            Debug.LogWarning("BulletPrefab or FirePoint not assigned!");
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        UpdateAmmoUI();
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            if (!isReloading)
            {
                ammoText.text = currentAmmo + " / " + maxAmmo;
            }
            else
            {
                ammoText.text = "Reloading... / " + maxAmmo;
            }
        }
    }

    // When the game window regains focus, set a delay before input is accepted.
    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            ignoreInputUntil = Time.time + inputIgnoreDelay;
        }
    }
}
