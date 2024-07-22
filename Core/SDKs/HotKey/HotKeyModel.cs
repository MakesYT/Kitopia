using System.Text.Json.Serialization;

namespace Core.SDKs.HotKey;

/// <summary>
///     快捷键模型
/// </summary>
public struct HotKeyModel
{
    public HotKeyModel()
    {
        UUID = Guid.NewGuid().ToString();
    }

    [Obsolete("此方法仅供Json反序列化使用")]
    public HotKeyModel(string uuid)
    {
        UUID = uuid;
    }

    public string UUID { get; init; }
    public string? MainName { get; init; }

    /// <summary>
    ///     设置项名称
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     设置项快捷键是否可用
    /// </summary>
    public bool IsUsable { get; init; }

    /// <summary>
    ///     是否勾选Ctrl按键
    /// </summary>
    public bool IsSelectCtrl { get; init; }

    /// <summary>
    ///     是否勾选Shift按键
    /// </summary>
    public bool IsSelectShift { get; init; }

    /// <summary>
    ///     是否勾选Alt按键
    /// </summary>
    public bool IsSelectAlt { get; init; }

    /// <summary>
    ///     是否勾选Alt按键
    /// </summary>
    public bool IsSelectWin { get; init; }

    /// <summary>
    ///     选中的按键
    /// </summary>
    public EKey SelectKey { get; init; }

    /// <summary>
    ///     快捷键按键集合
    /// </summary>
    public static Array Keys => Enum.GetValues(typeof(EKey));


    [JsonIgnore] public string SignName => $"{this.MainName}_{this.Name}";
}