editor_log_analyze
==================

这是一个分析Unity编译后Editor.log的工具，目前有三个模块：
1.AssetsSizeAnalyze
这个模块分析资源包大小，列出app中各资源占用情况，如：
Textures      0.0 kb	 0.0%
Meshes        0.0 kb	 0.0%
Animations    0.0 kb	 0.0%
Sounds        0.0 kb	 0.0%
Shaders       0.0 kb	 0.0%
Other Assets  0.2 kb	 0.0%
Levels        10.1 kb	 0.2%
Scripts       170.4 kb	 4.0%
Included DLLs 3.9 mb	 95.5%
File headers  8.2 kb	 0.2%
Complete size 4.1 mb	 100.0%

例举出大于100kb的资源并排序，方便缩减包大小时定位目标

如果使用了NGUI，且图集放在Assets/Atlas目录，则会分析图集中的图片被当成assets再次打包的情况

列出Resources目录的资源并排序，Resources目录的资源Unity无法分辨是否会用到，都会被打包

列出StreamingAssets目录的资源并排序


2.Invalid Script Attach Analyze
列出无效的脚本引用

3.Dll Reference Analyze
列出app引用的Dll

模块是可以扩展的，通过实现ILogAnalyzeModule来实现自定义模块，用LogAnalyzer.RegisterModule进行注册
