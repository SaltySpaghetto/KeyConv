using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;
using Toolbox.Core;
using Toolbox.Core.Imaging;
using Toolbox.Core.IO;
using Toolbox.Core.OpenGL;
using GCNLibrary.LM.MDL;

namespace GCNLibrary.LM2
{

    public class MDL : IFileFormat, IReplaceableModel
    {
        public class SamplerOrder
        {
            public List<SamplerConvert> Samplers = new List<SamplerConvert>();
        }

        public class SamplerConvert
        {
            public ushort TextureIndex;

            public byte WrapModeU;

            public byte WrapModeV;

            public byte MinFilter;

            public byte MagFilter;
        }

        private class ShapeFlags
        {
            public byte NormalsFlags { get; set; }

            public byte Unknown1 { get; set; }

            public byte Unknown2 { get; set; }

            public byte Unknown3 { get; set; }

            public ushort NodeIndex { get; set; } = 0;

        }

        private class MaterialConvert
        {
            public ShapeFlags MeshSettings { get; set; }

            public byte DiffuseR { get; set; }

            public byte DiffuseG { get; set; }

            public byte DiffuseB { get; set; }

            public byte DiffuseA { get; set; }

            public byte AlphaFlags { get; set; }

            public ushort Unknown1 { get; set; }

            public byte Unknown2 { get; set; }

            public TevStageConvert[] TevStages { get; set; }
        }

        private class TevStageConvert
        {
            public ushort SamplerIndex { get; set; }

            public ushort Unknown { get; set; }

            public float[] Values { get; set; }
        }

        public class MDL_Material : STGenericMaterial
        {
            public STColor8 TintColor { get; set; }
        }

        public class Texture : STGenericTexture
        {
            public byte[] ImageData;

            public Texture(TextureHeader header)
            {
                base.Width = header.Width;
                base.Height = header.Height;
                base.MipCount = 1u;
                Platform = new GamecubeSwizzle(header.Format);
                ImageData = header.ImageData;
            }

            public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0)
            {
                return ImageData;
            }

            public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
            {
                throw new NotImplementedException();
            }
        }

        public List<STGenericTexture> Textures = new List<STGenericTexture>();

        public MDL_Parser Header;

        private STGenericModel Model;

        public bool CanSave { get; set; } = true;


        public string[] Description { get; set; } = new string[1] { "LM Actor Model" };


        public string[] Extension { get; set; } = new string[1] { "*.mdl" };


        public File_Info FileInfo { get; set; }


        public bool Identify(File_Info fileInfo, Stream stream)
        {
            FileReader reader = new FileReader(stream, leaveOpen: true);
            reader.SetByteOrder(IsBigEndian: true);
            return reader.ReadUInt32() == 78905344;
        }

        public void Load(Stream stream)
        {
            Header = new MDL_Parser(stream);
            TextureHeader[] textures = Header.Textures;
            foreach (TextureHeader tex in textures)
            {
                Textures.Add(new Texture(tex)
                {
                    Name = $"Texture{Textures.Count}"
                });
            }
            //ToGeneric().Skeleton.PreviewScale = 3f;
        }

