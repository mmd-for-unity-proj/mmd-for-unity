using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD
{
    public class ToByteUtil
    {
        public static byte[] ListToBytes<M>(Dictionary<string, List<M>> list, uint list_count, int size)
            where M : IBinary
        {
            byte[] count = BitConverter.GetBytes(list_count);
            byte[] retarr = new byte[list_count * size + 4];
            SafeCopy(count, retarr, 0, 4);

            int cnt = 0;
            foreach (var mlist in list)
            {
                foreach (var m in mlist.Value)
                {
                    byte[] bin = m.ToBytes();
                    SafeCopy(bin, retarr, cnt * size + 4, size);
                    cnt++;
                }
            }
            return retarr;
        }

        public static byte[] ArrayToBytes<M>(M[] array, uint array_count, int size)
            where M : IBinary
        {
            byte[] count = BitConverter.GetBytes(array_count);
            byte[] retarr = new byte[array_count * size + 4];
            SafeCopy(count, retarr, 0, 4);

            int cnt = 0;
            foreach (var a in array)
            {
                byte[] bin = a.ToBytes();
                SafeCopy(bin, retarr, cnt * size + 4, size);
                cnt++;
            }
            return retarr;
        }

        public static byte[] Vector3ToBytes(ref Vector3 v)
        {
            byte[] x = BitConverter.GetBytes(v.x);
            byte[] y = BitConverter.GetBytes(v.y);
            byte[] z = BitConverter.GetBytes(v.z);
            List<byte> ret = new List<byte>();
            ret.AddRange(x);
            ret.AddRange(y);
            ret.AddRange(z);
            return ret.ToArray();
        }

        public static byte[] FloatRGBToBytes(ref Color c)
        {
            byte[] r = BitConverter.GetBytes(c.r);
            byte[] g = BitConverter.GetBytes(c.g);
            byte[] b = BitConverter.GetBytes(c.b);
            List<byte> ret = new List<byte>();
            ret.AddRange(r);
            ret.AddRange(g);
            ret.AddRange(b);
            return ret.ToArray();
        }

        public static void SafeCopy(byte[] source, byte[] dest, int start, int length)
        {
            Array.Resize(ref source, length);
            Array.Copy(source, 0, dest, start, length);
        }

        public static byte[] EncodeUTFToSJIS(string str)
        {
            try
            {
                return USEncoder.ToEncoding.ToSJIS(str);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return new byte[15];
            }
        }
    }
}