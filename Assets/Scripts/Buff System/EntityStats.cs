using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EntityStats je trieda, ktor˙ dedia PlayerStats aj EnemyStats.
/// Poskytuje spÙsob, ako aplikovaù buffy na obe tieto entity.
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    protected float health;

    // SystÈm tÛnovania (Tinting system).
    protected SpriteRenderer sprite;
    protected Animator animator;
    protected Color originalColor;
    protected List<Color> appliedTints = new List<Color>();
    public const float TINT_FACTOR = 4f;

    [System.Serializable]
    public class Buff
    {
        public BuffData data;
        public float remainingDuration, nextTick;
        public int variant;

        public ParticleSystem effect;
        public Color tint;
        public float animationSpeed = 1f;

        public Buff(BuffData d, EntityStats owner, int variant = 0, float durationMultiplier = 1f)
        {
            data = d;
            BuffData.Stats buffStats = d.Get(variant);
            remainingDuration = buffStats.duration * durationMultiplier;
            nextTick = buffStats.tickInterval;
            this.variant = variant;

            if (buffStats.effect)
            {
                effect = Instantiate(buffStats.effect, owner.transform);
            }
            if (buffStats.tint.a > 0)
            {
                tint = buffStats.tint;
                owner.ApplyTint(buffStats.tint);
            }

            animationSpeed = buffStats.animationSpeed;
            owner.ApplyAnimationMultiplier(animationSpeed);
        }

        public BuffData.Stats GetData()
        {
            return data.Get(variant);
        }
    }

    protected List<Buff> activeBuffs = new List<Buff>();

    [System.Serializable]
    public class BuffInfo
    {
        public BuffData data;
        public int variant;
        [Range(0f, 1f)] public float probability = 1f;
    }

    protected virtual void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalColor = sprite.color;
        animator = GetComponent<Animator>();
    }

    public virtual void ApplyAnimationMultiplier(float factor)
    {
        // Zabr·nime tomu, aby bol faktor nula, pretoûe to spÙsobuje problÈmy pri odstraÚovanÌ (delenie nulou).
        animator.speed *= Mathf.Approximately(0, factor) ? 0.000001f : factor;
    }

    public virtual void RemoveAnimationMultiplier(float factor)
    {
        animator.speed /= Mathf.Approximately(0, factor) ? 0.000001f : factor;
    }

    public virtual void ApplyTint(Color c)
    {
        appliedTints.Add(c);
        UpdateColor();
    }

    public virtual void RemoveTint(Color c)
    {
        appliedTints.Remove(c);
        UpdateColor();
    }

    protected virtual void UpdateColor()
    {
        // V˝poËet cieæovej farby.
        Color targetColor = originalColor;
        float totalWeight = 1f;

        foreach (Color c in appliedTints)
        {
            targetColor = new Color(
                targetColor.r + c.r * c.a * TINT_FACTOR,
                targetColor.g + c.g * c.a * TINT_FACTOR,
                targetColor.b + c.b * c.a * TINT_FACTOR,
                targetColor.a
            );
            totalWeight += c.a * TINT_FACTOR;
        }

        targetColor = new Color(
            targetColor.r / totalWeight,
            targetColor.g / totalWeight,
            targetColor.b / totalWeight,
            targetColor.a
        );

        // NastavÌme farbu n·öho spritu na vypoËÌtan˙ cieæov˙ farbu.
        sprite.color = targetColor;
    }

    // ZÌska konkrÈtny buff zo zoznamu aktÌvnych buffov. Ak nespecifikujeme tak vratime hocijaky variant
    public virtual Buff GetBuff(BuffData data, int variant = -1)
    {
        foreach (Buff b in activeBuffs)
        {
            if (b.data == data)
            {
                // Ak je öpecifikovan˝ variant, musÌme skontrolovaù aj ten.
                if (variant >= 0)
                {
                    if (b.variant == variant) return b;
                }
                else
                {
                    return b;
                }
            }
        }
        return null;
    }

    // Aplikuje buff cez BuffInfo (skontroluje pravdepodobnosù).
    public virtual bool ApplyBuff(BuffInfo info, float durationMultiplier = 1f)
    {
        if (Random.value <= info.probability)
        {
            return ApplyBuff(info.data, info.variant, durationMultiplier);
        }
        return false;
    }

    // Prid· buff entite na z·klade jeho pravidiel vrstvenia (stacking).
    public virtual bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1f)
    {
        Buff b;
        BuffData.Stats s = data.Get(variant);

        switch (s.stackType)
        {
            case BuffData.StackType.stacksFully:
                activeBuffs.Add(new Buff(data, this, variant, durationMultiplier));
                RecalculateStats();
                return true;

            case BuffData.StackType.refreshDurationOnly:
                b = GetBuff(data, variant);
                if (b != null)
                {
                    b.remainingDuration = s.duration * durationMultiplier;
                }
                else
                {
                    activeBuffs.Add(new Buff(data, this, variant, durationMultiplier));
                    RecalculateStats();
                }
                return true;

            case BuffData.StackType.doesNotStack:
                b = GetBuff(data, variant);
                if (b == null)
                {
                    activeBuffs.Add(new Buff(data, this, variant, durationMultiplier));
                    RecalculateStats();
                    return true;
                }
                return false;
        }
        return false;
    }

    // Odstr·ni vöetky kÛpie konkrÈtneho buffu. Inac vsetky varianty vymazeme
    public virtual bool RemoveBuff(BuffData data, int variant = -1)
    {
        List<Buff> toRemove = new List<Buff>();
        foreach (Buff b in activeBuffs)
        {
            if (b.data == data)
            {
                if (variant >= 0)
                {
                    if (b.variant == variant) toRemove.Add(b);
                }
                else
                {
                    toRemove.Add(b);
                }
            }
        }

        if (toRemove.Count > 0)
        {
            foreach (Buff b in toRemove)
            {
                if (b.effect)
                {
                    Destroy(b.effect.gameObject);
                }
                if (b.tint.a > 0)
                {
                    RemoveTint(b.tint);
                }
                RemoveAnimationMultiplier(b.animationSpeed);
                activeBuffs.Remove(b);
            }
            RecalculateStats();
            return true;
        }
        return false;
    }

    // AbstraktnÈ metÛdy, ktorÈ musia implementovaù potomkovia (Player/Enemy).
    public abstract void TakeDamage(float dmg);
    public abstract void RestoreHealth(float amount);
    public abstract void Kill();
    public abstract void RecalculateStats();

    protected virtual void Update()
    {
        // OdpoËÌtavanie trvania buffov a ich tikov (lieËenie/poökodenie).
        List<Buff> expired = new List<Buff>();
        foreach (Buff b in activeBuffs)
        {
            BuffData.Stats s = b.data.Get(b.variant);

            // Tiky pre poökodenie a lieËenie.
            b.nextTick -= Time.deltaTime;
            if (b.nextTick < 0)
            {
                float tickDmg = b.data.GetTickDamage(b.variant);
                if (tickDmg > 0) TakeDamage(tickDmg);

                float tickHeal = b.data.GetTickHeal(b.variant);
                if (tickHeal > 0) RestoreHealth(tickHeal);

                b.nextTick = s.tickInterval;
            }

            // Ak m· buff trvanie 0 alebo menej, zost·va navûdy.
            if (s.duration <= 0) continue;

            // Zniûovanie zost·vaj˙ceho Ëasu buffu.
            b.remainingDuration -= Time.deltaTime;
            if (b.remainingDuration < 0) expired.Add(b);
        }

        // Odstr·nenie expirovan˝ch buffov mimo foreach cyklu.
        if (expired.Count > 0)
        {
            foreach (Buff b in expired)
            {
                if (b.effect)
                {
                    Destroy(b.effect.gameObject);
                }
                if (b.tint.a > 0)
                {
                    RemoveTint(b.tint);
                }
                RemoveAnimationMultiplier(b.animationSpeed);
                activeBuffs.Remove(b);
            }
            RecalculateStats();
        }
    }
}
