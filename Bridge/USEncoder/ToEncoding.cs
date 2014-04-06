using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USEncoder
{
    /// <summary>
    /// SJISとUTF-8の相互変換を行うクラス
    /// </summary>
    public class ToEncoding
    {
        public static byte[] ToSJIS(string unicode_str)
        {
            byte[] uni_bytes = Encoding.BigEndianUnicode.GetBytes(unicode_str); // 対応表がBigEndianだった
            List<byte> sjis_bytes = new List<byte>();

            for (int i = 0; i < uni_bytes.Length; i++)
            {
                // unicodeは英数字でも2バイト使う
                ushort uni_code = (ushort)(uni_bytes[i] << 8);
                uni_code += uni_bytes[++i];

                ushort sjis_code = USEncoder.ToSJIS.GetCode(uni_code);
                byte top = (byte)(sjis_code >> 8);
                byte under = (byte)(sjis_code & 0xFF);

                if ((top >= 0x81 && top <= 0x9F) || (top >= 0xE0 && top <= 0xEA))
                {
                    // 2バイト文字の範囲
                    sjis_bytes.Add(top);
                    sjis_bytes.Add(under);
                }
                else
                {
                    sjis_bytes.Add(under);
                }
            }

            return sjis_bytes.ToArray();
        }

        public static string ToUnicode(byte[] sjis_bytes)
        {
            List<byte> uni_bytes = new List<byte>();

            for (int i = 0; i < sjis_bytes.Length; i++)
            {
                ushort sjis_code;

                if ((sjis_bytes[i] >= 0x81 && sjis_bytes[i] <= 0x9F) || (sjis_bytes[i] >= 0xE0 && sjis_bytes[i] <= 0xEA))
                {
                    // 2バイト文字
                    sjis_code = (ushort)(sjis_bytes[i] << 8);
                    sjis_code += sjis_bytes[++i];
                }
                else
                {
                    // 1バイト文字
                    sjis_code = sjis_bytes[i];
                }

                ushort uni_code = USEncoder.ToUnicode.GetCode(sjis_code);
                byte top = (byte)(uni_code >> 8);
                byte under = (byte)(uni_code & 0xFF);
                uni_bytes.Add(under);
                uni_bytes.Add(top);     // テーブルがBigEndianだったので，underとtopを逆にしてLittleに戻す
            }

            return Encoding.Unicode.GetString(uni_bytes.ToArray());
        }
    }
}
