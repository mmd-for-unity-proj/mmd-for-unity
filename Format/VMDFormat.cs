using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Text;

// Reference URL:
//	  http://blog.goo.ne.jp/torisu_tetosuki/e/209ad341d3ece2b1b4df24abf619d6e4
//	  http://mikudan.blog120.fc2.com/blog-entry-280.html

namespace MMD
{
    namespace VMD
    {
        // VMDのフォーマットクラス
        public class VMDFormat
        {
            public string name;
            public string path;
            public string folder;

            public Header header;
            public MotionList motion_list;
            public SkinList skin_list;
            public LightList light_list;
            public CameraList camera_list;
            public SelfShadowList self_shadow_list;

            public class Header : IBinary
            {
                public string vmd_header; // 30byte, "Vocaloid Motion Data 0002"
                public string vmd_model_name; // 20byte

                public Header() { }
                public Header(BinaryReader bin)
                {
	                vmd_header = ToFormatUtil.ConvertByteToString(bin.ReadBytes(30), "");
                    vmd_model_name = ToFormatUtil.ConvertByteToString(bin.ReadBytes(20), "");
                }

                public byte[] ToBytes()
                {
                    byte[] header = Encoding.ASCII.GetBytes(vmd_header);
                    byte[] model_name = ToByteUtil.EncodeUTFToSJIS(vmd_model_name);
                    byte[] retarr = new byte[50];
                    ToByteUtil.SafeCopy(header, retarr, 0, 30);
                    ToByteUtil.SafeCopy(model_name, retarr, 30, 20);
                    return retarr;
                }
            }

            public class MotionList : IBinary
            {
                public uint motion_count;
                public Dictionary<string, List<Motion>> motion = new Dictionary<string, List<Motion>>();

                public MotionList() { }
                public MotionList(BinaryReader bin)
                {
                    motion_count = bin.ReadUInt32();

                    // 一度バッファに貯めてソートする
                    VMDFormat.Motion[] buf = new VMDFormat.Motion[motion_count];
                    for (int i = 0; i < motion_count; i++)
                    {
                        buf[i] = new Motion(bin);
                    }
                    Array.Sort(buf, (x, y) => ((int)x.frame_no - (int)y.frame_no));

                    // モーションの数だけnewされないよね？
                    for (int i = 0; i < motion_count; i++)
                    {
                        try { motion.Add(buf[i].bone_name, new List<VMDFormat.Motion>()); }
                        catch { }
                    }

                    // dictionaryにどんどん登録
                    for (int i = 0; i < motion_count; i++)
                    {
                        motion[buf[i].bone_name].Add(buf[i]);
                    }
                }

                public byte[] ToBytes()
                {
                    return ToByteUtil.ListToBytes(motion, motion_count, 111);
                }

                public void Insert(Motion motion)
                {
                    this.motion[motion.bone_name].Add(motion);
                    motion_count++;
                }
            }

            public class Motion : IBinary
            {
                public string bone_name;	// 15byte
                public uint frame_no;
                public Vector3 location;
                public Quaternion rotation;
                public byte[] interpolation;	// [4][4][4], 64byte

                public Motion() { }
                public Motion(BinaryReader bin)
                {
                    bone_name = ToFormatUtil.ConvertByteToString(bin.ReadBytes(15), "");
                    frame_no = bin.ReadUInt32();
                    location = ToFormatUtil.ReadSinglesToVector3(bin);
                    rotation = ToFormatUtil.ReadSinglesToQuaternion(bin);
                    interpolation = bin.ReadBytes(64);
                }

                // なんか不便になりそうな気がして
                public byte GetInterpolation(int i, int j, int k)
                {
                    return this.interpolation[i * 16 + j * 4 + k];
                }

                public void SetInterpolation(byte val, int i, int j, int k)
                {
                    this.interpolation[i * 16 + j * 4 + k] = val;
                }

                public byte[] ToBytes()
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// 表情リスト
            /// </summary>
            public class SkinList : IBinary
            {
                public uint skin_count;
                public Dictionary<string, List<SkinData>> skin = new Dictionary<string,List<SkinData>>();

                public SkinList() { }
                public SkinList(BinaryReader bin)
                {
                    skin_count = bin.ReadUInt32();

                    // 一度バッファに貯めてソートする
                    VMDFormat.SkinData[] buf = new VMDFormat.SkinData[skin_count];
                    for (int i = 0; i < skin_count; i++)
                    {
                        buf[i] = new SkinData(bin);
                    }
                    Array.Sort(buf, (x, y) => ((int)x.frame_no - (int)y.frame_no));

                    // 全てのモーションを探索し、利用されているボーンを特定する
                    for (int i = 0; i < skin_count; i++)
                    {
                        try { skin.Add(buf[i].skin_name, new List<VMDFormat.SkinData>()); }
                        catch
                        {
                            //重複している場合はこの処理に入る
                        }
                    }

                    // 辞書に登録する作業
                    for (int i = 0; i < skin_count; i++)
                    {
                        skin[buf[i].skin_name].Add(buf[i]);
                    }
                }

                public byte[] ToBytes()
                {
                    return ToByteUtil.ListToBytes(skin, skin_count, 23);
                }

                public void Insert(SkinData skin_obj)
                {
                    if (!skin.ContainsKey(skin_obj.skin_name))
                        skin[skin_obj.skin_name] = new List<SkinData>();

                    skin[skin_obj.skin_name].Add(skin_obj);
                    skin_count++;
                }
            }

