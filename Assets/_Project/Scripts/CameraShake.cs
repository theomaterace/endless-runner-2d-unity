using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 initialPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Zapamiętujemy startową pozycję (zwykle 0,0,-10)
        initialPosition = transform.localPosition;
    }

    /// <summary>
    /// Wywołuje wstrząs kamery.
    /// </summary>
    /// <param name="duration">Czas trwania w sekundach.</param>
    /// <param name="magnitude">Siła wstrząsu (amplituda).</param>
    public void Shake(float duration, float magnitude)
    {
        // Zatrzymujemy poprzedni wstrząs, jeśli jeszcze trwa
        StopAllCoroutines();
        StartCoroutine(PerformShake(duration, magnitude));
    }

    private IEnumerator PerformShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Generujemy losowe przesunięcie w osi X i Y
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(
                initialPosition.x + x,
                initialPosition.y + y,
                initialPosition.z
            );

            // Time.unscaledDeltaTime pozwala na wstrząs nawet gdy gra jest zapauzowana (Time.timeScale = 0)
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        // Przywracamy kamerę do pierwotnej pozycji
        transform.localPosition = initialPosition;
    }
}