        public STGenericModel ToGeneric(string fileName)
        {
            if (Model != null)
            {
                return Model;
            }
            STGenericModel model = new STGenericModel(fileName);
            model.Textures = Textures;
            Matrix4 matrix = Matrix4.Identity;
            List<STGenericMaterial> materials = new List<STGenericMaterial>();
            for (int i = 0; i < Header.Materials.Length; i++)
            {
                materials.Add(CreateMaterial(Header.Materials[i], i));
            }
            model.Skeleton = new STSkeleton();
            Matrix4[] transforms = new Matrix4[Header.FileHeader.JointCount];
            for (int k = 0; k < Header.FileHeader.JointCount; k++)
            {
                Matrix4 transfrom = Header.Matrix4Table[k];
                transfrom.Invert();
                transfrom.Transpose();
                transforms[k] = transfrom;
                model.Skeleton.Bones.Add(new STBone(model.Skeleton)
                {
                    Name = ((Header.Nodes[k].ShapeCount > 0) ? $"Mesh{k}" : $"Bone{k}"),
                    Position = transfrom.ExtractTranslation(),
                    Rotation = transfrom.ExtractRotation(),
                    Scale = transfrom.ExtractScale()
                });
            }
            TraverseNodeGraph(model.Skeleton, 0);
            model.Skeleton.ConvertWorldToLocalSpace();
            model.Skeleton.Reset();
            model.Skeleton.Update();
            for (int j = 0; j < Header.Meshes.Count; j++)
            {
                STGenericMesh mesh = new STGenericMesh
                {
                    Name = $"Mesh{j}"
                };
                model.Meshes.Add(mesh);
                ushort matIndex = Header.Meshes[j].DrawElement.MaterialIndex;
                STPolygonGroup group = new STPolygonGroup();
                group.Material = materials[matIndex];
                group.PrimitiveType = STPrimitiveType.Triangles;
                mesh.PolygonGroups.Add(group);
                ShapePacket[] packets = Header.Meshes[j].Packets;
                foreach (ShapePacket packet in packets)
                {
                    foreach (ShapePacket.DrawList drawList in packet.DrawLists)
                    {
                        List<STVertex> verts = new List<STVertex>();
                        for (int v = 0; v < drawList.Vertices.Count; v++)
                        {
                            matrix = ((drawList.Vertices[v].MatrixIndex == -1 || drawList.Vertices[v].MatrixDataIndex >= Header.FileHeader.JointCount) ? Matrix4.Identity : transforms[drawList.Vertices[v].MatrixDataIndex]);
                            verts.Add(ToVertex(drawList.Vertices[v], ref matrix));
                        }
                        switch (drawList.OpCode)
                        {
                            case 160:
                                verts = ConvertTriFans(verts);
                                mesh.Vertices.AddRange(verts);
                                break;
                            case 144:
                                mesh.Vertices.AddRange(verts);
                                break;
                            case 152:
                                verts = ConvertTriStrips(verts);
                                mesh.Vertices.AddRange(verts);
                                break;
                            default:
                                throw new Exception("Unknown opcode " + drawList.OpCode);
                        }
                    }
                }
                mesh.Optmize(group);
            }
            Model = model;
            return model;
        }

        private static int GetMaterialIndex(string name)
        {
            int index = 0;
            string value = name.Replace("Material", string.Empty);
            int.TryParse(value, out index);
            return index;
        }

        private static int GetTextureIndex(string name)
        {
            int index = 0;
            string value = name.Replace("Texture", string.Empty);
            int.TryParse(value, out index);
            return index;
        }

        private static int GetBoneIndex(string name)
        {
            int index = 0;
            string value = name.Replace("Bone", string.Empty).Replace("Mesh", string.Empty);
            int.TryParse(value, out index);
            return index;
        }

