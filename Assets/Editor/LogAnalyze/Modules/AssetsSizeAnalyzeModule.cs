using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace JDK.Edit.LogAnalyze.Modules
{
	public class AssetsSizeAnalyzeModule : ILogAnalyzeModule {

		private class MyTextReporter : IAnalyzeResultReporter
		{
			public IList<string> Headers{get;set;}
			public IList<AssetInfo> AssetInfos {get;set;}
			public IList<AssetInfo> StreamingAssets {get;set;}
			public IList<AssetInfo> AssetsAlreadyAppearedInAtlas {get;set;}

			public Dictionary<string, string> prefabSpriteDict;

			public string Report()
			{
				if (Headers == null || Headers.Count == 0)
				{
					return "NOT FOUND";
				}

				StringBuilder sb = new StringBuilder();
				foreach (string line in Headers)
				{
					sb.AppendLine(line);
				}

				if (AssetsAlreadyAppearedInAtlas != null)
				{
					sb.AppendLine();
					sb.AppendLine("Assets already in NGUI Atlas:");
					foreach (AssetInfo info in AssetsAlreadyAppearedInAtlas)
					{
						sb.AppendLine(info.ToString() + " (" + info.Memo + ")");
					}
				}

				sb.AppendLine();
				sb.AppendLine("List assets > 100kb:");
				foreach (AssetInfo info in AssetInfos)
				{
					if (info.Size > 100000f)
					{
						sb.AppendLine(info.ToString());
					}
				}

				if (StreamingAssets != null)
				{
					sb.AppendLine();
					sb.AppendLine("Assets in StreamingAssets folder:");
					foreach (AssetInfo info in StreamingAssets)
					{
						sb.AppendLine(info.ToString());
					}
				}


//				sb.AppendLine("FOR Debug: ");
//
//				foreach(string s in prefabSpriteDict.Keys)
//				{
//					sb.AppendLine(s + " in " + prefabSpriteDict[s]);
//				}

//				sb.AppendLine();
//				sb.AppendLine("Assets in resources folder:");
//				foreach (AssetInfo info in AssetInfos)
//				{
//					if (info.Path.Contains("Assets/Resources/"))
//					{
//						sb.AppendLine(info.ToString());
//					}
//				}

				return sb.ToString();
			}
		}

		private enum ReadState{
			NotBegin,
			HasBeganHeader,
			HasFinishHeader,
			HasBeganAssetsDetail,
			HasEndAssetsDetail
		}

		public IAnalyzeResultReporter Analyze(string logContent, string reportStyle)
		{
			string[] lines = logContent.Split('\n');

			ReadState state = ReadState.NotBegin;
			string begin = "Textures      ";

			IList<string> header = null;
			IList<string> detail = null;

			foreach (string l in lines)
			{
				if (state == ReadState.NotBegin && l.Contains(begin))
				{
					state = ReadState.HasBeganHeader;
				}

				if (state == ReadState.HasBeganHeader)
				{
					if (header == null)
					{
						header = new List<string>();
					}
					header.Add(l);

					if (l.Contains("Complete size "))
					{
						state = ReadState.HasFinishHeader;
						continue;
					}
				}

				if (state == ReadState.HasFinishHeader)
				{
					if (l.Contains("Used Assets, sorted by uncompressed size:"))
					{
						state = ReadState.HasBeganAssetsDetail;
						continue;
					}
				}

				if (state == ReadState.HasBeganAssetsDetail)
				{
					if (!l.Contains("% "))
					{
						state = ReadState.HasEndAssetsDetail;
						break;
					}

					if (detail == null)
					{
						detail = new List<string>();
					}

					detail.Add(l);
				}
			}

			MyTextReporter rp = new MyTextReporter();
			rp.Headers = header;

			IList<AssetInfo> assetInfos = null;
			if (detail!=null && detail.Count > 0)
			{
				assetInfos = new List<AssetInfo>();
				foreach (string d in detail)
				{
					Debug.Log("detail:" + d);

					if (string.IsNullOrEmpty(d.Trim()))
					{
						continue;
					}

					//parse size number
					int firstSpaceInex = d.IndexOf(' ', 1);

					if (firstSpaceInex < 0)
					{
						continue;
					}

					string sizeNumber = d.Substring(0, firstSpaceInex);

					float size = float.Parse(sizeNumber);
					
					//get kb or mb
					string unit = d.Substring(firstSpaceInex + 1, 2);
					if (unit.Equals("mb"))
					{
						size *= 1000000;
					}
					if (unit.Equals("kb"))
					{
						size *= 1000;
					}
					
					//get the path
					int fileNameStar = d.IndexOf("% ") + 2;
					string path = d.Substring(fileNameStar);
					
					AssetInfo info = new AssetInfo(path, size, sizeNumber + unit);
					assetInfos.Add(info);
				}

				rp.AssetInfos = assetInfos;
			}

			//Streamming assets
			string streamingAssetsDirectory = System.IO.Path.Combine(Application.dataPath, "StreamingAssets/");
			if (Directory.Exists(streamingAssetsDirectory))
			{
				List<AssetInfo> streamingAssets = new List<AssetInfo>();
				string[] files = Directory.GetFiles(streamingAssetsDirectory, "*", SearchOption.AllDirectories);
				foreach (string f in files)
				{
					if (f.EndsWith(".meta"))
					{
						continue;
					}

					//file size
					FileInfo file = new FileInfo(f);
					long size = file.Length;

					int index = f.IndexOf("Assets/StreamingAssets/");
					string path = f.Substring(index);
					AssetInfo i = new AssetInfo(path, size, GetFileSizeDescription(size));

					streamingAssets.Add(i);
				}

				streamingAssets.Sort(delegate(AssetInfo x, AssetInfo y) {
					return y.Size.CompareTo(x.Size);
				});

				rp.StreamingAssets = streamingAssets;
			
			}

			//For NGUI useage optmize, duplicated icon in Atlas and assets
			string nguiAtlasFolder = System.IO.Path.Combine(Application.dataPath, "Atlas/");
			if (Directory.Exists(nguiAtlasFolder))
			{
				string[] files = Directory.GetFiles(nguiAtlasFolder, "*.prefab", SearchOption.AllDirectories);
				if (files != null && files.Length > 0)
				{
					Dictionary<string, string> dict = new Dictionary<string, string>();

					//read atlas prefab, record sprite
					foreach(string f in files)
					{
						List<string> spriteNames = GetSpriteNamesInAtlasPrefab(f);
						foreach (string sprite in spriteNames)
						{
							if (!dict.ContainsKey(sprite))
							{
								dict.Add(sprite, f);
							}
							else
							{
								//duplicated sprite in multi atlas
							}
						}
					}

					rp.prefabSpriteDict = dict;

					if (assetInfos != null)
					{
						List<AssetInfo> assetsAlreadyAppearedInAtlas = null;
						foreach (AssetInfo asset in assetInfos)
						{
							string ext = Path.GetExtension(asset.Path);
							if (ext.Equals(".png") || ext.Equals(".jpg"))
							{
								string s = Path.GetFileNameWithoutExtension(asset.Path);
								if (dict.ContainsKey(s))
								{
									if (assetsAlreadyAppearedInAtlas == null)
									{
										assetsAlreadyAppearedInAtlas = new List<AssetInfo>();
									}
									
									//record this asset in Atlas but still use as a asset again.
									asset.Memo = "already exist in Atlas:" + dict[s];
									assetsAlreadyAppearedInAtlas.Add(asset);
								}
							}
						}

						rp.AssetsAlreadyAppearedInAtlas = assetsAlreadyAppearedInAtlas;
					}
				}
			}

			return rp;
		}

		private List<string> GetSpriteNamesInAtlasPrefab(string prefabPath)
		{
			List<string> sprites = new List<string>();

			string[] lines = File.ReadAllLines(prefabPath);
			foreach (string l in lines)
			{
				if (l.StartsWith("  - name: "))
				{
					int pos = l.LastIndexOf(' ');
					string sprite = l.Substring(pos + 1);
					sprites.Add(sprite);
				}
			}

			return sprites;
		}

		public string ModuleName{
			get {
				return "Asset Size Analyze";
			}
		}

		public string Version{
			get{
				return "1.0";
			}
		}

		private class AssetInfo{
			public float Size{get;set;}
			public string SizeToShow {get;set;}
			public string Path {get;set;}

			public string Memo {get;set;}

			public AssetInfo(string path, float size, string sizeToShow)
			{
				SizeToShow = sizeToShow;
				Size = size;
				Path = path;
			}

			public override string ToString ()
			{
				return string.Format ("{0}  \t{1}", SizeToShow, Path);
			}

			public static int CompareBySize(AssetInfo asset1, AssetInfo asset2)
			{
				if (asset1.Size > asset2.Size)
				{
					return 1;
				}
				else
				{
					return -1;
				}
			}
		}

		private string GetFileSizeDescription(long fileLength)
		{
			double len = fileLength;
			string[] sizes = { "b", "kb", "mb", "gb" };
			int order = 0;
			while (len >= 1024 && order + 1 < sizes.Length) {
				order++;
				len = len / 1024;
			}
			string result = string.Format("{0:0.#}{1}", len, sizes[order]);

			return result;
		}

	}
	
}