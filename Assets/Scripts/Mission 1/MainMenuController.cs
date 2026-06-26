using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string normalControllerSceneName = "Prototype1_Normal";

    [SerializeField] private string wheelColliderSceneName = "Prototype1_WheelCollider";

    [Header("Buttons")]
    [SerializeField] private Button normalControllerButton;
    [SerializeField] private Button wheelColliderButton;

    private void OnEnable()
    {
        if (normalControllerButton != null)
        {
            normalControllerButton.onClick.AddListener(LoadNormalControllerScene);
        }

        if (wheelColliderButton != null)
        {
            wheelColliderButton.onClick.AddListener(LoadWheelColliderScene);
        }
    }

    private void OnDisable()
    {
        if (normalControllerButton != null)
        {
            normalControllerButton.onClick.RemoveListener(LoadNormalControllerScene);
        }

        if (wheelColliderButton != null)
        {
            wheelColliderButton.onClick.RemoveListener(LoadWheelColliderScene);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
        }
    }

    private void LoadNormalControllerScene()
    {
        SceneManager.LoadScene(normalControllerSceneName);
    }

    private void LoadWheelColliderScene()
    {
        SceneManager.LoadScene(wheelColliderSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}