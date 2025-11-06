using UnityEngine;

public class Bullet : MonoBehaviour {
    [Header("Behavior")]
    [Tooltip("The layer that is considered ground.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("The maximum distance the bullet can travel before being destroyed.")]
    [SerializeField] private float range = 10f;

    [Tooltip("How many seconds before the bullet is destroyed automatically (fallback).")]
    [SerializeField] private float lifetime = 3f;

    private Vector3 startPosition;

    private void Awake() {
        // Store the starting position
        startPosition = transform.position;

        // Destroy the bullet after its lifetime expires (as a fallback)
        Destroy(gameObject, lifetime);
    }

    private void Update() {
        // Check if the bullet has exceeded its range
        if (Vector3.Distance(startPosition, transform.position) >= range) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Check if the object we collided with is on the ground layer
        if (groundLayer == (groundLayer | (1 << other.gameObject.layer))) {
            // If it is ground, destroy the bullet
            Destroy(gameObject);
        }

        // Optional: Example of how to damage an enemy
        // if (other.CompareTag("Enemy")) {
        //     // other.GetComponent<EnemyHealth>().TakeDamage(10);
        //     Destroy(gameObject);
        // }
    }
}
