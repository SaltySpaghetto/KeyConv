using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;
using Toolbox.Core;
using Toolbox.Library.Animations;
using GCNLibrary.LM;

namespace GCNLibrary.LM2
{
    // Token: 0x02000041 RID: 65
    public class KEY : Animation, IFileFormat, IConvertableTextFormat
    {
        // Token: 0x170000C7 RID: 199
        // (get) Token: 0x06000262 RID: 610 RVA: 0x0000AA47 File Offset: 0x00008C47
        // (set) Token: 0x06000263 RID: 611 RVA: 0x0000AA4F File Offset: 0x00008C4F
        public bool CanSave { get; set; } = false;

        // Token: 0x170000C8 RID: 200
        // (get) Token: 0x06000264 RID: 612 RVA: 0x0000AA58 File Offset: 0x00008C58
        // (set) Token: 0x06000265 RID: 613 RVA: 0x0000AA60 File Offset: 0x00008C60
        public string[] Description { get; set; } = new string[] { "LM Skeletal Animation" };

        // Token: 0x170000C9 RID: 201
        // (get) Token: 0x06000266 RID: 614 RVA: 0x0000AA69 File Offset: 0x00008C69
        // (set) Token: 0x06000267 RID: 615 RVA: 0x0000AA71 File Offset: 0x00008C71
        public string[] Extension { get; set; } = new string[] { "*.key" };

        // Token: 0x170000CA RID: 202
        // (get) Token: 0x06000268 RID: 616 RVA: 0x0000AA7A File Offset: 0x00008C7A
        // (set) Token: 0x06000269 RID: 617 RVA: 0x0000AA82 File Offset: 0x00008C82
        public File_Info FileInfo { get; set; }

        // Token: 0x0600026A RID: 618 RVA: 0x0000AA8C File Offset: 0x00008C8C
        public bool Identify(File_Info fileInfo, Stream stream)
        {
            return fileInfo.Extension == ".key";
        }

        // Token: 0x170000CB RID: 203
        // (get) Token: 0x0600026B RID: 619 RVA: 0x0000AAAE File Offset: 0x00008CAE
        public TextFileType TextFileType
        {
            get
            {
                return TextFileType.Yaml;
            }
        }

        // Token: 0x170000CC RID: 204
        // (get) Token: 0x0600026C RID: 620 RVA: 0x0000AAB1 File Offset: 0x00008CB1
        public bool CanConvertBack
        {
            get
            {
                return true;
            }
        }

        // Token: 0x0600026D RID: 621 RVA: 0x0000AAB4 File Offset: 0x00008CB4
        public string ConvertToString()
        {
            return this.ToText(this.Header);
        }

        // Token: 0x0600026E RID: 622 RVA: 0x0000AAD2 File Offset: 0x00008CD2
        public void ConvertFromString(string text)
        {
            this.Header = this.FromText(text);
        }

        public string fileName = "";

        // Token: 0x0600026F RID: 623 RVA: 0x0000AAE4 File Offset: 0x00008CE4
        public void Load(Stream stream)
        {
            this.Header = new KEY_Parser(stream);
            base.Name = fileName;
            base.CanLoop = this.Header.Flags == 2U;
            base.FrameCount = this.Header.FrameCount;
            foreach (KEY_Parser.AnimJoint joinAnim in this.Header.AnimJoints)
            {
                AnimGroup group = new KEY.AnimGroup(joinAnim);
                this.AnimGroups.Add(group);
            }
        }

        // Token: 0x06000270 RID: 624 RVA: 0x0000AB7F File Offset: 0x00008D7F
        public void Save(Stream stream)
        {
            this.Header.Save(stream);
        }

