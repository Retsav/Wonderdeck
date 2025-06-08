using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class DollyPathMover : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Jeśli skrypt jest na tym samym obiekcie co VC, nie musisz nic przypisywać.")]
    public CinemachineVirtualCamera virtualCamera;

    [Header("Settings")]
    [Tooltip("Czas (w sekundach), w którym kamera przejedzie całą ścieżkę od początku do końca.")]
    public float duration = 5f;

    private CinemachineTrackedDolly dolly;
    private CinemachinePathBase path;
    private float minPos;
    private float maxPos;
    private float elapsed = 0f;
    private bool isPlaying = false;

    void Awake()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();

        dolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        if (dolly == null)
        {
            Debug.LogError("Brak komponentu TrackedDolly na Virtual Camera!");
            enabled = false;
            return;
        }

        path = dolly.m_Path;
        if (path == null)
        {
            Debug.LogError("TrackedDolly nie ma przypisanej ścieżki!");
            enabled = false;
            return;
        }

        // Pobieramy minimalną i maksymalną pozycję na ścieżce:
        minPos = path.MinPos;
        maxPos = path.MaxPos;
    }

    void OnEnable()
    {
        ResetMotion();
        isPlaying = true;
    }

    void Update()
    {
        if (isPlaying)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // interpolujemy pomiędzy minPos i maxPos:
            float pos = Mathf.Lerp(minPos, maxPos, t);
            pos = Mathf.Clamp(pos, minPos, maxPos);
            dolly.m_PathPosition = pos;

            if (elapsed >= duration)
                isPlaying = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetMotion();
            isPlaying = true;
        }
    }

    private void ResetMotion()
    {
        elapsed = 0f;
        dolly.m_PathPosition = minPos;
    }
}
