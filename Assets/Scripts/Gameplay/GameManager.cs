using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("References")]
    public GridSpawner spawner;
    public GameConfig config;
    public AudioManager audioManager;

    [Header("Card Visual Sprite Content")]
    [SerializeField] List<Sprite> cardSprites = new List<Sprite>(); // assign in Inspector

    // ordered symbol ids for the board
    List<int> deckSymbols;    
    List<Card> faceUpUnmatched = new List<Card>();

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

        if (a == null || b == null) yield break;
        if (a.symbolId == b.symbolId)
        {
            a.SetMatched();
            b.SetMatched();

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
            GameSignals.OnCardsMismatched?.Invoke(a, b);
        }
    }

}