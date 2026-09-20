using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

public static class RepairSpriteAtlasByName
{
    [MenuItem("Tools/SpriteAtlases/Repair Atlas GUIDs By Sprite Names")]
    public static void RepairAtlases()
    {
        if (!EditorUtility.DisplayDialog("Repair SpriteAtlas GUIDs",
            "Будут автоматически заменены устаревшие GUID в .spriteatlas по именам спрайтов.\nРезервная копия проекта рекомендуется.\nПродолжить?", "Yes", "No"))
            return;

        var report = new StringBuilder();
        report.AppendLine("Repair SpriteAtlas By Name Report");
        report.AppendLine("Date: " + System.DateTime.Now);
        report.AppendLine();

        // Build index of sprites by name -> list of Sprite assets
        var spriteIndex = new Dictionary<string, List<Sprite>>();
        var spriteGuids = AssetDatabase.FindAssets("t:Sprite");
        foreach (var sg in spriteGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(sg);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s)
                {
                    if (!spriteIndex.TryGetValue(s.name, out var list))
                    {
                        list = new List<Sprite>();
                        spriteIndex[s.name] = list;
                    }
                    list.Add(s);
                }
            }
        }

        var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas");
        int fixedAtlases = 0;
        foreach (var ag in atlasGuids)
        {
            var atlasPath = AssetDatabase.GUIDToAssetPath(ag);
            var full = Path.GetFullPath(atlasPath);
            report.AppendLine($"Processing: {atlasPath}");

            if (!File.Exists(full))
            {
                report.AppendLine("  File not found on disk, skipped.");
                continue;
            }

            var text = File.ReadAllText(full, Encoding.UTF8);

            // extract packed sprite names block
            var namesMatch = Regex.Match(text, @"m_PackedSpriteNamesToIndex:\s*((?:\r?\n\s*-\s*.*)+)", RegexOptions.Multiline);
            if (!namesMatch.Success)
            {
                report.AppendLine("  No m_PackedSpriteNamesToIndex block found -> skipped");
                continue;
            }

            var namesBlock = namesMatch.Groups[1].Value;
            var nameMatches = Regex.Matches(namesBlock, @"-\s*(.+)");
            var names = nameMatches.Cast<Match>().Select(m => m.Groups[1].Value.Trim().Trim('"')).ToList();
            if (names.Count == 0)
            {
                report.AppendLine("  No names parsed -> skipped");
                continue;
            }

            // extract list of GUIDs in m_PackedSprites (in order) if present
            var packedSpritesMatch = Regex.Match(text, @"m_PackedSprites:\s*((?:\r?\n\s*-\s*\{[^\r\n]*\})+)", RegexOptions.Multiline);
            List<string> packedGuids = new List<string>();
            if (packedSpritesMatch.Success)
            {
                var block = packedSpritesMatch.Groups[1].Value;
                var guidMatches = Regex.Matches(block, @"guid:\s*([0-9a-fA-F]{32})");
                packedGuids = guidMatches.Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            }

            // If number of names and packedGuids match, we'll pair by index; otherwise try other heuristics
            var pairs = new List<(string name, string oldGuid)>();
            if (packedGuids.Count == names.Count)
            {
                for (int i = 0; i < names.Count; i++)
                    pairs.Add((names[i], packedGuids[i]));
            }
            else
            {
                // fallback: try to extract packables GUIDs (in packables block)
                var packablesMatch = Regex.Match(text, @"packables:\s*((?:\r?\n\s*-\s*\{[^\r\n]*\})+)", RegexOptions.Multiline);
                var packableGuids = new List<string>();
                if (packablesMatch.Success)
                {
                    var pb = packablesMatch.Groups[1].Value;
                    var pgm = Regex.Matches(pb, @"guid:\s*([0-9a-fA-F]{32})");
                    packableGuids = pgm.Cast<Match>().Select(m => m.Groups[1].Value).ToList();
                }

                // pair by min length
                int count = Mathf.Min(names.Count, Mathf.Max(packedGuids.Count, packableGuids.Count));
                for (int i = 0; i < count; i++)
                {
                    string oldGuid = null;
                    if (i < packedGuids.Count) oldGuid = packedGuids[i];
                    else if (i < packableGuids.Count) oldGuid = packableGuids[i];
                    pairs.Add((names[i], oldGuid));
                }
            }

            int atlasChanges = 0;
            string newText = text;

            // For each pair, try to find a sprite by name and get its texture guid, then replace GUIDs in YAML
            foreach (var p in pairs)
            {
                var name = p.name;
                var oldGuid = p.oldGuid;
                if (string.IsNullOrEmpty(oldGuid))
                {
                    report.AppendLine($"  Name '{name}' has no old GUID info; will try name-based match.");
                }
                else
                {
                    // check whether oldGuid resolves to asset (if yes - nothing to do)
                    var oldPath = AssetDatabase.GUIDToAssetPath(oldGuid);
                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        report.AppendLine($"  Name '{name}' - old GUID {oldGuid} exists ({oldPath}) - skipping.");
                        continue;
                    }
                }

                // find sprite by name
                if (!spriteIndex.TryGetValue(name, out var candidates) || candidates.Count == 0)
                {
                    report.AppendLine($"  No sprite found with name '{name}'");
                    continue;
                }

                // choose candidate (first)
                var chosen = candidates[0];
                var spriteAssetPath = AssetDatabase.GetAssetPath(chosen); // path to texture asset
                var newGuid = AssetDatabase.AssetPathToGUID(spriteAssetPath);
                if (string.IsNullOrEmpty(newGuid))
                {
                    report.AppendLine($"  Found sprite '{name}' but could not get asset GUID for path {spriteAssetPath}");
                    continue;
                }

                // Replace all occurrences of oldGuid in the file with newGuid (only when oldGuid non-empty)
                if (!string.IsNullOrEmpty(oldGuid))
                {
                    // replace exact guid tokens only
                    newText = Regex.Replace(newText, $@"\b{oldGuid}\b", newGuid);
                    atlasChanges++;
                    report.AppendLine($"  Replaced GUID {oldGuid} -> {newGuid} for sprite '{name}' (asset: {spriteAssetPath})");
                }
                else
                {
                    // oldGuid missing: attempt to add entry in packables and m_PackedSprites (append)
                    // Here we perform best-effort: ensure packables contains this guid; if not - add to packables and m_PackedSprites
                    // Insert into packables block (after 'packables:' line)
                    var packablesPattern = @"packables:\s*(\r?\n)";
                    if (Regex.IsMatch(newText, packablesPattern))
                    {
                        var insert = $"  - {{fileID: 0, guid: {newGuid}, type: 2}}\n";
                        newText = Regex.Replace(newText, packablesPattern, $"packables:\n{insert}");
                        report.AppendLine($"  Inserted packable guid {newGuid} for sprite '{name}'");
                        atlasChanges++;
                    }

                    // add to m_PackedSprites block
                    var packedSpritesPattern = @"m_PackedSprites:\s*(\r?\n)";
                    if (Regex.IsMatch(newText, packedSpritesPattern))
                    {
                        var insert2 = $"  - {{fileID: 21300000, guid: {newGuid}, type: 2}}\n";
                        newText = Regex.Replace(newText, packedSpritesPattern, $"m_PackedSprites:\n{insert2}");
                        report.AppendLine($"  Inserted m_PackedSprites entry for guid {newGuid} for sprite '{name}'");
                        atlasChanges++;
                    }
                }
            }

            if (atlasChanges > 0)
            {
                try
                {
                    File.WriteAllText(full, newText, Encoding.UTF8);
                    AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);
                    report.AppendLine($"  Written and imported atlas. Changes: {atlasChanges}");
                    // attempt pack
                    var atlasObj = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                    if (atlasObj != null)
                    {
                        SpriteAtlasUtility.PackAtlases(new[] { atlasObj }, EditorUserBuildSettings.activeBuildTarget);
                        report.AppendLine("  Packed atlas via API.");
                    }
                    fixedAtlases++;
                }
                catch (System.Exception ex)
                {
                    report.AppendLine("  Error writing/importing atlas: " + ex.Message);
                }
            }
            else
            {
                report.AppendLine("  No changes required for this atlas.");
            }
        }

        report.AppendLine();
        report.AppendLine($"Atlases processed: {atlasGuids.Length}, fixed: {fixedAtlases}");
        var outPath = "Assets/RepairSpriteAtlasByName_Report.txt";
        File.WriteAllText(outPath, report.ToString(), Encoding.UTF8);
        AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
        EditorUtility.DisplayDialog("Repair complete", $"Готово. Исправлено атласов: {fixedAtlases}. Отчёт: {outPath}", "OK");
        Debug.Log("[RepairSpriteAtlasByName] Done. Report: " + outPath);
    }
}