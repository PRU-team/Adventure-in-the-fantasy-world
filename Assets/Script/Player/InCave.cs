using UnityEngine;
using UnityEngine.SceneManagement;

public class InCave : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("VA CH?M! Object: " + other.name); // B??C 1

        if (other.CompareTag("Player"))
        {
            Debug.Log("LÀ PLAYER! ?ang load scene..."); // B??C 2
            SceneManager.LoadScene("Map");
        }
        else
        {
            Debug.Log("KHÔNG PH?I PLAYER. Tag là: " + other.tag); // B??C 3
        }
    }
}
