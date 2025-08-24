using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Possible states of a card during the game.
/// </summary>
public enum CardState 
{ 
    FaceDown,      // Card is hidden and ready to be flipped
    Flipping,      // Card is currently animating between sides
    FaceUp,        // Card is revealed and waiting for comparison
    InComparison,  // Card is being checked with another
    Matched        // Card has been successfully matched and is disabled
}

/// <summary>
/// A single card in the memory matching game.  
/// Handles flipping animations, state changes, and pooling cleanup.
/// </summary>
[RequireComponent(typeof(Button))]
public class Card : MonoBehaviour, IPoolable
{
    [Header("Card Visuals")]
    [SerializeField] private Image backImage; // The hidden back side of the card
    public Image frontImage;          // The visible symbol side of the card

    [Header("Card Properties")]
    public int symbolId;              // Unique identifier for the symbol
    private float flipDuration;// Total time taken to flip the card

    [Header("Card State")]
    public CardState State = CardState.FaceDown;

    // Cached references
    private Button button;
    private RectTransform rt;

    #region Unity Lifecycle
    void Awake()
    {
        button = GetComponent<Button>();
        rt = GetComponent<RectTransform>();

        // Subscribe card click
        button.onClick.AddListener(OnClicked);
    }
    #endregion

    #region Setup & Initialization
    /// <summary>
    /// Prepares the card with the assigned symbol and visuals.
    /// Called when card is spawned or reused from the pool.
    /// </summary>
    public void Setup(int symbol, Sprite frontSprite, float flipDur)
    {
        symbolId = symbol;
        frontImage.sprite = frontSprite;
        flipDuration = flipDur;

        // Reset visuals to face down
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
        State = CardState.FaceDown;

        // Reset transforms (in case card is reused from pool)
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
    }
    #endregion

    #region Click Handling
    /// <summary>
    /// Called when the card is clicked.
    /// Starts the flip animation if the card is face down.
    /// </summary>
    private void OnClicked()
    {
        if (State != CardState.FaceDown) return; // Ignore if already flipped
        StartCoroutine(FlipToFront());
    }
    #endregion

    #region Flip Animations
    /// <summary>
    /// Flips the card from back to front with a smooth animation.
    /// Triggers the OnCardFlipped signal when complete.
    /// </summary>
    private IEnumerator FlipToFront()
    {
        State = CardState.Flipping;
        float half = flipDuration / 2f;

        // Animate first half: rotate from 0 -> 90 degrees
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(0, 90, t / half), 0);
            yield return null;
        }

        // Swap back to front at halfway
        transform.localEulerAngles = new Vector3(0, 90, 0);
        backImage.gameObject.SetActive(false);
        frontImage.gameObject.SetActive(true);

        // Animate second half: rotate back from 90 -> 0 degrees
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(90, 0, t / half), 0);
            yield return null;
        }

        transform.localEulerAngles = Vector3.zero;
        State = CardState.FaceUp;

        // Notify listeners (e.g., GameManager) that this card is now revealed
        GameSignals.OnCardFlipped?.Invoke(this);
    }

    /// <summary>
    /// Flips the card from front to back.  
    /// Can be delayed (used when player mismatch occurs).
    /// </summary>
    public IEnumerator FlipToBack(float delay = 0)
    {
        if (State == CardState.Matched) yield break; // Ignore if already matched
        if (delay > 0) yield return new WaitForSeconds(delay);

        State = CardState.Flipping;
        float half = flipDuration / 2f;

        // Animate first half: rotate 0 -> 90
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(0, 90, t / half), 0);
            yield return null;
        }

        // Swap front -> back
        transform.localEulerAngles = new Vector3(0, 90, 0);
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);

        // Animate second half: rotate back 90 -> 0
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(90, 0, t / half), 0);
            yield return null;
        }

        transform.localEulerAngles = Vector3.zero;
        State = CardState.FaceDown;
    }
    #endregion

    #region Match Handling
    /// <summary>
    /// Marks the card as matched and plays a simple disappearing animation.
    /// </summary>
    public void SetMatched()
    {
        State = CardState.Matched;
        StartCoroutine(MatchedAnim());
    }

    /// <summary>
    /// Shrinks the card down and disables it (removes from board).
    /// </summary>
    private IEnumerator MatchedAnim()
    {
        float dur = 0.15f;
        Vector3 start = transform.localScale;

        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t / dur);
            yield return null;
        }

        transform.localScale = Vector3.zero;

        // Deactivate the card so it disappears
        gameObject.SetActive(false);

        // Notify game systems that this card is now disabled
        GameSignals.OnCardMatchedDisabled?.Invoke(this);
    }
    #endregion

    #region Pooling
    /// <summary>
    /// Resets card state and visuals when returned to the pool.
    /// Ensures no animations continue running in the background.
    /// </summary>
    public void OnReturnToPool()
    {
        StopAllCoroutines();
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
        State = CardState.FaceDown;
        symbolId = -1;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
    }
    #endregion
}
