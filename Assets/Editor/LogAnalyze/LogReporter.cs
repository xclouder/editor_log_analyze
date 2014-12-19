using UnityEngine;
using System.Collections;

using System.Text;

namespace JDK.Edit.LogAnalyze
{
	public class LogReporter {

		private const string LINE_SEPERATOR = "-----------------------------";

		private StringBuilder reportText;

		public void Collect(IAnalyzeResultReporter resultReporter, ILogAnalyzeModule module)
		{
			if (reportText == null)
			{
				reportText = new StringBuilder();
			}

			reportText.AppendLine("【" + module.ModuleName + "】");
			reportText.AppendLine(resultReporter.Report());
		}
		
		public override string ToString()
		{
			if (reportText != null)
			{
				return reportText.ToString();
			}
			else{
				return "None";
			}
		}
		
	}
}