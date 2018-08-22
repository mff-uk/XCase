using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ProjectConverter
{
    public class ProjectConverterV1V2
    {
        private void ReplaceTypeIds(ref string contents)
        {
            Regex r = new Regex("datatype\\s+id=\"(?<id>\\d+)\"\\s+name=\"(?<type>\\w+)\"");
            contents = r.Replace(contents, ReplaceIdEvaluator);
        }

        private string ReplaceIdEvaluator(Match match)
        {
            int id = int.Parse(match.Groups["id"].Value);
            string name = match.Groups["type"].Value;
            oldNewTypeDictionary[id] = types[name];
            return string.Format("datatype id=\"{0}\" name=\"{1}\"", types[name], name);
        }

        private readonly Dictionary<int, int> oldNewTypeDictionary = new Dictionary<int, int>();
        private readonly Dictionary<int, int> oldNewReferenceDictionary = new Dictionary<int, int>();

        private readonly Dictionary<string, int> types = new Dictionary<string, int>
                                                             {
                                                                 {"integer", 27},
                                                                 {"string", 28},
                                                                 {"double", 29},
                                                                 {"date", 30},
                                                                 {"datetime", 31},
                                                                 {"time", 32},
                                                                 {"boolean", 33},
                                                                 {"decimal", 34},
                                                                 {"object", 35}
                                                             };

        public void ConvertV1V2(FileInfo file)
        {
            string contents;
            using (StreamReader reader = file.OpenText())
            {
                contents = reader.ReadToEnd();
                ShiftAllIds(ref contents);
                ShiftAllReferences(ref contents);
                ReplaceTypeIds(ref contents);
                ReplaceTypeReferences(ref contents);

                RemoveSimpleDataTypesDefinitions(ref contents);
                RemoveXSemProfile(ref contents);
                ReplaceVersionAttribute(ref contents);
            }

            FileStream fileStream = file.Open(FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
            
            sw.Write(contents);
            sw.Flush();
            sw.Close();
        }

        private static void ReplaceVersionAttribute(ref string contents)
        {
            //<xc:project xmlns:xc="http://kocour.ms.mff.cuni.cz/~necasky/xcase" version="1.0">
            int si = contents.IndexOf("project");
            int ei = contents.IndexOf(">", si);
            si = contents.IndexOf("version=\"1.0\"", si);
            if (si > 0 && si < ei)
            {
                contents = contents.Remove(si, "version=\"1.0\"".Length);
                contents = contents.Insert(si, "version=\"2.0\"");
            }
            else
            {
                contents = contents.Insert(ei, " version=\"2.0\"");
            }
        }

        private static void RemoveXSemProfile(ref string contents)
        {
            //<xc:profile id="0" name="XSem">
            Regex r = new Regex("<xc:profile\\s+id=\"\\d+\"\\s+name=\"XSem\"\\s*>.*</xc:profile>");
            contents = r.Replace(contents, "");
        }

        private void RemoveSimpleDataTypesDefinitions(ref string contents)
        {
            Regex r = new Regex("<xc:datatype\\s+id=\"(?<id>\\d+)\"\\s+name=\"(?<type>\\w+)\"[^/]*/>");
            contents = r.Replace(contents, RemoveSimpleDataTypesDefinitionsEvaluator);
        }

        private string RemoveSimpleDataTypesDefinitionsEvaluator(Match match)
        {
            string name = match.Groups["type"].Value;
            if (types.ContainsKey(name))
                return String.Empty;
            else return match.Groups[0].Value;
        }

        private void ShiftAllReferences(ref string contents)
        {
            Regex r = new Regex("(?<attribute>ref)=\"(?<id>\\d+)\"");
            contents = r.Replace(contents, ReplaceElementReferencesEvaluator);

            r = new Regex("(?<attribute>class)=\"(?<id>\\d+)\"");
            contents = r.Replace(contents, ReplaceElementReferencesEvaluator);

            r = new Regex("(?<attribute>general)=\"(?<id>\\d+)\"");
            contents = r.Replace(contents, ReplaceElementReferencesEvaluator);

            r = new Regex("(?<attribute>specific)=\"(?<id>\\d+)\"");
            contents = r.Replace(contents, ReplaceElementReferencesEvaluator);

            r = new Regex("(?<attribute>structural_representative)=\"(?<id>\\d+)\"");
            contents = r.Replace(contents, ReplaceElementReferencesEvaluator);
        }

        private string ReplaceElementReferencesEvaluator(Match match)
        {
            int id = int.Parse(match.Groups["id"].Value);
            int newId;
            if (oldNewReferenceDictionary.TryGetValue(id, out newId))
                return string.Format("{0}=\"{1}\"", match.Groups["attribute"].Value, newId);
            else
                return match.Value;
        }

        private void ReplaceTypeReferences(ref string contents)
        {
            Regex r = new Regex("type=\"(?<id>\\d+)\"");
            contents = r.Replace(contents, ReplaceTypeReferencesEvaluator);
        }

        private string ReplaceTypeReferencesEvaluator(Match match)
        {
            int id = int.Parse(match.Groups["id"].Value);
            int newId;
            if (oldNewTypeDictionary.TryGetValue(id, out newId))
                return string.Format("type=\"{0}\"", newId);
            else
                return match.Value;
        }

        private void ShiftAllIds(ref string contents)
        {
            Regex r = new Regex("id=\"(?<id>\\d+)\"");

            int start = contents.IndexOf("<xc:profile");

            contents = r.Replace(contents, ShiftAllIdsEvaluator, -1, start);
        }

        private string ShiftAllIdsEvaluator(Match match)
        {
            int id = int.Parse(match.Groups["id"].Value);
            oldNewReferenceDictionary.Add(id, id + SHIFT);
            return String.Format("id=\"{0}\"", id + SHIFT);
        }

        private const int SHIFT = 100;
    }
}