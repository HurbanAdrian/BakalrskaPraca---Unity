using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    float currentEventCooldown = 0;

    public EventData[] events;

    [Tooltip("Ako dlho èaka, kım sa tento manaér stane aktívnym.")]
    public float firstTriggerDelay = 180f;

    [Tooltip("Ako dlho èaka medzi jednotlivımi udalosami.")]
    public float triggerInterval = 30f;

    public static EventManager instance;

    [System.Serializable]
    public class Event
    {
        public EventData data;
        public float duration, cooldown = 0;
    }

    // Zoznam udalostí, ktoré boli aktivované a momentálne beia.
    List<Event> runningEvents = new List<Event>();

    PlayerStats[] allPlayers;

    void Start()
    {
        if (instance) Debug.LogWarning("V scéne je viac ako 1 Event Manager! Odstráòte nadbytoèné.");
        instance = this;

        currentEventCooldown = firstTriggerDelay > 0 ? firstTriggerDelay : triggerInterval;
        allPlayers = FindObjectsByType<PlayerStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    void Update()
    {
        // Cooldown pre pridanie ïalšej udalosti do zoznamu
        currentEventCooldown -= Time.deltaTime;

        if (currentEventCooldown <= 0)
        {
            // Získaj náhodnú udalos a skús ju vykona
            EventData e = GetRandomEvent();

            // Skontroluj, èi udalos existuje a èi prešla testom pravdepodobnosti (Luck)
            if (e && e.CheckIfWillHappen(allPlayers[Random.Range(0, allPlayers.Length)]))
            {
                runningEvents.Add(new Event
                {
                    data = e,
                    duration = e.duration
                });
            }

            // Nastav cooldown pre ïalší pokus o spustenie udalosti
            currentEventCooldown = triggerInterval;
        }

        // Zoznam udalostí, ktoré chceme odstráni (skonèili)
        List<Event> toRemove = new List<Event>();

        // Cooldown pre existujúce udalosti, aby sme videli, èi majú pokraèova
        foreach (Event e in runningEvents)
        {
            // Zniuj celkové trvanie udalosti
            e.duration -= Time.deltaTime;
            if (e.duration <= 0)
            {
                toRemove.Add(e);
                continue;
            }

            // Zniuj cooldown pre vnútornú akciu udalosti
            e.cooldown -= Time.deltaTime;
            if (e.cooldown <= 0)
            {
                // Vyber náhodného hráèa, na ktorého udalos zacielime, a aktivuj ju
                e.data.Activate(allPlayers[Random.Range(0, allPlayers.Length)]);
                e.cooldown = e.data.GetSpawnInterval();
            }
        }

        // Odstráò všetky udalosti, ktorım vypršal èas
        foreach (Event e in toRemove) runningEvents.Remove(e);
    }

    public EventData GetRandomEvent()
    {
        // Ak nie sú priradené iadne udalosti, nevrá niè
        if (events.Length <= 0) return null;

        // Získaj zoznam všetkıch monıch udalostí
        List<EventData> possibleEvents = new List<EventData>(events);

        // Pridaj udalosti do zoznamu 'possibleEvents' iba v prípade, e je udalos aktívna.
        foreach (EventData e in events)
        {
            if (e.IsActive())
            {
                possibleEvents.Add(e);
            }
        }

        // Náhodne vyber jednu udalos zo zoznamu monıch udalostí na spustenie.
        if (possibleEvents.Count > 0)
        {
            EventData result = possibleEvents[Random.Range(0, possibleEvents.Count)];
            return result;
        }

        return null;
    }
}
