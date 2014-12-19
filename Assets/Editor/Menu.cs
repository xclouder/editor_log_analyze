using UnityEngine;
using UnityEditor;
using System.Collections;
using JDK.Edit.LogAnalyze;

public class Menu {

	[MenuItem("Tools/Editor Log Analyze")]
	public static void EditorLogAnalyze()
	{
		LogAnalyzer a = new LogAnalyzer();
		LogReporter r = a.Analyze();

		ShowReportEditorWindow window = (ShowReportEditorWindow)EditorWindow.GetWindow (typeof (ShowReportEditorWindow));
		window.reportText = r.ToString();
	}

}
