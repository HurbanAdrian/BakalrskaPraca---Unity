using UnityEngine;

public class ChunkTrigger : MonoBehaviour
{
    MapController mapController;
    public GameObject targetMap;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapController = FindFirstObjectByType<MapController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mapController.currentChunk = targetMap;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (mapController.currentChunk == targetMap)
            {
                mapController.currentChunk = null;
            }
        }
    }
}
