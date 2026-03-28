using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toto je trieda, z ktorej môu dedi iné triedy, aby sa tejto triedy automaticky radili pod¾a osi Y.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Sortable : MonoBehaviour
{
    SpriteRenderer sorted;

    [Header("Sorting Settings")]
    public bool sortingActive = true;       // Umoòuje nám to deaktivova na urèitıch objektoch.
    public const float MIN_DISTANCE = 0.2f;
    int lastSortOrder = 0;

    [Header("Optimization (Culling)")]
    [Tooltip("Ak je zapnuté, objekt sa úplne vypne, keï nie je na kamere (VİBORNÉ PRE XP GEMY, ZLÉ PRE NEPRIATE¼OV).")]
    public bool deactivateOffScreen = false;

    [Tooltip("Ak je objekt od kamery ïalej ako toto èíslo, natrvalo sa znièí (ochrana RAM). 0 = nenièi.")]
    public float destroyDistance = 200f;

    public static List<SpriteRenderer> allCullableObjects = new List<SpriteRenderer>();

    protected virtual void Start()
    {
        sorted = GetComponent<SpriteRenderer>();

        if (deactivateOffScreen)
        {
            allCullableObjects.Add(sorted);
        }
    }

    protected virtual void Update()
    {
        if (deactivateOffScreen && sorted)
        {
            gameObject.SetActive(sorted.isVisible);
        }
    }

    public static void ReactivateAll()
    {
        allCullableObjects.RemoveAll(item => item == null); // Vyèistí znièené objekty (napr. zozbierané gemy)

        Camera cam = Camera.main;
        if (cam == null) return;

        foreach (SpriteRenderer r in allCullableObjects)
        {
            if (!r.gameObject.activeSelf)
            {
                Sortable s = r.GetComponent<Sortable>();

                // 1. OCHRANA PAMÄTE: Ak je objekt príliš ïaleko, natrvalo ho zma
                if (s != null && s.destroyDistance > 0)
                {
                    Vector2 distance = (Vector2)cam.transform.position - (Vector2)r.transform.position;
                    if (distance.sqrMagnitude > s.destroyDistance * s.destroyDistance)
                    {
                        Destroy(r.gameObject);
                        continue; // Ideme na ïalší objekt, tento je u màtvy
                    }
                }

                // 2. PREBÚDZANIE: Zapneme ho tesne predtım, ako vojde do obrazovky
                Vector3 viewPos = cam.WorldToViewportPoint(r.transform.position);
                bool isOnScreen = viewPos.x >= -0.1f && viewPos.x <= 1.1f &&
                                  viewPos.y >= -0.1f && viewPos.y <= 1.1f;

                if (isOnScreen)
                {
                    r.gameObject.SetActive(true);
                }
            }
        }
    }

    // LateUpdate sa volá raz za snímok po všetkıch Update metódach
    protected virtual void LateUpdate()
    {
        if (!sortingActive || !sorted) return;
        int newSortOrder = (int)(-transform.position.y / MIN_DISTANCE);
        if (lastSortOrder != newSortOrder)
        {
            lastSortOrder = newSortOrder;        // sorted.sortingOrder
            sorted.sortingOrder = newSortOrder;
        }
    }
}
