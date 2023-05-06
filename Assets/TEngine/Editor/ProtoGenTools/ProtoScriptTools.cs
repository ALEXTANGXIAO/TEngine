using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using TEngine;
using TEngine.Editor;
using Debug = UnityEngine.Debug;

class GenNetScriptWindow : EditorWindow
{
    private static GenNetScriptWindow _window = null;

    //绑定通知协议
    private StringBuilder _strBindNotify;

    //枚举对应协议号
    private readonly Dictionary<string, int> _dicName2ID = new Dictionary<string, int>();
    
    //xxx.xml下的协议枚举list
    private readonly Dictionary<string, List<string>> _dicPath2Name = new Dictionary<string, List<string>>();

    //协议号有没有被勾选
    private readonly Dictionary<int, bool> _dicID2Select = new Dictionary<int, bool>();
    
    //记录回包协议号
    private readonly Dictionary<int, int> _dicID2ID = new Dictionary<int, int>();

    private Vector2 _scrollPos;
    private Vector2 _fileListScrollPos;

    private string _path = @"G:\github\TEngine\Luban\Proto\pb_schemas\";
    private readonly List<string> _filePathList = new List<string>();
    private string _curSelectFile = string.Empty;

    private string _filterFileName = string.Empty;
    private string _filterProName = string.Empty;

    [MenuItem("TEngine/协议生成工具|Protobuf Tools")]
    static void OpenGenNetScriptWindow()
    {
        if (!_window)
        {
            _window = ScriptableObject.CreateInstance<GenNetScriptWindow>();
            _window.maxSize = new Vector2(1000, 800);
            _window.minSize = _window.maxSize / 2;
            _window.LoadLastPath();
        }

        _window.ShowUtility();
    }

