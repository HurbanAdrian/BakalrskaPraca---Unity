using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public List<WeaponController> weaponSlots = new List<WeaponController>(6);
    public int[] weaponLevels = new int[6];
    public List<Image> weaponUiSlotImages = new List<Image>(6);

    public List<PassiveItem> passiveItemSlots = new List<PassiveItem>(6);
    public int[] passiveItemLevels = new int[6];
    public List<Image> passiveItemUiSlotImages = new List<Image>(6);

    [System.Serializable]
    public class WeaponUpgrade
    {
        public int weaponUpgradeIndex;
        public GameObject initialWeapon;
        public WeaponScriptableObject weaponData;
    }

    [System.Serializable]
    public class PassiveItemUpgrade
    {
        public int passiveItemUpgradeIndex;
        public GameObject initialPassiveItem;
        public PassiveItemScriptableObject passiveItemData;

    }

    [System.Serializable]
    public class UpgradeUI
    {
        public TMP_Text upgradeNameDisplay;
        public TMP_Text upgradeDescriptionDisplay;
        public Image upgradeIcon;
        public Button upgradeButton;
    }

    // Zoznam mo�n�ch upgradov pre zbrane a pasivne itemy, kter� se budou ponukat pri levelupe
    public List<WeaponUpgrade> weaponUpgradeOptions = new List<WeaponUpgrade>();
    public List<PassiveItemUpgrade> passiveItemUpgradeOptions = new List<PassiveItemUpgrade>();
    public List<UpgradeUI> upgradeUIOptions = new List<UpgradeUI>();        // List pre UI upragdy moznosti, ktore budu dostupne v level up screene

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
    }

    // Pridat zbran do slotu
    public void AddWeapon(int slotIndex, WeaponController weapon)
    {
        weaponSlots[slotIndex] = weapon;
        weaponLevels[slotIndex] = weapon.weaponData.Level;
        weaponUiSlotImages[slotIndex].enabled = true;                       // povol� zobrazenie ikony zbrane v UI
        weaponUiSlotImages[slotIndex].sprite = weapon.weaponData.Icon;     // aktualizuje ikonu zbrane v UI

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    // Pridat pasivny item do slotu
    public void AddPassiveItem(int slotIndex, PassiveItem item)
    {
        passiveItemSlots[slotIndex] = item;
        passiveItemLevels[slotIndex] = item.passiveItemData.Level;
        passiveItemUiSlotImages[slotIndex].enabled = true;                       // povol� zobrazenie ikony itemu v UI
        passiveItemUiSlotImages[slotIndex].sprite = item.passiveItemData.Icon;

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void LevelUpWeapon(int slotIndex, int upgradeIndex)
    {
        if (weaponSlots.Count > slotIndex)
        {
            WeaponController weapon = weaponSlots[slotIndex];
            if (weapon.weaponData.NextLevelPrefab == null)          // kontrola, �i existuje dal�� level pro tuto zbra�
            {
                Debug.LogWarning("No next level prefab for this weapon - " + weapon.name);
                return;
            }
            GameObject upgradedWeapon = Instantiate(weapon.weaponData.NextLevelPrefab, transform.position, Quaternion.identity);
            upgradedWeapon.transform.SetParent(transform);      // nastavenie zbrane ako potomok k hracovi
            AddWeapon(slotIndex, upgradedWeapon.GetComponent<WeaponController>());
            Destroy(weapon.gameObject);             // zni�� povodnu zbran
            weaponLevels[slotIndex] = upgradedWeapon.GetComponent<WeaponController>().weaponData.Level;     // aktualizuje level zbrane v poli

            weaponUpgradeOptions[upgradeIndex].weaponData = upgradedWeapon.GetComponent<WeaponController>().weaponData;     // aktualizuje data zbrane v upgrade moznosti, aby sa pri dalsom levelupe ponukala dalsia upgrade moznost

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLevelUp();
            }
        }
    }

    public void LevelUpPassiveItem(int slotIndex, int upgradeIndex)
    {
        if (passiveItemSlots.Count > slotIndex)
        {
            PassiveItem item = passiveItemSlots[slotIndex];
            if (item.passiveItemData.NextLevelPrefab == null)          // kontrola, �i existuje dal�� level pro tento item
            {
                Debug.LogWarning("No next level prefab for this passive item - " + item.name);
                return;
            }
            GameObject upgradedItem = Instantiate(item.passiveItemData.NextLevelPrefab, transform.position, Quaternion.identity);
            upgradedItem.transform.SetParent(transform);      // nastavenie itemu ako potomok k hracovi
            AddPassiveItem(slotIndex, upgradedItem.GetComponent<PassiveItem>());
            Destroy(item.gameObject);             // zni�� povodny item
            passiveItemLevels[slotIndex] = upgradedItem.GetComponent<PassiveItem>().passiveItemData.Level;     // aktualizuje level itemu v poli

            passiveItemUpgradeOptions[upgradeIndex].passiveItemData = upgradedItem.GetComponent<PassiveItem>().passiveItemData;     // aktualizuje data itemu v upgrade moznosti, aby sa pri dalsom levelupe ponukala dalsia upgrade moznost

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLevelUp();
            }
        }
    }

    void ApplyUpgradeOptions()
    {
        List<WeaponUpgrade> availableWeaponUpgrades = new List<WeaponUpgrade>(weaponUpgradeOptions);
        List<PassiveItemUpgrade> availablePassiveItemUpgrades = new List<PassiveItemUpgrade>(passiveItemUpgradeOptions);

        foreach (var upgradeOption in upgradeUIOptions)
        {
            if (availableWeaponUpgrades.Count == 0 && availablePassiveItemUpgrades.Count == 0)
            {
                return;
            }

            int upgradeType;

            if (availableWeaponUpgrades.Count > 0 && availablePassiveItemUpgrades.Count > 0)
            {
                upgradeType = Random.Range(1, 3);     // 1 pre zbran, 2 pre pasivny item
            }
            else if (availableWeaponUpgrades.Count > 0)
            {
                upgradeType = 1;
            }
            else
            {
                upgradeType = 2;
            }

            // Ak je upgrade typu zbran
            if (upgradeType == 1)
            {
                WeaponUpgrade chosenWeaponUpgrade = availableWeaponUpgrades[Random.Range(0, availableWeaponUpgrades.Count)];      // nejaky random upgrade z listu zbran�

                availableWeaponUpgrades.Remove(chosenWeaponUpgrade);     // odstrani zvolenu zbran z dostupn�ch upgradou, aby sa neopakovala

                if (chosenWeaponUpgrade != null)
                {
                    EnableUpgradeUI(upgradeOption);       // povol� zobrazenie UI pre tento upgrade

                    bool newWeapon = false;
                    // Hladame ci je nova zbran alebo nie
                    for (int i = 0; i < weaponSlots.Count; i++)
                    {
                        // je taka ista zbran v inventari kontrola
                        if (weaponSlots[i] != null && weaponSlots[i].weaponData == chosenWeaponUpgrade.weaponData)
                        {
                            newWeapon = false;


                            if (!newWeapon)
                            {
                                // SafeGuard pre pripad, ze by zbran nemala dalsi level prefab
                                if (!chosenWeaponUpgrade.weaponData.NextLevelPrefab)
                                {
                                    DisableUpgradeUI(upgradeOption);        // pokial zbran nema dalsi level, zakazeme tuto upgrade moznost, aby se neponukala
                                    break;
                                }

                                upgradeOption.upgradeButton.onClick.AddListener(() =>
                                {
                                    LevelUpWeapon(i, chosenWeaponUpgrade.weaponUpgradeIndex);      // levelup zbrane v danom slotu
                                });

                                // nastavenie popisu a nazvu podla dalsieho levelu zbrane
                                upgradeOption.upgradeDescriptionDisplay.text = chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>().weaponData.Description;
                                upgradeOption.upgradeNameDisplay.text = chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>().weaponData.ItemName;
                            }
                            break;      // break aby sme mohli najst dalsiu upgrade moznost
                        }
                        else
                        {
                            newWeapon = true;
                        }
                    }

                    if (newWeapon)      // ak je nova zbran, pridame ju do inventara
                    {
                        upgradeOption.upgradeButton.onClick.AddListener(() =>
                        {
                            player.SpawnWeapon(chosenWeaponUpgrade.initialWeapon);     // spawn novej zbrane
                        });

                        // nastavenie popisu a nazvu (inicialneho) podla zbrane, ktoru chceme pridat
                        upgradeOption.upgradeDescriptionDisplay.text = chosenWeaponUpgrade.weaponData.Description;
                        upgradeOption.upgradeNameDisplay.text = chosenWeaponUpgrade.weaponData.ItemName;
                    }

                    upgradeOption.upgradeIcon.sprite = chosenWeaponUpgrade.weaponData.Icon;
                }
            }
            // to iste pre pasivne itemy
            else if (upgradeType == 2)
            {
                PassiveItemUpgrade chosenPassiveItemUpgrade = availablePassiveItemUpgrades[Random.Range(0, availablePassiveItemUpgrades.Count)];      // nejaky random upgrade z listu pasivnych itemov

                availablePassiveItemUpgrades.Remove(chosenPassiveItemUpgrade);     // odstrani zvoleny item z dostupn�ch upgradou, aby sa neopakoval

                if (chosenPassiveItemUpgrade != null)
                {
                    EnableUpgradeUI(upgradeOption);       // povol� zobrazenie UI pre tento upgrade

                    bool newPassiveItem = false;
                    // Hladame ci je novy item alebo nie
                    for (int i = 0; i < passiveItemSlots.Count; i++)
                    {
                        // je taky ista pasivny item v inventari kontrola
                        if (passiveItemSlots[i] != null && passiveItemSlots[i].passiveItemData == chosenPassiveItemUpgrade.passiveItemData)
                        {
                            newPassiveItem = false;

                            if (!newPassiveItem)
                            {
                                // SafeGuard pre pripad, ze by item nemal dalsi level prefab
                                if (!chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab)
                                {
                                    DisableUpgradeUI(upgradeOption);        // pokial item nema dalsi level, zakazeme tuto upgrade moznost, aby se neponukala
                                    break;
                                }

                                upgradeOption.upgradeButton.onClick.AddListener(() =>
                                {
                                    LevelUpPassiveItem(i, chosenPassiveItemUpgrade.passiveItemUpgradeIndex);      // levelup itemu v danom slotu
                                });

                                // nastavenie popisu a nazvu podla dalsieho levelu itemu
                                upgradeOption.upgradeDescriptionDisplay.text = chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab.GetComponent<PassiveItem>().passiveItemData.Description;
                                upgradeOption.upgradeNameDisplay.text = chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab.GetComponent<PassiveItem>().passiveItemData.ItemName;
                            }
                            break;      // break aby sme mohli najst dalsiu upgrade moznost
                        }
                        else
                        {
                            newPassiveItem = true;
                        }
                    }
                    if (newPassiveItem)      // ak je novy item, pridame ho do inventara
                    {
                        upgradeOption.upgradeButton.onClick.AddListener(() =>
                        {
                            player.SpawnPassiveItem(chosenPassiveItemUpgrade.initialPassiveItem);     // spawn noveho itemu
                        });

                        // nastavenie popisu a nazvu (inicialneho) podla itemu, ktory chceme pridat
                        upgradeOption.upgradeDescriptionDisplay.text = chosenPassiveItemUpgrade.passiveItemData.Description;
                        upgradeOption.upgradeNameDisplay.text = chosenPassiveItemUpgrade.passiveItemData.ItemName;
                    }

                    upgradeOption.upgradeIcon.sprite = chosenPassiveItemUpgrade.passiveItemData.Icon;
                }
            }
        }
    }

    void RemoveUpgradeOptions()
    {
        foreach (var upgradeOption in upgradeUIOptions)
        {
            upgradeOption.upgradeButton.onClick.RemoveAllListeners();
            DisableUpgradeUI(upgradeOption);            // Disablnutie vsetky UI moznosti pred aplikovanim upgradov nim
        }
    }

    public void RemoveAndApplyUpgrades()
    {
        RemoveUpgradeOptions();
        ApplyUpgradeOptions();
    }

    void DisableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);
    }

    void EnableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
    }

}
