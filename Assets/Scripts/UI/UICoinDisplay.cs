using TMPro;
using UnityEngine;

/// <summary>
/// Komponent pripojený k objektom, ktorý zobrazuje mince hráèa.
/// Buï v hre, alebo celkový poèet mincí, ktoré hráè vlastní, v závislosti od toho, èi je nastavená premenná collector.
/// </summary>
public class UICoinDisplay : MonoBehaviour
{
    TextMeshProUGUI displayTarget;
    public PlayerCollector collector;

    void Start()
    {
        displayTarget = GetComponentInChildren<TextMeshProUGUI>();
        UpdateDisplay();
        if (collector != null)
            collector.onCoinCollected += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        // Ak je priradený collector, zobrazujeme mince, ktoré má momentálne pri sebe.
        if (collector != null)
        {
            float roundedCoins = Mathf.Floor(collector.GetCoins() * 10f) / 10f;
            displayTarget.text = roundedCoins.ToString("F1");
        }
        else
        {
            // Ak nie, naèítame a zobrazíme celkový poèet mincí zo save súboru (napr. v menu).
            float coins = SaveManager.LastLoadedGameData.coins;
            float roundedCoins = Mathf.Floor(coins * 10f) / 10f;
            displayTarget.text = roundedCoins.ToString("F1");
        }
    }
}