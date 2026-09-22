using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float jumpForce = 10f;
    public Rigidbody2D playerRB;
    bool isGrounded = false;
    void Start()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the overlapping object is a collectible
        if (other.CompareTag("floor"))
        {
            isGrounded = true;
            Debug.Log("Player is grounded");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded == true)
        {
            playerRB.linearVelocity = new Vector2(playerRB.linearVelocity.x, jumpForce);
        }
    }
}
