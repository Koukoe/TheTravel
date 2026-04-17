音频：
// 音轨播放与停止
AudioManager.Instance.PlayBGM(string name, float fadeTime = 1f);
AudioManager.Instance.StopBGM(StopTarget target = All, float fadeTime = 1f);  // Oldest, Latest, All
PlayAmbient/Voice, StopAmbient/Voice

// 音效播放与停止
AudioManager.Instance.PlaySFX(string name, Vector3? pos = null);

// 总体控制
AudioManager.Instance.PauseAllChannels();
AudioManager.Instance.ResumeAllChannels();
AudioManager.Instance.StopAllChannels();


对象池：
PoolManager.Global    // 全局单例（DontDestroyOnLoad）
PoolManager.Scene     // 场景单例（随场景销毁）

GameObject obj = PoolManager.Global.Get("对象名称");
// 或
GameObject obj = PoolManager.Scene.Get("对象名称");

PoolManager.Release(obj);

WhiteTown的喷泉涟漪效果可以通过直接调用FountainWaterRipple.CreateRipple()方法来实现