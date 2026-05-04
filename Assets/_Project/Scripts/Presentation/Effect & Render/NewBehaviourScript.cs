using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class CameraCapture
{
    /// <summary>
    /// 捕获指定相机的画面并保存为 JPG 文件
    /// </summary>
    /// <param name="sceneCamera">负责渲染场景的相机（不含 UI）</param>
    /// <param name="savePath">文件保存的完整路径</param>
    /// <param name="width">截图宽度</param>
    /// <param name="height">截图高度</param>
    public static async UniTask CaptureAndSaveAsync(Camera sceneCamera, string savePath, int width = 640, int height = 360)
    {
        // 1. 获取临时渲染纹理 (GPU 画布)
        // 使用 24 位深度缓冲区以确保 3D 场景渲染正确
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);

        // 记录相机原本的渲染目标，避免破坏主屏幕显示
        RenderTexture originalTarget = sceneCamera.targetTexture;
        sceneCamera.targetTexture = rt;

        // 2. 强制相机进行一次手动渲染
        // 因为目标是 RT，所以这次渲染不会直接显示在玩家屏幕上
        sceneCamera.Render();

        // 3. 将像素数据从 GPU 复制到 CPU
        // 设置当前活跃纹理，ReadPixels 只能从 RenderTexture.active 中读取数据
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // 此步为同步操作，由于分辨率已调低 (640x360)，性能损耗极小
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // 4. 环境清理（极其重要，防止内存泄漏和渲染错误）
        sceneCamera.targetTexture = originalTarget;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        // 5. 异步编码 (耗时压缩逻辑)
        // 使用 UniTask 切换到线程池执行，防止 EncodeToJPG 导致主线程卡顿
        byte[] jpgBytes = await UniTask.RunOnThreadPool(() =>
        {
            // 在子线程中将像素数组压缩成 JPG 格式
            return texture.EncodeToJPG(85); // 85 为质量参数
        });

        // 6. 销毁 CPU 端的纹理对象
        // Texture2D 是 UnityEngine 对象，必须在主线程 Destroy 释放内存
        Object.Destroy(texture);

        // 7. 异步写入硬盘
        // 使用 C# 原生异步文件流，避免 I/O 阻塞
        await File.WriteAllBytesAsync(savePath, jpgBytes);

        Debug.Log($"[SaveSystem] 截图已存至: {savePath}");
    }
}