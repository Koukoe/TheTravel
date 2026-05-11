using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class ImageLoader
{
    /// <summary>
    /// 从存档根目录异步加载图片
    /// </summary>
    public static async UniTask<Texture2D> LoadTextureAsync(string fileName)
    {
        string filePath = Path.Combine(DataPath.GetRoot(), fileName);

        if (!File.Exists(filePath))
        {
            // 空存档位没有图
            return null;
        }

        try
        {
            // 异步读取磁盘上的二进制数据
            // 使用 C# 原生异步 IO，不会卡住主线程
            byte[] fileData = await File.ReadAllBytesAsync(filePath);

            // 创建 Texture 对象
            // LoadImage 根据字节流自动调整纹理大小
            // 使用 RGB24 格式节省内存（截图不需要透明通道）
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);

            // 将字节流解码为像素数据
            // LoadImage 必须在主线程调用（UniTask 自动切回主线程）
            if (tex.LoadImage(fileData))
            {
                return tex;
            }
            else
            {
                Debug.LogError($"[ImageLoader] 解码图片失败: {fileName}");
                Object.Destroy(tex);
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ImageLoader] 加载图片时出错: {e.Message}");
            return null;
        }
    }
}