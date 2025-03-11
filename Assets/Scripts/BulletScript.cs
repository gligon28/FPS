using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float myDamage;
    public float survivalTime; // How long the bullet will be allowed to fly before it deletes itself
    public PlayerMovement playerMovementScript;

    void Start()
    {
        Destroy(gameObject, survivalTime); // Automatically destroy bullet after survivalTime is up

        GameObject playerObject = GameObject.Find("Player");
        playerMovementScript = playerObject.GetComponent<PlayerMovement>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerMovementScript.playerHealth -= myDamage;
            Debug.Log("Player hit! New health: " + playerMovementScript.playerHealth);
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Enemy")) // Check if the collided object is tagged "Enemy"
        {
            EnemyMovement enemyMovementScript = other.transform.parent.GetComponent<EnemyMovement>();
            
            if (enemyMovementScript != null) // Ensure the script is found on the parent object
            {
                enemyMovementScript.myHealth -= myDamage; // Subtract the damage from the enemy's health
                Destroy(gameObject); // Destroy the bullet after it hits
            }
        }
        else if (other.gameObject.tag != "Bullet")
        {
            Destroy(gameObject); // Destroy bullet on impact with anything except other bullets
        }
    }
}
