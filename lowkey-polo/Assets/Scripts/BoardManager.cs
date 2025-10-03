using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Header("Board Configuration")]
    [SerializeField] private Transform boardContainer;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private List<CardData> availableCardTypes;

    [Header("Layout")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("Card Scaling")]
    [SerializeField] private Vector2 cardScale = Vector2.one;
    [SerializeField] private Vector2 cardSpacing = new Vector2(10f, 10f);
    [SerializeField] private bool autoCalculateSize = true;

    private ObjectPool<Card> _cardPool;
    private List<Card> _activeCards = new List<Card>();
    private List<CardData> _currentDeck = new List<CardData>();

    void Start()
    {
        if (boardContainer == null)
            boardContainer = transform;

        SetupGridLayoutGroup();
        InitializeCardPool();
    }

    private void SetupGridLayoutGroup()
    {
        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = boardContainer.GetComponent<GridLayoutGroup>();
            
            if (gridLayoutGroup == null)
            {
                gridLayoutGroup = boardContainer.gameObject.AddComponent<GridLayoutGroup>();
            }
        }

        gridLayoutGroup.spacing = cardSpacing;
        gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    }

    private void InitializeCardPool()
    {
        _cardPool = new ObjectPool<Card>(cardPrefab, boardContainer, 50);
    }

    public void CreateBoard(Vector2Int boardSize)
    {
        ClearBoard();
        SetupGridLayout(boardSize);

        var deck = GenerateDeck(boardSize);
        ShuffleDeck(deck);
        _currentDeck = new List<CardData>(deck);

        for (int i = 0; i < deck.Count; i++)
        {
            var card = _cardPool.Get();
            card.Setup(deck[i]);
            _activeCards.Add(card);
        }
    }

    public void LoadBoard(GameSaveData saveData)
    {
        ClearBoard();
        SetupGridLayout(saveData.boardSize);
        _currentDeck.Clear();

        for (int i = 0; i < saveData.cardIDs.Length; i++)
        {
            CardData cardData = availableCardTypes.FirstOrDefault(c => c.cardID == saveData.cardIDs[i]);
            
            if (cardData != null)
            {
                _currentDeck.Add(cardData);
            }
        }

        for (int i = 0; i < _currentDeck.Count; i++)
        {
            var card = _cardPool.Get();
            card.Setup(_currentDeck[i]);

            if (i < saveData.matchedCards.Length && saveData.matchedCards[i])
            {
                card.OnMatchFound();
            }
            else if (i < saveData.flippedCards.Length && saveData.flippedCards[i])
            {
                card.FlipUp();
            }

            _activeCards.Add(card);
        }
    }

    public void PopulateSaveData(GameSaveData saveData)
    {
        saveData.cardIDs = new int[_currentDeck.Count];
        saveData.matchedCards = new bool[_activeCards.Count];
        saveData.flippedCards = new bool[_activeCards.Count];

        for (int i = 0; i < _activeCards.Count; i++)
        {
            if (i < _currentDeck.Count)
            {
                saveData.cardIDs[i] = _currentDeck[i].cardID;
            }

            saveData.matchedCards[i] = _activeCards[i].CurrentState == CardState.Matched;
            saveData.flippedCards[i] = _activeCards[i].CurrentState == CardState.FaceUp;
        }
    }

    public void ClearBoard()
    {
        foreach (var card in _activeCards)
        {
            _cardPool.Return(card);
        }

        _activeCards.Clear();
        _currentDeck.Clear();
    }

    private void SetupGridLayout(Vector2Int boardSize)
    {
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraintCount = boardSize.x;

            if (autoCalculateSize)
            {
                RectTransform containerRect = boardContainer as RectTransform;
                
                if (containerRect != null)
                {
                    float containerWidth = containerRect.rect.width;
                    float containerHeight = containerRect.rect.height;
                    float totalSpacingX = (boardSize.x - 1) * cardSpacing.x;
                    float totalSpacingY = (boardSize.y - 1) * cardSpacing.y;
                    float availableWidth = containerWidth - totalSpacingX;
                    float availableHeight = containerHeight - totalSpacingY;
                    float cellWidth = availableWidth / boardSize.x;
                    float cellHeight = availableHeight / boardSize.y;
                    float cellSize = Mathf.Min(cellWidth, cellHeight) * 0.9f;

                    gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
                }
            }
            else
            {
                float baseSize = 100f;
                Vector2 finalSize = new Vector2(baseSize * cardScale.x, baseSize * cardScale.y);
                gridLayoutGroup.cellSize = finalSize;
            }
        }
    }

    private List<CardData> GenerateDeck(Vector2Int boardSize)
    {
        int totalCards = boardSize.x * boardSize.y;
        int pairsNeeded = totalCards / 2;
        var deck = new List<CardData>();

        var usedCardTypes = availableCardTypes.Take(pairsNeeded).ToList();

        foreach (var cardType in usedCardTypes)
        {
            deck.Add(cardType);
            deck.Add(cardType);
        }

        return deck;
    }

    private void ShuffleDeck(List<CardData> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void DebugBoardState()
    {
        Debug.Log($"Active cards: {_activeCards.Count}");
        Debug.Log($"Current deck size: {_currentDeck.Count}");

        for (int i = 0; i < _activeCards.Count; i++)
        {
            var card = _activeCards[i];
            Debug.Log($"Card {i}: ID={card.cardData.cardID}, State={card.CurrentState}");
        }
    }

    public List<Card> GetCardsByState(CardState state)
    {
        return _activeCards.Where(card => card.CurrentState == state).ToList();
    }

    public int GetMatchedPairsCount()
    {
        return _activeCards.Count(card => card.CurrentState == CardState.Matched) / 2;
    }

    public bool IsGameComplete()
    {
        return _activeCards.All(card => card.CurrentState == CardState.Matched);
    }
}
