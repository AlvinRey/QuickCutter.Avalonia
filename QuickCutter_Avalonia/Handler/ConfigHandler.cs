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
            return 1;
        }

        #endregion

    }
}
