using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Transform))]
public class HoverAndRotate : MonoBehaviour
{
    [Header("Lewitacja")]
    [Tooltip("Amplituda ruchu w pionie")]
    [SerializeField] private float hoverAmplitude = 0.5f;
    [Tooltip("Minimalny czas trwania jednego cyklu lewitacji")]
    [SerializeField] private float hoverDurationMin = 2f;
    [Tooltip("Maksymalny czas trwania jednego cyklu lewitacji")]
    [SerializeField] private float hoverDurationMax = 3f;

    [Header("Obrót")]
    [Tooltip("Kąty obrotu względem lokalnych osi")]
    [SerializeField] private Vector3 rotationAngles = new Vector3(0f, 360f, 0f);
    [Tooltip("Minimalny czas trwania jednego cyklu obrotu")]
    [SerializeField] private float rotateDurationMin = 10f;
    [Tooltip("Maksymalny czas trwania jednego cyklu obrotu")]
    [SerializeField] private float rotateDurationMax = 20f;

    private Vector3 startPosition;

    private void Start()
    {
        // Zapamiętanie początkowej pozycji
        startPosition = transform.localPosition;

        // Tort ruchu lewitacji
        float hoverDuration = Random.Range(hoverDurationMin, hoverDurationMax);
        transform.DOLocalMoveY(startPosition.y + hoverAmplitude, hoverDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(Random.Range(0f, hoverDuration)); // Losowe opóźnienie dla odchylenia fazy

        // Skrypt obrotu
        float rotateDuration = Random.Range(rotateDurationMin, rotateDurationMax);
        transform.DOLocalRotate(rotationAngles, rotateDuration, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear)
            .SetDelay(Random.Range(0f, rotateDuration)); // Losowe opóźnienie
    }
}