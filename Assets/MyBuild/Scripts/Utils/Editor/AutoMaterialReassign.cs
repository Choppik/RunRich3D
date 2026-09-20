using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Утилита: анализирует материалы с "Hidden/InternalErrorShader" и пытается переназначить им подходящие шейдеры по эвристике.
/// Две команды в меню:
/// - Tools/Materials/Analyze Missing Shaders  — покажет предлагаемые замены (dry-run).
/// - Tools/Materials/Apply Auto Reassign       — применит замены (делает Undo/Backup и изменяет .mat объекты).
/// </summary>
public static class AutoMaterialReassign
{
    // Приоритетный список целевых шейдеров (имена точь-в-точь как в вашем проекте)
    private static readonly Dictionary<string, string> NamedShaders = new Dictionary<string, string>()
    {
        // TCP2 / Toony
        { "ramp", "Toony Colors Pro 2/User/SliderRamp" },
        { "slideramp", "Toony Colors Pro 2/User/SliderRamp" },
        // Voodoo variants
        { "voodoo_spec_rim", "Voodoo_LaunchOps/SliderRampSpecRimNormal" },
        { "voodoo_spec", "Voodoo_LaunchOps/SliderRampSpecularNormal" },
        // Mobile simple shaders
        { "emission_spec", "Mobile/Diffuse + Emission +  Specular" },
        { "emission", "Mobile/Diffuse + Emission " },
        { "diffuse_color", "Mobile/Diffuse + Color " },
        // Particles / legacy
        { "particles_additive", "Mobile/Particles/Additive" },
        { "particles_alphablend", "Legacy Shaders/Particles/Alpha Blended" },
        // Skybox
        { "skybox", "Skybox/Panoramic" },
        // Fallback
        { "standard", "Standard" },
    };

    [MenuItem("Tools/Materials/Analyze Missing Shaders")]
    public static void AnalyzeMissing()
    {
        var report = new System.Text.StringBuilder();
        var mats = AssetDatabase.FindAssets("t:Material")
            .Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();

        report.AppendLine("AutoMaterialReassign - Analysis");
        report.AppendLine("Found materials: " + mats.Length);
        report.AppendLine();

        foreach (var path in mats)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            string shaderName = mat != null && mat.shader != null ? mat.shader.name : "(null)";
            if (shaderName != "Hidden/InternalErrorShader" && shaderName != "(null)") continue;

            var props = ReadMatProperties(path);
            var suggestion = SuggestShaderByProps(props, path);
            report.AppendLine(path + " -> current shader: " + shaderName);
            report.AppendLine("  props: " + (props.Count > 0 ? string.Join(", ", props.Take(8)) : "(none)"));
            report.AppendLine("  suggestion: " + (suggestion ?? "(no candidate)"));
            report.AppendLine();
        }

