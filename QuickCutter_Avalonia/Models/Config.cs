using System;
using System.Text.Json.Serialization;

namespace QuickCutter_Avalonia.Mode
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WindowStartUpStyles
    {
        AUTOADJUST = 1,
        ALWAYSMAXIMIZE = 2,
        HISTORY = 3
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TextLanguages
    {
        ENGLISH = 1,
        CHINESE = 2
    }

    /// <summary>
    /// 本软件配置文件实体类
    /// </summary>
    [Serializable]
    internal class Config
    {
        public WindowStartUpStyles windowStartUpStyles {  get; set; }
        public double windowHistoryWidth { get; set; }
        public double windowHistoryHeight { get; set;}
        public TextLanguages Languages { get; set; }
        // Video Setting
        public int moveStep { get; set; }
    }
}
