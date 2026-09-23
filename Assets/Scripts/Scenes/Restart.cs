using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void Awake()
    {
        // Automatically grab the Button component on this GameObject
        Button button = GetComponent<Button>();

        // Register the click listener programmatically
        button.onClick.AddListener(() => SceneManager.LoadScene("sceneName"));
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
