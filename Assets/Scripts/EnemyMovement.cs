using UnityEditor.Callbacks;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float myHealth = 15;
    public bool isDead = false; // new flag for death
    public bool sentryMode = false;
    public bool meleeMode = false;
    public float detectionRadius = 20f;
    public float stopRadius = 10f;
    public float rotationSpeed = 5f;
    public float moveSpeed = 3f;
    public float aimThreshold = 15f; // Angle threshold to determine "facing the player"
    public float deathMagnitude = 10f; // Adjust this value as needed

    private Transform playerTransform;
    public bool isFacingPlayer; // Public bool to be read by EnemyGun
    public bool isPlayerDetected = false;
    public bool canSeePlayer = false; // True if there's a clear line of sight

    public bool showDetectionRadius = true; // Toggle for showing the visual indicator
    private GameObject detectionIndicator;

    void Start()
    {
        // Create a detection indicator if enabled
        if (showDetectionRadius)
        {
            // Create a sphere primitive
            detectionIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // Remove its collider so it doesn’t interfere with gameplay
            Destroy(detectionIndicator.GetComponent<Collider>());
            // Parent it to the enemy so it moves with it
            detectionIndicator.transform.SetParent(transform);
            detectionIndicator.transform.localPosition = Vector3.zero;
            // Scale it so its diameter equals detectionRadius * 2
            detectionIndicator.transform.localScale = new Vector3(detectionRadius * 2, detectionRadius * 2, detectionRadius * 2);

            // Create a semi-transparent material
            Renderer rend = detectionIndicator.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            Color transparentRed = new Color(1f, 0f, 0f, 0.2f);
            mat.color = transparentRed;
            // Set up the material for transparency
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            rend.material = mat;
        }
    }

    void Update()
    {
        // If the detection radius can change dynamically, update the indicator's scale.
        if (detectionIndicator != null)
        {
            detectionIndicator.transform.localScale = new Vector3(detectionRadius * 2, detectionRadius * 2, detectionRadius * 2);
        }
        
        if (!isDead)
        {
            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Directly assign the result of CanSeePlayer() to the class-level canSeePlayer variable
            canSeePlayer = CanSeePlayer();

            if (!canSeePlayer) // Check if we have a clear line of sight from enemy to player
            {
                isPlayerDetected = false; // If not, then we don't know where the player is
            }
            else
            {
                if (distanceToPlayer <= detectionRadius) // If the player is within the detection radius
                {
                    isPlayerDetected = true; // We detect the player
                    RotateTowardsPlayer(); // We rotate towards the player

                    if (distanceToPlayer > stopRadius && !sentryMode) // If outside the stopRadius...
                    {
                        MoveTowardsPlayer(); // Move towards player
                    }
                    CheckFacingPlayer();
                }
                else
                {
                    isPlayerDetected = false;
                }
            }
        }

        if (myHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Enemy died");
        
        // Drop the gun: Get the EnemyGun component from the child
        EnemyGun enemyGun = GetComponentInChildren<EnemyGun>();
        if (enemyGun != null)
        {
            // Detach the gun so it becomes independent
            enemyGun.transform.parent = null;
            // Add a Rigidbody to the gun so that physics will make it fall
            if (enemyGun.GetComponent<Rigidbody>() == null)
            {
                enemyGun.gameObject.AddComponent<Rigidbody>();
            }
            // Disable the shooting behavior on the dropped gun
            enemyGun.enabled = false;
        }

        // Let the enemy fall: Ensure the enemy has a Rigidbody and enable gravity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;

        // Apply a torque to make the enemy fall backward.
        // Using -transform.right rotates the enemy so that its head falls backward.
        rb.AddTorque(-transform.right * deathMagnitude, ForceMode.Impulse);

        // Disable the movement script
        // this.enabled = false;
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void CheckFacingPlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        isFacingPlayer = angle <= aimThreshold;
    }

    // Check if the enemy has a clear line of sight to the player
    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        RaycastHit hit;

        // Raycast that ignores triggers, checks for obstacles
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, detectionRadius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // If the hit object is the player, the player is visible
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    // Draw gizmos for detection and stop distance if the enemy is selected
    private void OnDrawGizmosSelected()
    {
        // Draw the detection radius in red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw the stop distance in blue
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopRadius);
    }
}