using System;
using System.Collections;
using UnityEngine;

namespace GorillaShirts.Extensions
{
    public static class CameraEx
    {
        public static IEnumerator SnapPhoto(this Camera camera, Action<Texture2D> onPhotoSnapped)
        {
            yield return new WaitForEndOfFrame();

            RenderTexture renderTexture = camera.targetTexture;

            RenderTexture.active = renderTexture;

            int width = renderTexture.width;
            int height = renderTexture.height;
            RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
            Texture2D tex = new(width, height, TextureFormat.RGB24, false);

            RenderTexture.active = renderTex;
            camera.targetTexture = renderTex;

            camera.Render();

            camera.targetTexture = renderTexture;

            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            RenderTexture.active = null;

            RenderTexture.ReleaseTemporary(renderTex);

            onPhotoSnapped?.Invoke(tex);
        }
    }
}
