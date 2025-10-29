using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public AudioSource src;
    public AudioClip ShootSound, FootstepsSound, 
    EnemyHurtSound, PlayerDeathSound, WinSound, ClickSound;
    

    public int health;
    public int maxHealth = 100;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    [Header("Shooting")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletForce = 20f;
    public float shootingAnimationDuration = 0.3f; // How long the shooting animation plays
    
    [Header("Animation Settings")]
    public string shootingAnimationTrigger = "isShooting"; 

    private Rigidbody2D rb;
    [SerializeField] private bool isGrounded;
    private float inputX;
    private bool facingRight = true;
    private bool isShooting = false;
    private float shootingTimer = 0f;

    public HealthBar healthBar;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        health = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Flip exactly once based on input
        if (inputX > 0 && !facingRight)
        {
            Flip();
        }
        else if (inputX < 0 && facingRight)
        {
            Flip();
        }

        // Handle shooting animation timer
        if (isShooting)
        {
            shootingTimer -= Time.deltaTime;
            if (shootingTimer <= 0f)
            {
                isShooting = false;
            }
        }

        animator.SetBool("isRunning", inputX != 0);
        
        // Set shooting animation using the configurable trigger name
        if (animator != null && !string.IsNullOrEmpty(shootingAnimationTrigger))
        {
            animator.SetBool(shootingAnimationTrigger, isShooting);
        }

        if (Input.GetButtonDown("Fire1")) //plays the sound when the player shoots
        {
            Shoot();
            src.PlayOneShot(ShootSound, 0.25f);
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        transform.Rotate(0f, 180f, 0f);
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(inputX * moveSpeed, rb.velocity.y);

        bool overlapGrounded = groundCheck != null &&
                               Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        bool contactGrounded = rb.IsTouchingLayers(groundMask);
        isGrounded = overlapGrounded || contactGrounded;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visualize firePoint in editor
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.05f);
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        // Trigger shooting animation
        isShooting = true;
        shootingTimer = shootingAnimationDuration;
        
    }

    public bool IsShooting()
    {
        return isShooting;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        
        healthBar.SetHealth(health);

        if (health <= 0)
        {
            Destroy(gameObject);
            SceneManager.LoadScene("GameOver");
        }
        
    }

}
