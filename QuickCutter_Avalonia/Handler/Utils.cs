using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ReactiveUI;
using System.Text.Json;

namespace QuickCutter_Avalonia.Handler
{
    internal class Utils
    {
        static public IList<Codec>? Codec { get; set; }

        //static public string GetGraphicsCardManufacturer()
        //{
        //    string query = "SELECT * FROM Win32_VideoController";
        //    ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

        //    foreach (ManagementObject obj in searcher.Get())
        //    {
        //        string name = obj["Name"] as string;

        //        // 判断显卡品牌
        //        if (name != null && name.ToLower().Contains("nvidia"))
        //        {
        //            return "NVIDIA";
        //        }
        //        else if (name != null && name.ToLower().Contains("amd") || name.ToLower().Contains("ati"))
        //        {
        //            return "AMD";
        //        }
        //        // 可以根据具体情况添加其他品牌的判断条件
        //    }

        //    return "Unknown"; // 如果无法识别或未找到显卡信息，则返回 Unknown
        //}

        static public IEnumerable<Codec> GetCodec()
        {
            if (Codec != null) { return Codec; }

            Codec = new List<Codec>();
            try
            {
                Codec.Add(FFMpeg.GetCodec("libx264"));
                //switch (GetGraphicsCardManufacturer())
                //{
                //    case "NVIDIA":
                //        Codec.Add(FFMpeg.GetCodec("h264_nvenc"));
                //        break;
                //    case "AMD":
                //        Codec.Add(FFMpeg.GetCodec("h264_amf"));
                //        break;
                //}
                Codec.Add(FFMpeg.GetCodec("libx265"));
            }
            catch (FFMpegException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Codec;
        }



        #region TempPath

        public static string StartupPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetLogPath(string filename = "")
        {
            string _tempPath = Path.Combine(StartupPath(), "Logs");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if (string.IsNullOrEmpty(filename))
            {
                return _tempPath;
            }
            else
            {
                return Path.Combine(_tempPath, filename);
            }
        }

        public static string GetConfigPath(string filename = "")
        {
            string _tempPath = Path.Combine(StartupPath(), "guiConfigs");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if (string.IsNullOrEmpty(filename))
            {
                return _tempPath;
            }
            else
            {
                return Path.Combine(_tempPath, filename);
            }
        }
        #endregion

        #region Json 
        /// <summary>
        /// 取得存储资源
        /// </summary>
        /// <returns></returns>
        public static string? LoadResource(string res)
        {
            try
            {
                if (!File.Exists(res)) return null;
                return File.ReadAllText(res);
            }
            catch (Exception ex)
            {
                SaveLog(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// 反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strJson"></param>
        /// <returns></returns>
        public static T? FromJson<T>(string? strJson)
        {
            try
            {
                if (string.IsNullOrEmpty(strJson))
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(strJson);
            }
            catch
            {
                return default;
            }
        }
        #endregion

        #region Log
        public static void SaveLog(string message)
        {
            if(!string.IsNullOrEmpty(message))
            {
                MessageBus.Current.SendMessage(message, Global.LogTarget);
            }
        }
        #endregion
    }
}
