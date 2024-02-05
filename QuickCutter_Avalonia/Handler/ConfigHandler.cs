using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCutter_Avalonia.Mode;
using System.IO;

namespace QuickCutter_Avalonia.Handler
{
    /// <summary>
    /// 本软件配置文件处理类
    /// </summary>
    internal class ConfigHandler
    {
        private static string configRes = Global.ConfigFileName;
        private static readonly object objLock = new();

        #region ConfigHandler 
        ///<summary>
        /// Load Config
        /// </summary>
        ///<param name="config"></param>
        /// <returns></returns>
        public static int LoadConfig(ref Config config)
        {
            string? result = Utils.LoadResource(Utils.GetConfigPath(configRes));
            if(string.IsNullOrEmpty(result))
            {
                //转成Json
                config = Utils.FromJson<Config>(result);
            }
            else
            {
                if (File.Exists(Utils.GetConfigPath(configRes)))
                {
                    Utils.SaveLog("LoadConfig Exception");
                    return -1;
                }
            }

            if (config == null)
            {
                config = new Config
                {
                };
            }

            config.windowStartUpStyles = WindowStartUpStyles.AUTOADJUST;

            return 1;
        }

        /// <summary>
        /// 保参数
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static int SaveConfig(ref Config config, bool reload = true)
        {
            ToJsonFile(config);

            return 0;
        }

        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="config"></param>
        private static void ToJsonFile(Config config)
        {
            lock (objLock)
            {
                try
                {
                    //save temp file
                    var resPath = Utils.GetConfigPath(configRes);
                    var tempPath = $"{resPath}_temp";
                    if (Utils.ToJsonFile(config, tempPath) != 0)
                    {
                        return;
                    }

                    if (File.Exists(resPath))
                    {
                        File.Delete(resPath);
                    }
                    //rename
                    File.Move(tempPath, resPath);
                }
                catch
                {
                    Utils.SaveLog("Expection occur when To Json File");
                }
            }
        }

        #endregion
    }
}
