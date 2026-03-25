using UnityEngine;

public class PickUp : MonoBehaviour
{
    public float lifespan = 0.5f;
    protected PlayerStats target;   // Ak má predmet cie¾, letí smerom k nemu.
    protected float speed;          // Rıchlos, ktorou sa predmet pohybuje.
    Vector2 initialPosition;
    float initialOffset;

    [System.Serializable]
    public struct BobbingAnimation
    {
        public float frequency;
        public Vector2 direction;
    }

    public BobbingAnimation bobbingAnimation = new BobbingAnimation
    {
        frequency = 2f,
        direction = new Vector2(0, 0.3f)
    };

    [Header("Bonuses")]
    public int experience;
    public int health;

    protected virtual void Start()
    {
        initialPosition = transform.position;
        initialOffset = Random.Range(0, bobbingAnimation.frequency);
    }

    protected virtual void Update()
    {
        if (target)
        {
            // Presuò predmet smerom k hráèovi a skontroluj vzdialenos medzi nimi.
            Vector2 distance = target.transform.position - transform.position;

            if (distance.sqrMagnitude > speed * speed * Time.deltaTime * Time.deltaTime)
            {
                transform.position += (Vector3)distance.normalized * speed * Time.deltaTime;
            }
            else
            {
                GrantRewardsAndDestroy();
            }
        }
        else
        {
            // Spracuj animáciu (poskakovanie) objektu. Vypocet novej pozicie na zaklade sinusovej funkcie
            transform.position = initialPosition + bobbingAnimation.direction * Mathf.Sin((Time.time + initialOffset) * bobbingAnimation.frequency);  
        }
    }

    public virtual bool Collect(PlayerStats target, float speed, float lifespan = 0f)
    {
        if (!this.target)
        {
            this.target = target;
            this.speed = speed;
            if (lifespan > 0) this.lifespan = lifespan;                 // ak nebude moct dobehnut hraca tak sa znici a aplikuje svoje efekty (fail save)
            Destroy(gameObject, Mathf.Max(0.01f, this.lifespan));
            return true;
        }

        return false;
    }

    // Lepsie ako OnDestroy
    protected virtual void GrantRewardsAndDestroy()
    {
        if (target)
        {
            if (experience != 0) target.IncreaseExperience(experience);
            if (health != 0) target.RestoreHealth(health);
        }

        Destroy(gameObject); // Odmeny sú odovzdané, môeme ho znièi
    }
}