        public void FromGeneric(STGenericScene scene)
        {
            STGenericModel model = scene.Models[0];
            bool useTriangeStrips = false;
            MDL_Parser mdl = new MDL_Parser();
            mdl.FileHeader = new MDL_Parser.Header();
            mdl.Meshes = new List<MDL_Parser.Mesh>();
            List<Node> nodes = new List<Node>();
            List<Matrix4> matrices = new List<Matrix4>();
            List<Material> materials = new List<Material>();
            List<TextureHeader> textures = new List<TextureHeader>();
            List<Sampler> samplers = new List<Sampler>();
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<Vector4> colors = new List<Vector4>();
            List<Shape> shapes = new List<Shape>();
            List<ShapePacket> packets = new List<ShapePacket>();
            List<DrawElement> elements = new List<DrawElement>();
            List<MDL_Parser.Weight> weights = new List<MDL_Parser.Weight>();
            model.Textures = model.Textures.OrderBy((STGenericTexture x) => GetTextureIndex(x.Name)).ToList();
            model.OrderBones(model.Skeleton.Bones.OrderBy((STBone x) => GetBoneIndex(x.Name)).ToList());
            foreach (STGenericTexture texture in model.Textures)
            {
                textures.Add(new TextureHeader
                {
                    Width = (ushort)texture.Width,
                    Height = (ushort)texture.Height,
                    Format = Decode_Gamecube.TextureFormats.CMPR,
                    ImageData = Decode_Gamecube.EncodeFromBitmap(texture.GetBitmap(), Decode_Gamecube.TextureFormats.CMPR).Item1
                });
            }
            if (Header.Samplers.Length == 0)
            {
                foreach (STGenericTexture texture2 in model.Textures)
                {
                    samplers.Add(new Sampler
                    {
                        WrapModeU = 2,
                        WrapModeV = 2,
                        MagFilter = 0,
                        MinFilter = 0,
                        TextureIndex = (ushort)model.Textures.IndexOf(texture2)
                    });
                }
            }
            else
            {
                samplers = Header.Samplers.ToList();
            }
            if (model.Skeleton.Bones.Count == 0)
            {
                nodes.Add(new Node
                {
                    ChildIndex = 1,
                    SiblingIndex = 0,
                    ShapeCount = (ushort)model.Meshes.Count,
                    ShapeIndex = 0
                });
            }
            foreach (STBone bone2 in model.Skeleton.Bones.Where((STBone x) => x.ParentIndex == -1))
            {
                CreateNodeGraph(nodes, bone2);
            }
            model.Skeleton.Reset();
            foreach (STBone bone in model.Skeleton.Bones)
            {
                Matrix4 transform = bone.Transform;
                transform.Transpose();
                transform.Invert();
                matrices.Add(transform);
            }
            ushort shapeIndex = 0;
            for (int l = 0; l < nodes.Count; l++)
            {
                nodes[l].NodeIndex = (ushort)l;
                if (l == 0)
                {
                    nodes[l].ShapeCount = (ushort)model.Meshes.Count;
                    shapeIndex += (ushort)model.Meshes.Count;
                }
                foreach (STGenericMesh mesh in model.Meshes)
                {
                    for (int v = 0; v < mesh.Vertices.Count; v++)
                    {
                        if (mesh.Vertices[v].BoneIndices.Contains(l))
                        {
                            nodes[l].ShapeIndex = shapeIndex;
                        }
                    }
                }
            }
            List<STGenericMaterial> genericMats = model.GetMaterials();
            genericMats = genericMats.OrderBy((STGenericMaterial x) => GetMaterialIndex(x.Name)).ToList();
            foreach (STGenericMaterial material in genericMats)
            {
                Material mat2 = new Material();
                if (material.TextureMaps.Count > 0)
                {
                    string name = material.TextureMaps[0].Name;
                    int index = model.Textures.FindIndex((STGenericTexture x) => x.Name == name);
                    int samplerIndex = samplers.FindIndex((Sampler x) => x.TextureIndex == index);
                    if (samplerIndex != -1)
                    {
                        mat2.TevStages[0].Unknown = 0;
                        mat2.TevStages[0].SamplerIndex = (ushort)samplerIndex;
                    }
                }
                materials.Add(mat2);
            }
            if (genericMats.Count == 0)
            {
                Material mat = new Material();
                materials.Add(mat);
            }
            int packetIndex = 0;
            mdl.FileHeader.FaceCount = 0;
            foreach (STGenericMesh mesh2 in model.Meshes)
            {
                int materialIndex = 0;
                if (mesh2.PolygonGroups[0].Material != null)
                {
                    materialIndex = genericMats.IndexOf(mesh2.PolygonGroups[0].Material);
                }
                List<ushort> boneIndices = new List<ushort>();
                List<ShapePacket> shapePackets = new List<ShapePacket>();
                ShapePacket packet3 = new ShapePacket();
                shapePackets.Add(packet3);
                int vindex = 0;
                mdl.FileHeader.FaceCount += (ushort)(mesh2.PolygonGroups.Sum((STPolygonGroup x) => x.Faces.Count) / 3);
                STPolygonGroup group = mesh2.PolygonGroups[0];
                for (int v2 = 0; v2 < group.Faces.Count; v2 += 3)
                {
                    int maxBoneIndices = boneIndices.Count;
                    for (int k = 0; k < 3; k++)
                    {
                        STVertex vertex = mesh2.Vertices[(int)group.Faces[v2 + (2 - k)]];
                        for (int m = 0; m < vertex.BoneIndices.Count; m++)
                        {
                            int index2 = vertex.BoneIndices[m];
                            if (!boneIndices.Contains((ushort)index2) || vertex.BoneIndices.Count > 1)
                            {
                                maxBoneIndices++;
                            }
                        }
                    }
                    if (maxBoneIndices > 9)
                    {
                        boneIndices = new List<ushort>();
                        packet3 = new ShapePacket();
                        shapePackets.Add(packet3);
                    }
                    ShapePacket.DrawList drawList = new ShapePacket.DrawList();
                    drawList.OpCode = 144;
                    packet3.DrawLists.Add(drawList);
                    for (int j = 0; j < 3; j++)
                    {
                        ShapePacket.VertexGroup vertexGroup = new ShapePacket.VertexGroup();
                        drawList.Vertices.Add(vertexGroup);
                        STVertex vertex2 = mesh2.Vertices[(int)group.Faces[v2 + (2 - j)]];
                        Vector3 pos = vertex2.Position;
                        Vector3 nrm = vertex2.Normal;
                        if (vertex2.TexCoords.Length != 0)
                        {
                            if (!texCoords.Contains(vertex2.TexCoords[0]))
                            {
                                texCoords.Add(vertex2.TexCoords[0]);
                            }
                            vertexGroup.TexCoordIndex = (short)texCoords.IndexOf(vertex2.TexCoords[0]);
                        }
                        if (vertex2.BoneIndices.Count == 1)
                        {
                            ushort index4 = (ushort)vertex2.BoneIndices[0];
                            if (!boneIndices.Contains(index4))
                            {
                                boneIndices.Add(index4);
                            }
                            vertexGroup.MatrixIndex = (sbyte)(boneIndices.IndexOf(index4) * 3);
                            vertexGroup.Tex0MatrixIndex = (sbyte)(boneIndices.IndexOf(index4) * 3);
                            vertexGroup.Tex1MatrixIndex = (sbyte)(boneIndices.IndexOf(index4) * 3);
                            Matrix4 matrix = matrices[index4];
                            matrix.Transpose();
                            pos = Vector3.TransformPosition(pos, matrix);
                            nrm = Vector3.TransformNormal(nrm, matrix);
                        }
                        else if (vertex2.BoneIndices.Count > 1)
                        {
                            vertex2.SortBoneIndices();
                            List<float> vertexWeights = vertex2.BoneWeights.ToList();
                            for (int j5 = 0; j5 < vertex2.BoneWeights.Count; j5++)
                            {
                                vertexWeights.Add(vertex2.BoneWeights[j5]);
                            }
                            vertexWeights = NormalizeByteType(vertexWeights);
                            MDL_Parser.Weight weightEntry = new MDL_Parser.Weight();
                            for (int j4 = 0; j4 < vertex2.BoneIndices.Count; j4++)
                            {
                                weightEntry.Weights.Add(vertexWeights[j4]);
                            }
                            for (int j3 = 0; j3 < vertex2.BoneIndices.Count; j3++)
                            {
                                weightEntry.JointIndices.Add(vertex2.BoneIndices[j3]);
                            }
                            MDL_Parser.Weight existingWeight = null;
                            for (int w = 0; w < weights.Count; w++)
                            {
                                int matchedWeights = 0;
                                for (int j2 = 0; j2 < vertex2.BoneIndices.Count; j2++)
                                {
                                    int jointIndex = weights[w].JointIndices.IndexOf(vertex2.BoneIndices[j2]);
                                    if (jointIndex != -1 && weights[w].Weights[jointIndex] == vertexWeights[j2])
                                    {
                                        matchedWeights++;
                                    }
                                }
                                if (matchedWeights == vertex2.BoneIndices.Count)
                                {
                                    existingWeight = weights[w];
                                }
                            }
                            if (existingWeight == null)
                            {
                                existingWeight = weightEntry;
                                weights.Add(existingWeight);
                            }
                            ushort rigidIndex = (ushort)nodes.Count;
                            ushort index3 = (ushort)(weights.IndexOf(existingWeight) + rigidIndex);
                            if (!boneIndices.Contains(index3))
                            {
                                boneIndices.Add(index3);
                            }
                            vertexGroup.MatrixIndex = (sbyte)(boneIndices.IndexOf(index3) * 3);
                            vertexGroup.Tex0MatrixIndex = (sbyte)(boneIndices.IndexOf(index3) * 3);
                            vertexGroup.Tex1MatrixIndex = (sbyte)(boneIndices.IndexOf(index3) * 3);
                        }
                        else if (vertex2.BoneIndices.Count == 0)
                        {
                            if (!boneIndices.Contains(0))
                            {
                                boneIndices.Add(0);
                            }
                            vertexGroup.MatrixIndex = 0;
                            vertexGroup.Tex0MatrixIndex = 0;
                            vertexGroup.Tex1MatrixIndex = 0;
                        }
                        if (!positions.Contains(pos))
                        {
                            positions.Add(pos);
                        }
                        if (!normals.Contains(nrm))
                        {
                            normals.Add(nrm);
                        }
                        vertexGroup.PositionIndex = (short)positions.IndexOf(pos);
                        vertexGroup.NormalIndex = (short)normals.IndexOf(nrm);
                        for (int n = 0; n < boneIndices.Count; n++)
                        {
                            packet3.MatrixIndices[n] = boneIndices[n];
                        }
                        packet3.MatrixIndicesCount = (ushort)boneIndices.Count;
                        vindex++;
                    }
                }
                elements.Add(new DrawElement
                {
                    ShapeIndex = (ushort)shapes.Count,
                    MaterialIndex = (ushort)materialIndex
                });
                shapes.Add(new Shape
                {
                    PacketBeginIndex = (ushort)packetIndex,
                    PacketCount = (ushort)shapePackets.Count
                });
                packetIndex += shapePackets.Count;
                packets.AddRange(shapePackets);
            }
            foreach (ShapePacket packet2 in packets)
            {
                packet2.Data = packet2.CreateDrawList(packet2.DrawLists, IsLOD: false, normals.Count > 0, texCoords.Count > 0, colors.Count > 0);
                packet2.DataSize = (uint)packet2.Data.Length;
            }
            List<ShapePacket> lodPackets = new List<ShapePacket>();
            foreach (ShapePacket packet in packets)
            {
                ShapePacket lodPacket = new ShapePacket();
                lodPacket.MatrixIndices = packet.MatrixIndices;
                lodPacket.MatrixIndicesCount = packet.MatrixIndicesCount;
                lodPacket.Data = packet.CreateDrawList(packet.DrawLists, IsLOD: true, normals.Count > 0, texCoords.Count > 0, colors.Count > 0);
                lodPacket.DataSize = (uint)lodPacket.Data.Length;
                lodPackets.Add(lodPacket);
            }
            packets.AddRange(lodPackets);
            mdl.Matrix4Table = new Matrix4[matrices.Count];
            for (int i = 0; i < matrices.Count; i++)
            {
                mdl.Matrix4Table[i] = matrices[i];
            }
            mdl.Materials = materials.ToArray();
            mdl.Nodes = nodes.ToArray();
            mdl.Samplers = samplers.ToArray();
            mdl.Colors = colors.ToArray();
            mdl.Positions = positions.ToArray();
            mdl.TexCoords = texCoords.ToArray();
            mdl.Normals = normals.ToArray();
            mdl.DrawElements = elements.ToArray();
            mdl.Shapes = shapes.ToArray();
            mdl.ShapePackets = packets.ToArray();
            mdl.Weights = weights.ToArray();
            mdl.Textures = textures.ToArray();
            mdl.LODPositions = new Vector3[0];
            mdl.LODNormals = new Vector3[0];
            Header = mdl;
        }

