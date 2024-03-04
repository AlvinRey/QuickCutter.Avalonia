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
        public static int LoadConfig(ref Config config)
        {
            string? result = Utils.LoadResource(Utils.GetConfigPath(configRes));
            if(!string.IsNullOrEmpty(result))
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
                config = Config.DefaultConfig;
            }

            VerifyConfig(config);
            Utils.SetConfig(config);
            return 0;
        }

        /// <summary>
        /// 保参数
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static int SaveConfig()
        {
            ToJsonFile(Utils.GetConfig());

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
       
        private static void VerifyConfig(Config config) 
        {
            if(config.moveStep < 1)
            {
                config.moveStep = 1;
            }
        }
        #endregion
    }
}
