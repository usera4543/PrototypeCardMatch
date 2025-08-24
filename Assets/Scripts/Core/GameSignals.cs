using System;
using UnityEngine;

/// <summary>
/// Centralized, static events for decoupling
/// </summary>
public static class GameSignals
{
    public static Action<Card> OnCardFlipped;
    public static Action<Card, Card> OnCardsMatched;
    public static Action<Card, Card> OnCardsMismatched;
    public static Action<Card> OnCardMatchedDisabled;
    public static Action OnGameOver;
    public static Action<int> OnScoreChanged;
}