using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

public static class ApplyAutoMaterialReassignNoBackup
{
    [MenuItem("Tools/Materials/Apply Auto Reassign (No Backup)")]
    public static void ApplyReassignNoBackup()
    {
        if (!EditorUtility.DisplayDialog("Apply Auto Reassign (No Backup)",
            "Будут автоматически переназначены шейдеры у материалов без файловых бэкапов.\nРекомендуется сделать commit/backup заранее.\nПродолжить?", "Yes", "No"))
            return;

        var matGuids = AssetDatabase.FindAssets("t:Material");
        int changed = 0;
        var logSb = new System.Text.StringBuilder();
        logSb.AppendLine("Auto reassign (no backup) report");
        logSb.AppendLine("Date: " + System.DateTime.Now);
        logSb.AppendLine();

        foreach (var g in matGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            string currentShader = mat.shader != null ? mat.shader.name : "(null)";
            bool needs = currentShader == "Hidden/InternalErrorShader" || mat.shader == null || currentShader == "(null)";

            if (!needs) continue;

            var props = ReadMatProperties(path);
            var suggestion = SuggestShaderByProps(props, path);

            if (string.IsNullOrEmpty(suggestion))
            {
                logSb.AppendLine($"{path} -> no suggestion (skipped)");
                continue;
            }

            var newShader = Shader.Find(suggestion);
            if (newShader == null)
            {
                logSb.AppendLine($"{path} -> suggested shader not found: {suggestion} (skipped)");
                continue;
            }

            // Apply change (no file backup)
            Undo.RecordObject(mat, "Auto Reassign Shader (No Backup)");
            mat.shader = newShader;
            EditorUtility.SetDirty(mat);
            changed++;
            logSb.AppendLine($"{path} : {currentShader} => {suggestion}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        logSb.AppendLine();
        logSb.AppendLine($"Materials changed: {changed}");
        var outPath = "Assets/AutoMaterialReassign_NoBackup_Report.txt";
        File.WriteAllText(outPath, logSb.ToString());
        AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);

        EditorUtility.DisplayDialog("Apply Auto Reassign (No Backup)", $"Готово. Изменено материалов: {changed}\nОтчёт: {outPath}", "OK");
        Debug.Log("[ApplyAutoMaterialReassignNoBackup] Done. Changed: " + changed + ". Report: " + outPath);
    }

    private static List<string> ReadMatProperties(string assetPath)
    {
        var props = new List<string>();
        try
        {
            var full = Path.GetFullPath(assetPath);
            if (!File.Exists(full)) return props;
            var text = File.ReadAllText(full);
            var matches = Regex.Matches(text, @"(_[A-Za-z0-9_]+)");
            foreach (Match m in matches)
            {
                var v = m.Value;
                if (!props.Contains(v)) props.Add(v);
            }
            var matchName = Regex.Match(text, @"shader:\s*""?([^""]+)""?", RegexOptions.IgnoreCase);
            if (matchName.Success)
            {
                var sn = matchName.Groups[1].Value;
                if (!string.IsNullOrEmpty(sn) && !props.Contains(sn)) props.Insert(0, sn);
            }
        }
        catch { }
        return props;
    }

    private static string SuggestShaderByProps(List<string> props, string assetPath)
    {
        var text = string.Join(" ", props).ToLower();
        var lowerPath = assetPath.ToLower();

        // Priority rules (match available shaders in project)
        if ((text.Contains("ramp") || text.Contains("slideramp") || lowerPath.Contains("ramp")) &&
            Shader.Find("Toony Colors Pro 2/User/SliderRamp") != null) return "Toony Colors Pro 2/User/SliderRamp";

        if ((text.Contains("_rim") || text.Contains("rim")) &&
            Shader.Find("Voodoo_LaunchOps/SliderRampSpecRimNormal") != null) return "Voodoo_LaunchOps/SliderRampSpecRimNormal";

        if ((text.Contains("_speccolor") || text.Contains("specular") || text.Contains("_spec")) &&
            Shader.Find("Voodoo_LaunchOps/SliderRampSpecularNormal") != null) return "Voodoo_LaunchOps/SliderRampSpecularNormal";

        if (text.Contains("emission") || text.Contains("_emission"))
        {
            if ((text.Contains("shininess") || text.Contains("gloss") || text.Contains("_spec")) &&
                Shader.Find("Mobile/Diffuse + Emission +  Specular") != null)
                return "Mobile/Diffuse + Emission +  Specular";

            if (Shader.Find("Mobile/Diffuse + Emission ") != null) return "Mobile/Diffuse + Emission ";
        }

        if (text.Contains("_color") || lowerPath.Contains("font") || lowerPath.Contains("text"))
        {
            if (Shader.Find("Mobile/Diffuse + Color ") != null) return "Mobile/Diffuse + Color ";
        }

        if (lowerPath.Contains("spark") || lowerPath.Contains("star") || lowerPath.Contains("particle") || lowerPath.Contains("halo") || lowerPath.Contains("fading"))
        {
            if (Shader.Find("Mobile/Particles/Additive") != null) return "Mobile/Particles/Additive";
            if (Shader.Find("Legacy Shaders/Particles/Alpha Blended") != null) return "Legacy Shaders/Particles/Alpha Blended";
        }

        if (lowerPath.Contains("skybox") && Shader.Find("Skybox/Panoramic") != null) return "Skybox/Panoramic";

        // fallback to Hidden/Blit tonemap for fullscreen textures if present
        if (lowerPath.Contains("blit") || lowerPath.Contains("tonemap"))
            if (Shader.Find("Hidden/BlitCopyHDRTonemap") != null) return "Hidden/BlitCopyHDRTonemap";

        // final fallback: Standard if present
        if (Shader.Find("Standard") != null) return "Standard";

        return null;
    }
}