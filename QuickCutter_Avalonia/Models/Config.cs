﻿using System;
using System.Text.Json.Serialization;

namespace QuickCutter_Avalonia.Mode
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WindowStartUpStyles
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
        public double windowHistoryWidth { get; set; }
        public double windowHistoryHeight { get; set;}

        // Video Setting
        public int moveStep { get; set; }
    }
}
