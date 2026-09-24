using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerJump : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D playerRB;
    public TextMeshProUGUI max;
    private float airTime = 0f;
    public float holdTime = 0.5f;
    public float jumpForceShort = 2f;
    public float jumpForceLong = 4f;
    bool isGrounded = false;
    void Start()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the floor
        if (collision.gameObject.CompareTag("floor"))
        {
            isGrounded = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && isGrounded == true)
        {
            airTime += Time.deltaTime;
            if (airTime > holdTime)
            {
                max.text = "Max Power!";
            }
        }
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame && airTime <= holdTime && isGrounded == true)
        {
            playerRB.linearVelocity = new Vector2(playerRB.linearVelocity.x, jumpForceShort);
            isGrounded = false;
            airTime = 0f;
            max.text = "";
        }
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame && airTime > holdTime && isGrounded == true)
        {
            playerRB.linearVelocity = new Vector2(playerRB.linearVelocity.x, jumpForceLong);
            max.text = "";
            isGrounded = false;
            airTime = 0f;
        }
    }
}
