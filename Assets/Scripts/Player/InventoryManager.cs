using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public List<WeaponController> weaponSlots = new List<WeaponController>(6);
    public int[] weaponLevels = new int[6];
    public List<Image> weaponUiSlotImages = new List<Image>(6);

    public List<PassiveItem> passiveItemSlots = new List<PassiveItem>(6);
    public int[] passiveItemLevels = new int[6];
    public List<Image> passiveItemUiSlotImages = new List<Image>(6);

    // Pridat zbran do slotu
    public void AddWeapon(int slotIndex, WeaponController weapon)
    {
        weaponSlots[slotIndex] = weapon;
        weaponLevels[slotIndex] = weapon.weaponData.Level;
        weaponUiSlotImages[slotIndex].enabled = true;                       // povolí zobrazenie ikony zbrane v UI
        weaponUiSlotImages[slotIndex].sprite = weapon.weaponData.Icon;     // aktualizuje ikonu zbrane v UI
    }

    // Pridat pasivny item do slotu
    public void AddPassiveItem(int slotIndex, PassiveItem item)
    {
        passiveItemSlots[slotIndex] = item;
        passiveItemLevels[slotIndex] = item.passiveItemData.Level;
        passiveItemUiSlotImages[slotIndex].enabled = true;                       // povolí zobrazenie ikony itemu v UI
        passiveItemUiSlotImages[slotIndex].sprite = item.passiveItemData.Icon;
    }

    public void LevelUpWeapon(int slotIndex)
    {
        if (weaponSlots.Count > slotIndex)
        {
            WeaponController weapon = weaponSlots[slotIndex];
            if (weapon.weaponData.NextLevelPrefab == null)          // kontrola, èi existuje další level pro tuto zbraò
            {
                Debug.LogWarning("No next level prefab for this weapon - " + weapon.name);
                return;
            }
            GameObject upgradedWeapon = Instantiate(weapon.weaponData.NextLevelPrefab, transform.position, Quaternion.identity);
            upgradedWeapon.transform.SetParent(transform);      // nastavenie zbrane ako potomok k hracovi
            AddWeapon(slotIndex, upgradedWeapon.GetComponent<WeaponController>());
            Destroy(weapon.gameObject);             // znièí povodnu zbran
            weaponLevels[slotIndex] = upgradedWeapon.GetComponent<WeaponController>().weaponData.Level;     // aktualizuje level zbrane v poli
        }
    }

    public void LevelUpPassiveItem(int slotIndex)
    {
        if (passiveItemSlots.Count > slotIndex)
        {
            PassiveItem item = passiveItemSlots[slotIndex];
            if (item.passiveItemData.NextLevelPrefab == null)          // kontrola, èi existuje další level pro tento item
            {
                Debug.LogWarning("No next level prefab for this passive item - " + item.name);
                return;
            }
            GameObject upgradedItem = Instantiate(item.passiveItemData.NextLevelPrefab, transform.position, Quaternion.identity);
            upgradedItem.transform.SetParent(transform);      // nastavenie itemu ako potomok k hracovi
            AddPassiveItem(slotIndex, upgradedItem.GetComponent<PassiveItem>());
            Destroy(item.gameObject);             // znièí povodny item
            passiveItemLevels[slotIndex] = upgradedItem.GetComponent<PassiveItem>().passiveItemData.Level;     // aktualizuje level itemu v poli
        }
    }


}
