using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Die : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the floor
        if (collision.gameObject.CompareTag("obstacles"))
        {
            SceneManager.LoadScene("sceneName");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
