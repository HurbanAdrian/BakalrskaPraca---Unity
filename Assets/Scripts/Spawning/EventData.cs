using UnityEngine;

public abstract class EventData : SpawnData
{
    [Header("Event Data")]
    [Range(0f, 1f)] public float probability = 1f; // Èi k tejto udalosti dôjde.
    [Range(0f, 1f)] public float luckFactor = 1f; // Ako ve¾mi šastie ovplyvòuje pravdepodobnos tejto udalosti.

    [Tooltip("Ak je zadaná hodnota, táto udalos sa spustí a po tom, èo úroveò beí stanovenı poèet sekúnd.")]
    public float activeAfter = 0;

    public abstract bool Activate(PlayerStats player = null);

    // Kontroluje, èi je táto udalos momentálne aktívna.
    public bool IsActive()
    {
        if (!GameManager.instance) return false;
        if (GameManager.instance.GetElapsedTime() > activeAfter) return true;

        return false;
    }

    // Vypoèíta náhodnú pravdepodobnos, èi k tejto udalosti dôjde.
    public bool CheckIfWillHappen(PlayerStats s)
    {
        // Pravdepodobnos 1 znamená, e sa to stane vdy.
        if (probability >= 1) return true;

        // V opaènom prípade vygeneruj náhodné èíslo a skontroluj, èi sme prešli testom pravdepodobnosti.
        if (probability / Mathf.Max(1, (s.Stats.luck * luckFactor)) >= Random.Range(0f, 1f))
        {
            return true;
        }

        return false;
    }
}
