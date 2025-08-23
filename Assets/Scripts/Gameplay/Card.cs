using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class Card : MonoBehaviour, IPoolable
{
    public Image frontImage;
    [SerializeField] private Image backImage;
    public int symbolId;
    public float flipDuration = 0.25f;

    public CardState State = CardState.FaceDown;

    Button button;
    RectTransform rt;

    void Awake()
    {
        button = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        button.onClick.AddListener(OnClicked);
    }

    public void Setup(int symbol, Sprite frontSprite, float flipDur)
    {
        symbolId = symbol;
        frontImage.sprite = frontSprite;
        flipDuration = flipDur;
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
        State = CardState.FaceDown;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
    }

    void OnClicked()
    {
        if (State != CardState.FaceDown) return;
        StartCoroutine(FlipToFront());
    }

    IEnumerator FlipToFront()
    {
        State = CardState.Flipping;
        float half = flipDuration / 2f;
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(0, 90, t / half), 0);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0, 90, 0);
        backImage.gameObject.SetActive(false);
        frontImage.gameObject.SetActive(true);
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(90, 0, t / half), 0);
            yield return null;
        }
        transform.localEulerAngles = Vector3.zero;
        State = CardState.FaceUp;
        GameSignals.OnCardFlipped?.Invoke(this);
    }

    public IEnumerator FlipToBack(float delay = 0)
    {
        if (State == CardState.Matched) yield break;
        if (delay > 0) yield return new WaitForSeconds(delay);
        State = CardState.Flipping;
        float half = flipDuration / 2f;
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(0, 90, t / half), 0);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0, 90, 0);
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            transform.localEulerAngles = new Vector3(0, Mathf.Lerp(90, 0, t / half), 0);
            yield return null;
        }
        transform.localEulerAngles = Vector3.zero;
        State = CardState.FaceDown;
    }

    public void SetMatched()
    {
        State = CardState.Matched;
        // simple scale animation
        StartCoroutine(MatchedAnim());
    }

    IEnumerator MatchedAnim()
    {
        float dur = 0.15f;
        Vector3 start = transform.localScale;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t / dur);
            yield return null;
        }
        transform.localScale = Vector3.zero;
        // card stays inactive; caller should Return to pool
    }

    // IPoolable
    public void OnTakeFromPool()
    {
        // When get from pool
    }

    public void OnReturnToPool()
    {
        // cleanup
        StopAllCoroutines();
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
        State = CardState.FaceDown;
        symbolId = -1;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
    }
}

public enum CardState { FaceDown, Flipping, FaceUp, InComparison, Matched }