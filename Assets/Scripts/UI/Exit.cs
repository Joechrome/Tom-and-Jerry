using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class exit : MonoBehaviour
{
    public Button exit_button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        exit_button.onClick.AddListener(() => SceneManager.LoadScene("Main Menu"));
    }
}