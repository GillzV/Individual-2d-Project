using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; 
    public Rigidbody2D rb;
    public int damage = 40;
    
    [Header("Destroy Settings")]
    [Tooltip("Extra margin beyond camera bounds before destroying")]
    public float destroyMargin = 2f;
    
    private Camera mainCamera;
    private bool hasLeftScreen = false;
    
    void Start()
    {
        // Only set default velocity if no custom velocity has been set
        if (rb.velocity == Vector2.zero)
        {
            rb.velocity = transform.right * speed;
        }
        
        mainCamera = Camera.main;
        
    }
    
    void Update()
    {
        // Check if bullet is outside camera bounds
        if (mainCamera != null && IsOutsideCameraBounds())
        {
            Destroy(gameObject);
        }
    }
    
    private bool IsOutsideCameraBounds()
    {
        // Get bullet position in viewport coordinates (0-1 range)
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        
        // Convert margin from world units to viewport units
        float marginX = destroyMargin / (mainCamera.orthographicSize * mainCamera.aspect * 2f);
        float marginY = destroyMargin / (mainCamera.orthographicSize * 2f);
        
        // Check if outside bounds (with margin)
        bool outsideX = viewportPos.x < -marginX || viewportPos.x > 1f + marginX;
        bool outsideY = viewportPos.y < -marginY || viewportPos.y > 1f + marginY;
        
        return outsideX || outsideY;
    }
    
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Check for Enemy component
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        
        // Check for Raider component
        Raider raider = hitInfo.GetComponent<Raider>();
        if (raider != null)
        {
            raider.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        
        // If it hits something else (like walls), destroy the bullet
        Destroy(gameObject);
    }
}