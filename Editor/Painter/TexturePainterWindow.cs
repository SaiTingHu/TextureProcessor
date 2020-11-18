using UnityEditor;
using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理绘画器窗口
    /// </summary>
    internal sealed class TexturePainterWindow : EditorWindow
    {
        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void OpenWindow()
        {
            TexturePainterWindow window = GetWindow<TexturePainterWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("ContentSizeFitter Icon").image;
            window.titleContent.text = "Texture Painter";
        }

        private TexturePainter _texturePainter;
        private bool _isTextureDragging = false;
        private float _textureScale = 1;
        private Rect _textureRect = Rect.zero;
        private Rect _textureBGRect = Rect.zero;
        private Rect _toolkitRect = new Rect(10, 30, 120, 230);
        private GUIContent _helpGC;
        private Texture2D _textureBG;
        
        private bool _isAdjustBrightness = false;
        private Rect _adjustBrightnessRect = new Rect(135, 150, 160, 60);
        private float _brightness = 1;

        private bool _isAdjustSaturation = false;
        private Rect _adjustSaturationRect = new Rect(135, 170, 160, 60);
        private float _saturation = 0;

        private bool _isAdjustValue = false;
        private Rect _adjustValueRect = new Rect(135, 190, 160, 60);
        private float _value = 0;

        /// <summary>
        /// 当前的绘画器是否是空的
        /// </summary>
        private bool IsNull
        {
            get
            {
                return _texturePainter == null || _texturePainter.IsNull;
            }
        }

        private void OnEnable()
        {
            _helpGC = new GUIContent();
            _helpGC.image = EditorGUIUtility.IconContent("_Help").image;
            _helpGC.tooltip = "Help";
            _textureBG = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/HT/TextureProcessor/Editor/Texture/黑白格子.png");
        }

        private void OnGUI()
        {
            OnPainterGUI();
            OnTitleGUI();
            OnDragTexture();
            OnEventHandler();
        }

        private void OnDestroy()
        {
            if (_texturePainter != null)
            {
                _texturePainter.Dispose();
                _texturePainter = null;
            }
        }

        /// <summary>
        /// 绘画GUI
        /// </summary>
        private void OnPainterGUI()
        {
            if (IsNull)
                return;

            _textureRect.Set(_texturePainter.Anchor.x, _texturePainter.Anchor.y, _texturePainter.PaintValue.width * _textureScale, _texturePainter.PaintValue.height * _textureScale);
            _textureBGRect.Set(0, 0, _textureRect.width / 32, _textureRect.height / 32);
            GUI.DrawTextureWithTexCoords(_textureRect, _textureBG, _textureBGRect);
            GUI.DrawTexture(_textureRect, _texturePainter.PaintValue);

            #region Toolkit
            GUILayout.BeginArea(_toolkitRect, "Toolkit", "Window");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Size: " + _texturePainter.PaintValue.width + "x" + _texturePainter.PaintValue.height);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Multiple Of 4:");
            GUILayout.FlexibleSpace();
            GUILayout.Toggle(_texturePainter.IsMultipleOf4, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Alpha:");
            GUILayout.FlexibleSpace();
            GUILayout.Toggle(_texturePainter.Format != FileFormat.JPG, "");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUI.enabled = !_texturePainter.IsMultipleOf4;
            if (GUILayout.Button("Fix Multiple Of 4", EditorStyles.miniButton))
            {
                _texturePainter.FixMultipleOf4();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = _texturePainter.Format != FileFormat.JPG;
            if (GUILayout.Button("Cut Blank Pixels", EditorStyles.miniButton))
            {
                _texturePainter.CutBlankPixels();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Grayscale", EditorStyles.miniButton))
            {
                _texturePainter.Grayscale();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            bool value = _isAdjustBrightness;
            if (GUILayout.Toggle(_isAdjustBrightness, "Brightness", EditorStyles.miniButton) != value)
            {
                _isAdjustBrightness = true;

                if (_isAdjustSaturation)
                {
                    _texturePainter.AdjustSaturationRestore();
                    _isAdjustSaturation = false;
                    _saturation = 0;
                }

                if (_isAdjustValue)
                {
                    _texturePainter.AdjustValueRestore();
                    _isAdjustValue = false;
                    _value = 0;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            value = _isAdjustSaturation;
            if (GUILayout.Toggle(_isAdjustSaturation, "Saturation", EditorStyles.miniButton) != value)
            {
                _isAdjustSaturation = true;

                if (_isAdjustBrightness)
                {
                    _texturePainter.AdjustBrightnessRestore();
                    _isAdjustBrightness = false;
                    _brightness = 1;
                }

                if (_isAdjustValue)
                {
                    _texturePainter.AdjustValueRestore();
                    _isAdjustValue = false;
                    _value = 0;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            value = _isAdjustValue;
            if (GUILayout.Toggle(_isAdjustValue, "Value", EditorStyles.miniButton) != value)
            {
                _isAdjustValue = true;

                if (_isAdjustSaturation)
                {
                    _texturePainter.AdjustSaturationRestore();
                    _isAdjustSaturation = false;
                    _saturation = 0;
                }

                if (_isAdjustBrightness)
                {
                    _texturePainter.AdjustBrightnessRestore();
                    _isAdjustBrightness = false;
                    _brightness = 1;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("MirrorLR", EditorStyles.miniButtonLeft))
            {
                _texturePainter.LeftRightMirror();
            }
            if (GUILayout.Button("MirrorTB", EditorStyles.miniButtonRight))
            {
                _texturePainter.TopBottomMirror();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.yellow;
            GUI.enabled = !Mathf.Approximately(_textureScale, 1f);
            if (GUILayout.Button("Original Scale", EditorStyles.miniButton))
            {
                _textureScale = 1;
                _texturePainter.Anchor = position.center - position.position - new Vector2(_texturePainter.PaintValue.width * _textureScale / 2, _texturePainter.PaintValue.height * _textureScale / 2);
                GUI.changed = true;
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Clear", EditorStyles.miniButton))
            {
                if (_texturePainter != null)
                {
                    _texturePainter.Dispose();
                    _texturePainter = null;
                    GUI.changed = true;
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            #endregion

            #region Adjust Brightness
            if (_isAdjustBrightness)
            {
                GUILayout.BeginArea(_adjustBrightnessRect, "Brightness", "Window");

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                float brightness = EditorGUILayout.Slider(_brightness, 0.1f, 1.9f);
                if (EditorGUI.EndChangeCheck())
                {
                    _brightness = brightness;
                    _texturePainter.AdjustBrightness(_brightness);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Sure", EditorStyles.miniButtonLeft))
                {
                    _texturePainter.AdjustBrightnessSave();
                    _isAdjustBrightness = false;
                    _brightness = 1;
                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButtonRight))
                {
                    _texturePainter.AdjustBrightnessRestore();
                    _isAdjustBrightness = false;
                    _brightness = 1;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
            #endregion

            #region Adjust Saturation
            if (_isAdjustSaturation)
            {
                GUILayout.BeginArea(_adjustSaturationRect, "Saturation", "Window");

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                float saturation = EditorGUILayout.Slider(_saturation, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    _saturation = saturation;
                    _texturePainter.AdjustSaturation(_saturation);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Sure", EditorStyles.miniButtonLeft))
                {
                    _texturePainter.AdjustSaturationSave();
                    _isAdjustSaturation = false;
                    _saturation = 0;
                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButtonRight))
                {
                    _texturePainter.AdjustSaturationRestore();
                    _isAdjustSaturation = false;
                    _saturation = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
            #endregion

            #region Adjust Value
            if (_isAdjustValue)
            {
                GUILayout.BeginArea(_adjustValueRect, "Value", "Window");

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                float ivalue = EditorGUILayout.Slider(_value, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    _value = ivalue;
                    _texturePainter.AdjustValue(_value);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Sure", EditorStyles.miniButtonLeft))
                {
                    _texturePainter.AdjustValueSave();
                    _isAdjustValue = false;
                    _value = 0;
                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButtonRight))
                {
                    _texturePainter.AdjustValueRestore();
                    _isAdjustValue = false;
                    _value = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
            #endregion
        }

        /// <summary>
        /// 标题GUI
        /// </summary>
        private void OnTitleGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (!IsNull)
            {
                if (GUILayout.Button(_texturePainter.Path, EditorStyles.toolbarButton))
                {
                    Selection.activeObject = _texturePainter.Value;
                    EditorGUIUtility.PingObject(_texturePainter.Value);
                }
            }
            GUILayout.FlexibleSpace();
            if (!IsNull)
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("Prompt", "Are you sure you want to save the texture? It will override the original texture!", "Yes", "No"))
                    {
                        _texturePainter.Save();
                    }
                }
                if (GUILayout.Button("Save As...", EditorStyles.toolbarButton))
                {
                    string path = EditorUtility.SaveFilePanel("Save As Texture", Application.dataPath, "NewTextrue", _texturePainter.Format.ToString());
                    if (path != "")
                    {
                        _texturePainter.SaveAs(path);
                    }
                }
            }
            if (GUILayout.Button(_helpGC, "IconButton"))
            {
                Application.OpenURL("https://wanderer.blog.csdn.net/article/details/109770759");
            }
            GUILayout.EndHorizontal();

            if (IsNull)
            {
                EditorGUILayout.HelpBox("Current painter is empty, please drag a texture to this window!", MessageType.Info);
            }
        }

        /// <summary>
        /// 拖动纹理到窗口中
        /// </summary>
        private void OnDragTexture()
        {
            if (Event.current != null)
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0].GetType() == typeof(Texture2D))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0].GetType() == typeof(Texture2D))
                    {
                        Focus();

                        Texture2D texture = DragAndDrop.objectReferences[0] as Texture2D;
                        if (!texture.IsSupportPaint())
                        {
                            Utility.LogError("暂不支持此种压缩格式的纹理：" + texture.format);
                            return;
                        }

                        Vector2 anchor = position.center - position.position - new Vector2(texture.width * _textureScale / 2, texture.height * _textureScale / 2);
                        string path = AssetDatabase.GetAssetPath(texture);
                        if (path.IsJpg() || path.IsPng() || path.IsTga())
                        {
                            if (_texturePainter == null)
                            {
                                _texturePainter = new TexturePainter(texture, anchor);
                            }
                            else
                            {
                                _texturePainter.OpenTexture(texture, anchor);
                            }
                            _isAdjustBrightness = false;
                            _isAdjustSaturation = false;
                        }
                        else
                        {
                            Utility.LogError("暂不支持此种文件格式的纹理：" + path);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 事件处理
        /// </summary>
        private void OnEventHandler()
        {
            if (IsNull)
                return;
            
            if (Event.current != null)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (Event.current.button == 0)
                        {
                            if (_textureRect.Contains(Event.current.mousePosition))
                            {
                                _isTextureDragging = true;
                                GUI.FocusControl(null);
                                GUI.changed = true;
                            }
                        }
                        break;
                    case EventType.MouseDrag:
                        if (_isTextureDragging)
                        {
                            _texturePainter.Anchor += Event.current.delta;
                            GUI.changed = true;
                        }
                        break;
                    case EventType.MouseUp:
                        _isTextureDragging = false;
                        break;
                    case EventType.ScrollWheel:
                        _textureScale -= Event.current.delta.y * 0.01f;
                        GUI.changed = true;
                        break;
                }
            }
        }
    }
}