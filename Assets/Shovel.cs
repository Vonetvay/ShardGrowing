using UnityEngine;

public class Shovel : Tool, IInteractWithTerrain
{
    [SerializeField] private float radius = 3f;

    public void TerrainInteract(TerrainManager terrain, int cx, int cy)
    {
        terrain.DrawCircleCPU(cx, cy, radius);
    }
}
