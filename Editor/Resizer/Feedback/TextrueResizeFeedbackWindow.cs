using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理缩放处理反馈信息窗口
    /// </summary>
    internal sealed class TextrueResizeFeedbackWindow : EditorWindow
    {
        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="feedbacks">反馈信息</param>
        /// <param name="timeSpan">耗时</param>
        public static void OpenWindow(List<TextrueResizeFeedback> feedbacks, TimeSpan timeSpan)
        {
            if (feedbacks == null || feedbacks.Count <= 0)
            {
                Utility.Log("纹理缩放器：未找到不达标（宽或高不是4的倍数）的纹理！");
                return;
            }

            TextrueResizeFeedbackWindow window = GetWindow<TextrueResizeFeedbackWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("console.infoicon.sml").image;
            window.titleContent.text = "Textrue Resize Feedback";
            window.minSize = window.maxSize = new Vector2(1050, 665);
            window._feedbacks = feedbacks;
            window._timeSpan = timeSpan;
            window.IndexStart = 0;
            window.CalculateTotal();
        }
        
        private List<TextrueResizeFeedback> _feedbacks;
        private TimeSpan _timeSpan;
        private Vector2 _scroll;
        private long _rawStorageMemoryTotal = 0;
        private long _rawRuntimeMemoryTotal = 0;
        private long _storageMemoryTotal = 0;
        private long _runtimeMemoryTotal = 0;
        private long _savedStorageMemoryTotal = 0;
        private long _savedRuntimeMemoryTotal = 0;
        private int _indexStart = 0;
        private int _showNumber = 50;

        /// <summary>
        /// 索引起点
        /// </summary>
        private int IndexStart
        {
            get
            {
                return _indexStart;
            }
            set
            {
                _indexStart = value;
                if (_indexStart + _showNumber > _feedbacks.Count)
                {
                    _indexStart = _feedbacks.Count - _showNumber;
                }
                if (_indexStart < 0)
                {
                    _indexStart = 0;
                }

                IndexCount = _indexStart + _showNumber;
                if (IndexCount > _feedbacks.Count) IndexCount = _feedbacks.Count;
            }
        }
        /// <summary>
        /// 索引终点
        /// </summary>
        private int IndexCount { get; set; } = 0;

        private void OnGUI()
        {
            OnTotalGUI();
            OnDetailGUI();
        }

        private void Update()
        {
            if (_feedbacks == null || _feedbacks.Count <= 0)
            {
                Close();
            }
        }

        /// <summary>
        /// 总值显示GUI
        /// </summary>
        private void OnTotalGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Resized Textrue Count:", GUILayout.Width(200));
            GUILayout.Label(_feedbacks.Count.ToString());
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Export To File"))
            {
                ExportToFile();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Consumed Time:", GUILayout.Width(200));
            GUILayout.Label(_timeSpan.ToString(@"hh\:mm\:ss"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Raw Storage Memory Total:", GUILayout.Width(200));
            GUI.color = Color.red;
            GUILayout.Label(Utility.FormatBytes(_rawStorageMemoryTotal));
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Raw Runtime Memory Total:", GUILayout.Width(200));
            GUI.color = Color.red;
            GUILayout.Label(Utility.FormatBytes(_rawRuntimeMemoryTotal));
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Resized Storage Memory Total:", GUILayout.Width(200));
            GUI.color = Color.cyan;
            GUILayout.Label(Utility.FormatBytes(_storageMemoryTotal));
            GUI.color = Color.green;
            GUILayout.Label(Utility.FormatIncrementBytes(_savedStorageMemoryTotal));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Resized Runtime Memory Total:", GUILayout.Width(200));
            GUI.color = Color.cyan;
            GUILayout.Label(Utility.FormatBytes(_runtimeMemoryTotal));
            GUI.color = Color.green;
            GUILayout.Label(Utility.FormatIncrementBytes(_savedRuntimeMemoryTotal));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 详细信息显示GUI
        /// </summary>
        private void OnDetailGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Texture", GUILayout.Width(160));
            GUILayout.Label("Raw Storage", GUILayout.Width(100));
            GUILayout.Label("Raw Runtime", GUILayout.Width(100));
            GUILayout.Label("Raw Size", GUILayout.Width(100));
            GUILayout.Label("Resized Storage", GUILayout.Width(100));
            GUILayout.Label("Resized Runtime", GUILayout.Width(100));
            GUILayout.Label("Resized Size", GUILayout.Width(100));
            GUILayout.Label("Saved Storage", GUILayout.Width(100));
            GUILayout.Label("Saved Runtime", GUILayout.Width(100));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical("HelpBox");
            Vector2 scroll = GUILayout.BeginScrollView(_scroll);

            for (int i = IndexStart; i < IndexCount; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i + ".", GUILayout.Width(30));
                EditorGUILayout.ObjectField(_feedbacks[i].Value, typeof(Texture2D), false, GUILayout.Width(120));
                GUI.color = Color.red;
                GUILayout.Label(Utility.FormatBytes(_feedbacks[i].RawStorageMemory), GUILayout.Width(100));
                GUILayout.Label(Utility.FormatBytes(_feedbacks[i].RawRuntimeMemory), GUILayout.Width(100));
                GUILayout.Label(_feedbacks[i].RawSize, GUILayout.Width(100));
                GUI.color = Color.cyan;
                GUILayout.Label(Utility.FormatBytes(_feedbacks[i].StorageMemory), GUILayout.Width(100));
                GUILayout.Label(Utility.FormatBytes(_feedbacks[i].RuntimeMemory), GUILayout.Width(100));
                GUILayout.Label(_feedbacks[i].Size, GUILayout.Width(100));
                GUI.color = Color.green;
                GUILayout.Label(Utility.FormatIncrementBytes(_feedbacks[i].SavedStorageMemory), GUILayout.Width(100));
                GUILayout.Label(Utility.FormatIncrementBytes(_feedbacks[i].SavedRuntimeMemory), GUILayout.Width(100));
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            //视野滚动
            if (scroll != _scroll)
            {
                //视野往上滚
                if (scroll.y < _scroll.y)
                {
                    IndexStart -= 1;
                    //如果还未抵达第一个元素，则将视野值下移半个单位（60为一个单位），以防止滚动条抵达最顶部（导致无法再往上滚动视野）
                    if (IndexStart > 0)
                    {
                        scroll.y += 30;
                    }
                }
                //视野往下滚
                else
                {
                    IndexStart += 1;
                    //如果还未抵达最后一个元素，则将视野值上移半个单位（60为一个单位），以防止滚动条抵达最底部（导致无法再往下滚动视野）
                    if (IndexCount < _feedbacks.Count)
                    {
                        scroll.y -= 30;
                    }
                }
                _scroll = scroll;
            }
        }
        
        /// <summary>
        /// 计算总值
        /// </summary>
        private void CalculateTotal()
        {
            for (int i = 0; i < _feedbacks.Count; i++)
            {
                _rawStorageMemoryTotal += _feedbacks[i].RawStorageMemory;
                _rawRuntimeMemoryTotal += _feedbacks[i].RawRuntimeMemory;
                _storageMemoryTotal += _feedbacks[i].StorageMemory;
                _runtimeMemoryTotal += _feedbacks[i].RuntimeMemory;
                _savedStorageMemoryTotal += _feedbacks[i].SavedStorageMemory;
                _savedRuntimeMemoryTotal += _feedbacks[i].SavedRuntimeMemory;
            }
        }

        /// <summary>
        /// 导出数据到文件
        /// </summary>
        private void ExportToFile()
        {
            string path = EditorUtility.SaveFilePanel("Export To File", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TextrueResizeFeedback", "txt");
            if (path != "")
            {
                File.AppendAllText(path, "Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(path, "Resized Textrue Count: " + _feedbacks.Count + "\r\n");
                File.AppendAllText(path, "Consumed Time: " + _timeSpan.ToString(@"hh\:mm\:ss") + "\r\n");
                File.AppendAllText(path, "Raw Storage Memory Total: " + Utility.FormatBytes(_rawStorageMemoryTotal) + "\r\n");
                File.AppendAllText(path, "Raw Runtime Memory Total: " + Utility.FormatBytes(_rawRuntimeMemoryTotal) + "\r\n");
                File.AppendAllText(path, "Resized Storage Memory Total: " + Utility.FormatBytes(_storageMemoryTotal) + "\r\n");
                File.AppendAllText(path, "Resized Runtime Memory Total: " + Utility.FormatBytes(_runtimeMemoryTotal) + "\r\n");
                File.AppendAllText(path, "Saved Storage Memory Total: " + Utility.FormatIncrementBytes(_savedStorageMemoryTotal) + "\r\n");
                File.AppendAllText(path, "Saved Runtime Memory Total: " + Utility.FormatIncrementBytes(_savedRuntimeMemoryTotal) + "\r\n");
                for (int i = 0; i < _feedbacks.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Export To File", _feedbacks[i].Name, (float)i / _feedbacks.Count);
                    File.AppendAllText(path, (i + 1) + "." + _feedbacks[i].ToString() + "\r\n");
                }
                EditorUtility.ClearProgressBar();
            }
        }
    }
}