        // Token: 0x06000271 RID: 625 RVA: 0x0000AB90 File Offset: 0x00008D90
        public string ToText(KEY_Parser header)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                writer.WriteLine(string.Format("FrameCount: {0}", header.FrameCount));
                writer.WriteLine(string.Format("FrameDelay: {0}", header.AnimationDelay));
                writer.WriteLine(string.Format("Flags: {0}", header.Flags));
                for (int i = 0; i < this.Header.AnimJoints.Length; i++)
                {
                    KEY_Parser.AnimJoint joint = this.Header.AnimJoints[i];
                    writer.WriteLine(string.Format("- Joint: {0}", i));
                    this.WriteGroupText(writer, joint.ScaleX, "Scale X");
                    this.WriteGroupText(writer, joint.ScaleY, "Scale Y");
                    this.WriteGroupText(writer, joint.ScaleZ, "Scale Z");
                    this.WriteGroupText(writer, joint.RotateX, "Rotate X");
                    this.WriteGroupText(writer, joint.RotateY, "Rotate Y");
                    this.WriteGroupText(writer, joint.RotateZ, "Rotate Z");
                    this.WriteGroupText(writer, joint.PositionX, "Position X");
                    this.WriteGroupText(writer, joint.PositionY, "Position Y");
                    this.WriteGroupText(writer, joint.PositionZ, "Position Z");
                }
            }
            return sb.ToString();
        }

        // Token: 0x06000272 RID: 626 RVA: 0x0000AD2C File Offset: 0x00008F2C
        private void WriteGroupText(StringWriter writer, KEY_Parser.Group group, string Name)
        {
            writer.WriteLine("    " + Name + ":");
            for (int i = 0; i < group.KeyFrames.Count; i++)
            {
                string slopeInfo = "";
                bool flag = group.KeyFrames.Count > 1;
                if (flag)
                {
                    slopeInfo = string.Format(", InSlope: {0}", group.KeyFrames[i].InSlope);
                }
                bool flag2 = group.SlopeFlag > 0;
                if (flag2)
                {
                    slopeInfo = string.Format(", {0}, OutSlope: {1}", slopeInfo, group.KeyFrames[i].OutSlope);
                }
                writer.WriteLine(string.Format("      - Frame: [{0}, Value : {1}", group.KeyFrames[i].Frame, group.KeyFrames[i].Value) + slopeInfo + "]");
            }
        }

        // Token: 0x06000273 RID: 627 RVA: 0x0000AE28 File Offset: 0x00009028
        public KEY_Parser FromText(string text)
        {
            KEY_Parser parser = new KEY_Parser();
            List<KEY_Parser.AnimJoint> joints = new List<KEY_Parser.AnimJoint>();
            KEY_Parser.AnimJoint activeJoint = null;
            KEY_Parser.Group activeGroup = null;
            foreach (string line in text.Split(new char[] { '\n' }))
            {
                bool flag = line.Contains("FrameCount:");
                if (flag)
                {
                    parser.FrameCount = ushort.Parse(line.Split(new char[] { ':' })[1]);
                }
                bool flag2 = line.Contains("FrameDelay:");
                if (flag2)
                {
                    parser.AnimationDelay = ushort.Parse(line.Split(new char[] { ':' })[1]);
                }
                bool flag3 = line.Contains("Flags:");
                if (flag3)
                {
                    parser.Flags = uint.Parse(line.Split(new char[] { ':' })[1]);
                }
                bool flag4 = line.Contains("Joint:");
                if (flag4)
                {
                    activeJoint = new KEY_Parser.AnimJoint();
                    joints.Add(activeJoint);
                }
                bool flag5 = line.Contains("Scale X:");
                if (flag5)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.ScaleX = activeGroup;
                }
                bool flag6 = line.Contains("Scale Y:");
                if (flag6)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.ScaleY = activeGroup;
                }
                bool flag7 = line.Contains("Scale Z:");
                if (flag7)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.ScaleZ = activeGroup;
                }
                bool flag8 = line.Contains("Position X:");
                if (flag8)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.PositionX = activeGroup;
                }
                bool flag9 = line.Contains("Position Y:");
                if (flag9)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.PositionY = activeGroup;
                }
                bool flag10 = line.Contains("Position Z:");
                if (flag10)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.PositionZ = activeGroup;
                }
                bool flag11 = line.Contains("Rotate X:");
                if (flag11)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.RotateX = activeGroup;
                }
                bool flag12 = line.Contains("Rotate Y:");
                if (flag12)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.RotateY = activeGroup;
                }
                bool flag13 = line.Contains("Rotate Z:");
                if (flag13)
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.RotateZ = activeGroup;
                }
                bool flag14 = line.Contains("Frame:");
                if (flag14)
                {
                    string value = line.Split(new char[] { '[' })[1];
                    value = value.Replace("[", string.Empty);
                    value = value.Replace("]", string.Empty);
                    string[] values = value.Split(new char[] { ',' });
                    KEY_Parser.KeyFrame keyFrame = new KEY_Parser.KeyFrame();
                    for (int i = 0; i < values.Length; i++)
                    {
                        bool flag15 = i == 0;
                        if (flag15)
                        {
                            keyFrame.Frame = float.Parse(values[i]);
                        }
                        else
                        {
                            string val = values[i].Split(new char[] { ':' })[1].Replace(" ", string.Empty);
                            bool flag16 = i == 1;
                            if (flag16)
                            {
                                keyFrame.Value = float.Parse(val);
                            }
                            bool flag17 = i == 2;
                            if (flag17)
                            {
                                keyFrame.InSlope = float.Parse(val);
                            }
                            bool flag18 = i == 3;
                            if (flag18)
                            {
                                keyFrame.OutSlope = float.Parse(val);
                            }
                        }
                    }
                    activeGroup.KeyFrames.Add(keyFrame);
                    activeGroup.FrameCount = (ushort)activeGroup.KeyFrames.Count;
                }
            }
            parser.AnimJoints = joints.ToArray();
            return parser;
        }

        public List<AnimGroup> AnimGroups = new List<AnimGroup>();

        // Token: 0x06000274 RID: 628 RVA: 0x0000B1B8 File Offset: 0x000093B8
        public void NextFrame(Toolbox.Core.STSkeleton skeleton)
        {
            bool flag2 = skeleton == null;
            if (!flag2)
            {
                bool Updated = false;
                for (int i = 0; i < this.AnimGroups.Count; i++)
                {
                    bool flag3 = i >= skeleton.Bones.Count;
                    if (flag3)
                    {
                        break;
                    }
                    STBone joint = skeleton.Bones[i];
                    KEY.AnimGroup group = (KEY.AnimGroup)this.AnimGroups[i];
                    Updated = true;
                    Vector3 position = joint.Position;
                    Vector3 scale = joint.Scale;
                    Vector3 rotate = joint.EulerRotation;
                    bool hasKeys = group.PositionX.HasKeys;
                    if (hasKeys)
                    {
                        position.X = group.PositionX.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys2 = group.PositionY.HasKeys;
                    if (hasKeys2)
                    {
                        position.Y = group.PositionY.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys3 = group.PositionZ.HasKeys;
                    if (hasKeys3)
                    {
                        position.Z = group.PositionZ.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys4 = group.RotateX.HasKeys;
                    if (hasKeys4)
                    {
                        rotate.X = group.RotateX.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys5 = group.RotateY.HasKeys;
                    if (hasKeys5)
                    {
                        rotate.Y = group.RotateY.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys6 = group.RotateZ.HasKeys;
                    if (hasKeys6)
                    {
                        rotate.Z = group.RotateZ.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys7 = group.ScaleX.HasKeys;
                    if (hasKeys7)
                    {
                        scale.X = group.ScaleX.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys8 = group.ScaleY.HasKeys;
                    if (hasKeys8)
                    {
                        scale.Y = group.ScaleY.GetFrameValue(base.Frame, 0f);
                    }
                    bool hasKeys9 = group.ScaleZ.HasKeys;
                    if (hasKeys9)
                    {
                        scale.Z = group.ScaleZ.GetFrameValue(base.Frame, 0f);
                    }
                    joint.AnimationController.Position = position;
                    joint.AnimationController.Scale = scale;
                    joint.AnimationController.EulerRotation = rotate;
                }
                bool flag4 = Updated;
                if (flag4)
                {
                    skeleton.Update();
                }
            }
        }

        // Token: 0x0400012C RID: 300
        public KEY_Parser Header;

        // Token: 0x020000A2 RID: 162
        public class AnimGroup : STAnimGroup
        {
            // Token: 0x060004FD RID: 1277 RVA: 0x00014F94 File Offset: 0x00013194
            public AnimGroup(KEY_Parser.AnimJoint animJoint)
            {
                this.PositionX = new KEY.AnimationTrack(animJoint.PositionX);
                this.PositionY = new KEY.AnimationTrack(animJoint.PositionY);
                this.PositionZ = new KEY.AnimationTrack(animJoint.PositionZ);
                this.RotateX = new KEY.AnimationTrack(animJoint.RotateX);
                this.RotateY = new KEY.AnimationTrack(animJoint.RotateY);
                this.RotateZ = new KEY.AnimationTrack(animJoint.RotateZ);
                this.ScaleX = new KEY.AnimationTrack(animJoint.ScaleX);
                this.ScaleY = new KEY.AnimationTrack(animJoint.ScaleY);
                this.ScaleZ = new KEY.AnimationTrack(animJoint.ScaleZ);
            }

            // Token: 0x040002CB RID: 715
            public STAnimationTrack PositionX = new STAnimationTrack();

            // Token: 0x040002CC RID: 716
            public STAnimationTrack PositionY = new STAnimationTrack();

            // Token: 0x040002CD RID: 717
            public STAnimationTrack PositionZ = new STAnimationTrack();

            // Token: 0x040002CE RID: 718
            public STAnimationTrack RotateX = new STAnimationTrack();

            // Token: 0x040002CF RID: 719
            public STAnimationTrack RotateY = new STAnimationTrack();

            // Token: 0x040002D0 RID: 720
            public STAnimationTrack RotateZ = new STAnimationTrack();

            // Token: 0x040002D1 RID: 721
            public STAnimationTrack ScaleX = new STAnimationTrack();

            // Token: 0x040002D2 RID: 722
            public STAnimationTrack ScaleY = new STAnimationTrack();

            // Token: 0x040002D3 RID: 723
            public STAnimationTrack ScaleZ = new STAnimationTrack();
        }

        // Token: 0x020000A3 RID: 163
        public class AnimationTrack : STAnimationTrack
        {
            // Token: 0x060004FE RID: 1278 RVA: 0x000150A8 File Offset: 0x000132A8
            public AnimationTrack(KEY_Parser.Group track)
            {
                base.InterpolationType = STInterpoaltionType.Hermite;
                foreach (KEY_Parser.KeyFrame keyFrame in track.KeyFrames)
                {
                    this.KeyFrames.Add(new STHermiteKeyFrame
                    {
                        Frame = keyFrame.Frame,
                        Value = keyFrame.Value,
                        TangentIn = keyFrame.InSlope,
                        TangentOut = keyFrame.OutSlope
                    });
                }
            }
        }
    }
}
