using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central game manager for the Memory Match game.
/// Handles deck generation, scorekeeping, state transitions, and high score persistence.
/// Follows Singleton pattern for global access.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("References")]
    [SerializeField] private GridSpawner spawner;
    [SerializeField] private GameConfig config;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;

    [Header("Card Visual Sprite Content")]
    [SerializeField] private List<Sprite> cardSprites = new List<Sprite>();

    // Runtime state
    private List<int> deckSymbols;
    private readonly List<Card> faceUpUnmatched = new List<Card>();

    private int score;
    private int moves;
    private int matches;
    private int highScore;

    private const string HighScoreKey = "HighScore";

    // ===================== UNITY LIFECYCLE =====================

    private void OnEnable()
    {
        GameSignals.OnCardFlipped += HandleCardFlip;
        GameSignals.OnCardMatchedDisabled += HandleCardDisabled;
    }

    private void OnDisable()
    {
        GameSignals.OnCardFlipped -= HandleCardFlip;
        GameSignals.OnCardMatchedDisabled -= HandleCardDisabled;
    }

    private void Start()
    {
        // Load saved high score
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        // Show start menu first
        if (uiManager != null)
            uiManager.ShowStart();
    }

    // ===================== GAME FLOW =====================

    /// <summary>
    /// Start a new game with a given grid layout.
    /// </summary>
    public void StartNewGame(int rows, int cols)
    {
        int total = rows * cols;
        if (total % 2 != 0)
        {
            Debug.LogError($"Invalid layout {rows}x{cols}. Must have an even number of cards.");
            return;
        }

        // Reset runtime state
        score = 0;
        moves = 0;
        matches = 0;
        faceUpUnmatched.Clear();

        uiManager?.UpdateHUD(score, moves, matches, highScore);

        // Build deck
        int pairs = total / 2;
        deckSymbols = BuildDeck(pairs);

        if (cardSprites.Count < pairs)
            Debug.LogWarning($"Not enough face sprites. Need {pairs}, have {cardSprites.Count}.");

        // Build the card grid
        spawner?.BuildGrid(rows, cols, deckSymbols, cardSprites, config.flipDuration);

        uiManager?.ShowHUD();
    }

    /// <summary>
    /// Restart the game with a random valid grid layout.
    /// </summary>
    public void RestartRandomGame()
    {
        int rows = Random.Range(config.minRows, config.maxRows + 1);
        int cols = Random.Range(config.minCols, config.maxCols + 1);

        // Ensure even number of cards
        if ((rows * cols) % 2 != 0)
        {
            if (cols > config.minCols) cols--;
            else rows--;
        }

        StartNewGame(rows, cols);
    }

    /// <summary>
    /// Called when all matches are found and the game ends.
    /// </summary>
    private void OnGameOver()
    {
        audioManager?.PlayGameOver();

        // Save new high score if beaten
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        uiManager?.UpdateHUD(score, moves, matches, highScore);
        uiManager?.ShowGameOver();
    }

    // ===================== GAME LOGIC =====================

    /// <summary>
    /// Builds a shuffled deck of symbol IDs with pairs.
    /// </summary>
    private List<int> BuildDeck(int pairs)
    {
        var list = new List<int>(pairs * 2);
        for (int i = 0; i < pairs; i++)
        {
            list.Add(i);
            list.Add(i);
        }

        // Fisher-Yates shuffle
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    /// <summary>
    /// Handles when a card is flipped face-up.
    /// </summary>
    private void HandleCardFlip(Card card)
    {
        audioManager?.PlayFlip();

        if (card == null || card.State != CardState.FaceUp)
            return;

        lock (faceUpUnmatched)
        {
            if (!faceUpUnmatched.Contains(card))
                faceUpUnmatched.Add(card);
        }

        TryStartComparisons();
    }

    /// <summary>
    /// Handles when a matched card is disabled.
    /// </summary>
    private void HandleCardDisabled(Card card)
    {
        if (spawner != null && spawner.RemainingActiveCards() == 0)
            OnGameOver();
    }

    /// <summary>
    /// Checks if enough cards are flipped to start comparison.
    /// </summary>
    private void TryStartComparisons()
    {
        lock (faceUpUnmatched)
        {
            while (faceUpUnmatched.Count >= 2)
            {
                var a = faceUpUnmatched[0];
                var b = faceUpUnmatched[1];

                if (a == null || b == null)
                {
                    faceUpUnmatched.RemoveRange(0, 2);
                    continue;
                }

                a.State = CardState.InComparison;
                b.State = CardState.InComparison;

                faceUpUnmatched.RemoveRange(0, 2);

                StartCoroutine(ComparePair(a, b));
            }
        }
    }

    /// <summary>
    /// Coroutine to compare two flipped cards with a delay.
    /// </summary>
    private IEnumerator ComparePair(Card a, Card b)
    {
        yield return new WaitForSeconds(config.compareDelay);

        moves++;

        if (a == null || b == null) yield break;

        if (a.symbolId == b.symbolId)
        {
            a.SetMatched();
            b.SetMatched();

            matches++;
            score += config.matchScore;

            audioManager?.PlayMatch();
            GameSignals.OnCardsMatched?.Invoke(a, b);
        }
        else
        {
            audioManager?.PlayMismatch();

            StartCoroutine(a.FlipToBack(0.05f));
            StartCoroutine(b.FlipToBack(0.05f));

            score = Mathf.Max(0, score - config.mismatchPenalty);
            GameSignals.OnCardsMismatched?.Invoke(a, b);
        }

        uiManager?.UpdateHUD(score, moves, matches, highScore);
    }

    // ===================== UI BUTTON HOOKS =====================

    public void OnPlayButton() => StartNewGame(config.rows, config.cols);
    public void OnRestartButton() => RestartRandomGame();
    public void OnHomeButton() => uiManager?.ShowStart();
}
