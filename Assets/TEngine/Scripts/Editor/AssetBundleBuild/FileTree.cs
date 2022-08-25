using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TEngineCore.Editor
{
    public class FileTree : IDisposable
    {
        private bool _disposed = false;
        private string[] _extIncludeFilter;
        private string[] _extExcludeFilter;

        private List<FileInfo> _files = new List<FileInfo>();
        private DirectoryInfo[] _subFolders;
        private DirectoryInfo _directoryInfo;
        private FileTree[] _subTrees;
        private bool _isLeafFolder;
        private FileTree _parent;

        public string Name
        {
            get
            {
                return _directoryInfo.Name;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (null != _subTrees)
                {
                    for (var i = 0; i < _subTrees.Length; ++i)
                    {
                        _subTrees[i].Dispose();
                    }
                    _subTrees = null;
                }
            }
        }
        ~FileTree()
        {
            Dispose();
        }

        private FileTree() { }

        public static FileTree CreateWithIncludeFilter(string rootPath, string[] includeFilter = null)
        {
            FileTree fileTree = new FileTree();
            fileTree._extIncludeFilter = includeFilter;
            fileTree._directoryInfo = new DirectoryInfo(rootPath);
            fileTree.MakeSubTree();

            return fileTree;
        }

        public static FileTree CreateWithExcludeFilter(string rootPath, string[] excludeFilter = null)
        {
            FileTree fileTree = new FileTree();
            fileTree._extExcludeFilter = excludeFilter;
            fileTree._directoryInfo = new DirectoryInfo(rootPath);
            fileTree.MakeSubTree();

            return fileTree;
        }

        public static FileTree CreateWithIncludeFilter(DirectoryInfo directoryInfo, string[] includeFilter = null)
        {
            if (directoryInfo != null)
            {
                FileTree fileTree = new FileTree();
                fileTree._extIncludeFilter = includeFilter;
                fileTree._directoryInfo = directoryInfo;
                fileTree.MakeSubTree();

                return fileTree;
            }
            else
            {
                Debug.LogError("Create FileTree with null DirectoryInfo");
                return null;
            }
        }

        public static FileTree CreateWithExcludeFilter(DirectoryInfo directoryInfo, string[] excludeFilter = null)
        {
            if (directoryInfo != null)
            {
                FileTree fileTree = new FileTree();
                fileTree._extExcludeFilter = excludeFilter;
                fileTree._directoryInfo = directoryInfo;
                fileTree.MakeSubTree();

                return fileTree;
            }
            else
            {
                Debug.LogError("Create FileTree with null DirectoryInfo");
                return null;
            }
        }

        private void FiltFiles(FileInfo[] vFiles)
        {
            _files.Clear();

            FileInfo curFileInfo;
            for (var i = 0; i < vFiles.Length; ++i)
            {
                curFileInfo = vFiles[i];

                if (_extIncludeFilter != null)
                {
                    for (int f = 0; f < _extIncludeFilter.Length; ++f)
                    {
                        if (_extIncludeFilter[f] == curFileInfo.Extension)
                        {
                            _files.Add(curFileInfo);
                            break;
                        }
                    }
                }
                else if (_extExcludeFilter != null)
                {
                    bool filtered = false;
                    for (int f = 0; f < _extExcludeFilter.Length; ++f)
                    {
                        if (_extExcludeFilter[f] == curFileInfo.Extension)
                            filtered = true;
                    }
                    if (!filtered) this._files.Add(curFileInfo);
                }
                else
                {
                    _files.Add(curFileInfo);
                }
            }
        }

        private void MakeSubTree()
        {
            FiltFiles(_directoryInfo.GetFiles());
            _subFolders = _directoryInfo.GetDirectories();
            _isLeafFolder = (_subFolders == null || _subFolders.Length == 0);
            if (!_isLeafFolder)
            {
                _subTrees = new FileTree[_subFolders.Length];
                for (var i = 0; i < _subFolders.Length; ++i)
                {
                    var subDir = _subFolders[i];
                    if (_extIncludeFilter != null)
                        _subTrees[i] = CreateWithIncludeFilter(subDir, _extIncludeFilter);
                    else
                        _subTrees[i] = CreateWithExcludeFilter(subDir, _extExcludeFilter);
                    _subTrees[i]._parent = this;
                }
            }
        }

        public List<FileTree> GetTotalLeafFolders(List<FileTree> ret = null)
        {
            if (null == ret)
            {
                ret = new List<FileTree>();
            }
            if (_isLeafFolder)
            {
                ret.Add(this);
            }
            else
            {
                for (var i = 0; i < _subTrees.Length; ++i)
                {
                    _subTrees[i].GetTotalLeafFolders(ret);
                }
            }

            return ret;
        }

        public List<FileInfo> GetAllFiles(List<FileInfo> ret = null)
        {
            if (ret == null)
                ret = new List<FileInfo>();

            ret.AddRange(this._files);
            if (!_isLeafFolder)
            {
                for (int i = 0; i < _subTrees.Length; ++i)
                {
                    _subTrees[i].GetAllFiles(ret);
                }
            }

            return ret;
        }

        /// <summary>
        /// 测试接口，用来查看信息是否正确
        /// </summary>
        public StringBuilder GetInfo(bool printSubFolder = false, StringBuilder builder = null)
        {
            if (null == builder)
            {
                builder = new StringBuilder(1024 * 8);
            }
            builder.AppendLine("文件夹：" + _directoryInfo.FullName);
            if (_files.Count > 0)
            {
                builder.AppendLine("子文件数量：" + _files.Count);
                for (var i = 0; i < _files.Count; ++i)
                {
                    builder.AppendLine("\t第" + (i + 1) + "个文件： " + _files[i].FullName);
                }
            }
            if (_isLeafFolder)
            {
                builder.Append("\t\t isLeafFolder");
            }
            else
            {
                builder.AppendLine("子文件夹数量：" + _subFolders.Length);
                for (var i = 0; i < _subFolders.Length; ++i)
                {
                    builder.AppendLine("\t第" + (i + 1) + "个文件夹： " + _subFolders[i].FullName);
                }
                if (printSubFolder)
                {
                    for (var i = 0; i < _subTrees.Length; ++i)
                    {
                        _subTrees[i].GetInfo(true, builder);
                    }
                }
            }

            builder.AppendLine();

            return builder;
        }
    }
}