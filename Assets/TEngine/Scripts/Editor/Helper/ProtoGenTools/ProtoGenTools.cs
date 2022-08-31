using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TEngine.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TEngine.Editor
{
    internal class OpcodeInfo
    {
        public string Name;
        public int Opcode;
    }

    public static class ProtoGenTools
    {
#if UNITY_EDITOR
        [MenuItem("TEngine/生成Proto|Gen Proto", false, 10)]
#endif
        public static void Export()
        {
            InnerProto2CS.Proto2CS();
            Log.Info("proto2cs succeed!");
        }
    }

    public static class InnerProto2CS
    {
        private static string ProtoPath = UnityEngine.Application.dataPath + "\\TEngine\\Tools~\\Protobuf\\Proto\\";

        private static string OutPutPath =
            UnityEngine.Application.dataPath + "\\TEngine\\Tools~\\Protobuf\\Proto_CSharp\\";

        private static readonly char[] splitChars = { ' ', '\t' };
        private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();

        public static void Proto2CS()
        {
            msgOpcode.Clear();
            Proto2CS("TEngineProto", "TEngineProto.proto", OutPutPath,10001,false);
        }

        public static void Proto2CS(string ns, string protoName, string outputPath, int startOpcode,bool useMemoryPool = false)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            msgOpcode.Clear();
            string proto = Path.Combine(ProtoPath, protoName);
            string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(proto) + ".cs");

            string s = File.ReadAllText(proto);

            StringBuilder sb = new StringBuilder();
            sb.Append("using ProtoBuf;\n");
            sb.Append("using TEngine.Runtime;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");

            bool isMsgStart = false;
            bool isEnumStart = false;
            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();

                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//ResponseType"))
                {
                    string responseType = line.Split(' ')[1].TrimEnd('\r', '\n');
                    sb.AppendLine($"\t[ResponseType(nameof({responseType}))]");
                    continue;
                }

                if (newline.StartsWith("//"))
                {
                    sb.Append($"{newline}\n");
                    continue;
                }

                if (newline.StartsWith("message"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });
                    sb.Append($"\t[global::ProtoBuf.ProtoContract()]\n");
                    if (useMemoryPool)
                    {
                        sb.Append($"\tpublic partial class {msgName}: IMemory");
                    }
                    else
                    {
                        sb.Append($"\tpublic partial class {msgName}");
                    }
                    if (parentClass != "")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else
                    {
                        sb.Append("\n");
                    }

                    continue;
                }

                if (isMsgStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isMsgStart = false;
                        sb.Append("\t}\n\n");
                        continue;
                    }

                    if (newline.Trim().StartsWith("//"))
                    {
                        sb.AppendLine(newline);
                        continue;
                    }

                    if (newline.Trim() != "" && newline != "}")
                    {
                        if (newline.StartsWith("repeated"))
                        {
                            Repeated(sb, ns, newline);
                        }
                        else
                        {
                            Members(sb, newline, true);
                        }
                    }
                }


                if (newline.StartsWith("enum"))
                {
                    isEnumStart = true;
                    string enumName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];

                    sb.Append($"\t[global::ProtoBuf.ProtoContract()]\n");
                    sb.Append($"\tpublic enum {enumName}");
                    sb.Append("\n");
                    continue;
                }

                if (isEnumStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isEnumStart = false;
                        sb.Append("\t}\n\n");
                        continue;
                    }

                    if (newline.Trim().StartsWith("//"))
                    {
                        sb.AppendLine(newline);
                        continue;
                    }

                    int index = newline.IndexOf(";");
                    newline = newline.Remove(index);
                    sb.Append($"\t\t{newline},\n\n");
                }
            }

            sb.Append("}\n");
            using (FileStream txt = new FileStream(csPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(txt))
                {
                    Log.Debug(sb.ToString());
                    sw.Write(sb.ToString());
                }
            }
        }

        private static void Repeated(StringBuilder sb, string ns, string newline)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                string name = ss[2];
                int n = int.Parse(ss[4]);

                sb.Append($"\t\t[global::ProtoBuf.ProtoMember({n})]\n");
                sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }

        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                default:
                    typeCs = type;
                    break;
            }

            return typeCs;
        }

        private static void Members(StringBuilder sb, string newline, bool isRequired)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[0];
                string name = ss[1];
                int n = int.Parse(ss[3]);
                string typeCs = ConvertType(type);

                sb.Append($"\t\t[global::ProtoBuf.ProtoMember({n})]\n");
                if (string.Equals(type,"string"))
                {
                    sb.Append($"\t\t[global::System.ComponentModel.DefaultValue(\"\")]\n");
                }
                sb.Append($"\t\tpublic {typeCs} {name} {{ get; set; }}\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }
    }
}