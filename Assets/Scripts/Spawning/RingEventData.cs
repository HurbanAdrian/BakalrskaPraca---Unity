using UnityEngine;

[CreateAssetMenu(fileName = "Ring Event Data", menuName = "Game/Event Data/Ring")]
public class RingEventData : EventData
{
    [Header("Mob Data")]
    public ParticleSystem spawnEffectPrefab;
    public Vector2 scale = new Vector2(1, 1);
    [Min(0)] public float spawnRadius = 10f;
    public float lifespan = 15f;

    public override bool Activate(PlayerStats player = null)
    {
        // Aktivuj iba v prípade, že je hráè prítomný.
        if (player)
        {
            GameObject[] spawns = GetSpawns();

            // Výpoèet uhlového odstupu medzi nepriate¾mi (v radiánoch).
            float angleOffset = 2 * Mathf.PI / Mathf.Max(1, spawns.Length);
            float currentAngle = 0;

            foreach (GameObject g in spawns)
            {
                // Výpoèet pozície pre spawn.
                Vector3 spawnPosition = player.transform.position + new Vector3(
                    spawnRadius * Mathf.Cos(currentAngle) * scale.x,
                    spawnRadius * Mathf.Sin(currentAngle) * scale.y
                );

                // Ak je priradený èasticový efekt (spawnEffectPrefab), prehraj ho na danej pozícii.
                if (spawnEffectPrefab)
                {
                    Instantiate(spawnEffectPrefab, spawnPosition, Quaternion.identity);
                }

                // Vytvorenie samotného nepriate¾a.
                GameObject s = Instantiate(g, spawnPosition, Quaternion.identity);

                // Ak je nastavená životnos (lifespan > 0), nastav nepriate¾a na automatické znièenie.
                if (lifespan > 0) Destroy(s, lifespan);

                currentAngle += angleOffset;
            }
        }

        return false;
    }
}