        private static List<float> NormalizeByteType(List<float> weights)
        {
            float scale = 0.003921569f;
            float MaxWeight = 1f;
            List<float> list = weights.ToList();
            List<float> normalized = new List<float>();
            int id = 0;
            foreach (float b in weights)
            {
                id++;
                float weight = (float)Math.Round(b, 2);
                if (list.Count == id)
                {
                    weight = MaxWeight;
                }
                if (weight >= MaxWeight)
                {
                    weight = MaxWeight;
                    MaxWeight = 0f;
                }
                else
                {
                    MaxWeight -= weight;
                }
                normalized.Add(weight);
            }
            return normalized;
        }

        private void CreateNodeGraph(List<Node> nodes, STBone bone, bool isSibling = false)
        {
            int currentIndex = nodes.Count;
            nodes.Add(new Node
            {
                NodeIndex = (ushort)currentIndex,
                ChildIndex = ((bone.Children.Count > 0) ? ((ushort)1) : ((ushort)0))
            });
            for (int i = 0; i < bone.Children.Count; i++)
            {
                bool sibling = i < bone.Children.Count - 1;
                CreateNodeGraph(nodes, bone.Children[i], sibling);
            }
            int siblingStart = 0;
            if (isSibling)
            {
                siblingStart = nodes.Count - currentIndex;
            }
            nodes[currentIndex].SiblingIndex = (ushort)siblingStart;
        }

