using System;
using System.Collections.Generic;
using System.Linq;
using GCNLibrary.LM;
using OpenTK;
using SELib;
using SELib.Utilities;
using Toolbox.Core.Animations;
using Toolbox.Core;
using static GCNLibrary.LM.KEY_Parser;

namespace Toolbox.Library.Animations
{
    public class SEANIM2
    {
        public static void Save(string FileName, KEY anim, STSkeleton skeleton)
        {
            anim.SetFrame(anim.FrameCount - 1); //from last frame
            for (int f = 0; f < anim.FrameCount; ++f) //go through each frame with nextFrame
                anim.NextFrame(skeleton);
            anim.NextFrame(skeleton);  //go on first frame

            SEAnim seAnim = new SEAnim();
            seAnim.Looping = anim.Loop;
            seAnim.AnimType = AnimationType.Absolute;
            //Reset active animation to 0
            anim.SetFrame(0);
            for (int frame = 0; frame < anim.FrameCount; frame++)
            {
                anim.NextFrame(skeleton);

                foreach (STBone boneAnim in skeleton.Bones)
                {
                    STBone bone = skeleton.SearchBone(boneAnim.Name);
                    if (bone == null) continue;

                    OpenTK.Vector3 position = bone.Position;
                    OpenTK.Quaternion rotation = bone.Rotation;
                    OpenTK.Vector3 scale = bone.Scale;

                    seAnim.AddTranslationKey(boneAnim.Name, frame, position.X, position.Y, position.Z);
                    seAnim.AddRotationKey(boneAnim.Name, frame, rotation.X, rotation.Y, rotation.Z, rotation.W);
                    seAnim.AddScaleKey(boneAnim.Name, frame, scale.X, scale.Y, scale.Z);
                }
            }

            seAnim.Write(FileName);
        }

    }
}
