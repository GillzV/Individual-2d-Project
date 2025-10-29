using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
            
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Check if both D and W are pressed for diagonal shooting
        bool isDPressed = Input.GetKey(KeyCode.D);
        bool isWPressed = Input.GetKey(KeyCode.W);
        
        if (isDPressed && isWPressed)
        {
            // Shoot diagonally up-right
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                // Get the bullet's speed from its component
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                float bulletSpeed = bulletScript != null ? bulletScript.speed : 20f;
                
                // Set diagonal velocity (up-right)
                Vector2 diagonalDirection = new Vector2(1f, 1f).normalized;
                bulletRb.velocity = diagonalDirection * bulletSpeed;
            }
        }
        
    }
}
