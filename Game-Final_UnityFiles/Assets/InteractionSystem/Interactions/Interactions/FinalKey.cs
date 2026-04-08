using UnityEngine;
using DG.Tweening;

public class FinalKey : MonoBehaviour
{
    [Header("Rising Settings")]
    [SerializeField] private float riseDistance = 50f;      // How far the key rises
    [SerializeField] private float riseDuration = 2f;      // How long it takes
    [SerializeField] private Ease riseEase = Ease.OutCubic; // Optional easing

    private Vector3 _originalPosition;

    private void Awake()
    {
        // Save the initial position (floor level)
        _originalPosition = transform.position;
    }

    private void OnEnable()
    {
        // Subscribe to the altar's event
        AltarInteraction.OnAltarActivated += OnGoodEnding;
    }

    private void OnDisable()
    {
        AltarInteraction.OnAltarActivated -= OnGoodEnding;
    }

    private void OnGoodEnding(AltarInteraction altar)
    {
        StartCoroutine(DelayedGoodEndingCoroutine());
        // Make sure it rises only once

    }

    private System.Collections.IEnumerator DelayedGoodEndingCoroutine()
    {
        yield return new WaitForSeconds(4f);

        transform.DOMoveY(_originalPosition.y + riseDistance, riseDuration)
        .SetEase(riseEase);
        yield return new WaitForSeconds(3f);
    }
}