            public class SkinData : IBinary
            {
                public string skin_name;	// 15byte
                public uint frame_no;
                public float weight;

                public SkinData() { }
                public SkinData(BinaryReader bin)
                {
                    skin_name = ToFormatUtil.ConvertByteToString(bin.ReadBytes(15), "");
                    frame_no = bin.ReadUInt32();
                    weight = bin.ReadSingle();
                }

                public byte[] ToBytes()
                {
                    byte[] skin_name = ToByteUtil.EncodeUTFToSJIS(this.skin_name);
                    byte[] frame = BitConverter.GetBytes(frame_no);
                    byte[] weight = BitConverter.GetBytes(this.weight);
                    byte[] retarr = new byte[23];

                    ToByteUtil.SafeCopy(skin_name, retarr, 0, 15);
                    ToByteUtil.SafeCopy(frame, retarr, 15, 4);
                    ToByteUtil.SafeCopy(weight, retarr, 19, 4);
                    return retarr;
                }
            }

            public class CameraList : IBinary
            {
                public uint camera_count;
                public CameraData[] camera;

                public CameraList() { }
                public CameraList(BinaryReader bin)
                {
                    camera_count = bin.ReadUInt32();
                    camera = new VMDFormat.CameraData[camera_count];
                    for (int i = 0; i < camera_count; i++)
                    {
                        camera[i] = new CameraData(bin);
                    }
                    Array.Sort(camera, (x, y) => ((int)x.frame_no - (int)y.frame_no));
                }

                public byte[] ToBytes()
                {
                    return ToByteUtil.ArrayToBytes(camera, camera_count, 61);
                }
            }

            public class CameraData : IBinary
            {
                public uint frame_no;
                public float length;
                public Vector3 location;
                public Vector3 rotation;	// オイラー角, X軸は符号が反転している
                public byte[] interpolation;	// [6][4], 24byte(未検証)
                public uint viewing_angle;
                public byte perspective;	// 0:on 1:off

                public CameraData() { }
                public CameraData(BinaryReader bin)
                {
                    frame_no = bin.ReadUInt32();
                    length = bin.ReadSingle();
                    location = ToFormatUtil.ReadSinglesToVector3(bin);
                    rotation = ToFormatUtil.ReadSinglesToVector3(bin);
                    interpolation = bin.ReadBytes(24);
                    viewing_angle = bin.ReadUInt32();
                    perspective = bin.ReadByte();
                }

                public byte GetInterpolation(int i, int j)
                {
                    return this.interpolation[i * 6 + j];
                }

                public void SetInterpolation(byte val, int i, int j)
                {
                    this.interpolation[i * 6 + j] = val;
                }

                public byte[] ToBytes()
                {
                    throw new NotImplementedException();
                }
            }

            public class LightList : IBinary
            {
                public uint light_count;
                public LightData[] light;

                public LightList() { }
                public LightList(BinaryReader bin)
                {
                    light_count = bin.ReadUInt32();
                    light = new VMDFormat.LightData[light_count];
                    for (int i = 0; i < light_count; i++)
                    {
                        light[i] = new LightData(bin);
                    }

                    Array.Sort(light, (x, y) => ((int)x.frame_no - (int)y.frame_no));
                }

                public byte[] ToBytes()
                {
                    return ToByteUtil.ArrayToBytes(light, light_count, 28);
                }
            }

            public class LightData : IBinary
            {
                public uint frame_no;
                public Color rgb;	// αなし, 256
                public Vector3 location;

                public LightData() { }
                public LightData(BinaryReader binary_reader_)
                {
                    frame_no = binary_reader_.ReadUInt32();
                    rgb = ToFormatUtil.ReadSinglesToColor(binary_reader_, 1);
                    location = ToFormatUtil.ReadSinglesToVector3(binary_reader_);
                }

                public byte[] ToBytes()
                {
                    byte[] frame = BitConverter.GetBytes(frame_no);
                    byte[] rgb = ToByteUtil.FloatRGBToBytes(ref this.rgb);
                    byte[] location = ToByteUtil.Vector3ToBytes(ref this.location);
                    List<byte> retarr = new List<byte>();
                    retarr.AddRange(frame);
                    retarr.AddRange(rgb);
                    retarr.AddRange(location);
                    if (retarr.Count != 28) throw new IndexOutOfRangeException("照明データが28バイトちょうどじゃない");
                    return retarr.ToArray();
                }
            }

            public class SelfShadowList : IBinary
            {
                public uint self_shadow_count;
                public SelfShadowData[] self_shadow;

                public SelfShadowList() { }

                public byte[] ToBytes()
                {
                    return ToByteUtil.ArrayToBytes(self_shadow, self_shadow_count, 9);
                }
            }

            public class SelfShadowData : IBinary
            {
                public uint frame_no;
                public byte mode; //00-02
                public float distance;	// 0.1 - (dist * 0.00001)

                public SelfShadowData() { }

                public byte[] ToBytes()
                {
                    byte[] frame = BitConverter.GetBytes(frame_no);
                    byte[] mode = { this.mode };
                    byte[] distance = BitConverter.GetBytes(this.distance);
                    List<byte> retarr = new List<byte>();
                    retarr.AddRange(frame);
                    retarr.AddRange(mode);
                    retarr.AddRange(distance);
                    if (retarr.Count != 9) throw new IndexOutOfRangeException("セルフシャドウデータが9バイトちょうどじゃない");
                    return retarr.ToArray();
                }
            }
        }
    }
}