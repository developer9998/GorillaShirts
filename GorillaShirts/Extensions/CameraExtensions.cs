using GorillaShirts.Tools;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace GorillaShirts.Extensions
{
    internal static class CameraExtensions
    {
        public static async Task<Texture2D> Render(this Camera camera)
        {
            await new WaitForEndOfFrame().AsAwaitable();

            Texture2D texture;

            RenderTexture targetTexture = camera.targetTexture;

            RenderTexture renderTexture;

            int width = targetTexture.width, height = targetTexture.height;

            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                Logging.Warning("AsyncGPUReadback is not supported");

                RenderTexture active = RenderTexture.active;
                RenderTexture.active = targetTexture;

                renderTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = targetTexture;

                texture = new(width, height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(renderTexture);

                return texture;
            }

            renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            camera.targetTexture = renderTexture;
            camera.Render();

            TaskCompletionSource<AsyncGPUReadbackRequest> taskCompletionSource = new();
            AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGB24, taskCompletionSource.SetResult);
            AsyncGPUReadbackRequest request = await taskCompletionSource.Task;

            camera.targetTexture = targetTexture;
            RenderTexture.ReleaseTemporary(renderTexture);

            if (request.hasError)
            {
                Logging.Error("AsyncGPUReadbackRequest.hasError");
                return null;
            }

            NativeArray<byte> data = request.GetData<byte>();
            texture = new Texture2D(width, height, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Point
            };
            texture.LoadRawTextureData(data);
            texture.Apply();

            return texture;
        }
    }
}
