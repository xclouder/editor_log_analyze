using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using JDK.Edit.LogAnalyze.Modules;

namespace JDK.Edit.LogAnalyze
{
	public class LogAnalyzer {

		public LogAnalyzer ()
		{
			RegisterDefaultModules();
		}

		private IList<ILogAnalyzeModule> registeredModules;
		public void RegisterModule(ILogAnalyzeModule module)
		{
			if (module == null)
			{
				Debug.LogError("module should not be null.");
				return;
			}

			if (registeredModules == null)
			{
				registeredModules = new List<ILogAnalyzeModule>();
			}

			//TODO:reject the existed module duplicate register;

			registeredModules.Add(module);
			Debug.Log("Register Module:" + module.ModuleName);
		}

		public void RegisterDefaultModules()
		{
			RegisterModule(new InvalidScriptAttachAnalyzeModule());
			RegisterModule(new DllReferenceAnalyzeModule());
			RegisterModule(new AssetsSizeAnalyzeModule());
		}
		
		public LogReporter Analyze(string logFilePath = "~/Library/Logs/Unity/Editor.log")
		{

			if (registeredModules == null || registeredModules.Count == 0)
			{
				Debug.LogWarning("no modules registered.");
				return null;
			}

			/*
			Mac OS X	~/Library/Logs/Unity/Editor.log
			Windows XP *	C:\Documents and Settings\username\Local Settings\Application Data\Unity\Editor\Editor.log
			Windows Vista/7 *	C:\Users\username\AppData\Local\Unity\Editor\Editor.log
			 */

			//FOR DEBUG
//			logFilePath = "~/Desktop/Editor.log";

			if (logFilePath.StartsWith("~"))
			{
				string home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
				logFilePath = logFilePath.Replace("~", home);
			}

			string logContent = System.IO.File.ReadAllText(logFilePath);
			if (string.IsNullOrEmpty(logContent))
			{
				Debug.LogError("log file not exist or is empty.");
				return null;
			}
			
			LogReporter logReporter = new LogReporter();
			foreach (ILogAnalyzeModule m in registeredModules)
			{
				IAnalyzeResultReporter rp = m.Analyze(logContent, "text");
				
				if (rp != null)
				{
					logReporter.Collect(rp, m);
				}
				else
				{
					Debug.LogError("log reporter is null");
				}
			}
			
			return logReporter;
		}
		
		
	}
	
}