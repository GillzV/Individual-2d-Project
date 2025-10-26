using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;
    
    [Header("Combat")]
    public int health = 100;
    public GameObject deathEffect;
    public float deathAnimationDuration = 1f; // How long the death animation plays
    public Animator animator; // Reference to animator for death animation
    
    [Header("Death Animation Settings")]
    public string deathAnimationTrigger = "isDead"; // Name of the death trigger in animator
    public bool destroyAfterAnimation = true; // Whether to destroy the object after animation
    
    [Header("Damage Animation Settings")]
    public string damageAnimationTrigger = "TakeDamage"; // Name of the damage trigger in animator
    public float damageAnimationDuration = 0.3f; // How long the damage animation plays
    public GameObject damageEffect; // Optional damage effect to spawn
    public Color damageFlashColor = Color.red; // Color to flash when taking damage
    public float damageFlashDuration = 0.1f; // How long to flash
    
    private bool isDead = false;
    private float deathTimer = 0f;
    private bool isTakingDamage = false;
    private float damageTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
            return; // this should stop the hurt animation from playing if im dead
        }

        OnTakeDamage(); // only if still alive
    }

    void BecomeCorpse()
    {
        // Stop physics
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;     // or Static
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Disable all colliders so player/bullets pass through
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // Optional: move to a non-colliding layer called "Dead"
        int deadLayer = LayerMask.NameToLayer("Dead");
        if (deadLayer >= 0)
            SetLayerRecursively(gameObject, deadLayer);

        // Optional: push visuals behind
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.sortingOrder -= 10; // or sr.sortingLayerName = "Background";
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }


    
    private void OnTakeDamage()
    {
        // Trigger damage animation
        if (animator != null && !string.IsNullOrEmpty(damageAnimationTrigger))
        {
            animator.SetTrigger(damageAnimationTrigger);
        }
        
        // Spawn damage effect
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // Start damage flash
        StartDamageFlash();
        
        // Set damage state
        isTakingDamage = true;
        damageTimer = damageAnimationDuration;
    }
    
    private void StartDamageFlash()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }
    
    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        // Flash to damage color
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        
        // Return to original color
        spriteRenderer.color = originalColor;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public bool IsTakingDamage()
    {
        return isTakingDamage;
    }

    void Die()
{
    if (isDead) return;
    isDead = true;

    if (animator != null)
    {
        if (!string.IsNullOrEmpty(damageAnimationTrigger))
            animator.ResetTrigger(damageAnimationTrigger);   // cancel any pending hurt

        animator.ResetTrigger(deathAnimationTrigger);        // clean slate
        animator.SetTrigger(deathAnimationTrigger);          // fire death
    }
    BecomeCorpse();

}


    [Header("Detection")]
    public float groundCheckDistance = 0.5f; // Distance to check ahead for ground/ledges

    private Rigidbody2D rb;
    [SerializeField] private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        // Handle death animation timer
        if (isDead && destroyAfterAnimation)
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }
        
        // Handle damage animation timer
        if (isTakingDamage)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                isTakingDamage = false;
            }
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        // Don't check ground if dead
        if (isDead) return;
        
        // Primary ground check using OverlapCircle
        bool overlapGrounded = groundCheck != null &&
                              Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        
        // Backup: contact-based grounded check
        bool contactGrounded = rb != null && rb.IsTouchingLayers(groundMask);

        // Player is grounded if any of the checks return true
        isGrounded = overlapGrounded || contactGrounded;
    }

    // Helper method to check if there's ground ahead (useful for AI)
    public bool CheckGroundAhead(float direction)
    {
        if (groundCheck == null) return false;

        Vector2 checkPosition = groundCheck.position + new Vector3(direction * groundCheckDistance, 0f, 0f);
        return Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundMask);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize ground check in editor
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            // Visualize ground check ahead (for AI)
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Vector2 checkAheadPos = groundCheck.position + new Vector3(groundCheckDistance, 0f, 0f);
                Gizmos.DrawWireSphere(checkAheadPos, groundCheckRadius);
            }
        }
    }
}
