using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIUpgradeRow : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text priceText;
    public Button buyButton;
    public TMP_Text buttonText;

    [Header("Colors")]
    public Color normalButtonColor = new Color(0.2f, 0.8f, 0.2f);
    public Color cannotAffordColor = new Color(0.8f, 0.2f, 0.2f);

    // Táto funkcia "nakàmi" tento jeden riadok dátami a povie tlaèidlu, èo má robi
    public void Setup(string upgradeName, int currentLevel, int maxLevel, int price, bool canAfford, UnityAction onBuyAction)
    {
        nameText.text = upgradeName;
        levelText.text = $"Lvl: {currentLevel}/{maxLevel}";

        if (currentLevel >= maxLevel)
        {
            priceText.text = "MAX";
            priceText.color = Color.darkBlue;
            buttonText.text = "MAX";
            buyButton.interactable = false;
        }
        else
        {
            priceText.text = $"{price} G";
            buttonText.text = "Buy";
            buyButton.interactable = canAfford;

            if (canAfford)
            {
                buyButton.image.color = normalButtonColor;
            }
            else
            {
                buyButton.image.color = cannotAffordColor;
            }
        }

        // Vymažeme staré príkazy na tlaèidle a pridáme ten nový (aby sme nekúpili omylom nieèo iné)
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(onBuyAction);
    }
}