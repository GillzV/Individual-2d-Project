using UnityEngine;

public class Win : MonoBehaviour
{
    public GameObject win; 

    private void Start()
    {
        if (win != null)
            win.SetActive(false); // hides it at the start
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && win != null)
            win.SetActive(true); // shows it when player enters
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && win != null)
            win.SetActive(false); //hides it after again
    }
}
