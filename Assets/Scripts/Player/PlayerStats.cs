using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : EntityStats
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField]
    CharacterData.Stats actualStats;

    public CharacterData.Stats Stats
    {
        get { return actualStats; }
        set
        {
            actualStats = value;
        }
    }

    public CharacterData.Stats Actual
    {
        get { return actualStats; }
    }

    #region Current Stats Properties
    public float CurrentHealth
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = value;
                UpdateHealthBar();
            }
        }
    }
    #endregion

    [Header("Visuals")]
    public ParticleSystem damageEffect;
    public ParticleSystem blockedEffect;

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

    PlayerCollector collector;
    PlayerInventory inventory;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TMP_Text levelText;

    PlayerAnimator playerAnimator;

    void Awake()
    {
        characterData = UICharacterSelector.GetData();

        inventory = GetComponent<PlayerInventory>();
        collector = GetComponentInChildren<PlayerCollector>();

        // Priradenie premenných
        baseStats = actualStats = characterData.stats;
        collector.SetRadius(actualStats.magnet);
        health = actualStats.maxHealth;
        
        playerAnimator = GetComponent<PlayerAnimator>();
        if (characterData.controller)
        {
            playerAnimator.SetAnimatorController(characterData.controller);
        }
    }

    protected override void Start()
    {
        base.Start();

        // Pridá globálny buff levelu, ak nejaký existuje.
        if (UILevelSelector.globalBuff && UILevelSelector.globalBuffAffectsPlayer)
        {
            ApplyBuff(UILevelSelector.globalBuff);
        }

        inventory.Add(characterData.StartingWeapon);

        // Inicializacia experienceCap na zaklade aktualneho levelu
        experienceCap = levelRanges[0].experienceCapIncrease;

        GameManager.instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
    }

    protected override void Update()
    {
        base.Update();
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

    public override void RecalculateStats()
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

        // Premenná na uloženie všetkých kumulatívnych hodnôt násobiteľov
        CharacterData.Stats multiplier = new CharacterData.Stats
        {
            maxHealth = 1f, recovery = 1f, armor = 1f, moveSpeed = 1f,
            might = 1f, area = 1f, speed = 1f, duration = 1f, amount = 1, cooldown = 1f,
            luck = 1f, growth = 1f, greed = 1f, curse = 1f, magnet = 1f, revival = 1
        };

        foreach (Buff b in activeBuffs)
        {
            BuffData.Stats bd = b.GetData();

            switch (bd.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += bd.playerModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    multiplier *= bd.playerModifier;
                    break;
            }
        }
        actualStats *= multiplier;

        collector.SetRadius(actualStats.magnet);
    }

    public void IncreaseExperience(int amount)
    {
        experience += Mathf.RoundToInt(amount * actualStats.expGain);
        LevelUpChecker();

        UpdateExpBar();
    }

    void LevelUpChecker() 
    {
        while (experience >= experienceCap)
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

    public override void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(float damage, Vector3? position = null)
    {
        // Poskodenie dostane iba ak nema snimky neporazitelnosti
        if (!isInvincible) 
        {
            // Pred udelením poškodenia vezmi do úvahy brnenie (armor).
            damage -= actualStats.armor;

            if (damage > 0)
            {
                CurrentHealth -= damage;

                // Ak je priradený efekt poškodenia, prehraj ho.
                if (damageEffect)
                {
                    Destroy(Instantiate(damageEffect, position ?? transform.position, Quaternion.identity).gameObject, 5f);
                }

                if (CurrentHealth <= 0)
                {
                    Kill();
                    return;
                }
            }
            else
            {
                // Ak je priradený efekt zablokovania (vďaka armoru), prehraj ho.
                if (blockedEffect) Destroy(Instantiate(blockedEffect, position ?? transform.position, Quaternion.identity).gameObject, 5f);
            }

            // Nastav časovač nesmrteľnosti a aktivuj ju.
            invincibilityTimer = invincibilityDuration;
            isInvincible = true;
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

    public override void Kill()
    {
        if (actualStats.revival > 0)
        {
            actualStats.revival--;
            CurrentHealth = actualStats.maxHealth / 2;

            invincibilityTimer = 2f;
            isInvincible = true;

            // Mozno este pridat efekt
        }
        else if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.GameOver();
        }
    }

    public override void RestoreHealth(float amount)
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
    }

    void Recover()
    {
            // Obnovovanie zdravia o hodnotu Recovery kazdu sekundu, ale nikdy neprekroci MaxHealth
            if (CurrentHealth < actualStats.maxHealth)
            {
                CurrentHealth += Stats.recovery * Time.deltaTime;
    
                if (CurrentHealth > actualStats.maxHealth)
                {
                    CurrentHealth = actualStats.maxHealth;
                }
        }
    }

}
