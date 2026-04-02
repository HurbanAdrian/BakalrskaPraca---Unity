using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [System.Flags]
    public enum DropType
    {
        // Kaûd· hodnota je mocnina dvojky, aby sa dali kombinovaù pomocou bitov˝ch oper·ciÌ
        NewPassive = 1, 
        NewWeapon = 2, 
        UpgradePassive = 4,
        UpgradeWeapon = 8, 
        Evolution = 16
    }

    // Predvolene s˙ nastavenÈ vöetky typy dropov (DropType)~0;
    public DropType possibleDrops = (DropType)~0;

    public enum DropCountType { sequential, random }
    public DropCountType dropCountType = DropCountType.sequential;
    public TreasureChestDropProfile[] dropProfiles;
    public static int totalPickups = 0;
    int currentDropProfileIndex = 0;
    public Sprite defaultDropSprite;

    PlayerInventory recipient;

    // Vr·ti aktu·lne vybran˝ profil podæa indexu
    public TreasureChestDropProfile GetCurrentDropProfile()
    {
        return dropProfiles[currentDropProfileIndex];
    }

    public TreasureChestDropProfile GetNextDropProfile()
    {
        if (dropProfiles == null || dropProfiles.Length == 0)
        {
            Debug.LogWarning("Drop profily nie s˙ nastavenÈ.");
            return null;
        }

        switch (dropCountType)
        {
            case DropCountType.sequential:
                // PostupnÈ striedanie profilov
                currentDropProfileIndex = Mathf.Clamp(totalPickups, 0, dropProfiles.Length - 1);
                break;

            case DropCountType.random:
                float playerLuck = recipient.GetComponentInChildren<PlayerStats>().Actual.luck;

                // VytvorÌme zoznam profilov s vypoËÌtanou v·hou (öancou)
                List<(int index, TreasureChestDropProfile profile, float weight)> weightedProfiles = new List<(int, TreasureChestDropProfile, float)>();

                for (int i = 0; i < dropProfiles.Length; i++)
                {
                    // Vzorec pre v·hu: baseChance * (1 + luckScaling * (luck - 1))
                    float weight = dropProfiles[i].baseDropChance * (1 + dropProfiles[i].luckScaling * (playerLuck - 1));
                    weightedProfiles.Add((i, dropProfiles[i], weight));
                }

                // Zoradenie podæa v·hy od najmenöej (pre Cumulative v˝ber)
                weightedProfiles.Sort((a, b) => a.weight.CompareTo(b.weight));

                // V˝poËet celkovej v·hy
                float totalWeight = 0f;
                foreach (var entry in weightedProfiles) totalWeight += entry.weight;

                // N·hodn˝ hod kockou a v˝ber podæa kumulatÌvnej v·hy
                float r = Random.Range(0, totalWeight);
                float cumulative = 0f;
                foreach (var entry in weightedProfiles)
                {
                    cumulative += entry.weight;
                    if (r <= cumulative)
                    {
                        currentDropProfileIndex = entry.index;
                        return entry.profile;
                    }
                }
                break;
        }

        return GetCurrentDropProfile();
    }

    private int GetRewardCount()
    {
        TreasureChestDropProfile dp = GetNextDropProfile();
        if (dp) return dp.noOfItems;
        return 1;
    }

    // Pok˙si sa o evol˙ciu n·hodnÈho predmetu v invent·ri.
    T TryEvolve<T>(PlayerInventory inventory, bool updateUI = true) where T : Item
    {
        // ZÌskame vöetky predmety v invent·ri schopnÈ evol˙cie.
        T[] evolvables = inventory.GetEvolvables<T>();

        foreach (Item i in evolvables)
        {
            // ZÌskame vöetky moûnÈ evol˙cie pre tento predmet.
            ItemData.Evolution[] possibleEvolutions = i.CanEvolve(0);
            foreach (ItemData.Evolution e in possibleEvolutions)
            {
                // Pok˙sime sa vykonaù evol˙ciu. Ak uspeje, ozn·mime to UI.
                if (i.AttemptEvolution(e, 0, updateUI))
                {
                    UITreasureChest.NotifyItemReceived(e.outcome.itemType.icon);
                    return i as T;
                }
            }
        }
        return null;
    }

    // Pok˙si sa vylepöiù n·hodn˝ predmet v invent·ri.
    T TryUpgrade<T>(PlayerInventory inventory, bool updateUI = true) where T : Item
    {
        // ZÌska vöetky zbrane/pasÌvky v invent·ri, ktorÈ eöte mÙûu r·sù (Level Up).
        T[] upgradables = inventory.GetUpgradables<T>();
        if (upgradables.Length == 0) return null; // Ak nie je Ëo vylepöovaù, ukonËÌme.

        // Vykon·me level up na n·hodne vybranom predmete.
        T t = upgradables[Random.Range(0, upgradables.Length)];
        inventory.LevelUp(t, updateUI);

        // Ozn·mime UI panelu, ûe sme zÌskali ikonu tohto predmetu.
        UITreasureChest.NotifyItemReceived(t.data.icon);
        return t;
    }

    // Pok˙si sa daù hr·Ëovi ˙plne nov˝ predmet.
    T TryGive<T>(PlayerInventory inventory, bool updateUI = true) where T : ItemData
    {
        // Vsetky sloty su plne
        if (inventory.GetSlotsLeftFor<T>() <= 0) return null;

        // ZÌska zoznam vöetk˝ch predmetov, ktorÈ hr·Ë eöte nem·.
        T[] possibilities = inventory.GetUnowned<T>();
        if (possibilities.Length == 0) return null;

        // Prid·me n·hodn˝ nov˝ predmet do invent·ra.
        T t = possibilities[Random.Range(0, possibilities.Length)];
        inventory.Add(t, updateUI);

        UITreasureChest.NotifyItemReceived(t.icon);
        return t;
    }

    // Funkcia ktoru zavolame ked je dokoncena animacia
    public void NotifyComplete()
    {
        recipient.weaponUI.Refresh();
        recipient.passiveUI.Refresh();
    }

    // Prech·dza zoznam priorÌt, k˝m jedna metÛda nevr·ti odmenu.
    void Open(PlayerInventory inventory)
    {
        if (inventory == null) return;

        // Priorita 1: Evol˙cia (ak je povolen· a moûn·)
        if (possibleDrops.HasFlag(DropType.Evolution) && TryEvolve<Weapon>(inventory, false)) return;
        // Priorita 2: Vylepöenie zbrane
        if (possibleDrops.HasFlag(DropType.UpgradeWeapon) && TryUpgrade<Weapon>(inventory, false)) return;
        // Priorita 3: Vylepöenie pasÌvky
        if (possibleDrops.HasFlag(DropType.UpgradePassive) && TryUpgrade<Passive>(inventory, false)) return;
        // Priorita 4: Nov· zbraÚ
        if (possibleDrops.HasFlag(DropType.NewWeapon) && TryGive<WeaponData>(inventory, false)) return;
        // Priorita 5: Nov· pasÌvka
        if (possibleDrops.HasFlag(DropType.NewPassive) && TryGive<PassiveData>(inventory, false)) return;
        if (defaultDropSprite) UITreasureChest.NotifyItemReceived(defaultDropSprite);
        return;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out PlayerInventory p))
        {
            recipient = p;

            // Najprv vygenerujeme odmeny
            int rewardCount = GetRewardCount();
            for (int i = 0; i < rewardCount; i++)
            {
                Open(p);
            }

            // Deaktivujeme truhlicu vo svete, aby sa nedala vybraù znova.
            gameObject.SetActive(false);

            // Aktivujeme UI panel, ktor˝ spustÌ anim·ciu a VFX.
            UITreasureChest.Activate(p.GetComponentInChildren<PlayerCollector>(), this);

            // Zv˝öime glob·lne poËÌtadlo vybran˝ch truhlÌc pre striedanie profilov. ? (dropProfiles.Length + 1)
            totalPickups = (totalPickups + 1) % (dropProfiles.Length);
        }
    }
}
