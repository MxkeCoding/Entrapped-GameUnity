using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Experimental.Rendering.Universal;

public class TilemapLighting : MonoBehaviour
{
    public GameObject lightPrefab; // Prefab with a 2D Light component
    private Tilemap tilemap;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void UpdateTileLighting(Vector3Int tilePosition)
    {
        Vector3 worldPosition = tilemap.GetCellCenterWorld(tilePosition);
        Instantiate(lightPrefab, worldPosition, Quaternion.identity, transform);
    }

    private void OnTileChanged(Vector3Int position, TileBase tile)
    {
        if (tile != null)
        {
            UpdateTileLighting(position);
        }
    }
}
