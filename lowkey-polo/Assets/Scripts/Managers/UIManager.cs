using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MaskTransitions;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject boardSelectPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Background Images")]
    [SerializeField] private Image mainMenuBackgroundImage;
    [SerializeField] private Image gameBackgroundImage;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button startGameButton;

    [Header("Board Selection Buttons")]
    [SerializeField] private Button size2x2Button;
    [SerializeField] private Button size3x4Button;
    [SerializeField] private Button size4x5Button;

    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Button pauseButton;

    [Header("Pause Panel Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button backToMenuButton;

    [Header("Victory Panel Buttons & Text")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Main Menu Title")]
    [SerializeField] private Image mainMenuTitleImage;
    [SerializeField] private TextMeshProUGUI mainMenuTitleText;

    private GameManager gameManager;
    private AudioManager audioManager;
    private TransitionManager transitionManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = AudioManager.Instance;
        transitionManager = TransitionManager.Instance;

        startGameButton.onClick.AddListener(ShowBoardSelect);
        size2x2Button.onClick.AddListener(() => StartGame(2, 2));
        size3x4Button.onClick.AddListener(() => StartGame(3, 4));
        size4x5Button.onClick.AddListener(() => StartGame(4, 5));
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        backToMenuButton.onClick.AddListener(BackToMainMenu);
        playAgainButton.onClick.AddListener(() => StartGame(gameManager.CurrentBoardSize.x, gameManager.CurrentBoardSize.y));
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnComboChanged += UpdateCombo;
        GameEvents.OnGameCompleted += ShowVictory;

        ShowMainMenu();
    }

    void OnDestroy()
    {
        GameEvents.OnScoreChanged -= UpdateScore;
        GameEvents.OnComboChanged -= UpdateCombo;
        GameEvents.OnGameCompleted -= ShowVictory;
    }

    private void ShowMainMenu()
    {
        if (transitionManager != null)
            transitionManager.PlayTransition(transitionManager.transitionTime);

        mainMenuPanel.SetActive(true);
        boardSelectPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(false);

        if (mainMenuBackgroundImage != null) mainMenuBackgroundImage.gameObject.SetActive(true);
        if (gameBackgroundImage != null) gameBackgroundImage.gameObject.SetActive(false);

        if (mainMenuTitleImage != null) mainMenuTitleImage.gameObject.SetActive(true);
        if (mainMenuTitleText != null) mainMenuTitleText.gameObject.SetActive(false);

        SetMenuInteractables(true);
        SetPauseControls(false);

        if (audioManager != null)
            audioManager.PlayMenuMusic();
    }

    private void ShowBoardSelect()
    {
        mainMenuPanel.SetActive(false);
        boardSelectPanel.SetActive(true);

        if (mainMenuTitleImage != null) mainMenuTitleImage.gameObject.SetActive(false);
        if (mainMenuTitleText != null) mainMenuTitleText.gameObject.SetActive(false);
    }

    private void StartGame(int cols, int rows)
    {
        if (transitionManager != null)
            transitionManager.PlayTransition(transitionManager.transitionTime);

        boardSelectPanel.SetActive(false);
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(false);

        if (mainMenuBackgroundImage != null) mainMenuBackgroundImage.gameObject.SetActive(false);
        if (gameBackgroundImage != null) gameBackgroundImage.gameObject.SetActive(true);
        if (mainMenuTitleImage != null) mainMenuTitleImage.gameObject.SetActive(false);
        if (mainMenuTitleText != null) mainMenuTitleText.gameObject.SetActive(false);

        scoreText.text = "Score: 0";
        comboText.text = "0";

        SetMenuInteractables(false);
        SetPauseControls(false);
        pauseButton.interactable = true;

        gameManager.StartNewGame(new Vector2Int(cols, rows));
    }

    private void PauseGame()
    {
        pausePanel.SetActive(true);
        gameManager.ChangeState(GameState.Paused);

        SetMenuInteractables(false);
        SetPauseControls(true);
        pauseButton.interactable = false;
    }

    private void ResumeGame()
    {
        pausePanel.SetActive(false);

        SetMenuInteractables(true);
        SetPauseControls(true);
        pauseButton.interactable = true;

        gameManager.ChangeState(GameState.Playing);
    }

    private void BackToMainMenu()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);

        gameManager.ResetBoard();

        if (transitionManager != null)
            transitionManager.PlayTransition(transitionManager.transitionTime);

        ShowMainMenu();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        victoryPanel.SetActive(false);

        gameManager.ResetBoard();

        if (transitionManager != null)
            transitionManager.PlayTransition(transitionManager.transitionTime);

        ShowMainMenu();
    }

    private void UpdateScore(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }

    private void UpdateCombo(int comboLevel)
    {
        comboText.text = comboLevel.ToString();
    }

    private void ShowVictory()
    {
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(true);

        finalScoreText.text = gameManager.Score.ToString();

        if (startGameButton != null) startGameButton.interactable = false;
        if (size2x2Button != null) size2x2Button.interactable = false;
        if (size3x4Button != null) size3x4Button.interactable = false;
        if (size4x5Button != null) size4x5Button.interactable = false;
        if (pauseButton != null) pauseButton.interactable = false;

        if (playAgainButton != null) playAgainButton.interactable = true;
        if (mainMenuButton != null) mainMenuButton.interactable = true;
    }

    private void SetMenuInteractables(bool enabled)
    {
        if (startGameButton != null) startGameButton.interactable = enabled;
        if (size2x2Button != null) size2x2Button.interactable = enabled;
        if (size3x4Button != null) size3x4Button.interactable = enabled;
        if (size4x5Button != null) size4x5Button.interactable = enabled;
        if (playAgainButton != null) playAgainButton.interactable = enabled;
        if (mainMenuButton != null) mainMenuButton.interactable = enabled;
    }

    private void SetPauseControls(bool enabled)
    {
        if (resumeButton != null) resumeButton.interactable = enabled;
        if (backToMenuButton != null) backToMenuButton.interactable = enabled;
    }
}
