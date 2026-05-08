using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class CameraUtils
{
    /// <summary>
    /// 捕获相机画面并保存到 DataPath 指定的根目录下
    /// </summary>
    /// <param name="sceneCamera">目标相机</param>
    /// <param name="fileName">文件名（如 "screenshot.jpg"）</param>
    /// <param name="width">分辨率宽</param>
    /// <param name="height">分辨率高</param>
    /// <returns>返回 Texture2D 对象</returns>
    public static async UniTask<Texture2D> CaptureAndSaveAsync(Camera sceneCamera, string fileName, int width = 640, int height = 360)
    {
        string savePath = Path.Combine(DataPath.GetRoot(), fileName);

        // 准备渲染纹理 (GPU)
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
        RenderTexture originalTarget = sceneCamera.targetTexture;
        sceneCamera.targetTexture = rt;

        // 执行渲染
        sceneCamera.Render();

        // 将像素读取到 Texture2D (CPU)
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // 还原环境并释放临时 RT
        sceneCamera.targetTexture = originalTarget;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        // 异步编码并写入文件
        // 获取原始字节（主线程）
        byte[] jpgBytes = texture.EncodeToJPG(85);

        // 在线程池中执行 IO 操作
        await UniTask.RunOnThreadPool(async () =>
        {
            try
            {
                string dir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                await File.WriteAllBytesAsync(savePath, jpgBytes);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CaptureSystem] 保存图片失败: {e.Message}");
            }
        });

        Debug.Log($"<color=cyan>[CaptureSystem]</color> 图片已保存至: {savePath}");

        return texture;
    }
}