        private void TraverseNodeGraph(STSkeleton skeleton, int index, int parentIndex = -1)
        {
            Node node = Header.Nodes[index];
            skeleton.Bones[index].ParentIndex = parentIndex;
            if (node.ChildIndex > 0)
            {
                TraverseNodeGraph(skeleton, index + node.ChildIndex, index);
            }
            if (node.SiblingIndex > 0)
            {
                TraverseNodeGraph(skeleton, index + node.SiblingIndex, parentIndex);
            }
        }

        private STGenericMaterial CreateMaterial(Material material, int index)
        {
            MDL_Material mat = new MDL_Material();
            mat.Name = $"Material{index}";
            mat.TintColor = material.Color;
            if (material.TevStages[0].SamplerIndex != ushort.MaxValue)
            {
                Sampler texturObj = Header.Samplers[material.TevStages[0].SamplerIndex];
                STGenericTexture tex = Textures[texturObj.TextureIndex];
                STTextureWrapMode wrapModeU = ConvertWrapMode(texturObj.WrapModeU);
                STTextureWrapMode wrapModeV = ConvertWrapMode(texturObj.WrapModeV);
                mat.TextureMaps.Add(new STGenericTextureMap
                {
                    Name = tex.Name,
                    WrapU = wrapModeU,
                    WrapV = wrapModeV,
                    Type = STTextureType.Diffuse
                });
            }
            return mat;
        }

