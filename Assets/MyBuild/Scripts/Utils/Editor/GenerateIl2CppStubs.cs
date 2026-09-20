using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

public static class GenerateIl2CppStubs
{
    [MenuItem("Tools/IL2CPP/Generate MonoBehaviour Stubs from DummyDll")]
    public static void Generate()
    {
        string searchPath = Path.Combine(Application.dataPath, "Other");
        string[] candidates = Directory.GetFiles(searchPath, "Assembly-CSharp*.dll", SearchOption.AllDirectories);
        if (candidates.Length == 0)
        {
            EditorUtility.DisplayDialog("Generate Stubs", $"Assembly-CSharp.dll not found in {searchPath}", "OK");
            return;
        }

        string dllPath = candidates[0];
        Assembly asm = null;
        try { asm = Assembly.LoadFile(dllPath); }
        catch (Exception ex)
        {
            Debug.LogError("Failed load assembly: " + ex.Message);
            EditorUtility.DisplayDialog("Generate Stubs", "Failed to load assembly. See Console.", "OK");
            return;
        }

        string outDir = Path.Combine(Application.dataPath, "GeneratedStubs");
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        int created = 0;
        foreach (var t in asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MonoBehaviour))))
        {
            string ns = t.Namespace ?? "";
            string className = t.Name;
            string fileName = Path.Combine(outDir, className + ".cs");
            if (File.Exists(fileName)) continue;

            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            if (!string.IsNullOrEmpty(ns)) sb.AppendLine($"namespace {ns} {{");
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    // auto-generated stub from dummy assembly");
            sb.AppendLine("}");
            if (!string.IsNullOrEmpty(ns)) sb.AppendLine("}");

            File.WriteAllText(fileName, sb.ToString(), System.Text.Encoding.UTF8);
            created++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Generate Stubs", $"Created {created} stub(s) in Assets/GeneratedStubs.", "OK");
    }
}