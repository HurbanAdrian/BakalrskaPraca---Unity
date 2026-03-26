using UnityEngine;

/// <summary>
/// GameObject ktory je spawnuty ako efekt vzdy ked sa aktivuje zbran.
/// </summary>
public abstract class WeaponEffect : MonoBehaviour
{
    [HideInInspector]
    public PlayerStats owner;
    [HideInInspector]
    public Weapon weapon;

    public PlayerStats Owner {  get { return owner; } }

    public float GetDamage()
    {
        return weapon.GetDamage();
    }
}
