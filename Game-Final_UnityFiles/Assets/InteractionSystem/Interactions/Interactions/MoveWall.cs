using UnityEngine;
using DG.Tweening;

public class MoveWall : MonoBehaviour
{
    [Header("Stand References")]
    [SerializeField] private StandInteraction standA;
    [SerializeField] private StandInteraction standB;
    [SerializeField] private AudioClip stoneScraping;

    [Header("Movement Settings")]
    [SerializeField] private Vector3 _moveAmount = new Vector3(0f, -21f, 0f);
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private bool activate = false;

    private bool hasMoved = false;
    private Vector3 targetPosition;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (hasMoved || standA == null || standB == null)
        {
            return;
        }

        if (!hasMoved && standA.isOpen && standB.isOpen)
        {
            activate = true;
        }

        if (activate)
        {
            transform.DOLocalMove(_moveAmount, _moveSpeed);
            SoundEffectsManager.instance?.PlayNineSecondSoundEffectsClip(stoneScraping, transform, 1f);
            hasMoved = true;
            activate = false;
        }

    }
}