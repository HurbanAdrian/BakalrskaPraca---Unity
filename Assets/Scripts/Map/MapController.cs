using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class MapController : MonoBehaviour
{

    public Camera referenceCamera;
    public float checkInterval = 0.5f;

    [Header("Chunk Settings")]
    public PropRandomizer[] terrainChunks;
    public Vector2 chunkSize = new Vector2(20f, 20f);
    public LayerMask terrainMask = 1;
    public bool deleteCulledChunks = false;

    // UloûÌ polohu a veækosù poslednej kamery.
    // UrËÌme, Ëi je potrebnÈ vykonaù kontroly.
    Vector3 lastCameraPosition;
    Rect lastCameraRect;
    float cullDistanceSqr;

    void Start()
    {
        // VypÌöe chyby, keÔ nie s˙ priradenÈ dÙleûitÈ premennÈ.
        if (!referenceCamera) 
            Debug.LogError("MapController cannot work without a reference camera.");

        if (terrainChunks.Length < 1)
            Debug.LogError("There are no Terrain Chunks assigned, so the map cannot be dynamically generated.");

        // Spustenie korutiny na kontrolu mapy.
        StartCoroutine(HandleMapCheck());
        HandleChunkSpawning(Vector2.zero, true);
    }

    void Reset()
    {
        referenceCamera = Camera.main;
    }

    // Korutina, ktor· sa periodicky sp˙öùa na kontrolu a vytv·ranie nov˝ch ËastÌ mapy.
    IEnumerator HandleMapCheck()
    {
        for(;;) 
        {
            yield return new WaitForSeconds(checkInterval);

            // Aktualizovaù mapu iba v prÌpade, ûe je platn· jedna z t˝chto podmienok.
            Vector3 moveDelta = referenceCamera.transform.position - lastCameraPosition;
            bool hasCamWidthChanged = !Mathf.Approximately(referenceCamera.pixelWidth - lastCameraRect.width, 0),
                 hasCamHeightChanged = !Mathf.Approximately(referenceCamera.pixelHeight - lastCameraRect.height, 0);

            if (hasCamWidthChanged || hasCamHeightChanged || moveDelta.magnitude > 0.1f) {
                HandleChunkCulling();
                HandleChunkSpawning(moveDelta, true);
            }

            lastCameraPosition = referenceCamera.transform.position;
            lastCameraRect = referenceCamera.pixelRect;
        }
    }

    // ZÌska obdÂûnik, ktor˝ predstavuje oblasù, ktor˙ kamera pokr˝va v hernom svete.
    public Rect GetWorldRectFromViewport() 
    {
        if (!referenceCamera) 
        {
            Debug.LogError("Reference camera not found. Using Main Camera instead.");
            referenceCamera = Camera.main;
        }

        Vector2 minPoint = referenceCamera.ViewportToWorldPoint(referenceCamera.rect.min),
                maxPoint = referenceCamera.ViewportToWorldPoint(referenceCamera.rect.max);
        Vector2 size = new Vector2(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);
        cullDistanceSqr = Mathf.Max(size.sqrMagnitude, chunkSize.sqrMagnitude) * 3;

        return new Rect(minPoint, size);
    }

    // ZÌska vöetky body, ktorÈ musÌme skontrolovaù na chunky.
    public Vector2[] GetCheckedPoints()
    {
        Rect viewArea = GetWorldRectFromViewport();
        Vector2Int tileCount = new Vector2Int(
            (int)Mathf.Ceil(viewArea.width / chunkSize.x) + 1,
            (int)Mathf.Ceil(viewArea.height / chunkSize.y) + 1
        );

        HashSet<Vector2> result = new HashSet<Vector2>();
        for (int y = -1; y < tileCount.y; y++)
        {
            for (int x = -1; x < tileCount.x; x++)
            {
                result.Add(new Vector2(
                    viewArea.min.x + chunkSize.x * x,
                    viewArea.min.y + chunkSize.y * y
                ));
            }
        }

        return result.ToArray();
    }

    void HandleChunkSpawning(Vector2 moveDelta, bool checkWithoutDelta = false)
    {

        HashSet<Vector2> spawnedPositions = new HashSet<Vector2>();
        Vector2 currentPosition = referenceCamera.transform.position;

        // Skontroluje vöetky body v˝rezu, ktorÈ n·s zaujÌmaj˙.
        foreach (Vector3 vp in GetCheckedPoints())
        {
            if(!checkWithoutDelta) {
                // Væavo/Vpravo kontrolujte iba vtedy, ak sa pohybujeme.
                if (moveDelta.x > 0 && vp.x < 0.5f) continue;
                else if (moveDelta.x < 0 && vp.x > 0.5f) continue;

                // Hore / dole kontrolujte iba vtedy, ak sa pohybujeme.
                if (moveDelta.y > 0 && vp.y < 0.5f) continue;
                else if (moveDelta.y < 0 && vp.y > 0.5f) continue;
            }

            // Snapne kontrolovan˙ pozÌciu k najbliûöej pozÌcii v blokoch.
            Vector3 checkedPosition = SnapPosition(vp);

            // Ak pozÌcia nem· ûiadne chunky, tak sa vytvorÌ chunk.
            if (!spawnedPositions.Contains(checkedPosition) && !Physics2D.OverlapPoint(checkedPosition, terrainMask))
                SpawnChunk(checkedPosition);

            spawnedPositions.Add(checkedPosition);
        }
    }

    // Zaokruhli Vector na najblizsiu poziciu danu chunkSizom
    Vector3 SnapPosition(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / chunkSize.x) * chunkSize.x,
            Mathf.Round(position.y / chunkSize.y) * chunkSize.y, 
            transform.position.z
        );
    }

    // Spawne chunk na danej pozicii
    PropRandomizer SpawnChunk(Vector3 spawnPosition, int variant = -1)
    {
        if (terrainChunks.Length < 1) return null;
        int rand = variant < 0 ? Random.Range(0, terrainChunks.Length) : variant;
        PropRandomizer chunk = Instantiate(terrainChunks[rand], transform);
        chunk.transform.position = spawnPosition;
        return chunk;
    }

    // Rozhodne ci chunk bude ukazany alebo schovany
    void HandleChunkCulling()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform chunk = transform.GetChild(i);
            Vector2 dist = referenceCamera.transform.position - chunk.position;
            bool cull = dist.sqrMagnitude > cullDistanceSqr;
            chunk.gameObject.SetActive(!cull);
            if(deleteCulledChunks && cull) Destroy(chunk.gameObject);
        }
    }
}