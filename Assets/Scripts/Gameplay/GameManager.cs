using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("References")]
    public GridSpawner spawner;
    public GameConfig config;
    public AudioManager audioManager;

    [Header("Card Visual Sprite Content")]
    [SerializeField] List<Sprite> cardSprites = new List<Sprite>(); // assign in Inspector



    [Header("HUD References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text matchesText;
    [SerializeField] private TMP_Text highScoreText;

    [Header("GameOver UI Panel")]
    [SerializeField] private GameObject gameOverPanel;

    //Runtime States
    // ordered symbol ids for the board
    List<int> deckSymbols;
    List<Card> faceUpUnmatched = new List<Card>();
    int score;
    int moves;
    int matches;

    void OnEnable()
    {
        GameSignals.OnCardFlipped += HandleCardFlip;
    }

    void OnDisable()
    {
        GameSignals.OnCardFlipped -= HandleCardFlip;
    }
    void Start()
    {
        // using values from ScriptableObject
        NewGame(config.rows, config.cols);
    }

    public void NewGame(int rows, int cols)
    {
        int total = rows * cols;
        if (total % 2 != 0)
        {
            Debug.LogError($"Invalid layout {rows}x{cols}. Must have an even number of cards.");
            return; // stop
        }

        // reset runtime state
        score = 0;
        moves = 0;
        matches = 0;
        UpdateHUD();
        gameOverPanel.SetActive(false);


        // Build a deck: pairs of symbol indices, then shuffle
        int pairs = total / 2;
        deckSymbols = BuildDeck(pairs);

        // Check to ensure we have enough sprites
        if (cardSprites.Count < pairs)
            Debug.LogWarning($"Not enough face sprites. Need {pairs}, have {cardSprites.Count}.");

        // Spawn cards from pool
        spawner.BuildGrid(rows, cols, deckSymbols, cardSprites, config.flipDuration);
    }
    List<int> BuildDeck(int pairs)
    {
        var list = new List<int>(pairs * 2);
        for (int i = 0; i < pairs; i++) { list.Add(i); list.Add(i); }
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
        //Play flip SFX
        audioManager.PlayFlip();

        lock (faceUpUnmatched)
        {
            if (!faceUpUnmatched.Contains(card) && card.State == CardState.FaceUp)
                faceUpUnmatched.Add(card);
        }
        TryStartComparisons();
    }

    void TryStartComparisons()
    {
        lock (faceUpUnmatched)
        {
            // as long as two face-up unmatched exist, pair them FIFO
            while (faceUpUnmatched.Count >= 2)
            {
                var a = faceUpUnmatched[0];
                var b = faceUpUnmatched[1];

                // mark as in comparison
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

        //Runtime state
        moves++;

        if (a == null || b == null) yield break;
        if (a.symbolId == b.symbolId)
        {
            a.SetMatched();
            b.SetMatched();

            //Runtime state
            matches++;
            score += config.matchScore;

            //Play Match SFX
            audioManager.PlayMatch();

            GameSignals.OnCardsMatched?.Invoke(a, b);
            if (spawner.RemainingActiveCards() == 0) GameSignals.OnGameOver?.Invoke();
        }
        else
        {
            //Play Mismatch SFX
            audioManager.PlayMismatch();

            StartCoroutine(a.FlipToBack(0.05f));
            StartCoroutine(b.FlipToBack(0.05f));

            //Runtime state
            score = Mathf.Max(0, score - config.mismatchPenalty);//Penalize

            GameSignals.OnCardsMismatched?.Invoke(a, b);
        }
        //Update HUD
        UpdateHUD();
    }


    //UI Handling
    void UpdateHUD()
    {
        scoreText.text = $"Score: {score}";
        movesText.text = $"Moves: {moves}";
        matchesText.text = $"Matches: {matches}";
    }

}