using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    WeaponData weaponData;
    string[] weaponSubtypes;
    int selectedWeaponSubtype;

    private void OnEnable()
    {
        weaponData = (WeaponData)target;

        // Zoberie vsetky subtypy Weapon a cachne ich
        System.Type baseType = typeof(Weapon);
        List<System.Type> subTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && p != baseType)
            .ToList();

        // Prida None moznost vo fronte
        List<string> subTypesString = subTypes.Select(t => t.Name).ToList();
        subTypesString.Insert(0, "None");
        weaponSubtypes = subTypesString.ToArray();

        // Zaisti ze pouzijeme spravny weapon subtype
        selectedWeaponSubtype = Math.Max(0, Array.IndexOf(weaponSubtypes, weaponData.behaviour));
    }

    public override void OnInspectorGUI()
    {
        // Vykreslenie rozbaæovacieho menu (dropdown) v Unity Inöpektore.
        selectedWeaponSubtype = EditorGUILayout.Popup("Behaviour", Math.Max(0, selectedWeaponSubtype), weaponSubtypes);

        if (selectedWeaponSubtype > 0)
        {
            // Aktualiz·cia premennej "behaviour" v d·tach zbrane podæa toho, Ëo sme vybrali v menu.
            weaponData.behaviour = weaponSubtypes[selectedWeaponSubtype].ToString();

            EditorUtility.SetDirty(weaponData); // OznaËenie objektu, ûe bol zmenen˝ a je potrebnÈ ho uloûiù.

            DrawDefaultInspector(); // Vykreslenie ostatn˝ch, predvolen˝ch premenn˝ch pod t˝mto menu.
        }
    }
}