        private static STTextureWrapMode ConvertWrapMode(byte value)
        {
            return (Decode_Gamecube.WrapModes)value switch
            {
                Decode_Gamecube.WrapModes.Repeat => STTextureWrapMode.Repeat,
                Decode_Gamecube.WrapModes.MirroredRepeat => STTextureWrapMode.Mirror,
                Decode_Gamecube.WrapModes.ClampToEdge => STTextureWrapMode.Clamp,
                _ => STTextureWrapMode.Repeat,
            };
        }

        private List<STVertex> ConvertTriFans(List<STVertex> vertices)
        {
            List<STVertex> outVertices = new List<STVertex>();
            int vertexId = 0;
            int firstVertex = vertexId;
            for (int index = 0; index < 3; index++)
            {
                outVertices.Add(vertices[index]);
            }
            for (int index2 = 2; index2 < vertices.Count; index2++)
            {
                STVertex vert1 = vertices[firstVertex];
                STVertex vert2 = vertices[index2 - 1];
                STVertex vert3 = vertices[index2];
                if (!vert1.Position.Equals(vert2.Position) && !vert2.Position.Equals(vert3.Position) && !vert3.Position.Equals(vert1.Position))
                {
                    outVertices.Add(vert2);
                    outVertices.Add(vert3);
                    outVertices.Add(vert1);
                }
            }
            return outVertices;
        }

