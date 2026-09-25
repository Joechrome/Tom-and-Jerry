using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Die : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("collide");

        if (collision.gameObject.CompareTag("Obstacles"))
        {
            Debug.Log("Player has collided with an obstacle and will die.");
            SceneManager.LoadScene("The end");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
