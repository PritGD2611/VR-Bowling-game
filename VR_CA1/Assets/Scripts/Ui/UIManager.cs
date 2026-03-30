using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all UI in the bowling game.
/// Controls Menu Canvas and Game UI Canvas.
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject UICanvas;
    [SerializeField] GameObject MenuCanvas;
    

    void Start()
    {
        MenuCanvas.SetActive(true);
        UICanvas.SetActive(false);


    }

    public void OnPlayClicked()
    {
        MenuCanvas.SetActive(false);
        UICanvas.SetActive(true);
    }

    
    public void OnQuitClicked()
    {
        Application.Quit();
    }

    
}