using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class start : MonoBehaviour
{
    public Button start_button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        start_button.onClick.AddListener(() => SceneManager.LoadScene("Main Menu"));
    }
}
