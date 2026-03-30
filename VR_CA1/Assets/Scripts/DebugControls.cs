using UnityEngine;
using UnityEngine.InputSystem;

public class DebugControls : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    void Update()
    {
        // Press R to restart game (keyboard testing)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            gameManager.RestartGame();
        }
    }
}