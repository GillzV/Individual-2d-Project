using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 20f; 
    public Rigidbody2D rb;
    public int damage = 25;
    
    [Header("Destroy Settings")]
    [Tooltip("Extra margin beyond camera bounds before destroying")]
    public float destroyMargin = 2f;
    
    private Camera mainCamera;

    private GameObject player;
    private float force;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        Vector3 direction = (player.transform.position - transform.position).normalized;
        rb.velocity = direction * speed;  

           
        mainCamera = Camera.main;
        Destroy(gameObject, 10f);
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
        // Check for Player component
        PlayerController2D player = hitInfo.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        
    }
}
