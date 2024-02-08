using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTK;
using System.Text;
using System.Threading;
using System.Globalization;
using Toolbox.Core.Animations;
using Toolbox.Core;
using GCNLibrary.LM;

namespace Toolbox.Library.Animations
{
    //Todo rewrite this
    //Currently from forge
    //https://raw.githubusercontent.com/jam1garner/Smash-Forge/master/Smash%20Forge/Filetypes/SMD.cs
    public class SMD
    {
        public STSkeleton Bones;
        public STAnimation Animation; // todo

        public SMD()
        {
            Bones = new STSkeleton();
        }

        public static void Save(String Fname, KEY anim, STSkeleton Skeleton)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@Fname))
            {
                file.WriteLine("version 1");

                file.WriteLine("nodes");
                foreach (STBone b in Skeleton.Bones)
                {
                    file.WriteLine(Skeleton.Bones.IndexOf(b) + " \"" + b.Name + "\" " + b.ParentIndex);
                }
                file.WriteLine("end");

                file.WriteLine("skeleton");
                anim.SetFrame(0);
                for (int i = 0; i <= anim.FrameCount; i++)
                {
                    anim.SetFrame(i);
                    anim.NextFrame();

                    file.WriteLine($"time {i}");

                    foreach (STBone b in Skeleton.Bones)
                    {
                        //STBone b = Skeleton.SearchBone(sb.Name);
                        if (b == null) continue;
                        Vector3 eul = STMath.ToEulerAngles(b.Rotation);
                        Vector3 scale = b.Scale;
                        Vector3 translate = b.Position;

                        file.WriteLine($"{Skeleton.Bones.IndexOf(b)} {translate.X} {translate.Y} {translate.Z} {eul.X} {eul.Y} {eul.Z}");
                    }

                }
                file.WriteLine("end");

                file.Close();
            }
        }

    }
}