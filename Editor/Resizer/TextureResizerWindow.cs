using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理缩放器窗口
    /// </summary>
    internal sealed class TextureResizerWindow : EditorWindow
    {
        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void OpenWindow()
        {
            TextureResizerWindow window = GetWindow<TextureResizerWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("ContentSizeFitter Icon").image;
            window.titleContent.text = "Texture Resizer";
            window.minSize = window.maxSize = new Vector2(835, 665);
        }

        private DefaultAsset _folder;
        private string _folderPath;
        private TextureResizer _textureResizer;
        private List<TextureResizeAgent> _agents;
        private Vector2 _scroll;
        private int _rawNumber = 0;
        private int _textureWidth = 120;
        private int _textureCount = 48;
        private int _currentPage = 0;
        private int _totalPage = 0;
        private string _nameFilter = "";
        private bool _isResizePng = true;
        private bool _isResizeJpg = true;
        private bool _isResizeTga = true;
        private GUIContent _nameGC = new GUIContent();
        private GUIContent _standardGC = new GUIContent();
        private GUIContent _sizeGC = new GUIContent();
        private GUIContent _helpGC;

        private void OnEnable()
        {
            _helpGC = new GUIContent();
            _helpGC.image = EditorGUIUtility.IconContent("_Help").image;
            _helpGC.tooltip = "Help";
        }

        private void OnGUI()
        {
            OnTitleGUI();
            OnFolderGUI();
            OnAgentsGUI();
        }

        private void OnDestroy()
        {
            if (_textureResizer != null)
            {
                _textureResizer.Dispose();
                _textureResizer = null;
            }
        }

        /// <summary>
        /// 标题GUI
        /// </summary>
        private void OnTitleGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_helpGC, "IconButton"))
            {
                Application.OpenURL("https://wanderer.blog.csdn.net/article/details/109770759");
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
                _nameFilter = "";
                ResetDisplayAgents();
            }
            GUI.enabled = _textureResizer != null;
            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                _textureResizer.Dispose();
                _textureResizer = null;
                _agents = null;
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
                GUILayout.Label("All Texture In: " + _folderPath);
                GUILayout.FlexibleSpace();
                string nameFilter = EditorGUILayout.TextField("", _nameFilter, "SearchTextField", GUILayout.Width(180));
                if (nameFilter != _nameFilter)
                {
                    _nameFilter = nameFilter;
                    ResetDisplayAgents();
                }
                if (GUILayout.Button("", _nameFilter != "" ? "SearchCancelButton" : "SearchCancelButtonEmpty", GUILayout.Width(20)))
                {
                    if (_nameFilter != "")
                    {
                        _nameFilter = "";
                        ResetDisplayAgents();
                        GUI.FocusControl(null);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Total: [" + _agents.Count + "]");
                GUILayout.Space(20);
                GUILayout.Label("Current Page Raw: [" + _rawNumber + "]");
                GUILayout.FlexibleSpace();
                _isResizePng = GUILayout.Toggle(_isResizePng, "png");
                _isResizeJpg = GUILayout.Toggle(_isResizeJpg, "jpg");
                _isResizeTga = GUILayout.Toggle(_isResizeTga, "tga");
                if (GUILayout.Button("Resize To Multiple Of 4", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog("Prompt", "Are you sure you want to resize to multiple of 4 at all raw texture2d? this is maybe time consuming!", "Yes", "No"))
                    {
                        List<TextrueResizeFeedback> feedbacks = null;
                        TimeSpan timeSpan = Utility.ExecutionInTimeMonitor(() =>
                        {
                            feedbacks = _textureResizer.ResizeToMultipleOf4(_isResizePng, _isResizeJpg, _isResizeTga);
                        });
                        TextrueResizeFeedbackWindow.OpenWindow(feedbacks, timeSpan);
                    }
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginVertical("HelpBox");
                _scroll = GUILayout.BeginScrollView(_scroll);
                _rawNumber = 0;
                for (int i = 0; i < _textureCount; i += 6)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < 6; j++)
                    {
                        int index = _currentPage * _textureCount + i + j;
                        if (index < _agents.Count)
                        {
                            TextureResizeAgent agent = _agents[index];
                            if (agent.Value == null)
                            {
                                RemoveAgent(agent);
                                j--;
                                continue;
                            }
                            bool isStandard = agent.IsMultipleOf4;
                            if (!isStandard) _rawNumber += 1;
                            OnTextureGUI(agent, isStandard);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Prev Page", EditorStyles.miniButton))
                {
                    PrevPage();
                }
                GUILayout.Label((_currentPage + 1) + "/" + _totalPage);
                if (GUILayout.Button("Next Page", EditorStyles.miniButton))
                {
                    NextPage();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                KeyboardEvent();
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
        /// 重置当前显示的纹理代理
        /// </summary>
        private void ResetDisplayAgents()
        {
            if (_agents == null)
            {
                _agents = new List<TextureResizeAgent>();
            }
            _agents.Clear();

            if (_nameFilter == "")
            {
                for (int i = 0; i < _textureResizer.Agents.Count; i++)
                {
                    if (_textureResizer.Agents[i].Value == null)
                    {
                        _textureResizer.Agents.RemoveAt(i);
                        i--;
                        continue;
                    }
                    _agents.Add(_textureResizer.Agents[i]);
                }
            }
            else
            {
                string nameFilter = _nameFilter.ToLower();
                for (int i = 0; i < _textureResizer.Agents.Count; i++)
                {
                    if (_textureResizer.Agents[i].Value == null)
                    {
                        _textureResizer.Agents.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (_textureResizer.Agents[i].Value.name.ToLower().Contains(nameFilter))
                    {
                        _agents.Add(_textureResizer.Agents[i]);
                    }
                }
            }

            ResetPage();
        }

        /// <summary>
        /// 重置当前页数
        /// </summary>
        /// <param name="isResetCurrent">是否重置当前页</param>
        private void ResetPage(bool isResetCurrent = true)
        {
            _totalPage = _agents.Count / _textureCount + (_agents.Count % _textureCount > 0 ? 1 : 0);
            if (isResetCurrent)
            {
                _currentPage = 0;
            }
            else
            {
                _currentPage = _currentPage >= _totalPage ? (_totalPage - 1) : _currentPage;
            }
        }

        /// <summary>
        /// 上一页
        /// </summary>
        private void PrevPage()
        {
            if (_currentPage > 0)
            {
                _currentPage -= 1;
            }
        }

        /// <summary>
        /// 下一页
        /// </summary>
        private void NextPage()
        {
            if (_currentPage < _totalPage - 1)
            {
                _currentPage += 1;
            }
        }

        /// <summary>
        /// 移除纹理代理
        /// </summary>
        /// <param name="agent">纹理代理</param>
        private void RemoveAgent(TextureResizeAgent agent)
        {
            _agents.Remove(agent);
            _textureResizer.Agents.Remove(agent);

            ResetPage(false);
        }

        /// <summary>
        /// 键盘事件处理
        /// </summary>
        private void KeyboardEvent()
        {
            if (Event.current != null && Event.current.rawType == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.LeftArrow:
                        PrevPage();
                        GUI.changed = true;
                        break;
                    case KeyCode.RightArrow:
                        NextPage();
                        GUI.changed = true;
                        break;
                }
            }
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
                        RemoveAgent(agent);
                    });
                    gm.ShowAsContext();
                    Event.current.Use();
                }
            }
        }
    }
}