        var outPath = "Assets/AutoMaterialReassign_Report.txt";
        File.WriteAllText(outPath, report.ToString());
        AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("[AutoMaterialReassign] Analysis complete. Report: " + outPath);
        EditorUtility.DisplayDialog("AutoMaterialReassign", "Analysis complete. Report: " + outPath, "OK");
    }

    [MenuItem("Tools/Materials/Apply Auto Reassign")]
    public static void ApplyReassign()
    {
        if (!EditorUtility.DisplayDialog("Apply Auto Reassign", "Будут изменены материалы. Сначала сделайте бэкап/commit. Продолжить?", "Yes", "No"))
            return;

        var mats = AssetDatabase.FindAssets("t:Material")
            .Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();

        int changed = 0;
        foreach (var path in mats)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            string shaderName = mat != null && mat.shader != null ? mat.shader.name : "(null)";
            if (shaderName != "Hidden/InternalErrorShader" && shaderName != "(null)") continue;

            var props = ReadMatProperties(path);
            var suggestion = SuggestShaderByProps(props, path);
            if (string.IsNullOrEmpty(suggestion))
            {
                Debug.LogWarning($"[AutoMaterialReassign] No suggestion for {path}");
                continue;
            }

            var newShader = Shader.Find(suggestion);
            if (newShader == null)
            {
                Debug.LogWarning($"[AutoMaterialReassign] Shader not found in project: '{suggestion}' for material {path}");
                continue;
            }

            // Backup .mat file (raw copy)
            var fullPath = Path.GetFullPath(path);
            try
            {
                var bak = fullPath + ".bak";
                if (!File.Exists(bak)) File.Copy(fullPath, bak);
            }
            catch { }

            // Apply change (Undo support)
            Undo.RecordObject(mat, "Auto Reassign Shader");
            mat.shader = newShader;
            EditorUtility.SetDirty(mat);
            changed++;
            Debug.Log($"[AutoMaterialReassign] Reassigned {path} -> {suggestion}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("AutoMaterialReassign", $"Done. Materials changed: {changed}", "OK");
    }

    // Читает .mat как текст и извлекает имена свойств (например _RampThreshold, _EmissionMap ...)
    private static List<string> ReadMatProperties(string assetPath)
    {
        var props = new List<string>();
        try
        {
            var full = Path.GetFullPath(assetPath);
            if (!File.Exists(full)) return props;
            var text = File.ReadAllText(full);
            // extract property names like _Something
            var matches = Regex.Matches(text, @"(_[A-Za-z0-9_]+)");
            foreach (Match m in matches)
            {
                var v = m.Value;
                if (!props.Contains(v)) props.Add(v);
            }
            // also try to find shader name text inside .mat (some formats)
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

    // Эвристика: на основе перечня свойств предлагается подходящий шейдер
    private static string SuggestShaderByProps(List<string> props, string assetPath)
    {
        var text = string.Join(" ", props).ToLower();

        // Priority rules
        if (text.Contains("rampthreshold") || text.Contains("ramp") || text.Contains("slideramp") || assetPath.ToLower().Contains("ramp"))
            if (Shader.Find("Toony Colors Pro 2/User/SliderRamp") != null)
                return "Toony Colors Pro 2/User/SliderRamp";

        if (text.Contains("_rimcolor") || text.Contains("rim"))
            if (Shader.Find("Voodoo_LaunchOps/SliderRampSpecRimNormal") != null)
                return "Voodoo_LaunchOps/SliderRampSpecRimNormal";

        if ((text.Contains("_speccolor") || text.Contains("_spec") || text.Contains("specular")) && Shader.Find("Voodoo_LaunchOps/SliderRampSpecularNormal") != null)
            return "Voodoo_LaunchOps/SliderRampSpecularNormal";

        if (text.Contains("_emissionmap") || text.Contains("_emissioncolor") || text.Contains("emission"))
        {
            if ((text.Contains("_gloss") || text.Contains("_shininess") || text.Contains("_spec")) && Shader.Find("Mobile/Diffuse + Emission +  Specular") != null)
                return "Mobile/Diffuse + Emission +  Specular";
            if (Shader.Find("Mobile/Diffuse + Emission ") != null)
                return "Mobile/Diffuse + Emission ";
        }

        if (text.Contains("_color") && Shader.Find("Mobile/Diffuse + Color ") != null)
            return "Mobile/Diffuse + Color ";

        // Particle heuristics by name
        var lower = assetPath.ToLower();
        if (lower.Contains("spark") || lower.Contains("star") || lower.Contains("particle") || lower.Contains("halo") || lower.Contains("fading") || lower.Contains("placehold"))
        {
            if (Shader.Find("Mobile/Particles/Additive") != null) return "Mobile/Particles/Additive";
            if (Shader.Find("Mobile/Particles/Alpha Blended") != null) return "Mobile/Particles/Alpha Blended";
            if (Shader.Find("Legacy Shaders/Particles/Alpha Blended") != null) return "Legacy Shaders/Particles/Alpha Blended";
        }

        if (lower.Contains("skybox") || lower.Contains("panoramic"))
            if (Shader.Find("Skybox/Panoramic") != null) return "Skybox/Panoramic";

        // Fallback to Standard if present
        if (Shader.Find("Standard") != null) return "Standard";

        return null;
    }
}