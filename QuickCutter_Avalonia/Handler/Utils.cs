using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using QuickCutter_Avalonia.Mode;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace QuickCutter_Avalonia.Handler
{
    internal class Utils
    {
        static public Dictionary<string, string> ISO639_2_Converter { get; }
            = new Dictionary<string, string>()
            {
                {"jpn", "Japanese"},
                {"eng", "English"},
                {"fre", "French"},
                {"fra", "French"},
                {"ger", "German"},
                {"deu", "German"},
                {"spa", "Spanish"},
                {"dut", "Dutch"},
                {"nld", "Dutch"},
                {"chi", "Chinese"},
                {"kor", "Korean"},
                {"por", "Portuguese"},
                {"ara", "Arabic"},
                {"hrv", "Croatian"},
                {"cze", "Czech"},
                {"ces", "Czech"},
                {"dan", "Danish"},
                {"fin", "Finnish"},
                {"gre", "Greek"},
                {"ell", "Greek"},
                {"heb", "Hebrew"},
                {"hun", "Hungarian"},
                {"nor", "Norwegian"},
                {"pol", "Polish"},
                {"rum", "Romanian"},
                {"ron", "Romanian"},
                {"rus", "Russian"},
                {"swe", "Swedish"},
                {"ita", "Italian"}
            };

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

        public static string ApplicationDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        public static string GetLogPath(string filename = "")
        {
            string _tempPath = Path.Combine(ApplicationDataPath(), Global.LogFolder);
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
            string _tempPath = Path.Combine(ApplicationDataPath(), Global.ConfigFolder);
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

        public static string GetFFmpegPath(string filename = "")
        {
            string _tempPath = Path.Combine(StartupPath(), @"bin\FFmepg");
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

        public static string GetFFmpegTempPath(string filename = "")
        {
            string _tempPath = Path.Combine(ApplicationDataPath(), Global.FFmpegTempFolder);
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


        /// <summary>
        /// 保存成json文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int ToJsonFile(object? obj, string filePath)
        {
            int result;
            try
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(obj, options);
                File.WriteAllText(filePath, jsonString);
                result = 0;
            }
            catch (Exception ex)
            {
                SaveLog(ex.Message);
                result = -1;
            }
            return result;
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

        #region Config
        static private Config? _config = null;

        static public void SetConfig(Config config)
        {
            _config = config;
        }
        static public Config GetConfig()
        {
            Console.WriteLine("GetConfig");
            //if (_config is null) throw new InvalidOperationException("Config is Invalid.");
            if (_config is null)
            {
                if(ConfigHandler.LoadConfig(ref _config) !=0)
                {
                    ShowNativeMessageBox("加载GUI配置文件异常,请重启应用");
                    Environment.Exit(0);
                }
            }

            return _config;
        }
        #endregion

        #region Native MessageBox
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public static void ShowNativeMessageBox(string message)
        {
            MessageBox(IntPtr.Zero, message, "Error", 0);
        }
        #endregion
    }
}
