using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class UIFlowController : MonoBehaviour
{
    [Header("Startup")]
    [Tooltip("Bật nếu muốn game bắt đầu từ Home. Tắt nếu muốn test trực tiếp gameplay.")]
    [SerializeField] private bool startFromHome = true;

    [Header("Panels")]
    [SerializeField] private UIPanelAnimator homePanel;
    [SerializeField] private UIPanelAnimator gameplayHudPanel;
    [SerializeField] private UIPanelAnimator pausePanel;
    [SerializeField] private UIPanelAnimator resultPanel;

    [Header("Retry Confirmation")]
    [Tooltip("Root của FadeOverlay dùng để xác nhận Retry.")]
    [SerializeField] private UIPanelAnimator retryConfirmPanel;

    [Tooltip("Bảng nhỏ bên trong FadeOverlay. Dùng để pop/thu lại.")]
    [SerializeField] private UIPopOnEnableAnimator retryConfirmContentPanel;

    [Header("Popup Contents")]
    [SerializeField] private UIPopOnEnableAnimator pauseContentPanel;

    [Header("Gameplay Root")]
    [Tooltip("Root chứa player, obstacle, gameplay object. Có thể để trống nếu không cần ẩn hiện gameplay.")]
    [SerializeField] private GameObject gameplayRoot;

    [Header("Camera")]
    [SerializeField] private VehicleCameraController vehicleCameraController;

    [Header("Race")]
    [SerializeField] private RaceManager raceManager;

    [Header("Player")]
    [SerializeField] private PlayerResetController playerResetController;

    private bool _isPlaying;
    private bool _isPaused;
    private bool _isShowingResult;
    private bool _isRetryConfirmOpen;

    private void Start()
    {
        if (startFromHome)
        {
            ShowHomeWithAnimation();
            return;
        }

        ShowGameplayInstant();
    }

    private void Update()
    {
        if (!_isPlaying && !_isShowingResult)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isRetryConfirmOpen)
            {
                CancelRetryConfirmation();
                return;
            }

            if (_isPaused)
            {
                ResumeGame();
                return;
            }

            if (_isPlaying)
            {
                PauseGame();
            }
        }
    }

    public void PlayGame()
    {
        _isPlaying = true;
        _isPaused = false;
        _isShowingResult = false;
        _isRetryConfirmOpen = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(true);

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(true);
        }

        homePanel.Hide();
        resultPanel.HideInstant();
        pausePanel.HideInstant();
        HideRetryConfirmInstant();

        gameplayHudPanel.Show();

        if (raceManager != null)
        {
            raceManager.StartRace();
        }
    }

    public void PauseGame()
    {
        if (!_isPlaying)
        {
            return;
        }

        _isPaused = true;

        SetCameraInputEnabled(false);

        pausePanel.ShowInstant();

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!_isPaused)
        {
            return;
        }

        if (pauseContentPanel != null)
        {
            pauseContentPanel.PlayClose(CompleteResumeGame);
            return;
        }

        CompleteResumeGame();
    }

    public void ShowResult()
    {
        _isPlaying = false;
        _isPaused = false;
        _isShowingResult = true;
        _isRetryConfirmOpen = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(false);

        gameplayHudPanel.Hide();
        pausePanel.HideInstant();
        HideRetryConfirmInstant();

        resultPanel.Show();
    }

    public void RetryGame()
    {
        if (_isRetryConfirmOpen)
        {
            return;
        }

        // Retry từ Pause: không cần hỏi lại, đóng Pause rồi chơi lại luôn.
        if (_isPaused && pauseContentPanel != null)
        {
            pauseContentPanel.PlayClose(CompleteRetryGame);
            return;
        }

        // Retry từ Result: hiện FadeOverlay xác nhận trước.
        if (_isShowingResult)
        {
            ShowRetryConfirmation();
            return;
        }

        CompleteRetryGame();
    }

    public void ConfirmRetryGame()
    {
        if (!_isRetryConfirmOpen)
        {
            return;
        }

        if (retryConfirmContentPanel != null)
        {
            retryConfirmContentPanel.PlayClose(CompleteConfirmedRetryGame);
            return;
        }

        CompleteConfirmedRetryGame();
    }

    public void CancelRetryConfirmation()
    {
        if (!_isRetryConfirmOpen)
        {
            return;
        }

        if (retryConfirmContentPanel != null)
        {
            retryConfirmContentPanel.PlayClose(CompleteCancelRetryConfirmation);
            return;
        }

        CompleteCancelRetryConfirmation();
    }

    public void BackToHome()
    {
        if (_isRetryConfirmOpen)
        {
            CancelRetryConfirmation();
            return;
        }

        if (_isPaused && pauseContentPanel != null)
        {
            pauseContentPanel.PlayClose(CompleteBackToHome);
            return;
        }

        CompleteBackToHome();
    }

    private void CompleteResumeGame()
    {
        _isPaused = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(true);

        pausePanel.HideInstant();
    }

    private void CompleteRetryGame()
    {
        Time.timeScale = 1f;
        SetCameraInputEnabled(true);

        ResetGameplay();

        _isPlaying = true;
        _isPaused = false;
        _isShowingResult = false;
        _isRetryConfirmOpen = false;

        pausePanel.HideInstant();
        resultPanel.HideInstant();
        HideRetryConfirmInstant();

        StartGameplayWithoutHudIntro();

        if (raceManager != null)
        {
            raceManager.StartRace();
        }
    }

    private void CompleteConfirmedRetryGame()
    {
        HideRetryConfirmInstant();
        CompleteRetryGame();
    }

    private void CompleteCancelRetryConfirmation()
    {
        _isRetryConfirmOpen = false;
        HideRetryConfirmInstant();

        // Vẫn ở ResultPanel, không làm gì thêm.
        _isPlaying = false;
        _isPaused = false;
        _isShowingResult = true;

        SetCameraInputEnabled(false);
    }

    private void CompleteBackToHome()
    {
        _isPlaying = false;
        _isPaused = false;
        _isShowingResult = false;
        _isRetryConfirmOpen = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(false);

        ResetGameplay();

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(false);
        }

        gameplayHudPanel.HideInstant();
        pausePanel.HideInstant();
        resultPanel.HideInstant();
        HideRetryConfirmInstant();

        homePanel.Show();
    }

    private void ShowRetryConfirmation()
    {
        _isRetryConfirmOpen = true;

        SetCameraInputEnabled(false);

        if (retryConfirmPanel != null)
        {
            retryConfirmPanel.ShowInstant();
        }

        if (retryConfirmContentPanel != null)
        {
            retryConfirmContentPanel.PlayOpen();
        }
    }

    private void HideRetryConfirmInstant()
    {
        _isRetryConfirmOpen = false;

        if (retryConfirmPanel != null)
        {
            retryConfirmPanel.HideInstant();
        }
    }

    private void ShowHomeInstant()
    {
        _isPlaying = false;
        _isPaused = false;
        _isShowingResult = false;
        _isRetryConfirmOpen = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(false);

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(false);
        }

        homePanel.ShowInstant();
        gameplayHudPanel.HideInstant();
        pausePanel.HideInstant();
        resultPanel.HideInstant();
        HideRetryConfirmInstant();
    }

    private void ShowHomeWithAnimation()
    {
        _isPlaying = false;
        _isPaused = false;
        _isShowingResult = false;
        _isRetryConfirmOpen = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(false);

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(false);
        }

        gameplayHudPanel.HideInstant();
        pausePanel.HideInstant();
        resultPanel.HideInstant();
        HideRetryConfirmInstant();

        homePanel.HideInstant();
        homePanel.Show();
    }

    private void ShowGameplayInstant()
    {
        _isPlaying = true;
        _isPaused = false;
        _isShowingResult = false;
        _isRetryConfirmOpen = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(true);

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(true);
        }

        homePanel.HideInstant();
        gameplayHudPanel.ShowInstant();
        pausePanel.HideInstant();
        resultPanel.HideInstant();
        HideRetryConfirmInstant();
    }

    private void StartGameplayWithoutHudIntro()
    {
        _isPlaying = true;
        _isPaused = false;
        _isShowingResult = false;
        _isRetryConfirmOpen = false;

        Time.timeScale = 1f;
        SetCameraInputEnabled(true);

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(true);
        }

        homePanel.HideInstant();
        resultPanel.HideInstant();
        pausePanel.HideInstant();
        HideRetryConfirmInstant();

        gameplayHudPanel.ShowInstant();
    }

    private void ResetGameplay()
    {
        if (raceManager != null)
        {
            raceManager.ResetRace();
        }

        if (playerResetController != null)
        {
            playerResetController.ResetToStart();
        }
    }

    private void SetCameraInputEnabled(bool isEnabled)
    {
        if (vehicleCameraController == null)
        {
            return;
        }

        vehicleCameraController.IsInputEnabled = isEnabled;
    }
}