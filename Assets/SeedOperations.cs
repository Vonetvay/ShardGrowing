using UnityEngine;

public static class SeedOperations
{
    public static float RandomNumberFromSeed(ref float seed, float min, float max)
    {
        float a = 1664525;
        float c = 1013904223;
        float m = Mathf.Pow(2, 32);

        float next_seed = (a * seed + c) % m;
        seed = next_seed;

        float random_fraction = next_seed / m;

        return min + random_fraction * (max - min);
    }
    public static int RandomNumberFromSeed(ref float seed, int min, int max)
    {
        float a = 1664525;
        float c = 1013904223;
        float m = Mathf.Pow(2, 32);

        float next_seed = (a * seed + c) % m;
        seed = next_seed;

        float random_fraction = next_seed / m;

        return (int)(min + random_fraction * (max - min));
    }

    public static float RandomNumberFromSeed(ref float seed, Vector2 range)
    {
        float a = 1664525;
        float c = 1013904223;
        float m = Mathf.Pow(2, 32);

        float next_seed = (a * seed + c) % m;
        seed = next_seed;

        float random_fraction = next_seed / m;

        return range.x + random_fraction * (range.y - range.x);
    }

    public static int RandomNumberFromSeed(ref float seed, Vector2Int range)
    {
        float a = 1664525;
        float c = 1013904223;
        float m = Mathf.Pow(2, 32);

        float next_seed = (a * seed + c) % m;
        seed = next_seed;

        float random_fraction = next_seed / m;

        return (int)(range.x + random_fraction * (range.y - range.x));
    }
}
