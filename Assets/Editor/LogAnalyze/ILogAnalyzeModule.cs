using UnityEngine;
using System.Collections;

namespace JDK.Edit.LogAnalyze
{
	public interface ILogAnalyzeModule {

		IAnalyzeResultReporter Analyze(string logContent, string reportStyle);
		string ModuleName{get;}
		string Version{get;}

	}
	
}