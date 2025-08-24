using System;
using UnityEngine;

/// <summary>
/// Centralized static events for decoupling game logic.
/// Other classes can subscribe to these signals without direct references.
/// </summary>
public static class GameSignals
{
    public static Action<Card> OnCardFlipped;            // Triggered when a card is flipped
    public static Action<Card, Card> OnCardsMatched;     // Triggered when two cards match
    public static Action<Card, Card> OnCardsMismatched;  // Triggered when two cards do not match
    public static Action<Card> OnCardMatchedDisabled;    // Triggered when a matched card is disabled
    public static Action<int> OnScoreChanged;            // Triggered when score is updated
}
