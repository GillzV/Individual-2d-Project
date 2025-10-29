using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class heart : MonoBehaviour
{
    [Header("Health Settings")]
    public int healthToRestore = 25; // Amount of health to restore
    
    [Header("Visual/Audio Effects")]
    public GameObject collectEffect; // Optional particle effect when collected
    public AudioClip collectSound; // Optional sound when collected
    
    private AudioSource audioSource;
    
    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            
            if (player != null)
            {
                // Restore health to the player
                RestorePlayerHealth(player);
                
                // Play collection effects but havent added any yet
                PlayCollectionEffects();
                
                // Destroy the heart after collection
                Destroy(gameObject);
            }
        }
    }
    
    private void RestorePlayerHealth(PlayerController2D player)
    {
        // Add health to the player
        player.health += healthToRestore;
        
        // Clamp health to not exceed max health
        player.health = Mathf.Clamp(player.health, 0, player.maxHealth);
        
        // Update the health bar
        if (player.healthBar != null)
        {
            player.healthBar.SetHealth(player.health);
        }
        
        Debug.Log($"Heart collected! Restored {healthToRestore} health. Current health: {player.health}");
    }
    
    private void PlayCollectionEffects()
    {
        // Spawn collection effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
    }
}
