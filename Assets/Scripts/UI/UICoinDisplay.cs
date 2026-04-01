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
            displayTarget.text = Mathf.RoundToInt(collector.GetCoins()).ToString();
        }
        else
        {
            // Ak nie, naèítame a zobrazíme celkový poèet mincí zo save súboru (napr. v menu).
            float coins = SaveManager.LastLoadedGameData.coins;
            displayTarget.text = Mathf.RoundToInt(coins).ToString();
        }
    }
}