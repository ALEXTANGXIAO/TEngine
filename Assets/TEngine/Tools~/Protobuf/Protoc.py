#!/usr/bin/env python
# 3.7.7

import codecs, re, os, shutil, sys, operator, parser

#输出类型， 0表示生成Lua和C#，1表示只生成C# 2表示只生成Lua, 默认为2
outputType = 2
if len(sys.argv) > 1:
    outputType = int(sys.argv[1])
isErrorQuit = False  #重名报错是否中断执行
if len(sys.argv) > 2:
    if sys.argv[2] == "True" or sys.argv[2] == "true":
        isErrorQuit = True

print("\noutputType : " + str(outputType))
print("isErrorQuit : " + str(isErrorQuit))

class ProtoBuf:
    def __init__(self):
        self.packageName = ''
        self.messages = []
        self.binary = ''

    def ReadBinary(self, fileName):
        size = os.path.getsize(fileName)
        file = open(fileName, 'rb')
        for _ in range(0, size):
            byte = ord(file.read(1))
            self.binary += '\\x{:0>2X}'.format(byte)
        file.close()

    def ParseProto(self, fileName, dictMsg2Paths):
        data = codecs.open(fileName, 'r', 'utf-8').read()
        # search package name
        matchObj = re.search(r'^\s*package\s+(.*);', data, re.M|re.I)
        if matchObj:
            self.packageName = matchObj.group(1)
        else:
            print('no package define!')
            return
        # search message with specified ID
        results = re.findall(r'message\s+(\w+)\s*{*', data)

        for item in results:
            if item in dictMsg2Paths:  #是否有定义重复的方法
                dictMsg2Paths[item].append(fileName)  
            else:
                dictMsg2Paths[item] = [fileName]
            self.messages.append(item)

#是否生成C#
def IsOutCSharp():
    return outputType == 0 or outputType == 1

#是否生成Lua
def IsOutLua():
    return outputType == 0 or outputType == 2

#判断两个list是否相等
def IsEqual(list1, list2):
    flag = True
    if len(list1) == len(list2):            #长度相等再判断列表里面的元素是否相等
        for i in range(len(list1) - 1):
            if not operator.eq(list1[i], list2[i]):  #是否每个元素都相等
                flag = False
                break
    else:
        flag = False
    return flag

#检测重复的MsgName，并输出错误日志
def CheckRepeatMsgName(dictMsg2Paths):
    dictPaths2Msg = {}
    dictStr2Paths = {}   #字符串与路径列表的字典
    for msg in dictMsg2Paths:
        if len(dictMsg2Paths[msg]) > 1:  #大于一的代表有重复
            curPaths = dictMsg2Paths[msg]
            curPaths.sort()
            isHave = False
            for pathStr in dictPaths2Msg:
                paths = dictStr2Paths[pathStr]
    
                if IsEqual(curPaths, paths):  
                    isHave = True
                    curPaths = paths
    
            pathStr = str(curPaths)
            if pathStr not in dictStr2Paths:
                dictStr2Paths[pathStr] = curPaths
 
            if isHave:   #如果存在，则把msg拼接在后面
                dictPaths2Msg[pathStr] = dictPaths2Msg[pathStr] + "," + msg  
            else:
                dictPaths2Msg[pathStr] = msg

    isExsitRepeatName = False
    for pathStr in dictPaths2Msg:
        isExsitRepeatName = True
        content = f"\n\033[1;31mERROR, 请处理下列文件中重复定义的方法，messages: [" + dictPaths2Msg[pathStr] + "]"
        pathList = []
        for path in dictStr2Paths[pathStr]:
            if path not in pathList:  #去掉重复的path
                pathList.append(path)
                content += "\n" + path
        
        content += "\033[0m"
        print(content)

    if isExsitRepeatName and isErrorQuit:
        quit()
           
#生成Lua     
def OutputLua(filePath, pbArray, gameProtoPath):
    content = '-- THIS FILE IS GENERATED AUTOMATICALLY, DO NOT MODIFY IT\nlocal ProtoBinary =\n{\n'
    content += '    binarys =\n    {\n'
    for pb in pbArray:
        content += '        "%s",\n'%(pb.binary)
    content += '    },\n'

    for pb in pbArray:
        for message in pb.messages:
            content += '    {\n        "%s",\n        "%s"\n    },\n'%(message, '.'.join([pb.packageName, message]))
    content += '}\nreturn ProtoBinary'
    file = codecs.open(filePath, 'w', 'utf-8')
    file.write(content)

    luaPath = gameProtoPath.replace("\\Game\\Protobuf", "\\Game\\LuaScripts")
    if not os.path.exists(luaPath):
        os.mkdir(luaPath)
    copyPath = luaPath + "\\Proto\\ProtoBinary.lua"
    print("\n复制ProtoBinary.lua，path: " + copyPath)
    shutil.copyfile(filePath, copyPath)

