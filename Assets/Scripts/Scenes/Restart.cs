using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void Awake()
    {
        Debug.Log("Restart script is awake.");

        // Register the click listener programmatically
        button.onClick.AddListener(() => SceneManager.LoadScene("Tom and Jerry"));
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
