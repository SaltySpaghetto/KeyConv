using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;
using GCNLibrary.LM;

namespace GCNLibrary.LM2
{
    // Token: 0x02000042 RID: 66
    public class KEY_Parser
    {
        // Token: 0x170000CD RID: 205
        // (get) Token: 0x06000276 RID: 630 RVA: 0x0000B4DC File Offset: 0x000096DC
        // (set) Token: 0x06000277 RID: 631 RVA: 0x0000B4E4 File Offset: 0x000096E4
        public KEY_Parser.AnimJoint[] AnimJoints { get; set; }

        // Token: 0x170000CE RID: 206
        // (get) Token: 0x06000278 RID: 632 RVA: 0x0000B4ED File Offset: 0x000096ED
        // (set) Token: 0x06000279 RID: 633 RVA: 0x0000B4F5 File Offset: 0x000096F5
        public ushort FrameCount { get; set; }

        // Token: 0x170000CF RID: 207
        // (get) Token: 0x0600027A RID: 634 RVA: 0x0000B4FE File Offset: 0x000096FE
        // (set) Token: 0x0600027B RID: 635 RVA: 0x0000B506 File Offset: 0x00009706
        public ushort AnimationDelay { get; set; }

        // Token: 0x170000D0 RID: 208
        // (get) Token: 0x0600027C RID: 636 RVA: 0x0000B50F File Offset: 0x0000970F
        // (set) Token: 0x0600027D RID: 637 RVA: 0x0000B517 File Offset: 0x00009717
        public uint Flags { get; set; }

        // Token: 0x0600027E RID: 638 RVA: 0x0000B520 File Offset: 0x00009720
        public KEY_Parser()
        {
        }

        // Token: 0x0600027F RID: 639 RVA: 0x0000B52A File Offset: 0x0000972A
        public KEY_Parser(Stream stream)
        {
            this.Read(new FileReader(stream, false));
        }

        // Token: 0x06000280 RID: 640 RVA: 0x0000B542 File Offset: 0x00009742
        public void Save(Stream stream)
        {
            this.Write(new FileWriter(stream, false));
        }

