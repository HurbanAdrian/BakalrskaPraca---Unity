using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroll : MonoBehaviour
{
    [Header("Rıchlos pohybu (X, Y)")]
    public Vector2 scrollSpeed = new Vector2(0.01f, 0.005f); // Ve¾mi pomalé

    private RawImage rawImage;
    private Rect currentRect;

    void Start()
    {
        // Nájdeme komponent RawImage na tomto GameObjecte
        rawImage = GetComponent<RawImage>();

        // Získame poèiatoèné UV Rect
        if (rawImage != null)
        {
            currentRect = rawImage.uvRect;
        }
        else
        {
            Debug.LogError("Skript BackgroundScroll potrebuje RawImage na fungovanie!");
        }
    }

    void Update()
    {
        if (rawImage == null) return;

        // Kadı snímok vypoèítame novı posun na základe èasu a rıchlosti
        currentRect.x += scrollSpeed.x * Time.deltaTime;
        currentRect.y += scrollSpeed.y * Time.deltaTime;

        // Priradíme novı UV Rect, èo spôsobí posun textúry
        rawImage.uvRect = currentRect;
    }
}