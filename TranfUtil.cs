using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NDEFReadWriteTool
{
    static class TranfUtil
    {
        public static string HexToString(byte[] hexFrame, int start, uint len)
        {
            string str = "";
            int i = 0;
            for (i = 0; i < len; i++)
            {
                str += hexFrame[start + i].ToString("X").PadLeft(2, '0');
            }

            return str;
        }



        /// <summary>
        /// 16进制字符串转成byte[]
        /// </summary>
        /// <param name="hexString">0004000100020003</param>
        /// <returns></returns>
        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static bool checkHexString(string info)
        {
            if (info.Length % 2 != 0)
            {
                return false;
            }
            const string PATTERN = @"[A-Fa-f0-9]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(info, PATTERN);
        }

        public static string asciiChange(string codeStr)
        {
            string asciiStr = "";
            if (codeStr != null)
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(codeStr);
                for (int i = 0; i < data.Length; i++)
                {
                    int asciiCode = (int)data[i];
                    asciiStr += Convert.ToString(asciiCode, 16);
                }
            }
            return asciiStr;
        }

        public static string byteToAscii(byte[] code)
        {
            return Encoding.ASCII.GetString(code);
        }

        public static string byteToUtf8(byte[] code)
        {
            return Encoding.UTF8.GetString(code);
        }

        public static byte[] asciiToByte(string code)
        {
            return System.Text.Encoding.ASCII.GetBytes(code);
        }

        public static string getCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        public static bool IsMacAddress(string input)
        {
            // 正则表达式：匹配由冒号或短横分隔的 MAC 地址格式
            string pattern = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
            return Regex.IsMatch(input, pattern);
        }
    }
}