#生成C#
def OutputCsharp(files, outputPath, rootPath):
    if os.path.exists(outputPath):
        shutil.rmtree(outputPath)
    os.mkdir(outputPath)
    print("\n开始生成C#文件: ")
    protogenPath = rootPath + "\\ProtobufGenCsharp\\protogen.exe"
    for filePath in files:
        rootPath = os.path.dirname(filePath) 
        fileName = os.path.split(filePath)[-1]
        outPath = outputPath + "\\" + fileName.replace(".proto", ".cs")
        #print(outPath)
        command = protogenPath + ' --proto_path=' + rootPath + ' --csharp_out=' + outputPath + ' ' + fileName
        os.system(command)


#查找指定目录下的所有Proto文件
def FindAllFilesWithSuffix(dirs, suffix = ["proto"]):
    allFiles = []
    print("\n开始从下列文件夹中收集Proto文件， dirs:")
    for dir in dirs:
        print(dir)
        for root, direct, files in os.walk(dir):
            if len(files) < 1:
                continue
            for filesPath in files:
                filesPath = os.path.join(root, filesPath)
                extension = os.path.splitext(filesPath)[1][1:]
                if extension in suffix:
                    allFiles.append(filesPath)
    
    return allFiles

#解析proto文件
def ParseProto(files, rootPath, pbPath):
    fileNames = []
    pbs = []
    dictMsg2Paths = {}
    protocPath = rootPath + "\\protoc.exe"
    if IsOutLua():
        print("\n开始解析Proto文件,并生成对应的PB文件，files:")
    else:
        print("\n开始解析Proto文件，files:")
    for filePath in files:
        print(filePath)
        dirPath = os.path.dirname(filePath) 
        fileName = os.path.split(filePath)[-1]

        pb = ProtoBuf()
        pb.ParseProto(filePath, dictMsg2Paths)
        
        if IsOutLua():   #是否需要生成Lua相关文件
            pbFile =  pbPath + "\\" + fileName.replace(".proto", ".pb")
            print("generated: " + pbFile)
            command = protocPath + ' -I' + dirPath + ' ' + fileName  +' -o' + pbFile
            os.system(command) 
            pb.ReadBinary(pbFile)
        else:
            os.system("")
     
        pbs.append(pb)
        fileName = fileName.split('.')[0]
        fileNames.append(fileName)
    
    CheckRepeatMsgName(dictMsg2Paths)

    return fileNames, pbs
   
def ProtoHandle():
    #path = os.path.realpath(sys.argv[0])
    path = os.path.abspath(__file__)
    rootPath = os.path.dirname(path) 

    #截取项目路径
    if rootPath.__contains__("\\Library\\PackageCache"):
        #projectPath = rootPath + "\\..\\..\\..\\..\\.."
        szStr = rootPath.split("\\")
        projectPath = ""
        for i  in range(0, len(szStr) - 1):
            if i >= len(szStr) - 5:
                break
            projectPath = projectPath + szStr[i] + "\\"
    else:
        szStr = rootPath.split("\\")
        projectPath = ""
        for i  in range(0, len(szStr) - 1):
            if i >= len(szStr) - 4:
                break
            projectPath = projectPath + szStr[i] + "\\"
        #projectPath = rootPath + "\\..\\..\\..\\.."
    print("项目路径：  " + projectPath)

    toolsProtoPath = projectPath + "Tools\\Protobuf"
    gameProtoPath = projectPath + "Assets\\Game\\Protobuf"
    frameProtoPath = projectPath + "Assets\\LuaFramework\\Protobuf"
    pbPath = projectPath + "Build"

    dirs = []
    dirs.append(rootPath)
    dirs.append(toolsProtoPath)
    dirs.append(gameProtoPath)
    dirs.append(frameProtoPath)

    files = FindAllFilesWithSuffix(dirs)   #收集proto文件
    if not os.path.exists(pbPath):  #不存在Build文件夹，则创建
        os.mkdir(pbPath)
    pbPath = pbPath + "\\Pb"
    if os.path.exists(pbPath):  #不存在Build/Pb文件夹，则创建
        shutil.rmtree(pbPath)
    os.mkdir(pbPath)
    fileNames, pbs = ParseProto(files, rootPath, pbPath)     #解析所有Proto文件

    if IsOutLua():
        print("\n开始生成ProtoBinary.lua，fileNames:")
        for name in fileNames:
            print(name)
        OutputLua(pbPath + '\\ProtoBinary.lua', pbs, gameProtoPath)   #生成Lua
    
    if IsOutCSharp():  
        outputPath = projectPath + "Assets\\Game\\Scripts\\Protobuf"
        OutputCsharp(files, outputPath, rootPath)   #生成C#

try:
    ProtoHandle()
except Exception:
    raise Exception("生成ProtoBinary文件异常")

os.system('PAUSE')
