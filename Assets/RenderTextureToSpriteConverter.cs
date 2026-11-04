using UnityEngine;
using UnityEngine.Experimental.Rendering; // Нужен для GraphicsFormat

public class RenderTextureToSpriteConverter
{
    public static Sprite ConvertRTToSprite(RenderTexture renderTexture)
    {
        if (renderTexture == null)
        {
            Debug.LogError("RenderTexture не назначена!");
            return null;
        }

        // 1. Создаем Texture2D, используя формат и цветовое пространство самой RenderTexture
        // Это автоматически учтет Linear/sRGB и исправит проблему с затемнением
        Texture2D texture2D = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            renderTexture.graphicsFormat,
            TextureCreationFlags.None
        );

        // 2. Читаем пиксели из RenderTexture в Texture2D
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = previousActive; // Безопасное восстановление предыдущей RT

        // 3. Создаем Sprite
        Sprite newSprite = Sprite.Create(
            texture2D,
            new Rect(0, 0, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f),
            128f
        );

        return newSprite;
    }
}