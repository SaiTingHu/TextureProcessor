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

        private void OnEnable()
        {
            
        }

        private void OnGUI()
        {
            OnTitleGUI();
            OnFolderGUI();
            OnAgentsGUI();
        }

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
                    _textureResizer.Agents[i].LoadValue();
                }
            }
            GUI.enabled = _textureResizer != null;
            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                _textureResizer.Dispose();
                _textureResizer = null;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void OnAgentsGUI()
        {
            if (_textureResizer != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("All Texture In [" + _folderPath + "]");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("Box");
                _scroll = GUILayout.BeginScrollView(_scroll);

                for (int i = 0; i < _textureResizer.Agents.Count; i += 4)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = i + j;
                        if (index < _textureResizer.Agents.Count)
                        {
                            TextureResizeAgent agent = _textureResizer.Agents[index];
                            if (agent.Value == null)
                            {
                                _textureResizer.Agents.RemoveAt(index);
                                j--;
                                continue;
                            }
                            OnTextureGUI(agent);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }

        private void OnTextureGUI(TextureResizeAgent agent)
        {
            EditorGUILayout.ObjectField(agent.Value, typeof(Texture2D), false, GUILayout.Width(60), GUILayout.Height(60));

            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label(agent.Value.name, GUILayout.Width(60));
            GUILayout.Label(agent.IsMultipleOf4.ToString(), GUILayout.Width(60));
            GUILayout.EndVertical();
        }
    }
}