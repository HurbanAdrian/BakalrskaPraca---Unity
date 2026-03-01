using UnityEngine;

public class BobbingAnimation : MonoBehaviour
{
    public float frequency;     // Rychlost bobbing efektu
    public float amplitude;     // Velikost bobbing efektu (vyska alebo rozdmedzie)
    public Vector3 direction;     // Smer bobbing efektu (napr. Vector3.up pro vertikální bobbing)
    Vector3 initialPosition;   // Pociatocna pozicia objektu

    void Start()
    {
        initialPosition = transform.position;   // Ulozenie pociatocnej pozicie
    }

    void Update()
    {
        transform.position = initialPosition + direction * Mathf.Sin(Time.time * frequency) * amplitude;   // Vypocet novej pozicie na zaklade sinusovej funkcie
    }
}
