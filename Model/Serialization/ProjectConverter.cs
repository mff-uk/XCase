using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Xml.Xsl;

namespace XCase.Model.Serialization
{
	public class ProjectConverter
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

		public TextReader ConvertV1V2(string fileName)
		{
			FileInfo f = new FileInfo(fileName);
			string contents;
			using (StreamReader reader = f.OpenText())
			{
				contents = reader.ReadToEnd();
				ShiftAllIds(ref contents);
				ShiftAllReferences(ref contents);
				ReplaceTypeIds(ref contents);
				ReplaceTypeReferences(ref contents);
				
			}

			return new StringReader(contents);
		}

		public void ConvertV1toV2(string fileName)
		{
			
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