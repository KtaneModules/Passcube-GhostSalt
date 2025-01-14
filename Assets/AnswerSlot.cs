using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerSlot : MonoBehaviour
{
    private const float _aLocation = -3.6855f;
    private int _lastLocation = 26;

    private Coroutine _scrollAnimCoroutine;

    private float LetterIxToFloat(int loc)
    {
        return Mathf.Lerp(_aLocation, -_aLocation, loc / 26f);
    }

    // Use this for initialization
    void Start()
    {
        SetLocation(-_aLocation);
    }

    private void SetLocation(float loc)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, loc, 0);
    }

    public void TravelToLocation(int loc)
    {
        if (_scrollAnimCoroutine != null)
            StopCoroutine(_scrollAnimCoroutine);
        _scrollAnimCoroutine = StartCoroutine(ScrollAnimation(loc));
    }

    private IEnumerator ScrollAnimation(int newLoc)
    {
        int prevLoc = _lastLocation;
        _lastLocation = newLoc;
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            SetLocation(Easing.OutSine(elapsed, LetterIxToFloat(prevLoc), LetterIxToFloat(newLoc), duration));
            yield return null;
            elapsed += Time.deltaTime;
        }
        SetLocation(LetterIxToFloat(newLoc));
    }

    public void FadeColourTo(Color32 color)
    {
        StartCoroutine(FadeColourAnimation(color));
    }

    private IEnumerator FadeColourAnimation(Color32 color)
    {
        var target = GetComponent<Text>();
        Color32 startingColor = target.color;

        float duration = 0.4f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            target.color = Color32.Lerp(startingColor, color, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        target.color = color;
    }
}
