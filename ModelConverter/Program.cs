using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using GCNLibrary.LM2;
using Toolbox.Library;
using Toolbox.Library.IO;
using Toolbox.Library.Animations;
using Toolbox.Core;
using GCNLibrary.LM.BIN;

/*
 *	CREDITS TO WHOEVER MADE THE MODELCONVERTER. 
 *	CREDITS TO KillzXGaming FOR ALL THE SWITCH-TOOLBOX CODE
 *	MOST OF THIS IS DECOMPILED, OR COPY PASTED, FOR I AM A LAZY IDIOT.
 *	READ THE READMEON GITHUB.
 *	SMD CODE IS BROKEN.
 *	SEANIM WORKS KIND OF.
 *	- Spaghetto207
 */

namespace ModelConverter
{
    // Token: 0x02000002 RID: 2
    internal class Program
    {
		public static void DoExport(string assetPathMDL, string assetPathKEY)
		{
            MDL model = new MDL();
            File_Info info = new File_Info();
			var fileName = Path.GetFileNameWithoutExtension(assetPathMDL);
			model.FileInfo = info;
            using (FileStream InputBin = new FileStream(assetPathMDL, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                model.Load(InputBin);
            }
            KEY anim = new KEY();
			anim.fileName = Path.GetFileNameWithoutExtension(assetPathKEY);
            using (FileStream InputBin2 = new FileStream(assetPathKEY, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                anim.Load(InputBin2);
            }

			ExportAnim(anim, model.ToGeneric(fileName).Skeleton);

        }

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        private static void Main(string[] args)
		{
			//DEBUG
            DoExport("./luige.mdl", "./wait0.key");

            bool flag = args.Length == 0;
			if (!flag)
			{
				DoExport(args[0], args[1]);
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000027EC File Offset: 0x000009EC
		private static void SearchDir(string folder)
		{
			foreach (string file in Directory.GetFiles(folder))
			{
				bool flag = file.EndsWith(".szp");
				if (flag)
				{
					Program.OpenSZP(file);
				}
			}
			foreach (string dir in Directory.GetDirectories(folder))
			{
				Program.SearchDir(dir);
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002858 File Offset: 0x00000A58
		private static void OpenSZP(string filePath)
		{
			/*
			IArchiveFile file = STFileLoader.OpenFileFormat(filePath, null) as IArchiveFile;
			foreach (ArchiveFileInfo f in file.Files)
			{
				bool flag = f.FileName.EndsWith(".mdl");
				if (flag)
				{
					MDL mdl = f.OpenFile() as MDL;
					byte[] og = f.As();
					string ogHash = Program.GetHashSHA1(og);
					MemoryStream mem = new MemoryStream();
					mdl.Save(mem);
					byte[] saved = mem.ToArray();
					string savedHash = Program.GetHashSHA1(saved);
					bool flag2 = savedHash != ogHash;
					if (flag2)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Save mismatch " + f.FileName);
						Console.ForegroundColor = ConsoleColor.White;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Save match " + f.FileName);
						Console.ForegroundColor = ConsoleColor.White;
					}
				}
			}
			*/
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002974 File Offset: 0x00000B74
		private static string GetHashSHA1(byte[] data)
		{
			string text;
			using (SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider())
			{
				text = string.Concat(from x in sha.ComputeHash(data)
					select x.ToString("X2"));
			}
			return text;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000029D8 File Offset: 0x00000BD8
		private static int GetBoneIndex(string name)
		{
			int index = 0;
			string value = name.Replace("Bone", string.Empty).Replace("Mesh", string.Empty);
			int.TryParse(value, out index);
			return index;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002A18 File Offset: 0x00000C18
        private static void ExportAnim(KEY model, Toolbox.Core.STSkeleton skeleton)
        {
            string daePath = model.fileName.Replace(".key", ".seanim");
            string folder = Path.GetFileNameWithoutExtension(model.fileName);
            string folderDir = Path.Combine(Path.GetDirectoryName(model.fileName), folder);
            bool flag = !Directory.Exists(folder ?? "");
            if (flag)
            {
                Directory.CreateDirectory(folder ?? "");
            
			}
			//SMD2.Save(model, skeleton, $"{daePath}.smd");
            SEANIM2.SaveAnimation($"{daePath}.seanim", model, skeleton);

            /*
            GCNLibrary.LM.MDL.Material[] materials = model.Header.Materials;
            DrawElement[] drawElements = model.Header.DrawElements;
            for (int i = 0; i < drawElements.Length; i++)
            {
                File.WriteAllText(string.Format("{0}/Mesh{1}.json", folder, i), model.ExportMaterial(drawElements[i]));
            }
            File.WriteAllText(folder + "/SamplerList.json", model.ExportSamplers());
            DAE.ExportSettings settings = new DAE.ExportSettings();
            settings.ImageFolder = folder;
            DAE.Export(Path.Combine(folderDir, daePath), settings, model);
            Console.WriteLine("Exported model " + model.FileInfo.FileName + "!");
			*/
        }

        // Token: 0x02000003 RID: 3
        private class Node
		{
			// Token: 0x04000001 RID: 1
			public List<DrawElement> DrawElements = new List<DrawElement>();
		}

		// Token: 0x02000004 RID: 4
		private class Draw
		{
			// Token: 0x04000002 RID: 2
			public int Index;

			// Token: 0x04000003 RID: 3
			public DrawElement element;
		}
	}
}
