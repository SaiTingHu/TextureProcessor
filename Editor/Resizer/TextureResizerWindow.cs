using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理缩放器窗口
    /// </summary>
    public sealed class TextureResizerWindow : EditorWindow
    {
        private DefaultAsset _folder;
        private string _folderPath;
        private TextureResizer _textureResizer;
        private Vector2 _scroll;
        private int _rawNumber = 0;
        private int _textureWidth = 120;
        private string _nameFilter = "";
        private bool _isDisplayStandard = false;
        private bool _isDisplayRaw = true;
        private GUIContent _nameGC = new GUIContent();
        private GUIContent _standardGC = new GUIContent();
        private GUIContent _sizeGC = new GUIContent();
        
        private void OnGUI()
        {
            OnTitleGUI();
            OnFolderGUI();
            OnAgentsGUI();
        }

        /// <summary>
        /// 标题GUI
        /// </summary>
        private void OnTitleGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("About", EditorStyles.toolbarButton))
            {
                //Application.OpenURL("https://wanderer.blog.csdn.net/article/details/102971712");
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 目标文件夹GUI
        /// </summary>
        private void OnFolderGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Folder:", GUILayout.Width(100));
            _folder = EditorGUILayout.ObjectField(_folder, typeof(DefaultAsset), false) as DefaultAsset;
            GUI.enabled = _folder != null && _textureResizer == null;
            if (GUILayout.Button("Search", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
            {
                _folderPath = AssetDatabase.GetAssetPath(_folder);
                _textureResizer = new TextureResizer(new string[] { _folderPath });
                for (int i = 0; i < _textureResizer.Agents.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Textrue Load", _textureResizer.Agents[i].Path, (float)i / _textureResizer.Agents.Count);
                    _textureResizer.Agents[i].LoadValue();
                }
                EditorUtility.ClearProgressBar();
            }
            GUI.enabled = _textureResizer != null;
            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                _textureResizer.Dispose();
                _textureResizer = null;
                _nameFilter = "";
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 所有纹理代理GUI
        /// </summary>
        private void OnAgentsGUI()
        {
            if (_textureResizer != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("All Texture In");
                GUI.color = Color.cyan;
                GUILayout.Label("[" + _folderPath + "]");
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                _nameFilter = EditorGUILayout.TextField("", _nameFilter, "SearchTextField", GUILayout.Width(180));
                if (GUILayout.Button("", _nameFilter != "" ? "SearchCancelButton" : "SearchCancelButtonEmpty", GUILayout.Width(20)))
                {
                    _nameFilter = "";
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Total - Raw");
                GUI.color = Color.cyan;
                GUILayout.Label("[" + _textureResizer.Agents.Count + " - " + _rawNumber + "]");
                GUI.color = Color.white;
                _isDisplayStandard = GUILayout.Toggle(_isDisplayStandard, "Standard", EditorStyles.miniButtonLeft);
                _isDisplayRaw = GUILayout.Toggle(_isDisplayRaw, "Raw", EditorStyles.miniButtonMid);
                GUI.enabled = _rawNumber > 0;
                if (GUILayout.Button("Resize [" + _rawNumber + "]", EditorStyles.miniButtonRight))
                {
                    if (EditorUtility.DisplayDialog("Prompt", "Are you sure you want to resize to multiple of 4 at all raw texture2d? this is maybe time consuming!", "Yes", "No"))
                    {
                        List<TextrueResizeFeedback> feedbacks = null;
                        TimeSpan timeSpan = Utility.ExecutionInTimeMonitor(() =>
                        {
                            feedbacks = _textureResizer.ResizeToMultipleOf4();
                        });
                        TextrueResizeFeedbackWindow.OpenWindow(feedbacks, timeSpan);
                    }
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                _textureWidth = EditorGUILayout.IntSlider(_textureWidth, 60, 180, GUILayout.Width(200));
                GUILayout.EndHorizontal();
                
                GUILayout.BeginVertical("HelpBox");
                _scroll = GUILayout.BeginScrollView(_scroll);

                int column = (int)(position.width / (_textureWidth + 10));
                int indexOffset = 0;
                _rawNumber = 0;
                for (int i = 0; i < _textureResizer.Agents.Count; i += column)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < column; j++)
                    {
                        int index = i + j + indexOffset;
                        if (index < _textureResizer.Agents.Count)
                        {
                            TextureResizeAgent agent = _textureResizer.Agents[index];
                            if (agent.Value == null)
                            {
                                _textureResizer.Agents.RemoveAt(index);
                                j--;
                                continue;
                            }
                            bool isStandard = agent.IsMultipleOf4;
                            if (!isStandard) _rawNumber += 1;
                            if (!IsDisplay(agent, isStandard))
                            {
                                j--;
                                indexOffset++;
                                continue;
                            }
                            OnTextureGUI(agent, isStandard);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a target folder, and click 'Search' button to process all texture in target folder!", MessageType.Info);
            }
        }

        /// <summary>
        /// 纹理代理GUI
        /// </summary>
        /// <param name="agent">纹理代理</param>
        /// <param name="isStandard">是否为标准纹理</param>
        private void OnTextureGUI(TextureResizeAgent agent, bool isStandard)
        {
            float widthHalf = _textureWidth * 0.5f;

            GUI.color = isStandard ? Color.white : Color.red;
            GUILayout.BeginHorizontal("Box", GUILayout.Width(_textureWidth), GUILayout.Height(widthHalf));

            EditorGUILayout.ObjectField(agent.Value, typeof(Texture2D), false, GUILayout.Width(widthHalf), GUILayout.Height(widthHalf));

            GUILayout.BeginVertical();
            _nameGC.text = agent.Value.name;
            _nameGC.tooltip = agent.Path;
            GUILayout.Label(_nameGC, GUILayout.Width(widthHalf));
            _standardGC.image = isStandard ? null : EditorGUIUtility.IconContent("console.warnicon.sml").image;
            _standardGC.text = isStandard ? "Standard" : "Raw";
            _standardGC.tooltip = isStandard ? "This texture is standard!" : "This texture is raw!you can resize it to multiple of 4!";
            GUILayout.Label(_standardGC, GUILayout.Width(widthHalf));
            _sizeGC.text = agent.Size;
            _sizeGC.tooltip = agent.Size;
            GUILayout.Label(_sizeGC, GUILayout.Width(widthHalf));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUI.color = Color.white;
            
            if (!isStandard)
            {
                MouseRightMenu(agent, GUILayoutUtility.GetLastRect());
            }
        }

        /// <summary>
        /// 是否显示纹理代理
        /// </summary>
        /// <param name="agent">纹理代理</param>
        /// <param name="isStandard">是否为标准纹理</param>
        /// <returns>是否显示</returns>
        private bool IsDisplay(TextureResizeAgent agent, bool isStandard)
        {
            if (!_isDisplayStandard && isStandard)
            {
                return false;
            }
            if (!_isDisplayRaw && !isStandard)
            {
                return false;
            }
            return agent.Value.name.Contains(_nameFilter);
        }

        /// <summary>
        /// 纹理代理的鼠标右键点击菜单
        /// </summary>
        /// <param name="agent">纹理代理</param>
        /// <param name="rect">纹理代理的显示区域</param>
        private void MouseRightMenu(TextureResizeAgent agent, Rect rect)
        {
            if (Event.current != null && Event.current.rawType == EventType.MouseDown && Event.current.button == 1)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    GenericMenu gm = new GenericMenu();
                    gm.AddItem(new GUIContent("Exclude this texture"), false, () =>
                    {
                        _textureResizer.Agents.Remove(agent);
                    });
                    gm.ShowAsContext();
                    Event.current.Use();
                }
            }
        }
    }
}