using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EnhancedHierarchy.Icons {
    public sealed class MonoBehaviourIcon : IconBase {
        private const string UnityEngineUiNamespace = "UnityEngine.UI";

        private static readonly Dictionary<Type, string> monoBehaviourNames = new Dictionary<Type, string>();
        private static readonly StringBuilder goComponents = new StringBuilder(500);
        private static readonly GUIContent tempTooltipContent = new GUIContent();
        private static bool hasMonoBehaviour;

        public override string Name { get { return "MonoBehaviour Icon"; } }
        public override float Width { get { return hasMonoBehaviour ? 15f : 0f; } }
        public override IconPosition Side { get { return IconPosition.All; } }

        public override Texture2D PreferencesPreview { get { return AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript)); } }

        //public override string PreferencesTooltip { get { return "Some tag for the tooltip here"; } }

        public override void Init() {
            hasMonoBehaviour = false;

            if (!EnhancedHierarchy.IsGameObject)
                return;

            var components = GetHierarchyWithUGuiFiltered();

            foreach (var component in components)
            {
                if (component.c is not MonoBehaviour) continue;
                hasMonoBehaviour = true;
                break;
            }
        }

        private static IEnumerable<(Type, Component c)> GetHierarchyWithUGuiFiltered()
        {
            return EnhancedHierarchy.Components
                .Select(c => (c.GetType(), c))
                .Where(t => t.Item1.Namespace != null && !t.Item1.Namespace.StartsWith(UnityEngineUiNamespace));
        }

        public override void DoGUI(Rect rect) {
            if (!EnhancedHierarchy.IsRepaintEvent || !EnhancedHierarchy.IsGameObject || !hasMonoBehaviour)
                return;

            if (Utility.ShouldCalculateTooltipAt(rect) && Preferences.Tooltips) {
                goComponents.Length = 0;
                var components = GetHierarchyWithUGuiFiltered();

                foreach (var component in components)
                {
                    if (component.c is MonoBehaviour)
                        goComponents.AppendLine(GetComponentName(component.c));
                }

                tempTooltipContent.tooltip = goComponents.ToString().TrimEnd('\n', '\r');
            } else
                tempTooltipContent.tooltip = string.Empty;

            rect.yMin += 1f;
            rect.yMax -= 1f;
            rect.xMin += 1f;

            GUI.DrawTexture(rect, Styles.monobehaviourIconTexture, ScaleMode.ScaleToFit);
            EditorGUI.LabelField(rect, tempTooltipContent);
        }

        private static string GetComponentName(Component component) {
            string result;
            var type = component.GetType();

            if (monoBehaviourNames.TryGetValue(type, out result))
                return result;
            else
                return monoBehaviourNames[type] = type.ToString();
        }
    }
}