    void OnGUI()
    {
        EditorGUILayout.PrefixLabel("protoPath");
        _path = EditorGUILayout.TextField(_path);
        var r = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r, GUIContent.none))
        {
            ReadPath();
        }
        GUILayout.Label("Search");
        EditorGUILayout.EndHorizontal();


        //加个文件筛选
        //EditorGUILayout.BeginHorizontal();
        GUILayout.Label("filter file:");
        _filterFileName = GUILayout.TextField(_filterFileName);
        //EditorGUILayout.EndHorizontal();
        //显示文件名部分
        if (_filePathList.Count > 0)
        {
            _fileListScrollPos = EditorGUILayout.BeginScrollView(_fileListScrollPos, GUILayout.Width(r.width), GUILayout.Height(200));
            for (int i = 0; i < _filePathList.Count; ++i)
            {
                var fileName = Path.GetFileNameWithoutExtension(_filePathList[i]);
                if (!string.IsNullOrEmpty(_filterFileName) && fileName.IndexOf(_filterFileName, StringComparison.Ordinal) == -1)
                {
                    continue;
                }
                if (GUILayout.Button(fileName))
                {
                    _curSelectFile = _filePathList[i];
                    LoadSelectFile();
                }
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.Label("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

        if (!string.IsNullOrEmpty(_curSelectFile))
        {
            //加个协议筛选
            GUILayout.Label("filter proto:");
            _filterProName = GUILayout.TextField(_filterProName);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(r.width), GUILayout.Height(200));
            var fileName2 = Path.GetFileNameWithoutExtension(_curSelectFile);
            List<string> list;
            if (_dicPath2Name.TryGetValue(fileName2, out list))
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < list.Count; ++i)
                {
                    var cmdName = list[i];
                    //筛选一下,忽略大小写
                    if (!string.IsNullOrEmpty(_filterProName) && cmdName.ToLower().IndexOf(_filterProName.ToLower()) == -1)
                        continue;
                    var cmdID = _dicName2ID[cmdName];
                    EditorGUILayout.BeginHorizontal(GUIStyle.none);
                    //协议名
                    EditorGUILayout.LabelField(cmdName);
                    //toggle
                    if (!_dicID2Select.ContainsKey(cmdID))
                        _dicID2Select[cmdID] = false;
                    _dicID2Select[cmdID] = EditorGUILayout.Toggle(cmdID.ToString(),_dicID2Select[cmdID]);
                    //回包协议号
                    if (!_dicID2ID.ContainsKey(cmdID))
                    {
                        if (cmdName.EndsWith("REQ"))
                            _dicID2ID[cmdID] = cmdID + 1;
                        else
                            _dicID2ID[cmdID] = 0;
                    }
                    _dicID2ID[cmdID] = EditorGUILayout.IntField(_dicID2ID[cmdID]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();
        }

        if (!string.IsNullOrEmpty(_curSelectFile))
        {
            if (GUILayout.Button("GenSelect"))
            {
                OnClickGenBtn(false);
            }
            if (GUILayout.Button("GenAll"))
            {
                OnClickGenBtn(true);
            }
        }

        if (GUILayout.Button("导出Proto To Csharp|Export Proto To Csharp"))
        {
            ExportProto();
        }
    }

    #region 加载
    private void LoadLastPath()
    {
        if (PlayerPrefs.HasKey("GenNetScriptWindow.Path"))
        {
            _path = PlayerPrefs.GetString("GenNetScriptWindow.Path");
        }
        ReadPath();
    }

    private void ReadPath()
    {
        PlayerPrefs.SetString("GenNetScriptWindow.Path", _path);
        _filePathList.Clear();
        _curSelectFile = String.Empty;
        JustLoadFileList(_filePathList, _path);
    }

    private void JustLoadFileList(List<string> exportList, string folderPath, bool deep = false)
    {
        if (!LoadFoldChildFileList(exportList, folderPath, deep))
        {
            EditorUtility.DisplayDialog("folder not exist", "folder not exist:"+_path, "ok");
        }
    }

    private void LoadSelectFile()
    {
        _dicID2Select.Clear();
        _dicID2ID.Clear();
        _dicName2ID.Clear();

        var xmlFilePath = _curSelectFile;
        var fileName = Path.GetFileNameWithoutExtension(xmlFilePath);
        var protocolNameList = new List<string>();
        //读xml
        if (fileName.StartsWith("proto_cs"))
        {
            Debug.Log("load xml.name:" + fileName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            GenProtocolNameList(xmlDoc.ChildNodes, protocolNameList);
            _dicPath2Name[fileName] = protocolNameList;
            Debug.Log(fileName + " success.");
        }
    }

    private bool LoadFoldChildFileList(List<string> exportList, string folderPath, bool deep = false)
    {
        if (!Directory.Exists(folderPath))
        {
            Log.Error("folder not exist: {0}", folderPath);
            return false;
        }

        string[] subFile = Directory.GetFiles(folderPath);
        foreach (string fileName in subFile)
        {
            //有些筛选条件，直接写死这里了。方便
            var name = Path.GetFileNameWithoutExtension(fileName);
            if (name.StartsWith("proto_cs"))
                exportList.Add(fileName);
        }

        if (deep)
        {
            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string folderName in subFolders)
            {
                LoadFoldChildFileList(exportList, folderName);
            }
        }

        return true;
    }

    //把macro里的协议号和协议枚举名对应起来
    private void GenProtocolNameList(XmlNodeList nodeList, List<string> nameList)
    {
        foreach (XmlNode node in nodeList)
        {
            if (node.Attributes == null)
                continue;
            if (node.Name == "macro")
            {
                var name = node.Attributes.GetNamedItem("name").Value;
                if (name.StartsWith("CS_CMD") || name.StartsWith("CS_NOTIFY"))
                {
                    var id = Convert.ToInt32(node.Attributes.GetNamedItem("value").Value);
                    _dicName2ID[name] = id;
                    if (nameList != null)
                        nameList.Add(name);
                }
            }
            GenProtocolNameList(node.ChildNodes, nameList);
        }
    }
    #endregion

    #region 点击处理
    private void OnClickGenBtn(bool genAll)
    {
        _strBindNotify = new StringBuilder();
        bool needRegCmdHandle = false;
        StringBuilder sb = new StringBuilder();
        List<int> listGenId = new List<int>();
        var iter = _dicID2Select.GetEnumerator();
        while (iter.MoveNext())
        {
            if (iter.Current.Value || genAll)
            {
                int resId = iter.Current.Key;
                int resID;
                if (_dicID2ID.TryGetValue(resId, out resID))
                {
                    if (resID != 0)
                    {
                        var oneStr = GenOneReq(resId, resID);
                        sb.Append(oneStr);
                        sb.Append("\n\n");
                        listGenId.Add(resId);
                        listGenId.Add(resID);
                    }
                    else if (!listGenId.Contains(resId))
                    {
                        needRegCmdHandle = true;
                        var oneStr = GenOneReq(0, resId);
                        sb.Append(oneStr);
                        sb.Append("\n\n");
                        listGenId.Add(resId);
                    }
                }
            }
        }
        
        if (needRegCmdHandle)
        {
            sb.Append("public void RegCmdHandle()\n");
            sb.Append("{\n");
            sb.Append(_strBindNotify);
            sb.Append("}\n");
        }

        TextEditor te = new TextEditor();
        te.text = sb.ToString();
        te.SelectAll();
        te.Copy();
    }

    private static readonly Dictionary<string, string> TempParamDic = new Dictionary<string, string>();
    private static string _reqClassName = string.Empty;
    private static string _resClassName = string.Empty;
    private static string _reqEnumName = string.Empty;
    private static string _resEnumName = string.Empty;
    private static string _reqDesc = string.Empty;
    private static bool _resResult;

    private string GenOneReq(int reqId, int resId)
    {
        TempParamDic.Clear();
        _reqClassName = string.Empty;
        _resClassName = string.Empty;
        _reqEnumName = string.Empty;
        _resEnumName = string.Empty;
        _resResult = false;

        var xmlFilePath = _curSelectFile;
        var fileName = Path.GetFileNameWithoutExtension(xmlFilePath);
        //读xml
        if (fileName.StartsWith("proto_cs"))
        {
            Debug.Log("load xml.name:" + fileName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            _GenOneReq(xmlDoc.ChildNodes, reqId,resId);
            Debug.Log(fileName + " success.");

            var sb = new StringBuilder();
            sb.Append("#region ");
            sb.Append(_reqDesc);
            sb.Append(string.Format("\n//{0}\n", _reqDesc));
            if (reqId != 0)
            {
                sb.Append("public void ");
                sb.Append(_reqClassName.Substring(2));
                sb.Append("(");
                foreach (var item in TempParamDic)
                {
                    sb.Append(string.Format("{0} {1}, ", item.Value, item.Key));
                }
                sb.Remove(sb.Length - 2, 2);//把多余的逗号和空格删了
                sb.Append(")\n{\n");
                sb.Append(string.Format("\tCSPkg reqPkg = ProtoUtil.BuildCSMsg(netMacros.{0});\n", _reqEnumName));
                sb.Append(string.Format("\t{0} reqData = reqPkg.Body.{1};\n", _reqClassName, _reqClassName.Substring(2)));
                foreach (var item in TempParamDic)
                {
                    sb.Append(string.Format("\treqData.{0} = {0};\n", item.Key));
                }

                sb.Append("\n");
                sb.Append(string.Format("\tGameClient.Instance.SendCSMsg(reqPkg, netMacros.{0}, {1});\n", _resEnumName,
                    _resClassName.Substring(2)));
                sb.Append("}\n\n");
            }
            else
            {
                _strBindNotify.Append(string.Format("\t\tGameClient.Instance.RegCmdHandle(netMacros.{0}, {1});\n", _resEnumName, _resClassName.Substring(2)));
            }

            //回包
            sb.Append(string.Format("private void {0}(CSMsgResult result, CSPkg msg)\n", _resClassName.Substring(2)));
            sb.Append("{\n");
            sb.Append(string.Format("\tif (DodUtil.CheckHaveError(result, msg, typeof({0})))\n", _resClassName));
            sb.Append("\t\treturn;\n\n");
            sb.Append(string.Format("\t{0} resData = msg.Body.{1};\n", _resClassName, _resClassName.Substring(2)));
            if (_resResult)
            {
                sb.Append("\tif (resData.Result.Ret != 0)\n");
                sb.Append("\t{\n");
                sb.Append("\t\tUISys.Mgr.ShowTipMsg(resData.Result);\n");
                sb.Append("\t\treturn;\n");
                sb.Append("\t}\n");
            }

            sb.Append("\t//todo\n");
            sb.Append("}\n#endregion");
            return sb.ToString();
        }

        return null;
    }

    private void _GenOneReq(XmlNodeList nodeList, int reqId,int resId)
    {
        foreach (XmlNode node in nodeList)
        {
            if (node.Attributes == null)
                continue;
            if (node.Name.Equals("macro"))
            {
                var name = node.Attributes.GetNamedItem("name").Value;
                if (name.StartsWith("CS_CMD") || name.StartsWith("CS_NOTIFY"))
                {
                    var id = Convert.ToInt32(node.Attributes.GetNamedItem("value").Value);
                    if (id == reqId)
                        _reqEnumName = name;
                    if (id == resId)
                        _resEnumName = name;
                }
            }

            if (node.Name.Equals("struct"))
            {
                if (node.Attributes.GetNamedItem("id") == null)
                    continue;
                var enumName = node.Attributes.GetNamedItem("id").Value;
                if (enumName.Equals(_reqEnumName))
                {
                    var name = node.Attributes.GetNamedItem("name").Value;
                    _reqClassName = name;
                    if (node.Attributes.GetNamedItem("name") != null)
                    {
                        _reqDesc = node.Attributes.GetNamedItem("desc").Value;
                    }
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode != null && childNode.Name.Equals("entry"))
                        {
                            var paramName = childNode.Attributes.GetNamedItem("name").Value;
                            var paramType = childNode.Attributes.GetNamedItem("type").Value;
                            TempParamDic.Add(paramName, paramType);
                        }
                    }
                }

                if (enumName.Equals(_resEnumName))
                {
                    var className = node.Attributes.GetNamedItem("name").Value;
                    _resClassName = className;
                }
            }
            _GenOneReq(node.ChildNodes, reqId, resId);
        }
    }
    #endregion

    #region 导出协议ToCsharp

    private void ExportProto()
    {
        ProtoGenTools.Export();
    }
    #endregion
}