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

    [Header("Main Menu Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitGameButton;

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

    private GameManager gameManager;
    private AudioManager audioManager;
    private TransitionManager transitionManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = AudioManager.Instance;
        transitionManager = TransitionManager.Instance;

        startGameButton.onClick.AddListener(ShowBoardSelect);
        quitGameButton.onClick.AddListener(QuitGame);

        // Updated board sizes: 2x2, 3x4, 4x5
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
        {
            transitionManager.PlayTransition(transitionManager.transitionTime);
        }
        mainMenuPanel.SetActive(true);
        boardSelectPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(false);
        // Play menu music when showing main menu
        if (audioManager != null)
        {
            audioManager.PlayMenuMusic();
        }
    }

    private void ShowBoardSelect()
    {
        mainMenuPanel.SetActive(false);
        boardSelectPanel.SetActive(true);
    }

    private void StartGame(int cols, int rows)
    {
        if (transitionManager != null)
        {
            transitionManager.PlayTransition(transitionManager.transitionTime);
        }
        boardSelectPanel.SetActive(false);
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(false);
        scoreText.text = "Score: 0";
        comboText.text = "";
        gameManager.StartNewGame(new Vector2Int(cols, rows));
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void PauseGame()
    {
        pausePanel.SetActive(true);
        gameManager.ChangeState(GameState.Paused);
        Time.timeScale = 0f; // Optional: Freeze time during pause
    }

    private void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Resume time
        gameManager.ChangeState(GameState.Playing);
    }

    private void BackToMainMenu()
    {
        Time.timeScale = 1f; // Make sure time is restored
        pausePanel.SetActive(false);
        // Reset the board completely
        gameManager.ResetBoard();
        if (transitionManager != null)
        {
            transitionManager.PlayTransition(transitionManager.transitionTime);
        }
        ShowMainMenu();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Make sure time is restored
        victoryPanel.SetActive(false);
        // Reset the board completely
        gameManager.ResetBoard();
        if (transitionManager != null)
        {
            transitionManager.PlayTransition(transitionManager.transitionTime);
        }
        ShowMainMenu();
    }

    private void UpdateScore(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }

    private void UpdateCombo(int comboLevel)
    {
        comboText.text = comboLevel > 1 ? $"Combo x{comboLevel}" : "";
    }

    private void ShowVictory()
    {
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(true);

        finalScoreText.text = scoreText.text;
    }
}
