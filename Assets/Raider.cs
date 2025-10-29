using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Raider : MonoBehaviour
{
    [Header("Detection")]
    public Transform player;
    public float detectionRange = 8f;
    public float loseRange = 12f;
    public LayerMask playerLayer = 1;


    public AudioSource src;
    public AudioClip ShootSound, HurtSound, DeathSound;
    
    [Header("Combat")]
    public int health = 100;
    public int maxHealth = 100;
    public int damage = 25;
    public float fireRate = 1.5f; // Shots per second
    public float bulletSpeed = 15f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    [Header("Animations")]
    public Animator animator;
    public string shootAnimationBool = "isShooting";
    public string hurtAnimationBool = "isHurt";
    public string deathAnimationBool = "isDead";
    
    [Header("Visual Effects")]
    public GameObject deathEffect;
    public GameObject damageEffect;
    public Color damageFlashColor = Color.red;
    public float damageFlashDuration = 0.1f;
    public float deathAnimationDuration = 1f;
    public float damageAnimationDuration = 0.3f;
    
    
    // Private variables
    private bool isDead = false;
    private bool isDetectingPlayer = false;
    private bool isShooting = false;
    private bool isTakingDamage = false;
    private float lastShotTime = 0f;
    private float deathTimer = 0f;
    private float damageTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private int lastFacingDirection = 1; // 1 for right, -1 for left

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Get animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Find player if not assigned
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Store initial facing direction
        lastFacingDirection = transform.localScale.x >= 0 ? 1 : -1;
        
        // Initialize health
        health = maxHealth;
    }
    
    void Update()
    {
        if (isDead)
        {
            HandleDeath();
            return;
        }
        
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Detection logic with hysteresis to prevent flickering
        if (!isDetectingPlayer)
        {
            if (distanceToPlayer <= detectionRange)
            {
                isDetectingPlayer = true;
                OnPlayerDetected();
            }
        }
        else
        {
            if (distanceToPlayer > loseRange)
            {
                isDetectingPlayer = false;
                OnPlayerLost();
            }
            else
            {
                // Player is still in range, face player and shoot
                FacePlayer();
                TryShoot();
            }
        }
        
        // Handle damage animation
        if (isTakingDamage)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                isTakingDamage = false;
                if (animator != null)
                {
                    animator.SetBool(hurtAnimationBool, false);
                }
            }
        }
    }
    
    void FacePlayer()
    {
        if (player == null) return;
        
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        int targetDirection = directionToPlayer.x >= 0 ? 1 : -1;
        
        if (targetDirection != lastFacingDirection)
        {
            lastFacingDirection = targetDirection;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * targetDirection;
            transform.localScale = scale;
        }
    }

    void TryShoot()
    {
        if (isTakingDamage) return;
        
        float timeSinceLastShot = Time.time - lastShotTime;
        float shootInterval = 1f / fireRate;
        
        if (timeSinceLastShot >= shootInterval)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        
        lastShotTime = Time.time;
        isShooting = true;
        
        // Create bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        src.PlayOneShot(ShootSound, 0.25f);
        
        // Set bullet direction and speed
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            Vector2 shootDirection = lastFacingDirection > 0 ? Vector2.right : Vector2.left;
            bulletRb.velocity = shootDirection * bulletSpeed;
        }
        
        // Set bullet damage
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
        }
        
        // Play shoot animation (bool on)
        if (animator != null)
        {
            animator.SetBool(shootAnimationBool, true);
        }
        else
        {
            Debug.LogWarning("Animator is null! Cannot play shoot animation.");
        }
        
       
        
        // Reset shooting state after animation
        StartCoroutine(ResetShootingState());
    }
    
    IEnumerator ResetShootingState()
    {
        yield return new WaitForSeconds(0.5f); // Adjust based on your shoot animation length
        isShooting = false;
        if (animator != null)
        {
            animator.SetBool(shootAnimationBool, false);
        }
    }
    
    void OnPlayerDetected() { }
    
    void OnPlayerLost() 
    { 
        isShooting = false;
        if (animator != null)
        {
            animator.SetBool(shootAnimationBool, false);
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (health <= 0)
        {
            Die();
            return;
            src.PlayOneShot(DeathSound, 0.15f);
        }

        OnTakeDamage();
    }

    void OnTakeDamage()
    {
        isTakingDamage = true;
        damageTimer = damageAnimationDuration;
        
        // Play hurt animation
        if (animator != null)
        {
            animator.SetBool(hurtAnimationBool, true);
        }
        else
        {
            Debug.LogWarning("Animator is null! Cannot play hurt animation.");
        }
        

        
        // Damage effect
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // Damage flash
        StartCoroutine(DamageFlash());
    }
    
    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
    {
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isDetectingPlayer = false;
        isShooting = false;
        isTakingDamage = false;

        // Play death animation
        if (animator != null)
        {
            animator.SetBool(deathAnimationBool, true);
        }
        else
        {
            Debug.LogWarning("Animator is null! Cannot play death animation.");
        }
        
        
        // Death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Disable colliders and physics
        DisablePhysics();
        
        deathTimer = deathAnimationDuration;
    }

    void DisablePhysics()
    {
        // Disable all colliders
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // Disable rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        // Move to dead layer if it exists
        int deadLayer = LayerMask.NameToLayer("Dead");
        if (deadLayer >= 0)
        {
            SetLayerRecursively(gameObject, deadLayer);
        }
    }
    
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    void HandleDeath()
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0f)
            {
                //Destroy(gameObject);
            }
        }
        
    // Public getters for other scripts
    public bool IsDead()
    {
        return isDead;
    }
    
    public bool IsDetectingPlayer()
    {
        return isDetectingPlayer;
    }
    
    public bool IsShooting()
    {
        return isShooting;
    }
    
    public bool IsTakingDamage()
    {
        return isTakingDamage;
    }
    
    public float GetHealthPercentage()
    {
        return (float)health / maxHealth;
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = isDetectingPlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw lose range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseRange);
        
        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }
    }
}