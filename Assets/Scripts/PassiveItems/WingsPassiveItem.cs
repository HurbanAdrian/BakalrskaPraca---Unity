using UnityEngine;

public class WingsPassiveItem : PassiveItem
{
    protected override void ApplyModifier()
    {
        playerStats.currentMoveSpeed *= 1 + passiveItemData.Multiplier / 100f;          // Najskor vydelime percenta, aby sme dostali decimalnu hodnotu, a potom to vynasobime s aktualnou rychlostou, aby sme ju zvysili o tu hodnotu
    
    }
}
