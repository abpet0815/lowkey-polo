using UnityEngine;
using System;

public static class GameEvents
{
    public static event Action<Card> OnCardFlipped;
    public static event Action<Card, Card> OnCardsMatched;
    public static event Action<Card, Card> OnCardsMismatched;
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnComboChanged;
    public static event Action OnGameCompleted;
    public static event Action<float> OnComboTimerUpdate;
    
    public static void CardFlipped(Card card) => OnCardFlipped?.Invoke(card);
    public static void CardsMatched(Card card1, Card card2) => OnCardsMatched?.Invoke(card1, card2);
    public static void CardsMismatched(Card card1, Card card2) => OnCardsMismatched?.Invoke(card1, card2);
    public static void GameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);
    public static void ScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
    public static void ComboChanged(int comboLevel) => OnComboChanged?.Invoke(comboLevel);
    public static void GameCompleted() => OnGameCompleted?.Invoke();
    public static void ComboTimerUpdate(float normalizedTime) => OnComboTimerUpdate?.Invoke(normalizedTime);
}
