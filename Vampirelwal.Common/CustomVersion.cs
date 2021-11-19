#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：CustomVersion
// 创 建 者：杨程
// 创建时间：2021/11/12 13:06:33
// 文件版本：V1.0.0
// ===============================================================
// 功能描述：
//		
//
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampirelwal.Common
{
    /// <summary>
    /// 自定义版本号
    /// </summary>
    public class CustomVersion
    {
        /*
         * 默认为4位，0.0.0.0格式
         */

        /// <summary>
        /// 版本验证
        /// </summary>
        /// <param name="ServerVersion"></param>
        /// <param name="ClientVersion"></param>
        /// <returns></returns>
        public static bool CheckVersion(string ServerVersion, string ClientVersion)
        {
            //先处理服务器的版本号
            string[] ServerStrs = ServerVersion.Split('.');
            int Server1 = Convert.ToInt32(ServerStrs[0]);
            int Server2 = Convert.ToInt32(ServerStrs[1]);
            int Server3 = Convert.ToInt32(ServerStrs[2]);
            int Server4 = Convert.ToInt32(ServerStrs[3]);

            //处理客户端版本号
            string[] ClientStrs = ClientVersion.Split('.');
            int Client1 = Convert.ToInt32(ClientStrs[0]);
            int Client2 = Convert.ToInt32(ClientStrs[1]);
            int Client3 = Convert.ToInt32(ClientStrs[2]);
            int Client4 = Convert.ToInt32(ClientStrs[3]);

            //验证
            if (Server1>Client1)
            {
                return true;
            }
            else if (Server1 == Client1)
            {
                if (Server2>Client2)
                {
                    return true;
                }
                else if (Server2 == Client2)
                {
                    if (Server3>Client3)
                    {
                        return true;
                    }
                    else if (Server3 == Client3)
                    {
                        if (Server4>Client4)
                        {
                            return true;
                        }
                        else if(Server4 == Client4)
                        {
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServerVersion"></param>
        /// <returns></returns>
        public static string CreateNewVersion(string ServerVersion)
        {
            //先处理服务器的版本号
            string[] ServerStrs = ServerVersion.Split('.');
            int Server1 = Convert.ToInt32(ServerStrs[0]);
            int Server2 = Convert.ToInt32(ServerStrs[1]);
            int Server3 = Convert.ToInt32(ServerStrs[2]);
            int Server4 = Convert.ToInt32(ServerStrs[3]);


            if (Server4<100)
            {
                Server4++;
                if (Server4==100)
                {
                    Server4 = 1;
                    Server3++;

                    if (Server3==100)
                    {
                        Server3 = 1;
                        Server2++;

                        if (Server2==100)
                        {
                            Server2 = 1;
                            Server1++;
                        }
                    }
                }
                
            }

            return $"{Server1}.{Server2}.{Server3}.{Server4}";
        }
    }
}
