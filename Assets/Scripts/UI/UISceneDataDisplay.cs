using System.Text;
using System;
using System.Reflection;
using UnityEngine;
using TMPro;

public class UISceneDataDisplay : UIPropertyDisplay
{
    public UILevelSelector levelSelector;
    TextMeshProUGUI extraStageInfo;

    public override object GetReadObject()
    {
        if (levelSelector && UILevelSelector.selectedLevel >= 0)
            return levelSelector.levels[UILevelSelector.selectedLevel];

        return new UILevelSelector.SceneData();
    }

    /* 
     * T·to funkcia je o nieËo zloûitejöia ako v UIStatDisplay, pretoûe zobrazuje 
     * vlastnÈ premennÈ scÈny plus ötatistiky n·jdenÈ v playerModifier a enemyModifier.
     * Tieto ötatistiky prid·vame manu·lne volanÌm ProcessName() a ProcessValue().
    */
    public override void UpdateFields()
    {
        // ZÌskame referencie na textovÈ objekty (n·zvy, hodnoty a extra info).
        if (!propertyNames) propertyNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!propertyValues) propertyValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (!extraStageInfo) extraStageInfo = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        // ZÌskame z·kladnÈ reùazce pre vlastnosti scÈny.
        StringBuilder[] allStats = GetProperties(
            BindingFlags.Public | BindingFlags.Instance,
            "UILevelSelector+SceneData"
        );

        // ZÌskame objekt d·t aktu·lnej scÈny.
        UILevelSelector.SceneData dat = (UILevelSelector.SceneData)GetReadObject();

        // RuËne prid·me n·zvy ötatistÌk, ktorÈ chceme zobraziù z modifik·torov.
        allStats[0].AppendLine("Move Speed").AppendLine("Gold Bonus").AppendLine("Luck Bonus").AppendLine("XP Bonus").AppendLine("Enemy Health");

        // Spracujeme hodnoty z playerModifier (hr·Ëske bonusy).
        Type characterDataStats = typeof(CharacterData.Stats);
        ProcessValue(dat.playerModifier.moveSpeed, allStats[1], characterDataStats.GetField("moveSpeed"));
        ProcessValue(dat.playerModifier.greed, allStats[1], characterDataStats.GetField("greed"));
        ProcessValue(dat.playerModifier.luck, allStats[1], characterDataStats.GetField("luck"));
        ProcessValue(dat.playerModifier.growth, allStats[1], characterDataStats.GetField("growth"));

        // Spracujeme hodnoty z enemyModifier (nepriateæskÈ posilnenia).
        Type enemyStats = typeof(EnemyStats.Stats);
        ProcessValue(dat.enemyModifier.maxHealth, allStats[1], enemyStats.GetField("maxHealth"));

        // Aktualizujeme textovÈ polia v UI.
        if (propertyNames) propertyNames.text = allStats[0].ToString();
        if (propertyValues) propertyValues.text = allStats[1].ToString();
    }

    // Definuje, ktorÈ polia zo SceneData sa maj˙ zobraziù automaticky.
    protected override bool IsFieldShown(FieldInfo field)
    {
        switch (field.Name)
        {
            default:
                return false;
            case "timeLimit":
            case "clockSpeed":
            case "moveSpeed":
            case "greed":
            case "luck":
            case "growth":
            case "maxHealth":
                return true;
        }
    }

    // Ignorujeme pole extraNotes pri generovanÌ n·zvov (spracujeme ho zvl·öù).
    protected override StringBuilder ProcessName(string name, StringBuilder output, FieldInfo field)
    {
        if (field.Name == "extraNotes") return output;
        return base.ProcessName(name, output, field);
    }

    // äpeci·lne spracovanie hodnÙt pre Ëasov˝ limit, r˝chlosù hodÌn a percent·.
    protected override StringBuilder ProcessValue(object value, StringBuilder output, FieldInfo field)
    {
        float fval;
        switch (field.Name)
        {
            case "timeLimit":
                fval = value is int ? (int)value : (float)value;
                if (fval == 0)
                {
                    output.Append(DASH).Append('\n');
                }
                else
                {
                    // Form·tovanie sek˙nd na MM:SS
                    string minutes = Mathf.FloorToInt(fval / 60).ToString();
                    string seconds = (fval % 60).ToString();
                    if (fval % 60 < 10) seconds = "0" + seconds;
                    output.Append(minutes).Append(':').Append(seconds).Append('\n');
                }
                return output;

            case "clockSpeed":
                fval = value is int ? (int)value : (float)value;
                output.Append(fval).Append('x').Append('\n');
                return output;

            case "maxHealth":
            case "moveSpeed":
            case "greed":
            case "luck":
            case "growth":
                // Form·tovanie ötatistÌk ako percentu·lny bonus (napr. +20%).
                fval = value is int ? (int)value : (float)value;
                float percentage = Mathf.Round(fval * 100);

                if (Mathf.Approximately(percentage, 0))
                {
                    output.Append(DASH).Append('\n');
                }
                else
                {
                    if (percentage > 0) output.Append('+');
                    output.Append(percentage).Append('%').Append('\n');
                }
                return output;

            case "extraNotes":
                if (value == null) return output;
                string msg = value.ToString();
                extraStageInfo.text = string.IsNullOrWhiteSpace(msg) ? DASH : msg;
                return output;
        }

        // Ak nejde o öpeci·lne pole, odovzd·me spracovanie rodiËovskej triede.
        return base.ProcessValue(value, output, field);
    }

    void Reset()
    {
        levelSelector = FindAnyObjectByType<UILevelSelector>();
    }
}