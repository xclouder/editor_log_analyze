using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using JDK.Edit.LogAnalyze;

namespace JDK.Edit.LogAnalyze.Modules
{
	public class DllReferenceAnalyzeModule : ILogAnalyzeModule {
		
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
				sb.AppendLine("Mono dependencies included in the build:");
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

			bool begin = false;
			IList<string> w = null;
			foreach (string l in lines)
			{
				if (l.Contains("Mono dependencies included in the build"))
				{
					begin = true;
					continue;
				}

				if (begin)
				{
					if (w == null)
					{
						w = new List<string>();
					}
					w.Add(l);

					if (!l.Contains("dll"))
					{
						break;
					}
				}
			}
			
			MyTextReporter rp = new MyTextReporter();
			rp.Warnings = w;
			
			return rp;
		}
		
		public string ModuleName{
			get {
				return "Dll Reference Analyze";
			}
		}
		
		public string Version{
			get{
				return "1.0";
			}
		}

	}
	
}