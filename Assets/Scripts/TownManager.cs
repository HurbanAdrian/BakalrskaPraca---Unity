using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TownManager : MonoBehaviour
{
    [Header("Global UI")]
    public UICoinDisplay coinDisplay;

    [Header("Map Building Images (Buttons)")]
    public Image blacksmithMapImage;
    public Image tavernMapImage;
    public Image marketMapImage;

    [Header("Building Sprites (0=Ruin, 1=Lv1, 2=Lv2, 3=Max)")]
    public Sprite[] blacksmithSprites;
    public Sprite[] tavernSprites;
    public Sprite[] marketSprites;

    [Header("Menu Panels")]
    public GameObject blacksmithMenuPanel;
    public GameObject tavernMenuPanel;
    public GameObject marketMenuPanel;

    // ==========================================
    // PREMENNÉ PRE RIADKY V MENU
    // ==========================================
    [Header("Blacksmith Rows")]
    public UIUpgradeRow bsMightRow;
    public UIUpgradeRow bsArmorRow;
    public UIUpgradeRow bsAmountRow;
    public UIUpgradeRow bsDiscountRow;

    [Header("Tavern Rows")]
    public UIUpgradeRow tvHealthRow;
    public UIUpgradeRow tvSpeedRow;
    public UIUpgradeRow tvCooldownRow;
    public UIUpgradeRow tvDiscountRow;

    [Header("Market Rows")]
    public UIUpgradeRow mkGreedRow;
    public UIUpgradeRow mkLuckRow;
    public UIUpgradeRow mkExpRow;
    public UIUpgradeRow mkDiscountRow;

    // ==========================================
    // CENNÍKY
    // ==========================================
    [Header("Base Prices & Increases")]
    public int priceBaseStat1 = 200; public int priceIncStat1 = 100; // Might, Health, Greed
    public int priceBaseStat2 = 200; public int priceIncStat2 = 100; // Armor, Speed, Luck
    public int priceBaseStat3 = 1000; public int priceIncStat3 = 500; // Amount, Cooldown, Exp
    public int priceBaseDiscount = 500; public int priceIncDiscount = 200; // Z¾avy

    void Start()
    {
        CloseAllMenus();
        RefreshTownUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { CloseAllMenus(); }
        if (Input.GetKeyDown(KeyCode.F12)) { SaveManager.ResetSave(); RefreshTownUI(); Debug.Log("DEV: Save vymazaný!"); }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            var data = SaveManager.LastLoadedGameData;
            data.coins += 10000;
            SaveManager.Save(data);
            RefreshTownUI();
            Debug.Log("DEV: +10 000 mincí!");
        }
    }

    // ==========================================
    // 1. MAPA A MENU LOGIKA
    // ==========================================

    public void CloseAllMenus()
    {
        blacksmithMenuPanel.SetActive(false);
        tavernMenuPanel.SetActive(false);
        marketMenuPanel.SetActive(false);
        RefreshTownUI();
    }

    public void OpenBlacksmithMenu() { CloseAllMenus(); blacksmithMenuPanel.SetActive(true); RefreshTownUI(); }
    public void OpenTavernMenu() { CloseAllMenus(); tavernMenuPanel.SetActive(true); RefreshTownUI(); }
    public void OpenMarketMenu() { CloseAllMenus(); marketMenuPanel.SetActive(true); RefreshTownUI(); }

    void UpdateBuildingSprite(Image buildingImage, Sprite[] sprites, int totalUpgrades)
    {
        if (sprites == null || sprites.Length == 0 || buildingImage == null) return;

        int visualLevel = 0;
        if (totalUpgrades == 0) visualLevel = 0;
        else if (totalUpgrades >= 1 && totalUpgrades <= 4) visualLevel = 1;
        else if (totalUpgrades >= 5 && totalUpgrades <= 8) visualLevel = 2;
        else visualLevel = 3;

        int spriteIndex = Mathf.Min(visualLevel, sprites.Length - 1);
        buildingImage.sprite = sprites[spriteIndex];
    }

    int CalculatePrice(int basePrice, int increasePerLevel, int currentLevel, int discountLevel)
    {
        float rawPrice = basePrice + (currentLevel * increasePerLevel);
        float discountMultiplier = 1f - (discountLevel * 0.1f);
        return Mathf.RoundToInt(rawPrice * discountMultiplier);
    }

    // ==========================================
    // 2. REFRESH UI (Kreslenie dát na obrazovku)
    // ==========================================

    public void RefreshTownUI()
    {
        var data = SaveManager.LastLoadedGameData;
        if (data == null) return;
        if (coinDisplay != null) coinDisplay.UpdateDisplay();

        // --- VIZUÁL MAPY ---
        UpdateBuildingSprite(blacksmithMapImage, blacksmithSprites, data.blacksmith.stat1Level + data.blacksmith.stat2Level + data.blacksmith.stat3Level);
        UpdateBuildingSprite(tavernMapImage, tavernSprites, data.tavern.stat1Level + data.tavern.stat2Level + data.tavern.stat3Level);
        UpdateBuildingSprite(marketMapImage, marketSprites, data.market.stat1Level + data.market.stat2Level + data.market.stat3Level);

        // --- BLACKSMITH MENU ---
        if (blacksmithMenuPanel.activeSelf)
        {
            var bs = data.blacksmith;
            int p1 = CalculatePrice(priceBaseStat1, priceIncStat1, bs.stat1Level, bs.discountLevel);
            int p2 = CalculatePrice(priceBaseStat2, priceIncStat2, bs.stat2Level, bs.discountLevel);
            int p3 = CalculatePrice(priceBaseStat3, priceIncStat3, bs.stat3Level, bs.discountLevel);
            int pD = CalculatePrice(priceBaseDiscount, priceIncDiscount, bs.discountLevel, 0);

            bsMightRow.Setup("Sharpened Steel", bs.stat1Level, 4, p1, data.coins >= p1, BuyBsMight);
            bsArmorRow.Setup("Reinforced Plate", bs.stat2Level, 4, p2, data.coins >= p2, BuyBsArmor);
            bsAmountRow.Setup("Multishot Scabbards", bs.stat3Level, 4, p3, data.coins >= p3, BuyBsAmount);
            bsDiscountRow.Setup("Steel Membership", bs.discountLevel, 4, pD, data.coins >= pD, BuyBsDiscount);
        }

        // --- TAVERN MENU ---
        if (tavernMenuPanel.activeSelf)
        {
            var tv = data.tavern;
            int p1 = CalculatePrice(priceBaseStat1, priceIncStat1, tv.stat1Level, tv.discountLevel);
            int p2 = CalculatePrice(priceBaseStat2, priceIncStat2, tv.stat2Level, tv.discountLevel);
            int p3 = CalculatePrice(priceBaseStat3, priceIncStat3, tv.stat3Level, tv.discountLevel);
            int pD = CalculatePrice(priceBaseDiscount, priceIncDiscount, tv.discountLevel, 0);

            tvHealthRow.Setup("Hearty Stew", tv.stat1Level, 4, p1, data.coins >= p1, BuyTvHealth);
            tvSpeedRow.Setup("Fleetfoot Brew", tv.stat2Level, 4, p2, data.coins >= p2, BuyTvSpeed);
            tvCooldownRow.Setup("Adrenaline Shot", tv.stat3Level, 4, p3, data.coins >= p3, BuyTvCooldown);
            tvDiscountRow.Setup("Drunken Membership", tv.discountLevel, 4, pD, data.coins >= pD, BuyTvDiscount);
        }

        // --- MARKET MENU ---
        if (marketMenuPanel.activeSelf)
        {
            var mk = data.market;
            int p1 = CalculatePrice(priceBaseStat1, priceIncStat1, mk.stat1Level, mk.discountLevel);
            int p2 = CalculatePrice(priceBaseStat2, priceIncStat2, mk.stat2Level, mk.discountLevel);
            int p3 = CalculatePrice(priceBaseStat3, priceIncStat3, mk.stat3Level, mk.discountLevel);
            int pD = CalculatePrice(priceBaseDiscount, priceIncDiscount, mk.discountLevel, 0);

            mkGreedRow.Setup("Merchant's Purse", mk.stat1Level, 4, p1, data.coins >= p1, BuyMkGreed);
            mkLuckRow.Setup("Four-Leaf Clover", mk.stat2Level, 4, p2, data.coins >= p2, BuyMkLuck);
            mkExpRow.Setup("Ancient Scrolls", mk.stat3Level, 4, p3, data.coins >= p3, BuyMkExp);
            mkDiscountRow.Setup("Guild Membership", mk.discountLevel, 4, pD, data.coins >= pD, BuyMkDiscount);
        }
    }

    // ==========================================
    // 3. NÁKUPNÉ FUNKCIE (Vykonanie nákupu)
    // ==========================================
    void PerformPurchase(int price, SaveManager.GameData data) { data.coins -= price; SaveManager.Save(data); RefreshTownUI(); }

    // --- BLACKSMITH ---
    void BuyBsMight() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat1, priceIncStat1, d.blacksmith.stat1Level, d.blacksmith.discountLevel); if (d.coins >= p && d.blacksmith.TryUpgradeStat1()) PerformPurchase(p, d); }
    void BuyBsArmor() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat2, priceIncStat2, d.blacksmith.stat2Level, d.blacksmith.discountLevel); if (d.coins >= p && d.blacksmith.TryUpgradeStat2()) PerformPurchase(p, d); }
    void BuyBsAmount() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat3, priceIncStat3, d.blacksmith.stat3Level, d.blacksmith.discountLevel); if (d.coins >= p && d.blacksmith.TryUpgradeStat3()) PerformPurchase(p, d); }
    void BuyBsDiscount() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseDiscount, priceIncDiscount, d.blacksmith.discountLevel, 0); if (d.coins >= p && d.blacksmith.TryUpgradeDiscount()) PerformPurchase(p, d); }

    // --- TAVERN ---
    void BuyTvHealth() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat1, priceIncStat1, d.tavern.stat1Level, d.tavern.discountLevel); if (d.coins >= p && d.tavern.TryUpgradeStat1()) PerformPurchase(p, d); }
    void BuyTvSpeed() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat2, priceIncStat2, d.tavern.stat2Level, d.tavern.discountLevel); if (d.coins >= p && d.tavern.TryUpgradeStat2()) PerformPurchase(p, d); }
    void BuyTvCooldown() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat3, priceIncStat3, d.tavern.stat3Level, d.tavern.discountLevel); if (d.coins >= p && d.tavern.TryUpgradeStat3()) PerformPurchase(p, d); }
    void BuyTvDiscount() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseDiscount, priceIncDiscount, d.tavern.discountLevel, 0); if (d.coins >= p && d.tavern.TryUpgradeDiscount()) PerformPurchase(p, d); }

    // --- MARKET ---
    void BuyMkGreed() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat1, priceIncStat1, d.market.stat1Level, d.market.discountLevel); if (d.coins >= p && d.market.TryUpgradeStat1()) PerformPurchase(p, d); }
    void BuyMkLuck() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat2, priceIncStat2, d.market.stat2Level, d.market.discountLevel); if (d.coins >= p && d.market.TryUpgradeStat2()) PerformPurchase(p, d); }
    void BuyMkExp() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseStat3, priceIncStat3, d.market.stat3Level, d.market.discountLevel); if (d.coins >= p && d.market.TryUpgradeStat3()) PerformPurchase(p, d); }
    void BuyMkDiscount() { var d = SaveManager.LastLoadedGameData; int p = CalculatePrice(priceBaseDiscount, priceIncDiscount, d.market.discountLevel, 0); if (d.coins >= p && d.market.TryUpgradeDiscount()) PerformPurchase(p, d); }
}