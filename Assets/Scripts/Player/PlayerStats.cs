using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    CharacterScriptableObject characterData;

    // Sucastne staty
    [HideInInspector]
    public float currentHealth;
    [HideInInspector]
    public float currentRecovery;
    [HideInInspector]
    public float currentMoveSpeed;
    [HideInInspector]
    public float currentMight;
    [HideInInspector]
    public float currentProjectileSpeed;
    [HideInInspector]
    public float currentMagnet;

    // Skusenosti a level hraca
    [Header("Experience/level")]
    public int experience = 0;
    public int level = 1;
    public int experienceCap;

    // Trieda pre definovanie rozsahu levelov a zodpovedajucich zvyseni capu skusenosti potrebnych pre level up | bez toho atributu nevidime v editore
    [System.Serializable]
    public class LevelRange
    {
        public int startLevel;
        public int endLevel;
        public int experienceCapIncrease;
    }

    // Invicibilty Frames
    [Header("I_Frames")]
    public float invincibilityDuration;
    float invincibilityTimer;
    bool isInvincible;

    public List<LevelRange> levelRanges;

    InventoryManager inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    public GameObject firstPassiveItemTest, secondPassiveItemTest, secondWeaponTest;

    void Awake()
    {
        characterData = CharacterSelector.GetData();
        CharacterSelector.instance.DestroySingleton();

        inventory = GetComponent<InventoryManager>();

        // Inicializacia statov z characterData
        currentHealth = characterData.MaxHealth;
        currentRecovery = characterData.Recovery;
        currentMoveSpeed = characterData.MoveSpeed;
        currentMight = characterData.Might;
        currentProjectileSpeed = characterData.ProjectileSpeed;
        currentMagnet = characterData.Magnet;

        // spawnutie zaciatocnej zbrane
        SpawnWeapon(characterData.StartingWeapon);
        SpawnWeapon(secondWeaponTest);

        SpawnPassiveItem(firstPassiveItemTest);
        SpawnPassiveItem(secondPassiveItemTest);
    }

    void Start()
    {
        // Inicializacia experienceCap na zaklade aktualneho levelu
        experienceCap = levelRanges[0].experienceCapIncrease;
    }

    void Update()
    {
        if(invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        else if (isInvincible)  // ak timer dosiel na 0 a hrac je stale neporazitelny, nastavime ho na normalny stav
        {
            isInvincible = false;
        }

        Recover();
    }

    public void IncreaseExperience(int amount)
    {
        experience += amount;
        LevelUpChecker();
    }

    void LevelUpChecker() 
    {
        if (experience >= experienceCap)
        {
            level++;
            experience -= experienceCap; // resetovanie skusenosti po level up
            // Zvysenie experience capu pre dalsi level na zaklade definovanych levelRanges
            foreach (LevelRange range in levelRanges)
            {
                if (level >= range.startLevel && level <= range.endLevel)
                {
                    experienceCap += range.experienceCapIncrease;
                    break;
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        // Poskodenie dostane iba ak nema snimky neporazitelnosti
        if (!isInvincible) 
        {
            currentHealth -= damage;

            invincibilityTimer = invincibilityDuration;
            isInvincible = true;

            if (currentHealth <= 0)
            {
                Kill();
            }
        }
    }

    public void Kill()
    {
        Debug.Log("Player has been killed!");
    }

    public void RestoreHealth(float amount)
    {
        // vylieci hraca o urcity amount, ale nikdy neprekroci MaxHealth definovany v characterData
        if (currentHealth < characterData.MaxHealth)
        {
            currentHealth += amount;

            if (currentHealth > characterData.MaxHealth)
            {
                currentHealth = characterData.MaxHealth;
            }
        }
    }

    void Recover()
    {
            // Obnovovanie zdravia o hodnotu Recovery kazdu sekundu, ale nikdy neprekroci MaxHealth
            if (currentHealth < characterData.MaxHealth)
            {
                currentHealth += currentRecovery * Time.deltaTime;
    
                if (currentHealth > characterData.MaxHealth)
                {
                    currentHealth = characterData.MaxHealth;
                }
        }
    }

    public void SpawnWeapon(GameObject weapon)
    {
        // Kontrola ci hrac uz nema maximum zbraní
        if (weaponIndex >= inventory.weaponSlots.Count - 1)
        {
            Debug.LogWarning("Player already has maximum number of weapons. Cannot spawn more.");
            return;
        }

        // spawnutie zaciatocnej zbrane
        GameObject spawnedWeapon = Instantiate(weapon, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform);     // nastavenie hraca ako rodica spawnutej zbrane, aby sa pohybovala spolu s nim
        inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>());       // pridanie spawnutej zbrane do slotu v inventory

        weaponIndex++;     // posunutie indexu pre zbrane, aby sa dalsia zbran pridala do dalsieho slotu
    }


    public void SpawnPassiveItem(GameObject passiveItem)
    {
        if (passiveItemIndex >= inventory.passiveItemSlots.Count - 1)
        {
            Debug.LogWarning("Player already has maximum number of weapons. Cannot spawn more.");
            return;
        }

        GameObject spawnedPassiveItem = Instantiate(passiveItem, transform.position, Quaternion.identity);
        spawnedPassiveItem.transform.SetParent(transform);
        inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItem.GetComponent<PassiveItem>());

        passiveItemIndex++;
    }

}
