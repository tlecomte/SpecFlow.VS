﻿using System;
using System.Linq;
using System.Reflection;

namespace SpecFlow.VisualStudio.Editor.Services.EditorConfig
{
    public static class EditorConfigOptionsExtensions
    {
        private static object GetOption(this IEditorConfigOptions editorConfigOptions, Type optionType, string editorConfigKey, object defaultValue)
        {
            if (editorConfigOptions == null) throw new ArgumentNullException(nameof(editorConfigOptions));
            if (optionType == null) throw new ArgumentNullException(nameof(optionType));
            if (editorConfigKey == null) throw new ArgumentNullException(nameof(editorConfigKey));

            var method = typeof(IEditorConfigOptions)
                .GetMethod(nameof(IEditorConfigOptions.GetOption))!
                .MakeGenericMethod(optionType);
            return method.Invoke(editorConfigOptions, new object[] { editorConfigKey, defaultValue });
        }

        public static void UpdateFromEditorConfig<TConfig>(this IEditorConfigOptions editorConfigOptions, TConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (editorConfigOptions == null)
                return;

            var propertiesWithEditorConfig = typeof(TConfig)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => new { PropertyInfo = p, ((EditorConfigSettingAttribute)Attribute.GetCustomAttribute((MemberInfo)p, typeof(EditorConfigSettingAttribute)))?.EditorConfigSettingName })
                .Where(p => p.EditorConfigSettingName != null);

            foreach (var property in propertiesWithEditorConfig)
            {
                var currentValue = property.PropertyInfo.GetValue(config);
                var updatedValue = editorConfigOptions.GetOption(property.PropertyInfo.PropertyType, property.EditorConfigSettingName, currentValue);
                if (!Equals(currentValue, updatedValue))
                    property.PropertyInfo.SetValue(config, updatedValue);
            }
        }
    }
}