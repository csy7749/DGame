#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Aspose.PSD.FileFormats.Psd;
using Aspose.PSD.FileFormats.Psd.Layers;
using Aspose.PSD.FileFormats.Psd.Layers.LayerResources;
using Aspose.PSD.ImageLoadOptions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Font = UnityEngine.Font;

namespace DGame.PSD2UGUI
{
    /// <summary>
    /// PSD 转 UGUI 窗口
    /// </summary>
    public class PSD2UGUIWindow : EditorWindow
    {
        private string m_psdPath;
        private bool m_updateMode;
        private bool m_notImage;
        private bool m_notPos;
        private bool m_notText;
        private bool m_onlySize;
        private bool m_onlyCenter;

        private PSD2UGUIAttribute m_attr;
        private Editor m_settingsEditor;
        private Vector2 m_scroll;
        private int m_tabIndex;
        private static readonly string[] s_tabs = { "PSD转UI", "设置" };

        [MenuItem("DGame Tools/UI/PSD转UI &h")]
        public static void Open()
        {
            var window = GetWindow<PSD2UGUIWindow>(false, "PSD转UI");
            window.minSize = new Vector2(420, 380);
            window.Show();
        }

        private void OnEnable()
        {
            var settings = PSD2UGUISettings.Instance;
            m_settingsEditor = Editor.CreateEditor(settings);
        }

        private void OnDisable()
        {
            if (m_settingsEditor != null)
            {
                DestroyImmediate(m_settingsEditor);
                m_settingsEditor = null;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            m_tabIndex = GUILayout.Toolbar(m_tabIndex, s_tabs, GUILayout.Height(24));
            EditorGUILayout.Space(6);
            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
            if (m_tabIndex == 0)
            {
                DrawPSDToUI();
            }
            else
            {
                DrawSettings();
            }
            EditorGUILayout.EndScrollView();
        }

        #region 设置区
        private void DrawSettings()
        {
            EditorGUI.BeginChangeCheck();
            if (m_settingsEditor != null)
            {
                m_settingsEditor.OnInspectorGUI();
            }
            if (EditorGUI.EndChangeCheck())
            {
                ComponentTypeResolver.ClearCache();
                PSD2UGUISettings.Instance.ClearCache();
            }
            EditorGUILayout.Space(8);
            if (GUILayout.Button("打开设置文件 (Ping)"))
            {
                Selection.activeObject = PSD2UGUISettings.Instance;
                EditorGUIUtility.PingObject(PSD2UGUISettings.Instance);
            }
        }
        #endregion

        #region PSD 主流程
        private void DrawPSDToUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("PSD 源文件:", GUILayout.Width(80));
            Rect pathFieldRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(80));
            m_psdPath = EditorGUI.TextField(pathFieldRect, m_psdPath);
            if (GUILayout.Button("...", GUILayout.Width(40)))
            {
                string p = EditorUtility.OpenFilePanel("选择PSD文件", "Assets", "psd");
                if (!string.IsNullOrEmpty(p)) m_psdPath = p;
            }
            EditorGUILayout.EndHorizontal();

            // 拖拽支持
            if (((Event.current.type == EventType.DragUpdated) || Event.current.type == EventType.DragExited) &&
                pathFieldRect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    string path = DragAndDrop.paths[0];
                    if (!string.IsNullOrEmpty(path) && !path.Equals(m_psdPath) &&
                        path.EndsWith(".psd", StringComparison.OrdinalIgnoreCase))
                    {
                        m_psdPath = path;
                    }
                }
            }

            EditorGUILayout.Space(10);
            if (!File.Exists(m_psdPath))
            {
                EditorGUILayout.HelpBox("拖入或选择一个 .psd 文件", MessageType.Info);
                return;
            }

