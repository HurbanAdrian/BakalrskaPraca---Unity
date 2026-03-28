using UnityEngine;

public class PickUp : Sortable
{
    public float lifespan = 0.5f;
    protected PlayerStats target;   // Ak m· predmet cieæ, letÌ smerom k nemu.
    protected float speed;          // R˝chlosù, ktorou sa predmet pohybuje.
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

    PlayerStats playerReference;

    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        initialOffset = Random.Range(0, bobbingAnimation.frequency);

        playerReference = FindAnyObjectByType<PlayerStats>();
    }

    protected override void Update()
    {
        base.Update();
        if (target)
        {
            // Zr˝chæujeme gem kaûd˝m framom, aby hr·Ëa ZARU»ENE dobehol
            speed += 20f * Time.deltaTime;

            // PresuÚ predmet smerom k hr·Ëovi a skontroluj vzdialenosù medzi nimi.
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
            // Spracuj anim·ciu (poskakovanie) objektu. Vypocet novej pozicie na zaklade sinusovej funkcie
            transform.position = initialPosition + bobbingAnimation.direction * Mathf.Sin((Time.time + initialOffset) * bobbingAnimation.frequency);  
        }
    }

    public virtual bool Collect(PlayerStats target, float speed, float lifespan = 0f)
    {
        if (!this.target)
        {
            this.target = target;
            this.speed = speed;
            //if (lifespan > 0) this.lifespan = lifespan;                 // ak nebude moct dobehnut hraca tak sa znici a aplikuje svoje efekty (fail save)
            //Invoke("GrantRewardsAndDestroy", Mathf.Max(0.01f, this.lifespan));
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

        Destroy(gameObject); // Odmeny s˙ odovzdanÈ, mÙûeme ho zniËiù
    }
}
