using System;
using System.Collections.Generic;
using System.Linq;
using GCNLibrary.LM2;
using OpenTK;
using SELib;
using SELib.Utilities;

namespace Toolbox.Library.Animations
{
    public class SEANIM2
    {
        public static Animation Read(string FileName, STSkeleton skeleton)
        {
            Animation animation = new Animation();
            SEAnim sEAnim = SEAnim.Read(FileName);
            animation.FrameCount = sEAnim.FrameCount;
            animation.CanLoop = sEAnim.Looping;
            foreach (string bone2 in sEAnim.Bones)
            {
                STBone bone = skeleton.GetBone(bone2);
                if (bone == null)
                {
                    continue;
                }

                Animation.KeyNode keyNode = new Animation.KeyNode(bone2);
                keyNode.RotType = Animation.RotationType.EULER;
                keyNode.UseSegmentScaleCompensate = false;
                animation.Bones.Add(keyNode);
                float num = 0f;
                float num2 = 0f;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = 0f;
                float num6 = 0f;
                float num7 = 0f;
                float num8 = 0f;
                float num9 = 0f;
                if (sEAnim.AnimType == AnimationType.Relative)
                {
                    num = bone.Position.X;
                    num2 = bone.Position.Y;
                    num3 = bone.Position.Z;
                    num4 = bone.EulerRotation.X;
                    num5 = bone.EulerRotation.Y;
                    num6 = bone.EulerRotation.Z;
                    num7 = bone.Scale.X;
                    num8 = bone.Scale.Y;
                    num9 = bone.Scale.Z;
                }

                Console.WriteLine(bone2);
                if (sEAnim.AnimationPositionKeys.ContainsKey(bone2))
                {
                    List<SEAnimFrame> list = sEAnim.AnimationPositionKeys[bone2];
                    foreach (SEAnimFrame item in list)
                    {
                        Console.WriteLine(item.Frame + " T " + ((SELib.Utilities.Vector3)item.Data).X);
                        Console.WriteLine(item.Frame + " T " + ((SELib.Utilities.Vector3)item.Data).Y);
                        Console.WriteLine(item.Frame + " T " + ((SELib.Utilities.Vector3)item.Data).Z);
                        keyNode.XPOS.Keys.Add(new Animation.KeyFrame
                        {
                            Value = (float)((SELib.Utilities.Vector3)item.Data).X + num,
                            Frame = item.Frame
                        });
                        keyNode.YPOS.Keys.Add(new Animation.KeyFrame
                        {
                            Value = (float)((SELib.Utilities.Vector3)item.Data).Y + num2,
                            Frame = item.Frame
                        });
                        keyNode.ZPOS.Keys.Add(new Animation.KeyFrame
                        {
                            Value = (float)((SELib.Utilities.Vector3)item.Data).Z + num3,
                            Frame = item.Frame
                        });
                    }
                }

                if (sEAnim.AnimationRotationKeys.ContainsKey(bone2))
                {
                    List<SEAnimFrame> list2 = sEAnim.AnimationRotationKeys[bone2];
                    foreach (SEAnimFrame item2 in list2)
                    {
                        SELib.Utilities.Quaternion quaternion = (SELib.Utilities.Quaternion)item2.Data;
                        OpenTK.Vector3 vector = STMath.ToEulerAngles(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
                        Console.WriteLine(item2.Frame + " R " + vector.X);
                        Console.WriteLine(item2.Frame + " R " + vector.Y);
                        Console.WriteLine(item2.Frame + " R " + vector.Z);
                        keyNode.XROT.Keys.Add(new Animation.KeyFrame
                        {
                            Value = vector.X + num4,
                            Frame = item2.Frame
                        });
                        keyNode.YROT.Keys.Add(new Animation.KeyFrame
                        {
                            Value = vector.Y + num5,
                            Frame = item2.Frame
                        });
                        keyNode.ZROT.Keys.Add(new Animation.KeyFrame
                        {
                            Value = vector.Z + num6,
                            Frame = item2.Frame
                        });
                    }
                }

                if (sEAnim.AnimationScaleKeys.ContainsKey(bone2))
                {
                    List<SEAnimFrame> list3 = sEAnim.AnimationScaleKeys[bone2];
                    foreach (SEAnimFrame item3 in list3)
                    {
                        Console.WriteLine(item3.Frame + " S " + ((SELib.Utilities.Vector3)item3.Data).X);
                        Console.WriteLine(item3.Frame + " S " + ((SELib.Utilities.Vector3)item3.Data).Y);
                        Console.WriteLine(item3.Frame + " S " + ((SELib.Utilities.Vector3)item3.Data).Z);
                        keyNode.XSCA.Keys.Add(new Animation.KeyFrame
                        {
                            Value = (float)((SELib.Utilities.Vector3)item3.Data).X + num7,
                            Frame = item3.Frame
                        });
                        keyNode.YSCA.Keys.Add(new Animation.KeyFrame
                        {
                            Value = (float)((SELib.Utilities.Vector3)item3.Data).Y + num8,
                            Frame = item3.Frame
                        });
                        keyNode.ZSCA.Keys.Add(new Animation.KeyFrame
                        {
                            Value = (float)((SELib.Utilities.Vector3)item3.Data).Z + num9,
                            Frame = item3.Frame
                        });
                    }
                }
                else
                {
                    keyNode.XSCA.Keys.Add(new Animation.KeyFrame
                    {
                        Value = 1f,
                        Frame = 0f
                    });
                    keyNode.YSCA.Keys.Add(new Animation.KeyFrame
                    {
                        Value = 1f,
                        Frame = 0f
                    });
                    keyNode.ZSCA.Keys.Add(new Animation.KeyFrame
                    {
                        Value = 1f,
                        Frame = 0f
                    });
                }
            }

            return animation;
        }

        private static void WriteKey(Animation.KeyGroup keys)
        {
            foreach (Animation.KeyFrame key in keys.Keys)
            {
            }
        }

        public static void Save(STSkeletonAnimation anim, string FileName)
        {
            STSkeleton activeSkeleton = anim.GetActiveSkeleton();
            SEAnim sEAnim = new SEAnim();
            sEAnim.Looping = anim.Loop;
            sEAnim.AnimType = AnimationType.Absolute;
            anim.SetFrame(0f);
            for (int i = 0; (float)i < Math.Max(1f, anim.FrameCount); i++)
            {
                anim.SetFrame(i);
                anim.NextFrame();
                foreach (STAnimGroup animGroup in anim.AnimGroups)
                {
                    if (animGroup.GetTracks().Any((STAnimationTrack x) => x.HasKeys))
                    {
                        STBone bone = activeSkeleton.GetBone(animGroup.Name);
                        if (bone != null)
                        {
                            OpenTK.Vector3 position = bone.GetPosition();
                            OpenTK.Quaternion rotation = bone.GetRotation();
                            OpenTK.Vector3 scale = bone.GetScale();
                            sEAnim.AddTranslationKey(animGroup.Name, i, position.X, position.Y, position.Z);
                            sEAnim.AddRotationKey(animGroup.Name, i, rotation.X, rotation.Y, rotation.Z, rotation.W);
                            sEAnim.AddScaleKey(animGroup.Name, i, scale.X, scale.Y, scale.Z);
                        }
                    }
                }
            }

            sEAnim.Write(FileName);
        }

        public static void SaveAnimation(string FileName, KEY anim, Toolbox.Core.STSkeleton skeleton)
        {
            anim.SetFrame(anim.FrameCount - 1);
            for (int i = 0; i < anim.FrameCount; i++)
            {
                anim.NextFrame(skeleton);
            }

            anim.NextFrame(skeleton);
            SEAnim sEAnim = new SEAnim();
            sEAnim.Looping = anim.CanLoop;
            sEAnim.AnimType = AnimationType.Absolute;
            anim.Frame = 0f;
            anim.SetFrame(0f);
            for (int j = 0; j < anim.FrameCount; j++)
            {
                anim.Frame = j;
                for (int i = 0; i < anim.AnimGroups.Count; i++)
                {
                    bool flag3 = i >= skeleton.Bones.Count;
                    if (flag3)
                    {
                        break;
                    }
                    Toolbox.Core.STBone joint = skeleton.Bones[i];
                    KEY.AnimGroup group = (KEY.AnimGroup)anim.AnimGroups[i];
                    //Updated = true;
                    OpenTK.Vector3 position = joint.Position;
                    OpenTK.Vector3 scale = joint.Scale;
                    OpenTK.Vector3 rotate = joint.EulerRotation;
                    bool hasKeys = group.PositionX.HasKeys;
                    if (hasKeys)
                    {
                        position.X = group.PositionX.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys2 = group.PositionY.HasKeys;
                    if (hasKeys2)
                    {
                        position.Y = group.PositionY.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys3 = group.PositionZ.HasKeys;
                    if (hasKeys3)
                    {
                        position.Z = group.PositionZ.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys4 = group.RotateX.HasKeys;
                    if (hasKeys4)
                    {
                        rotate.X = group.RotateX.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys5 = group.RotateY.HasKeys;
                    if (hasKeys5)
                    {
                        rotate.Y = group.RotateY.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys6 = group.RotateZ.HasKeys;
                    if (hasKeys6)
                    {
                        rotate.Z = group.RotateZ.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys7 = group.ScaleX.HasKeys;
                    if (hasKeys7)
                    {
                        scale.X = group.ScaleX.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys8 = group.ScaleY.HasKeys;
                    if (hasKeys8)
                    {
                        scale.Y = group.ScaleY.GetFrameValue(anim.Frame, 0f);
                    }
                    bool hasKeys9 = group.ScaleZ.HasKeys;
                    if (hasKeys9)
                    {
                        scale.Z = group.ScaleZ.GetFrameValue(anim.Frame, 0f);
                    }
                    joint.AnimationController.Position = position;
                    joint.AnimationController.Scale = scale;
                    joint.AnimationController.EulerRotation = rotate;
                    //if (bone2.HasKeyedFrames(j
                    OpenTK.Quaternion rotation = OpenTK.Quaternion.FromEulerAngles(rotate);
                    sEAnim.AddTranslationKey(joint.Name, j, -joint.AnimationController.Position.Z, joint.AnimationController.Position.X, joint.AnimationController.Position.X);
                    sEAnim.AddRotationKey(joint.Name, j, joint.AnimationController.Rotation.X, -joint.AnimationController.Rotation.Y, joint.AnimationController.Rotation.Z, joint.AnimationController.Rotation.W);
                    sEAnim.AddScaleKey(joint.Name, j, joint.AnimationController.Scale.X, joint.AnimationController.Scale.Y, joint.AnimationController.Scale.Z);
                }
            }

            sEAnim.Write(FileName);
        }
    }
}
