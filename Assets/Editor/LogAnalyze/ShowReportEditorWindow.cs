using UnityEngine;
using UnityEditor;
using System.Collections;

namespace JDK.Edit.LogAnalyze
{
	
	public class ShowReportEditorWindow : EditorWindow {
		
		public string reportText;
		private Vector2 scrollPos;

		void OnGUI () {
			GUILayout.Label ("Report", EditorStyles.boldLabel);

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, new GUILayoutOption[] {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)});
			EditorGUILayout.TextArea(reportText, new GUILayoutOption[] {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)});

			EditorGUILayout.EndScrollView();

		}
		
	}
}