        private List<STVertex> ConvertTriStrips(List<STVertex> vertices)
        {
            List<STVertex> outVertices = new List<STVertex>();
            for (int index = 2; index < vertices.Count; index++)
            {
                bool isEven = index % 2 != 1;
                STVertex vert1 = vertices[index - 2];
                STVertex vert2 = (isEven ? vertices[index] : vertices[index - 1]);
                STVertex vert3 = (isEven ? vertices[index - 1] : vertices[index]);
                if (!vert1.Position.Equals(vert2.Position) && !vert2.Position.Equals(vert3.Position) && !vert3.Position.Equals(vert1.Position))
                {
                    outVertices.Add(vert2);
                    outVertices.Add(vert3);
                    outVertices.Add(vert1);
                }
            }
            return outVertices;
        }

        private STVertex ToVertex(ShapePacket.VertexGroup drawList, ref Matrix4 transform)
        {
            Vector3 position = Header.Positions[drawList.PositionIndex];
            Vector3 normal = default(Vector3);
            Vector2 texCoord = default(Vector2);
            Vector4 color = Vector4.One;
            List<int> boneIndices = new List<int>();
            List<float> boneWeights = new List<float>();
            if (drawList.NormalIndex != -1)
            {
                normal = Header.Normals[drawList.NormalIndex];
            }
            if (drawList.TexCoordIndex != -1)
            {
                texCoord = Header.TexCoords[drawList.TexCoordIndex];
            }
            if (drawList.ColorIndex != -1)
            {
                color = Header.Colors[drawList.ColorIndex];
            }
            if (drawList.MatrixDataIndex >= Header.FileHeader.JointCount)
            {
                int weightIndex = drawList.MatrixDataIndex - Header.FileHeader.JointCount;
                MDL_Parser.Weight weights = Header.Weights[weightIndex];
                for (int i = 0; i < weights.JointIndices.Count; i++)
                {
                    boneIndices.Add(weights.JointIndices[i]);
                    boneWeights.Add(weights.Weights[i]);
                }
            }
            else if (drawList.MatrixIndex != -1)
            {
                boneIndices.Add(drawList.MatrixDataIndex);
                boneWeights.Add(1f);
            }
            position = Vector3.TransformPosition(position, transform);
            normal = Vector3.TransformNormal(normal, transform);
            STVertex sTVertex = new STVertex();
            sTVertex.Position = position;
            sTVertex.Normal = normal;
            sTVertex.BoneIndices = boneIndices;
            sTVertex.BoneWeights = boneWeights;
            sTVertex.TexCoords = new Vector2[1] { texCoord };
            sTVertex.Colors = new Vector4[1] { color };
            return sTVertex;
        }

        public void Save(Stream stream)
        {
            Header.Save(stream);
        }
    }

}