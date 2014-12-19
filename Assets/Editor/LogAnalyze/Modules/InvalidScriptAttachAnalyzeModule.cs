using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using JDK.Edit.LogAnalyze;

namespace JDK.Edit.LogAnalyze.Modules
{
	public class InvalidScriptAttachAnalyzeModule : ILogAnalyzeModule {

		private class MyTextReporter : IAnalyzeResultReporter
		{
			public IList<string> Warnings{get;set;}

			public string Report()
			{
				if (Warnings == null || Warnings.Count == 0)
				{
					return "No issue detected.";
				}

				StringBuilder sb = new StringBuilder();
				foreach (string line in Warnings)
				{
					sb.AppendLine(line);
				}

				return sb.ToString();
			}
		}

		public IAnalyzeResultReporter Analyze(string logContent, string reportStyle)
		{
			string[] lines = logContent.Split('\n');

			IList<string> w = null;
			foreach (string l in lines)
			{
				if (l.Contains("is missing or no valid script is attached"))
				{
					if (w == null)
					{
						w = new List<string>();
					}
					w.Add(l);
				}
			}

			MyTextReporter rp = new MyTextReporter();
			rp.Warnings = w;

			return rp;
		}
		
		public string ModuleName{
			get {
				return "Invalid Script Attach Analyze";
			}
		}
		
		public string Version{
			get{
				return "1.0";
			}
		}
		
	}
}