            m_updateMode = GUILayout.Toggle(m_updateMode, "设置为更新UI");
            if (m_updateMode)
            {
                EditorGUILayout.BeginHorizontal();
                m_notImage = GUILayout.Toggle(m_notImage, "不更新图片");
                m_notPos = GUILayout.Toggle(m_notPos, "不更新位置大小");
                m_notText = GUILayout.Toggle(m_notText, "不更新文本内容");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                if (GUILayout.Button("一键更新", GUILayout.Height(30)))
                {
                    UpdateUIByPSD();
                }
            }
            else
            {
                m_notImage = m_notPos = m_notText = false;
                EditorGUILayout.Space(10);
                if (GUILayout.Button("一键生成", GUILayout.Height(30)))
                {
                    ParsePSD();
                }
            }
        }

        private void ParsePSD()
        {
            var psdOpts = new PsdLoadOptions { LoadEffectsResource = true, ReadOnlyMode = false };
            Stack<GameObject> stack = new Stack<GameObject>();
            stack.Push(GetOrCreateCanvasGameObject());

            using (FileStream fs = new FileStream(m_psdPath, FileMode.Open))
            {
                try
                {
                    using (var psd = Aspose.PSD.Image.Load(fs, psdOpts) as PsdImage)
                    {
                        for (int i = 0; i < psd.Layers.Length; i++)
                        {
                            var layer = psd.Layers[i];
                            var curLayerType = layer.GetLayerType();
                            string name = layer.DisplayName;
                            foreach (var item in layer.Resources)
                            {
                                if (item is LuniResource luniRes) name = luniRes.Name;
                            }
                            if (curLayerType == PsdLayerType.SectionDividerLayer)
                            {
                                GameObject parent = CreateUI(stack.Peek(), name);
                                stack.Push(parent);
                            }
                            else if (curLayerType == PsdLayerType.LayerGroup)
                            {
                                GameObject group = stack.Count > 1 ? stack.Pop() : CreateUI(stack.Peek(), name);
                                if (!layer.IsVisible || !layer.IsVisibleInGroup || (layer as LayerGroup).Layers.Length == 0)
                                    DestroyImmediate(group);
                                else
                                    group.name = name;
                            }
                            else
                            {
                                if (layer.IsVisible && layer.IsVisibleInGroup)
                                {
                                    m_attr = new PSD2UGUIAttribute();
                                    m_attr.Parser(layer);
                                    CreateUIByAttribute(stack.Peek(), name);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void UpdateUIByPSD()
        {
            var psdOpts = new PsdLoadOptions { LoadEffectsResource = true, ReadOnlyMode = false };
            using (FileStream fs = new FileStream(m_psdPath, FileMode.Open))
            {
                try
                {
                    using (var psd = Aspose.PSD.Image.Load(fs, psdOpts) as PsdImage)
                    {
                        for (int i = 0; i < psd.Layers.Length; i++)
                        {
                            var layer = psd.Layers[i];
                            string name = layer.DisplayName;
                            foreach (var item in layer.Resources)
                            {
                                if (item is LuniResource luniRes) name = luniRes.Name;
                            }
                            m_attr = new PSD2UGUIAttribute();
                            m_attr.Parser(layer);
                            GameObject go = GameObject.Find(GetUIName(name));
                            if (go && go.transform.parent != null && go.transform.parent.name == FindGroupByLayer(psd.Layers, layer))
                            {
                                UpdateUIByAttribute(go);
                            }
                        }
                    }
                    SceneView.RepaintAll();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private string FindGroupByLayer(Layer[] layers, Layer findLayer)
        {
            foreach (var layer in layers)
            {
                if (layer.GetLayerType() == PsdLayerType.LayerGroup)
                {
                    foreach (var item in (layer as LayerGroup).Layers)
                    {
                        if (item == findLayer)
                        {
                            foreach (var res in layer.Resources)
                            {
                                if (res is LuniResource luniRes) return luniRes.Name;
                            }
                            return layer.DisplayName;
                        }
                    }
                }
            }
            return "";
        }
        #endregion

        #region UI 创建/更新
        private GameObject GetOrCreateCanvasGameObject()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null && canvas.gameObject.activeInHierarchy) return canvas.gameObject;
            var go = new GameObject("Canvas") { layer = LayerMask.NameToLayer("UI") };
            canvas = go.AddComponent<Canvas>();
            go.AddComponent<GraphicRaycaster>();
            var rect = GetOrAddComponent<RectTransform>(go);
            rect.sizeDelta = PSD2UGUISettings.Instance.resolution;
            return go;
        }

        private GameObject CreateUI(GameObject parent, string name)
        {
            var go = new GameObject(name) { layer = LayerMask.NameToLayer("UI") };
            go.AddComponent<RectTransform>();
            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, go.name);
            go.name = uniqueName;
            GameObjectUtility.SetParentAndAlign(go, parent);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeGameObject = go;
            return go;
        }

        private T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private Component GetOrAddComponentByType(GameObject go, Type type)
        {
            if (type == null) return null;
            var c = go.GetComponent(type);
            return c != null ? c : go.AddComponent(type);
        }

        private void SetPosAndSize(GameObject go)
        {
            if (m_notPos) return;
            Vector2 pos = m_attr.pos;
            if (!m_attr.isLocalPos)
            {
                var canvasTra = FindObjectOfType<Canvas>().transform;
                var worldPos = canvasTra.TransformPoint(m_attr.pos);
                var parentRect = go.transform.parent.GetComponent<RectTransform>();
                pos = parentRect.InverseTransformPoint(worldPos);
            }
            if (go.TryGetComponent<RectTransform>(out var rect))
            {
                rect.sizeDelta = m_attr.size;
                rect.anchoredPosition = m_onlyCenter ? Vector2.zero : pos;
            }
        }

        private void SetTextData(GameObject go)
        {
            SetPosAndSize(go);
            if (m_onlySize || m_attr.uiType != UIType.Text) return;

            var settings = PSD2UGUISettings.Instance;
            var textType = ComponentTypeResolver.Resolve(settings.textComponentTypeName, typeof(Text));
            var text = GetOrAddComponentByType(go, textType) as Text;
            if (text == null)
            {
                Debug.LogError($"[PSD2UGUI] 文本组件类型 {settings.textComponentTypeName} 不是 Text 派生类");
                return;
            }
            string fontPath = settings.GetFontPath(m_attr.fontName);
            if (!string.IsNullOrEmpty(fontPath))
            {
                text.font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            }
            text.fontStyle = m_attr.fontStyle;
            text.fontSize = m_attr.fontSize;
            text.color = m_attr.color;
            text.raycastTarget = false;

            if (!m_notText)
            {
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                switch (m_attr.alignType)
                {
                    case FontAlignType.Left: text.alignment = TextAnchor.MiddleLeft; break;
                    case FontAlignType.Right: text.alignment = TextAnchor.MiddleRight; break;
                    default: text.alignment = TextAnchor.MiddleCenter; break;
                }
                text.text = m_attr.textStr;
            }

            // 渐变
            ApplyGradient(go, text);

            // 描边
            if (m_attr.outline)
            {
                ApplyOutline(go, text, m_attr.outlineColor, m_attr.outlineSize);
            }
            // 阴影
            if (m_attr.shadow)
            {
                ApplyShadow(go, m_attr.shadowColor, m_attr.shadowDis);
            }
        }

        private void SetImageData(GameObject go)
        {
            SetPosAndSize(go);
            if (m_onlySize || m_attr.uiType != UIType.Image) return;

            var settings = PSD2UGUISettings.Instance;

            if (m_attr.isRawImage)
            {
                var rawType = ComponentTypeResolver.Resolve(settings.rawImageComponentTypeName, typeof(RawImage));
                var rawImage = GetOrAddComponentByType(go, rawType) as RawImage;
                if (rawImage == null)
                {
                    Debug.LogError($"[PSD2UGUI] RawImage 类型 {settings.rawImageComponentTypeName} 不是 RawImage 派生类");
                    return;
                }
                rawImage.color = m_attr.color;
                rawImage.raycastTarget = false;
                if (!string.IsNullOrEmpty(m_attr.imgPath) && !m_notImage)
                {
                    rawImage.texture = AssetDatabase.LoadAssetAtPath<Texture>(m_attr.imgPath);
                }
            }
            else
            {
                var imgType = ComponentTypeResolver.Resolve(settings.imageComponentTypeName, typeof(Image));
                var image = GetOrAddComponentByType(go, imgType) as Image;
                if (image == null)
                {
                    Debug.LogError($"[PSD2UGUI] Image 类型 {settings.imageComponentTypeName} 不是 Image 派生类");
                    return;
                }
                image.color = m_attr.color;
                image.raycastTarget = false;
                if (!string.IsNullOrEmpty(m_attr.imgPath) && !m_notImage)
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(m_attr.imgPath);
                    if (sprite != null && sprite.border != Vector4.zero)
                    {
                        image.type = Image.Type.Sliced;
                    }
                    image.sprite = sprite;
                }
                if (m_attr.isButton)
                {
                    image.raycastTarget = true;
                    var btnType = ComponentTypeResolver.Resolve(settings.buttonComponentTypeName, typeof(Button));
                    var btn = GetOrAddComponentByType(go, btnType) as Selectable;
                    if (btn != null) btn.targetGraphic = image;
                }
                ApplyGradient(go, image);
            }
        }

        /// <summary>
        /// 应用渐变。若 gradientComponentTypeName 留空则跳过。
        /// 优先尝试在 graphic 上调用 SetGradientColor(Color32 top, Color32 bottom, ...) 方法。
        /// 否则尝试添加配置的独立 GradientColor 组件并赋值 colorTop/colorBottom/colorLeft/colorRight 字段。
        /// </summary>
        private void ApplyGradient(GameObject go, Graphic graphic)
        {
            if (m_attr.gradient == null || m_attr.gradient.Count == 0) return;
            Color a = m_attr.gradient[0];
            Color b = m_attr.gradient[Mathf.Min(1, m_attr.gradient.Count - 1)];

            Color top, bottom, left, right;
            ResolveGradientDirection(a, b, m_attr.gradientAngle, out top, out bottom, out left, out right);

            // 先尝试 graphic 自身的 SetGradientColor 方法
            if (graphic != null)
            {
                var m = graphic.GetType().GetMethod("SetGradientColor",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(Color32), typeof(Color32), typeof(Color32), typeof(Color32), typeof(float), typeof(float), typeof(bool) },
                    null);
                if (m != null)
                {
                    m.Invoke(graphic, new object[] { (Color32)top, (Color32)bottom, (Color32)left, (Color32)right, 0f, 0f, false });
                    return;
                }
            }

            // 否则尝试独立组件
            var settings = PSD2UGUISettings.Instance;
            if (string.IsNullOrEmpty(settings.gradientComponentTypeName)) return;
            var gradType = ComponentTypeResolver.Resolve(settings.gradientComponentTypeName, null);
            if (gradType == null) return;
            var comp = GetOrAddComponentByType(go, gradType);
            SetField(comp, "colorTop", top);
            SetField(comp, "colorBottom", bottom);
            SetField(comp, "colorLeft", left);
            SetField(comp, "colorRight", right);
        }

        private void ResolveGradientDirection(Color a, Color b, int angle, out Color top, out Color bottom, out Color left, out Color right)
        {
            top = bottom = left = right = Color.white;
            if (angle == 0)
            {
                left = a; right = b;
            }
            else if (Mathf.Abs(angle) == 180)
            {
                right = a; left = b;
            }
            else if (angle == 90)
            {
                bottom = a; top = b;
            }
            else if (angle == -90)
            {
                top = a; bottom = b;
            }
            else
            {
                top = a; bottom = b;
            }
        }

        /// <summary>
        /// 应用描边。优先 SetOutLineColor(Color32, int) 方法，否则添加配置的组件并尝试通用字段。
        /// </summary>
        private void ApplyOutline(GameObject go, Graphic graphic, Color color, int size)
        {
            // 先尝试 graphic 自身方法 SetOutLineColor(Color32, int)
            if (graphic != null)
            {
                var m = graphic.GetType().GetMethod("SetOutLineColor",
                    BindingFlags.Instance | BindingFlags.Public,
                    null, new[] { typeof(Color32), typeof(int) }, null);
                if (m != null)
                {
                    m.Invoke(graphic, new object[] { (Color32)color, size });
                    // 启用 outline
                    var setUse = graphic.GetType().GetMethod("SetUseOutLine",
                        BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(bool) }, null);
                    if (setUse != null) setUse.Invoke(graphic, new object[] { true });
                    return;
                }
            }

            var settings = PSD2UGUISettings.Instance;
            var outlineType = ComponentTypeResolver.Resolve(settings.outlineComponentTypeName, typeof(Outline));
            var comp = GetOrAddComponentByType(go, outlineType);
            if (comp is Shadow s)
            {
                s.effectColor = color;
                s.effectDistance = new Vector2(size, -size);
            }
            else if (comp != null)
            {
                SetField(comp, "effectColor", color);
                SetField(comp, "m_OutlineWidth", size);
                SetField(comp, "OutlineWidth", size);
            }
        }

        private void ApplyShadow(GameObject go, Color color, Vector2 dis)
        {
            var settings = PSD2UGUISettings.Instance;
            var shadowType = ComponentTypeResolver.Resolve(settings.shadowComponentTypeName, typeof(Shadow));
            var comp = GetOrAddComponentByType(go, shadowType);
            if (comp is Shadow s)
            {
                s.effectColor = color;
                s.effectDistance = dis;
            }
            else if (comp != null)
            {
                SetField(comp, "effectColor", color);
                SetField(comp, "effectDistance", dis);
            }
        }

        private static void SetField(object target, string name, object value)
        {
            if (target == null) return;
            var t = target.GetType();
            var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && (value == null || f.FieldType.IsInstanceOfType(value) || CanAssign(f.FieldType, value)))
            {
                try { f.SetValue(target, ConvertValue(f.FieldType, value)); return; }
                catch { }
            }
            var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.CanWrite)
            {
                try { p.SetValue(target, ConvertValue(p.PropertyType, value)); }
                catch { }
            }
        }

        private static bool CanAssign(Type fieldType, object value)
        {
            if (value == null) return !fieldType.IsValueType;
            if (fieldType.IsInstanceOfType(value)) return true;
            if (fieldType == typeof(int) && (value is float || value is double)) return true;
            return false;
        }

        private static object ConvertValue(Type t, object v)
        {
            if (v == null) return null;
            if (t.IsInstanceOfType(v)) return v;
            if (t == typeof(int)) return Convert.ToInt32(v);
            if (t == typeof(float)) return Convert.ToSingle(v);
            return v;
        }

        private GameObject CreateUIByAttribute(GameObject parent, string name = null)
        {
            GameObject go;
            if (m_attr.uiType == UIType.Text)
            {
                go = CreateUI(parent, GetUIName(name));
                SetTextData(go);
            }
            else if (m_attr.uiType == UIType.Image)
            {
                go = CreateUI(parent, GetUIName(name));
                SetImageData(go);
            }
            else
            {
                go = CreateUI(parent, GetUIName(name));
                SetPosAndSize(go);
            }
            go.SetActive(!m_attr.isHide);
            return go;
        }

        private GameObject UpdateUIByAttribute(GameObject go)
        {
            if (m_attr.uiType == UIType.Text) SetTextData(go);
            else if (m_attr.uiType == UIType.Image) SetImageData(go);
            else SetPosAndSize(go);
            return go;
        }

        private string GetUIName(string name)
        {
            if (m_attr.uiType == UIType.Text)
            {
                return $"{(name == null ? "" : name + "_")}Text";
            }
            if (m_attr.uiType == UIType.Image)
            {
                return $"{(name == null ? "" : name + "_")}{(m_attr.isButton ? "Button" : "Image")}";
            }
            return $"{(name == null ? "" : name + "_")}Group";
        }
        #endregion
    }
}
#endif