        // Token: 0x06000281 RID: 641 RVA: 0x0000B554 File Offset: 0x00009754
        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            uint jointCount = reader.ReadUInt32();
            this.FrameCount = reader.ReadUInt16();
            this.AnimationDelay = reader.ReadUInt16();
            this.Flags = reader.ReadUInt32();
            this.ScaleKeyDataOffset = reader.ReadUInt32();
            this.RotationKeyDataOffset = reader.ReadUInt32();
            this.TranslationKeyDataOffset = reader.ReadUInt32();
            uint keyBeginIndicesOffset = reader.ReadUInt32();
            uint keyCountsOffset = reader.ReadUInt32();
            this.AnimJoints = new KEY_Parser.AnimJoint[jointCount];
            int i = 0;
            while ((long)i < (long)((ulong)jointCount))
            {
                this.AnimJoints[i] = new KEY_Parser.AnimJoint();
                i++;
            }
            reader.SeekBegin(keyBeginIndicesOffset);
            int j = 0;
            while ((long)j < (long)((ulong)jointCount))
            {
                this.AnimJoints[j].ScaleX.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].ScaleY.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].ScaleZ.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].RotateX.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].RotateY.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].RotateZ.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].PositionX.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].PositionY.BeginIndex = reader.ReadUInt32();
                this.AnimJoints[j].PositionZ.BeginIndex = reader.ReadUInt32();
                j++;
            }
            reader.SeekBegin(keyCountsOffset);
            int k = 0;
            while ((long)k < (long)((ulong)jointCount))
            {
                this.ReadGroupCount(this.AnimJoints[k].ScaleX, reader);
                this.ReadGroupCount(this.AnimJoints[k].ScaleY, reader);
                this.ReadGroupCount(this.AnimJoints[k].ScaleZ, reader);
                this.ReadGroupCount(this.AnimJoints[k].RotateX, reader);
                this.ReadGroupCount(this.AnimJoints[k].RotateY, reader);
                this.ReadGroupCount(this.AnimJoints[k].RotateZ, reader);
                this.ReadGroupCount(this.AnimJoints[k].PositionX, reader);
                this.ReadGroupCount(this.AnimJoints[k].PositionY, reader);
                this.ReadGroupCount(this.AnimJoints[k].PositionZ, reader);
                k++;
            }
            int l = 0;
            while ((long)l < (long)((ulong)jointCount))
            {
                this.ReadKeyframe(reader, this.AnimJoints[l].ScaleX, 0);
                this.ReadKeyframe(reader, this.AnimJoints[l].ScaleY, 0);
                this.ReadKeyframe(reader, this.AnimJoints[l].ScaleZ, 0);
                this.ReadKeyframe(reader, this.AnimJoints[l].RotateX, 1);
                this.ReadKeyframe(reader, this.AnimJoints[l].RotateY, 1);
                this.ReadKeyframe(reader, this.AnimJoints[l].RotateZ, 1);
                this.ReadKeyframe(reader, this.AnimJoints[l].PositionX, 2);
                this.ReadKeyframe(reader, this.AnimJoints[l].PositionY, 2);
                this.ReadKeyframe(reader, this.AnimJoints[l].PositionZ, 2);
                l++;
            }
        }

        // Token: 0x06000282 RID: 642 RVA: 0x0000B8E0 File Offset: 0x00009AE0
        private void Write(FileWriter writer)
        {
            /*
            writer.SetByteOrder(true);
            writer.Write(this.AnimJoints.Length);
            writer.Write(this.FrameCount);
            writer.Write(this.AnimationDelay);
            writer.Write(this.Flags);
            writer.Write(uint.MaxValue);
            writer.Write(uint.MaxValue);
            writer.Write(uint.MaxValue);
            writer.Write(uint.MaxValue);
            writer.Write(uint.MaxValue);
            writer.WriteUint32Offset(12L, 0L);
            this.WriteScaleGroup(writer, this.AnimJoints);
            writer.WriteUint32Offset(16L, 0L);
            this.WriteRotationGroup(writer, this.AnimJoints);
            writer.WriteUint32Offset(20L, 0L);
            this.WriteTranslationGroup(writer, this.AnimJoints);
            writer.WriteUint32Offset(24L, 0L);
            foreach (KEY_Parser.AnimJoint joint in this.AnimJoints)
            {
                writer.Write(joint.ScaleX.BeginIndex);
                writer.Write(joint.ScaleY.BeginIndex);
                writer.Write(joint.ScaleZ.BeginIndex);
                writer.Write(joint.RotateX.BeginIndex);
                writer.Write(joint.RotateY.BeginIndex);
                writer.Write(joint.RotateZ.BeginIndex);
                writer.Write(joint.PositionX.BeginIndex);
                writer.Write(joint.PositionY.BeginIndex);
                writer.Write(joint.PositionZ.BeginIndex);
            }
            writer.WriteUint32Offset(28L, 0L);
            foreach (KEY_Parser.AnimJoint joint2 in this.AnimJoints)
            {
                this.WriteKeyGroup(writer, joint2.ScaleX);
                this.WriteKeyGroup(writer, joint2.ScaleY);
                this.WriteKeyGroup(writer, joint2.ScaleZ);
                this.WriteKeyGroup(writer, joint2.RotateX);
                this.WriteKeyGroup(writer, joint2.RotateY);
                this.WriteKeyGroup(writer, joint2.RotateZ);
                this.WriteKeyGroup(writer, joint2.PositionX);
                this.WriteKeyGroup(writer, joint2.PositionY);
                this.WriteKeyGroup(writer, joint2.PositionZ);
            }
            */
        }

        // Token: 0x06000283 RID: 643 RVA: 0x0000BB2D File Offset: 0x00009D2D
        /*
        private void WriteKeyGroup(FileWriter writer, KEY_Parser.Group group)
        {
            //writer.Write(group.SlopeFlag);
            //writer.Write((byte)group.FrameCount);
        }

        // Token: 0x06000284 RID: 644 RVA: 0x0000BB4C File Offset: 0x00009D4C
        private void WriteTranslationGroup(FileWriter writer, KEY_Parser.AnimJoint[] joints)
        {
            List<float> buffer = new List<float>();
            for (int i = 0; i < joints.Length; i++)
            {
                this.SetKeyGroupDataF32(buffer, joints[i].PositionX);
                this.SetKeyGroupDataF32(buffer, joints[i].PositionY);
                this.SetKeyGroupDataF32(buffer, joints[i].PositionZ);
            }
            writer.Write(buffer.ToArray());
            buffer.Clear();
        }

        // Token: 0x06000285 RID: 645 RVA: 0x0000BBB8 File Offset: 0x00009DB8
        private void WriteRotationGroup(FileWriter writer, KEY_Parser.AnimJoint[] joints)
        {
            List<short> buffer = new List<short>();
            for (int i = 0; i < joints.Length; i++)
            {
                this.SetKeyGroupDataU16(buffer, joints[i].RotateX);
                this.SetKeyGroupDataU16(buffer, joints[i].RotateY);
                this.SetKeyGroupDataU16(buffer, joints[i].RotateZ);
            }
            writer.Write(buffer.ToArray());
            buffer.Clear();
        }

        // Token: 0x06000286 RID: 646 RVA: 0x0000BC24 File Offset: 0x00009E24
        private void WriteScaleGroup(FileWriter writer, KEY_Parser.AnimJoint[] joints)
        {
            List<float> buffer = new List<float>();
            for (int i = 0; i < joints.Length; i++)
            {
                this.SetKeyGroupDataF32(buffer, joints[i].ScaleX);
                this.SetKeyGroupDataF32(buffer, joints[i].ScaleY);
                this.SetKeyGroupDataF32(buffer, joints[i].ScaleZ);
            }
            writer.Write(buffer.ToArray());
            buffer.Clear();
        }
        */

        // Token: 0x06000287 RID: 647 RVA: 0x0000BC90 File Offset: 0x00009E90
        private void SetKeyGroupDataF32(List<float> buffer, KEY_Parser.Group group)
        {
            float[] values = this.GetKeyGroupDataF32(group);
            bool flag = values.Length == 1 && buffer.Contains(values[0]);
            if (flag)
            {
                group.BeginIndex = (uint)buffer.IndexOf(values[0]);
            }
            else
            {
                group.BeginIndex = (uint)buffer.Count;
                buffer.AddRange(values);
            }
        }

        // Token: 0x06000288 RID: 648 RVA: 0x0000BCE8 File Offset: 0x00009EE8
        private void SetKeyGroupDataU16(List<short> buffer, KEY_Parser.Group group)
        {
            short[] values = this.GetKeyGroupDataU16(group);
            bool flag = values.Length == 1 && buffer.Contains(values[0]);
            if (flag)
            {
                group.BeginIndex = (uint)buffer.IndexOf(values[0]);
            }
            else
            {
                int index = CompareUtility.SearchArray<short>(buffer.ToArray(), values);
                bool flag2 = index != -1;
                if (flag2)
                {
                    group.BeginIndex = (uint)index;
                }
                else
                {
                    group.BeginIndex = (uint)buffer.Count;
                    buffer.AddRange(values);
                }
            }
        }

        // Token: 0x06000289 RID: 649 RVA: 0x0000BD60 File Offset: 0x00009F60
        private short[] GetKeyGroupDataU16(KEY_Parser.Group group)
        {
            int numElements = this.GetElementCount(group);
            short[] values = new short[numElements];
            int index = 0;
            for (int i = 0; i < (int)group.FrameCount; i++)
            {
                bool flag = group.FrameCount == 1;
                if (flag)
                {
                    values[index++] = (short)(group.KeyFrames[0].Value / 0.001533981f);
                }
                else
                {
                    values[index++] = (short)group.KeyFrames[i].Frame;
                    values[index++] = (short)(group.KeyFrames[i].Value / 0.001533981f);
                    values[index++] = (short)(group.KeyFrames[i].InSlope / 0.001533981f);
                    bool flag2 = group.SlopeFlag > 0;
                    if (flag2)
                    {
                        values[index++] = (short)group.KeyFrames[i].OutSlope;
                    }
                }
            }
            return values;
        }

        // Token: 0x0600028A RID: 650 RVA: 0x0000BE5C File Offset: 0x0000A05C
        private float[] GetKeyGroupDataF32(KEY_Parser.Group group)
        {
            int numElements = this.GetElementCount(group);
            float[] values = new float[numElements];
            int index = 0;
            for (int i = 0; i < (int)group.FrameCount; i++)
            {
                bool flag = group.FrameCount == 1;
                if (flag)
                {
                    values[index++] = group.KeyFrames[0].Value;
                }
                else
                {
                    values[index++] = group.KeyFrames[i].Frame;
                    values[index++] = group.KeyFrames[i].Value;
                    values[index++] = group.KeyFrames[i].InSlope;
                    bool flag2 = group.SlopeFlag > 0;
                    if (flag2)
                    {
                        values[index++] = group.KeyFrames[i].OutSlope;
                    }
                }
            }
            return values;
        }

        // Token: 0x0600028B RID: 651 RVA: 0x0000BF3C File Offset: 0x0000A13C
        private int GetElementCount(KEY_Parser.Group group)
        {
            bool flag = group.FrameCount == 1;
            int num;
            if (flag)
            {
                num = 1;
            }
            else
            {
                num = (int)(group.FrameCount * ((group.SlopeFlag > 0) ? 4 : 3));
            }
            return num;
        }

        // Token: 0x0600028C RID: 652 RVA: 0x0000BF73 File Offset: 0x0000A173
        private void ReadGroupCount(KEY_Parser.Group group, FileReader reader)
        {
            group.SlopeFlag = reader.ReadByte();
            group.FrameCount = (ushort)reader.ReadByte();
        }

        // Token: 0x0600028D RID: 653 RVA: 0x0000BF90 File Offset: 0x0000A190
        private void ReadKeyframe(FileReader reader, KEY_Parser.Group group, int type)
        {
            uint offset = 0U;
            bool flag = type == 0;
            if (flag)
            {
                offset = this.ScaleKeyDataOffset;
            }
            bool flag2 = type == 1;
            if (flag2)
            {
                offset = this.RotationKeyDataOffset;
            }
            bool flag3 = type == 2;
            if (flag3)
            {
                offset = this.TranslationKeyDataOffset;
            }
            reader.SeekBegin((long)((ulong)offset + (ulong)group.BeginIndex * (ulong)((type == 1) ? 2L : 4L)));
            KEY_Parser.KeyFrame[] keyFrames = new KEY_Parser.KeyFrame[(int)group.FrameCount];
            bool flag4 = group.FrameCount == 1;
            if (flag4)
            {
                keyFrames[0] = new KEY_Parser.KeyFrame
                {
                    Value = this.ReadKeyData(reader, type),
                    Frame = 0f
                };
            }
            else
            {
                for (int i = 0; i < (int)group.FrameCount; i++)
                {
                    keyFrames[i] = new KEY_Parser.KeyFrame();
                    keyFrames[i].Frame = this.ReadKeyData(reader, type);
                    keyFrames[i].Value = this.ReadKeyData(reader, type);
                    keyFrames[i].InSlope = this.ReadKeyData(reader, type);
                    bool flag5 = group.SlopeFlag > 0;
                    if (flag5)
                    {
                        keyFrames[i].OutSlope = this.ReadKeyData(reader, type);
                    }
                }
            }
            bool flag6 = type == 1;
            if (flag6)
            {
                for (int j = 0; j < keyFrames.Length; j++)
                {
                    keyFrames[j].Value *= 0.001533981f;
                    keyFrames[j].InSlope *= 0.001533981f;
                    keyFrames[j].OutSlope *= 0.001533981f;
                }
            }
            group.KeyFrames = keyFrames.ToList<KEY_Parser.KeyFrame>();
        }

        // Token: 0x0600028E RID: 654 RVA: 0x0000C120 File Offset: 0x0000A320
        private float ReadKeyData(FileReader reader, int type)
        {
            bool flag = type == 1;
            float num;
            if (flag)
            {
                num = (float)reader.ReadInt16();
            }
            else
            {
                num = reader.ReadSingle();
            }
            return num;
        }

        // Token: 0x04000131 RID: 305
        private uint ScaleKeyDataOffset;

        // Token: 0x04000132 RID: 306
        private uint RotationKeyDataOffset;

        // Token: 0x04000133 RID: 307
        private uint TranslationKeyDataOffset;

        // Token: 0x020000A4 RID: 164
        public class AnimJoint
        {
            // Token: 0x040002D4 RID: 724
            public KEY_Parser.Group ScaleX = new KEY_Parser.Group();

            // Token: 0x040002D5 RID: 725
            public KEY_Parser.Group ScaleY = new KEY_Parser.Group();

            // Token: 0x040002D6 RID: 726
            public KEY_Parser.Group ScaleZ = new KEY_Parser.Group();

            // Token: 0x040002D7 RID: 727
            public KEY_Parser.Group RotateX = new KEY_Parser.Group();

            // Token: 0x040002D8 RID: 728
            public KEY_Parser.Group RotateY = new KEY_Parser.Group();

            // Token: 0x040002D9 RID: 729
            public KEY_Parser.Group RotateZ = new KEY_Parser.Group();

            // Token: 0x040002DA RID: 730
            public KEY_Parser.Group PositionX = new KEY_Parser.Group();

            // Token: 0x040002DB RID: 731
            public KEY_Parser.Group PositionY = new KEY_Parser.Group();

            // Token: 0x040002DC RID: 732
            public KEY_Parser.Group PositionZ = new KEY_Parser.Group();
        }

        // Token: 0x020000A5 RID: 165
        public class Group
        {
            // Token: 0x040002DD RID: 733
            public uint BeginIndex;

            // Token: 0x040002DE RID: 734
            public ushort FrameCount;

            // Token: 0x040002DF RID: 735
            public byte SlopeFlag;

            // Token: 0x040002E0 RID: 736
            public List<KEY_Parser.KeyFrame> KeyFrames = new List<KEY_Parser.KeyFrame>();
        }

        // Token: 0x020000A6 RID: 166
        public class KeyFrame
        {
            // Token: 0x1700019F RID: 415
            // (get) Token: 0x06000501 RID: 1281 RVA: 0x000151DB File Offset: 0x000133DB
            // (set) Token: 0x06000502 RID: 1282 RVA: 0x000151E3 File Offset: 0x000133E3
            public float Frame { get; set; }

            // Token: 0x170001A0 RID: 416
            // (get) Token: 0x06000503 RID: 1283 RVA: 0x000151EC File Offset: 0x000133EC
            // (set) Token: 0x06000504 RID: 1284 RVA: 0x000151F4 File Offset: 0x000133F4
            public float Value { get; set; }

            // Token: 0x170001A1 RID: 417
            // (get) Token: 0x06000505 RID: 1285 RVA: 0x000151FD File Offset: 0x000133FD
            // (set) Token: 0x06000506 RID: 1286 RVA: 0x00015205 File Offset: 0x00013405
            public float InSlope { get; set; }

            // Token: 0x170001A2 RID: 418
            // (get) Token: 0x06000507 RID: 1287 RVA: 0x0001520E File Offset: 0x0001340E
            // (set) Token: 0x06000508 RID: 1288 RVA: 0x00015216 File Offset: 0x00013416
            public float OutSlope { get; set; }
        }
    }
}
