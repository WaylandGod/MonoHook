using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;

[InitializeOnLoad]
public class HookEditor
{
	static HookEditor()
	{
		var tType = System.Type.GetType("UnityEditor.TextureInspector,UnityEditor.dll");
		MethodInfo tTarget = tType.GetMethod("GetInfoString", BindingFlags.Instance | BindingFlags.Public); //要Hook的方法
		tType = typeof(TextureInspectorHook);
		MethodInfo tReplace = tType.GetMethod("GetInfoString", BindingFlags.Instance | BindingFlags.Public); //待替换的新方法
		MethodInfo tProxy = tType.GetMethod("Ori_GetInfoString", BindingFlags.Instance | BindingFlags.Public); //调用原始方法
		var tHook = new MethodHook(tTarget, tReplace, tProxy);
		tHook.Install();

		tType = System.Type.GetType("UnityEditor.ModelInspector,UnityEditor.dll");
		tTarget = tType.GetMethod("GetInfoString", BindingFlags.Instance | BindingFlags.Public); //要Hook的方法
		tType = typeof(ModelInspectorHook);
		tReplace = tType.GetMethod("GetInfoString", BindingFlags.Instance | BindingFlags.Public); //待替换的新方法
		tProxy = tType.GetMethod("Ori_GetInfoString", BindingFlags.Instance | BindingFlags.Public); //调用原始方法
		tHook = new MethodHook(tTarget, tReplace, tProxy);
		tHook.Install();

	}
	public class TextureInspectorHook
	{
		public string GetInfoString()
		{
			var tObj = Selection.activeObject;
			if(tObj == null)
			{
				return "Hook " + Ori_GetInfoString();
			}
			var tTex = (tObj as Sprite)?.texture ?? (tObj as Texture);
			if(tTex != null)
			{
				System.Type tType = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
				MethodInfo tMethod = tType.GetMethod("GetStorageMemorySize", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

				string tSize = "内存: " + EditorUtility.FormatBytes(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tTex))
					+ " 硬盘: " + EditorUtility.FormatBytes((int)tMethod.Invoke(null, new object[] { tTex }));
				return tSize + " - " + Ori_GetInfoString();	//必须放在原函数调用之前，否则无法执行
			}
			else
			{
				var tMesh = tObj as Mesh;
				if(tMesh != null)
				{
					string tSize = "内存: " + EditorUtility.FormatBytes(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tMesh));
					return tSize + " - " + Ori_GetInfoString();
				}
				else
				{
					return "Hook " + Ori_GetInfoString();
				}
			}
		}
		public string Ori_GetInfoString()	//保证跟原函数函数签名一致即可
		{
			return "";
		}
	}
	public class ModelInspectorHook
	{
		public string GetInfoString()
		{
			var tObj = Selection.activeObject;
			if(tObj == null)
			{
				return "Hook " + Ori_GetInfoString();
			}
			var tMesh = tObj ?? (tObj as Mesh);
			if(tMesh != null)
			{
				string tSize = "内存: " + EditorUtility.FormatBytes(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tMesh));
				return tSize + " - " + Ori_GetInfoString(); //必须放在原函数调用之前，否则无法执行
			}
			else
			{
				return "Hook " + Ori_GetInfoString();
			}
		}
		public string Ori_GetInfoString()   //保证跟原函数函数签名一致即可
		{
			return "";
		}
	}
}
