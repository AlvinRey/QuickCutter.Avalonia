using System;
using System.Text.Json.Serialization;

namespace QuickCutter_Avalonia.Mode
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    enum WindowStartUpStyles
    {
        AUTOADJUST = 1,
        ALWAYSMAXIMIZE = AUTOADJUST<<1,
        HISTORY = ALWAYSMAXIMIZE<<1
    }

    /// <summary>
    /// 本软件配置文件实体类
    /// </summary>
    [Serializable]
    internal class Config
    {
        public WindowStartUpStyles windowStartUpStyles {  get; set; }
    }
}
