using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AudioManager audioManager;

    private BaseGameState _currentState;
    private Dictionary<GameState, BaseGameState> _states;
    private List<Card> _flippedCards = new List<Card>();
    private bool _isCheckingMatch = false;
    private int _score;
    private int _comboLevel;
    private float _comboTimer;
    private int _matchesFound;
    private int _totalMatches;
    private float _gameStartTime;
    private Vector2Int _currentBoardSize;
    private float _matchCheckDelay = 1f;
    private int _maxFlippedCards = 2;
    private GameState _currentGameState = GameState.Ready;

    public int Score => _score;
    public int ComboLevel => _comboLevel;
    public int MatchesFound => _matchesFound;
    public int TotalMatches => _totalMatches;
    public float GameTime => Time.time - _gameStartTime;
    public Vector2Int CurrentBoardSize => _currentBoardSize;
    public GameState CurrentGameState => _currentGameState;

    void Awake()
    {
        InitializeStates();
        _matchCheckDelay = gameSettings != null ? gameSettings.mismatchViewTime : 1f;
    }

    void Start()
    {
        ChangeState(GameState.Ready);
        GameEvents.OnCardFlipped += OnCardFlipped;
    }

    void OnDestroy()
    {
        GameEvents.OnCardFlipped -= OnCardFlipped;
    }

    void Update()
    {
        _currentState?.Update();
        UpdateComboTimer();
    }

    private void InitializeStates()
    {
        _states = new Dictionary<GameState, BaseGameState>
        {
            { GameState.Playing, new PlayingState(this) },
            { GameState.Processing, new ProcessingState(this) }
        };
    }

    public void StartNewGame(Vector2Int boardSize)
    {
        _currentBoardSize = boardSize;
        ResetGameState();
        boardManager.CreateBoard(boardSize);
        _totalMatches = (boardSize.x * boardSize.y) / 2;
        _gameStartTime = Time.time;
        ChangeState(GameState.Playing);
    }

    private void ResetGameState()
    {
        _score = 0;
        _comboLevel = 0;
        _matchesFound = 0;
        _comboTimer = gameSettings != null ? gameSettings.comboTimeLimit : 3.0f;
        _flippedCards.Clear();
        _isCheckingMatch = false;

        GameEvents.ScoreChanged(_score);
        GameEvents.ComboChanged(_comboLevel);
    }

    public void ChangeState(GameState newState)
    {
        _currentState?.Exit();
        _currentGameState = newState;

        if (_states.ContainsKey(newState))
        {
            _currentState = _states[newState];
        }

        _currentState?.Enter();
        GameEvents.GameStateChanged(newState);
    }

    private void OnCardFlipped(Card card)
    {
        if (_currentState is PlayingState && CanFlipCard(card))
        {
            AddFlippedCard(card);
        }
    }

    public bool CanFlipCard(Card card)
    {
        // Prevent flipping if:
        // 1. Card is not face down
        // 2. Already have max flipped cards
        // 3. Currently checking a match
        // 4. Game is not in Playing state (includes Paused)
        return card.CurrentState == CardState.FaceDown && 
               _flippedCards.Count < _maxFlippedCards && 
               !_isCheckingMatch &&
               _currentGameState == GameState.Playing;
    }

    public void AddFlippedCard(Card card)
    {
        if (!_flippedCards.Contains(card))
        {
            _flippedCards.Add(card);

            // Cancel auto-flip on all previously flipped cards when a second card is flipped
            if (_flippedCards.Count > 1)
            {
                foreach (Card flippedCard in _flippedCards)
                {
                    flippedCard.CancelAutoFlip();
                }
            }
        }

        if (_flippedCards.Count == _maxFlippedCards)
        {
            StartCoroutine(CheckForMatch());
        }
    }

    // New method to check if a single card should auto-flip back
    public void CheckSingleCardAutoFlip(Card card)
    {
        // Only flip back if it's still the only flipped card and game is in Playing state
        if (_flippedCards.Count == 1 && 
            _flippedCards.Contains(card) && 
            !_isCheckingMatch && 
            _currentGameState == GameState.Playing)
        {
            card.ForceFlipToBack();
            _flippedCards.Remove(card);
        }
    }

    private IEnumerator CheckForMatch()
    {
        _isCheckingMatch = true;
        ChangeState(GameState.Processing);

        yield return new WaitForSeconds(_matchCheckDelay);

        bool isMatch = true;
        int firstCardValue = _flippedCards[0].cardData.cardID;

        for (int i = 1; i < _flippedCards.Count; i++)
        {
            if (_flippedCards[i].cardData.cardID != firstCardValue)
            {
                isMatch = false;
                break;
            }
        }

        if (isMatch)
        {
            foreach (Card card in _flippedCards)
            {
                card.OnMatchFound();
            }

            _matchesFound++;
            int baseScore = gameSettings != null ? gameSettings.baseScore : 100;
            float multiplier = gameSettings != null ? gameSettings.comboMultiplier : 1.5f;
            int matchScore = Mathf.RoundToInt(baseScore * (_comboLevel + 1) * multiplier);
            _score += matchScore;
            _comboLevel++;
            _comboTimer = gameSettings != null ? gameSettings.comboTimeLimit : 3.0f;

            GameEvents.CardsMatched(_flippedCards[0], _flippedCards[1]);
            GameEvents.ScoreChanged(_score);
            GameEvents.ComboChanged(_comboLevel);
        }
        else
        {
            GameEvents.CardsMismatched(_flippedCards[0], _flippedCards[1]);
            
            foreach (Card card in _flippedCards)
            {
                card.ForceFlipToBack();
            }

            _comboLevel = 0;
            _comboTimer = gameSettings != null ? gameSettings.comboTimeLimit : 3.0f;
            GameEvents.ComboChanged(_comboLevel);
        }

        _flippedCards.Clear();
        _isCheckingMatch = false;

        if (_matchesFound >= _totalMatches)
        {
            ChangeState(GameState.Victory);
            GameEvents.GameCompleted();
        }
        else
        {
            ChangeState(GameState.Playing);
        }
    }

    private void UpdateComboTimer()
    {
        if (_comboLevel > 1 && gameSettings != null)
        {
            _comboTimer -= Time.deltaTime;
            GameEvents.ComboTimerUpdate(_comboTimer / gameSettings.comboTimeLimit);

            if (_comboTimer <= 0)
            {
                _comboLevel = 0;
                _comboTimer = gameSettings.comboTimeLimit;
                GameEvents.ComboChanged(_comboLevel);
            }
        }
    }

    public void ResetBoard()
    {
        _flippedCards.Clear();
        _isCheckingMatch = false;
        boardManager.ClearBoard();
        ChangeState(GameState.Ready);
    }

    public void LoadGame() { }
    public void SaveGame() { }
    public void AutoSave() { }

    void OnApplicationPause(bool pauseStatus) { }
    void OnApplicationFocus(bool hasFocus) { }
}
