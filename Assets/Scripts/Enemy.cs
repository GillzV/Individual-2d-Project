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
    public float deathAnimationDuration = 1f;
    public Animator animator;
    
    [Header("Death Animation Settings")]
    public string deathAnimationTrigger = "isDead";
    public bool destroyAfterAnimation = true;
    
    [Header("Damage Animation Settings")]
    public string damageAnimationTrigger = "TakeDamage";
    public float damageAnimationDuration = 0.3f;
    public GameObject damageEffect;
    public Color damageFlashColor = Color.red;
    public float damageFlashDuration = 0.1f;
    
    private bool isDead = false;
    private float deathTimer = 0f;
    private bool isTakingDamage = false;
    private float damageTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Chase Settings")]
    public Transform player;
    public float aggroRange = 6f;
    public float loseRange = 9f;
    public float moveSpeed = 2f;
    public float stoppingDistance = 0.35f;
    [Tooltip("Buffer zone to prevent animation flickering at range boundaries")]
    public float rangeTolerance = 0.5f;
    
    [Header("Advanced Chase Settings")]
    [Tooltip("Minimum distance before enemy starts fleeing")]
    public float fleeDistance = 1.5f;
    [Tooltip("How fast enemy moves when fleeing")]
    public float fleeSpeed = 3f;
    [Tooltip("Enable fleeing behavior when player gets too close")]
    public bool enableFleeing = false;
    [Tooltip("Smoothing for direction changes (lower = more responsive)")]
    public float directionSmoothTime = 0.1f;
    [Tooltip("Minimum velocity to consider enemy as moving")]
    public float movementThreshold = 0.2f;
    [Tooltip("Time delay before updating animation state")]
    public float animationUpdateDelay = 0.15f;
    
    private bool isChasing = false;
    private bool isFleeing = false;
    private float currentVelocityX = 0f;
    private int lastFacingDirection = 1; // 1 for right, -1 for left
    private bool isAnimationRunning = false;
    private float animationTimer = 0f;

    [Header("Detection")]
    public float groundCheckDistance = 0.5f;

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

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
        
        // Store initial facing direction
        lastFacingDirection = transform.localScale.x >= 0 ? 1 : -1;
    }

    void ChasePlayer(float distToPlayer)
    {
        if (player == null || isDead) return;

        Vector2 dirToPlayer = (player.position - transform.position);
        float horizontalDist = dirToPlayer.x;
        float absHorizontalDist = Mathf.Abs(horizontalDist);
        
        // Determine target direction (1 = right, -1 = left)
        int targetDirection = horizontalDist >= 0 ? 1 : -1;
        
        float targetVelocityX = 0f;
        
        // Check if player is too close and fleeing is enabled
        if (enableFleeing && distToPlayer < fleeDistance)
        {
            // FLEE: Run away from player
            isFleeing = true;
            targetVelocityX = -targetDirection * fleeSpeed;
        }
        else if (absHorizontalDist > stoppingDistance)
        {
            // CHASE: Move toward player
            isFleeing = false;
            targetVelocityX = targetDirection * moveSpeed;
        }
        else
        {
            // STOP: Within stopping distance
            targetVelocityX = 0f;
            isFleeing = false;
        }
        
        // Smooth the velocity change for more natural movement
        float velocityRef = 0f;
        currentVelocityX = Mathf.SmoothDamp(
            rb.velocity.x, 
            targetVelocityX, 
            ref velocityRef, 
            directionSmoothTime
        );
        
        // Apply velocity
        rb.velocity = new Vector2(currentVelocityX, rb.velocity.y);
        
        // Update facing direction - only change if we're actually moving
        if (Mathf.Abs(currentVelocityX) > movementThreshold)
        {
            int movementDirection = currentVelocityX > 0 ? 1 : -1;
            if (movementDirection != lastFacingDirection)
            {
                lastFacingDirection = movementDirection;
                var s = transform.localScale;
                s.x = Mathf.Abs(s.x) * movementDirection;
                transform.localScale = s;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        OnTakeDamage();
    }

    void BecomeCorpse()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        int deadLayer = LayerMask.NameToLayer("Dead");
        if (deadLayer >= 0)
            SetLayerRecursively(gameObject, deadLayer);

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.sortingOrder -= 10;
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
    
    private void OnTakeDamage()
    {
        if (animator != null && !string.IsNullOrEmpty(damageAnimationTrigger))
        {
            animator.SetTrigger(damageAnimationTrigger);
        }
        
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        StartDamageFlash();
        
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
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
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
                animator.ResetTrigger(damageAnimationTrigger);

            animator.ResetTrigger(deathAnimationTrigger);
            animator.SetTrigger(deathAnimationTrigger);
        }
        
        BecomeCorpse();
        deathTimer = deathAnimationDuration;
    }

    void Update()
    {
        if (!isDead && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            
            // Use hysteresis to prevent flickering at range boundaries
            if (!isChasing)
            {
                if (dist <= aggroRange)
                {
                    isChasing = true;
                }
            }
            else // Currently chasing
            {
                // Only stop if player is beyond lose range
                if (dist > loseRange)
                {
                    isChasing = false;
                    isFleeing = false;
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    currentVelocityX = 0f;
                }
                else
                {
                    ChasePlayer(dist);
                }
            }
            
            // Handle animation with delay to prevent flickering
            UpdateRunAnimation();
        }
        else
        {
            // Not chasing, ensure animation is off
            if (isAnimationRunning && animator)
            {
                animator.SetBool("isRunning", false);
                isAnimationRunning = false;
            }
        }
        
        if (isDead && destroyAfterAnimation)
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }
        
        if (isTakingDamage)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                isTakingDamage = false;
            }
        }
    }
    
    private void UpdateRunAnimation()
    {
        if (animator == null) return;
        
        // Check if enemy should be running based on velocity
        bool shouldBeRunning = isChasing && Mathf.Abs(rb.velocity.x) > movementThreshold;
        
        // If state changed, start timer
        if (shouldBeRunning != isAnimationRunning)
        {
            animationTimer += Time.deltaTime;
            
            // Only update animation after delay threshold
            if (animationTimer >= animationUpdateDelay)
            {
                animator.SetBool("isRunning", shouldBeRunning);
                isAnimationRunning = shouldBeRunning;
                animationTimer = 0f;
            }
        }
        else
        {
            // State is stable, reset timer
            animationTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        if (isDead) return;
        
        bool overlapGrounded = groundCheck != null &&
                              Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        
        bool contactGrounded = rb != null && rb.IsTouchingLayers(groundMask);

        isGrounded = overlapGrounded || contactGrounded;
    }

    public bool CheckGroundAhead(float direction)
    {
        if (groundCheck == null) return false;

        Vector2 checkPosition = groundCheck.position + new Vector3(direction * groundCheckDistance, 0f, 0f);
        return Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundMask);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Vector2 checkAheadPos = groundCheck.position + new Vector3(groundCheckDistance, 0f, 0f);
                Gizmos.DrawWireSphere(checkAheadPos, groundCheckRadius);
            }
        }
        
    }
}