using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private List<Card> faceUpUnmatched = new List<Card>();
    private int score;
    private int moves;
    private int matches;
    private int highScore;

    private const string HighScoreKey = "HighScore";

    void OnEnable()
    {
        GameSignals.OnCardFlipped += HandleCardFlip;
        GameSignals.OnCardMatchedDisabled += HandleCardDisabled;
    }

    void OnDisable()
    {
        GameSignals.OnCardFlipped -= HandleCardFlip;
        GameSignals.OnCardMatchedDisabled -= HandleCardDisabled;
    }

    void Start()
    {
        // Load saved high score
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        // Show start menu first
        uiManager.ShowStart();
    }

    // ===================== GAME FLOW =====================

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

        uiManager.UpdateHUD(score, moves, matches, highScore);

        // Build deck
        int pairs = total / 2;
        deckSymbols = BuildDeck(pairs);

        if (cardSprites.Count < pairs)
            Debug.LogWarning($"Not enough face sprites. Need {pairs}, have {cardSprites.Count}.");

        spawner.BuildGrid(rows, cols, deckSymbols, cardSprites, config.flipDuration);

        uiManager.ShowHUD();
    }

    public void RestartRandomGame()
    {
        int rows = Random.Range(config.minRows, config.maxRows + 1);
        int cols = Random.Range(config.minCols, config.maxCols + 1);

        // Ensure even number of cards
        if ((rows * cols) % 2 != 0)
        {
            if (cols > config.minCols) cols--; else rows--;
        }

        StartNewGame(rows, cols);
    }

    void OnGameOver()
    {
        audioManager.PlayGameOver();

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        uiManager.UpdateHUD(score, moves, matches, highScore);
        uiManager.ShowGameOver();

        GameSignals.OnGameOver?.Invoke();
    }

    // ===================== GAME LOGIC =====================

    private List<int> BuildDeck(int pairs)
    {
        var list = new List<int>(pairs * 2);
        for (int i = 0; i < pairs; i++)
        {
            list.Add(i);
            list.Add(i);
        }

        // Shuffle
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    void HandleCardFlip(Card card)
    {
        audioManager.PlayFlip();

        lock (faceUpUnmatched)
        {
            if (!faceUpUnmatched.Contains(card) && card.State == CardState.FaceUp)
                faceUpUnmatched.Add(card);
        }

        TryStartComparisons();
    }

    void HandleCardDisabled(Card card)
    {
        if (spawner.RemainingActiveCards() == 0)
            OnGameOver();
    }

    void TryStartComparisons()
    {
        lock (faceUpUnmatched)
        {
            while (faceUpUnmatched.Count >= 2)
            {
                var a = faceUpUnmatched[0];
                var b = faceUpUnmatched[1];

                a.State = CardState.InComparison;
                b.State = CardState.InComparison;

                faceUpUnmatched.RemoveAt(0);
                faceUpUnmatched.RemoveAt(0);

                StartCoroutine(ComparePair(a, b));
            }
        }
    }

    IEnumerator ComparePair(Card a, Card b)
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

            audioManager.PlayMatch();
            GameSignals.OnCardsMatched?.Invoke(a, b);
        }
        else
        {
            audioManager.PlayMismatch();

            StartCoroutine(a.FlipToBack(0.05f));
            StartCoroutine(b.FlipToBack(0.05f));

            score = Mathf.Max(0, score - config.mismatchPenalty);
            GameSignals.OnCardsMismatched?.Invoke(a, b);
        }

        uiManager.UpdateHUD(score, moves, matches, highScore);
    }

    // ===================== UI BUTTON HOOKS =====================

    public void OnPlayButton() => StartNewGame(config.rows, config.cols);
    public void OnRestartButton() => RestartRandomGame();
    public void OnHomeButton() => uiManager.ShowStart();
}