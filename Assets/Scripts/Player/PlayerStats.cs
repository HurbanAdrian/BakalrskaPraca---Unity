using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField]
    CharacterData.Stats actualStats;

    // Sucastne staty (neskor odstranit ostatne okrem healthu ak ich nevyuzijem)
    float currentHealth;

    #region Current Stats Properties
    public float CurrentHealth
    {
        get { return currentHealth; }

        // Ak sa pokúsime nastaviť aktuálne zdravie, používateľské rozhranie (UI) na obrazovke pauzy sa tiež aktualizuje.
        set
        {
            // Skontroluje, či sa hodnota zmenila
            if (currentHealth != value)
            {
                currentHealth = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentHealthDisplay.text = string.Format(
                        "Health: {0} / {1}",
                        currentHealth, actualStats.maxHealth
                    );
                }
            }
        }
    }

    public float MaxHealth
    {
        get { return actualStats.maxHealth; }

        // Ak sa pokúsime nastaviť maximálne zdravie, používateľské rozhranie (UI) na obrazovke pauzy sa tiež aktualizuje.
        set
        {
            // Skontroluje, či sa hodnota zmenila
            if (actualStats.maxHealth != value)
            {
                actualStats.maxHealth = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentHealthDisplay.text = string.Format(
                        "Health: {0} / {1}",
                        currentHealth, actualStats.maxHealth // Tu som použil tvoje currentHealth
                    );
                }

                // Aktualizuje hodnotu štatistiky v reálnom čase + pridanie dalsej logiky tu
            }
        }
    }

    public float CurrentRecovery
    {
        get { return Recovery; }
        set { Recovery = value; }
    }

    public float Recovery
    {
        get { return actualStats.recovery; }
        set
        {
            // Skontroluje, či sa hodnota zmenila
            if (actualStats.recovery != value)
            {
                actualStats.recovery = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + actualStats.recovery;
                }
            }
        }
    }

    public float CurrentMoveSpeed
    {
        get { return MoveSpeed; }
        set { MoveSpeed = value; }
    }

    public float MoveSpeed
    {
        get { return actualStats.moveSpeed; }
        set
        {
            // Skontroluje, či sa hodnota zmenila
            if (actualStats.moveSpeed != value)
            {
                actualStats.moveSpeed = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentSpeedDisplay.text = "Move Speed: " + actualStats.moveSpeed;
                }
            }
        }
    }

    public float CurrentMight
    {
        get { return Might; }
        set { Might = value; }
    }

    public float Might
    {
        get { return actualStats.might; }
        set
        {
            // Skontroluje, či sa hodnota zmenila
            if (actualStats.might != value)
            {
                actualStats.might = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMightDisplay.text = "Might: " + actualStats.might;
                }
            }
        }
    }

    public float CurrentProjectileSpeed
    {
        get { return Speed; }
        set { Speed = value; }
    }

    public float Speed
    {
        get { return actualStats.speed; }
        set
        {
            // Skontroluje, či sa hodnota zmenila
            if (actualStats.speed != value)
            {
                actualStats.speed = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + actualStats.speed;
                }
            }
        }
    }

    public float CurrentMagnet
    {
        get { return Magnet; }
        set { Magnet = value; }
    }

    public float Magnet
    {
        get { return actualStats.magnet; }
        set
        {
            // Skontroluje, či sa hodnota zmenila
            if (actualStats.magnet != value)
            {
                actualStats.magnet = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMagnetDisplay.text = "Magnet: " + actualStats.magnet;
                }
            }
        }
    }
    #endregion

    public ParticleSystem damageEffect;

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

    PlayerInventory inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TMP_Text levelText;

    PlayerAnimator playerAnimator;

    void Awake()
    {
        characterData = CharacterSelector.GetData();
        if (CharacterSelector.instance != null)
        {
            CharacterSelector.instance.DestroySingleton();
        }

        inventory = GetComponent<PlayerInventory>();

        // Priradenie premenných
        baseStats = actualStats = characterData.stats;
        currentHealth = actualStats.maxHealth;
        
        playerAnimator = GetComponent<PlayerAnimator>();
        if (characterData.controller)
        {
            playerAnimator.SetAnimatorController(characterData.controller);
        }
    }

    void Start()
    {
        inventory.Add(characterData.StartingWeapon);

        // Inicializacia experienceCap na zaklade aktualneho levelu
        experienceCap = levelRanges[0].experienceCapIncrease;

        // Nastavenie UI na zaciatocne hodnoty statov
        GameManager.instance.currentHealthDisplay.text = "Health: " + CurrentHealth;
        GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + CurrentRecovery;
        GameManager.instance.currentSpeedDisplay.text = "Move Speed: " + CurrentMoveSpeed;
        GameManager.instance.currentMightDisplay.text = "Might: " + CurrentMight;
        GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + CurrentProjectileSpeed;
        GameManager.instance.currentMagnetDisplay.text = "Magnet: " + CurrentMagnet;

        GameManager.instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
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

    public void RecalculateStats()
    {
        actualStats = baseStats;

        foreach (PlayerInventory.Slot s in inventory.passiveSlots)
        {
            Passive p = s.item as Passive;

            if (p)
            {
                actualStats += p.GetBoosts();
            }
        }
    }

    public void IncreaseExperience(int amount)
    {
        experience += amount;
        LevelUpChecker();

        UpdateExpBar();
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
            UpdateLevelText();

            GameManager.instance.StartLevelUp();
        }
    }

    public void TakeDamage(float damage, Vector3? position = null)
    {
        // Poskodenie dostane iba ak nema snimky neporazitelnosti
        if (!isInvincible) 
        {
            CurrentHealth -= damage;

            if (damageEffect)
            {
                Destroy(Instantiate(damageEffect, position ?? transform.position, Quaternion.identity), 5f);
            }

            invincibilityTimer = invincibilityDuration;
            isInvincible = true;

            if (CurrentHealth <= 0)
            {
                Kill();
            }

            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        healthBar.fillAmount = CurrentHealth / actualStats.maxHealth;
    }

    void UpdateExpBar()
    {
        expBar.fillAmount = (float)experience / experienceCap;
    }

    void UpdateLevelText()
    {
        levelText.text = "LV " + level.ToString();
    }

    public void Kill()
    {
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponSlots, inventory.passiveSlots);
            GameManager.instance.GameOver();
        }
    }

    public void RestoreHealth(float amount)
    {
        // vylieci hraca o urcity amount, ale nikdy neprekroci MaxHealth definovany v characterData
        if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += amount;

            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
        }

        UpdateHealthBar();
    }

    void Recover()
    {
            // Obnovovanie zdravia o hodnotu Recovery kazdu sekundu, ale nikdy neprekroci MaxHealth
            if (CurrentHealth < actualStats.maxHealth)
            {
                CurrentHealth += CurrentRecovery * Time.deltaTime;
    
                if (CurrentHealth > actualStats.maxHealth)
                {
                    CurrentHealth = actualStats.maxHealth;
                }
        }
    }

    // Odstranim stara Obsolete funkcia zo starym manazerom
    public void SpawnWeapon(GameObject weapon)
    {
        // Kontrola ci hrac uz nema maximum zbran�
        if (weaponIndex >= inventory.weaponSlots.Count - 1)
        {
            Debug.LogWarning("Player already has maximum number of weapons. Cannot spawn more.");
            return;
        }

        // spawnutie zaciatocnej zbrane
        GameObject spawnedWeapon = Instantiate(weapon, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform);     // nastavenie hraca ako rodica spawnutej zbrane, aby sa pohybovala spolu s nim
        //inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>());       // pridanie spawnutej zbrane do slotu v inventory

        weaponIndex++;     // posunutie indexu pre zbrane, aby sa dalsia zbran pridala do dalsieho slotu
    }

    // nebude treba spawnovat pasivne itemy samostatne uz
    public void SpawnPassiveItem(GameObject passiveItem)
    {
        if (passiveItemIndex >= inventory.passiveSlots.Count - 1)
        {
            Debug.LogWarning("Player already has maximum number of weapons. Cannot spawn more.");
            return;
        }

        GameObject spawnedPassiveItem = Instantiate(passiveItem, transform.position, Quaternion.identity);
        spawnedPassiveItem.transform.SetParent(transform);
        //inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItem.GetComponent<PassiveItem>());

        passiveItemIndex++;
    }

}
