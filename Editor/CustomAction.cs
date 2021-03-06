﻿// This script demonstrates how to create a new action that can be accessed from the ProBuilder toolbar.
// A new menu item is registered under "Geometry" actions called "Make Double-Sided".
// To enable, remove the #if PROBUILDER_API_EXAMPLE and #endif directives.

using UnityEngine;
using UnityEditor;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.MeshOperations;
using System.Linq;

namespace ProBuilder.ExampleActions
{
	// This class is responsible for loading the pb_MenuAction into the toolbar and menu.
	[InitializeOnLoad]
	static class RegisterCustomAction
	{
		// Static initializer is called when Unity loads the assembly.
		static RegisterCustomAction()
		{
			// This registers a new MakeFacesDoubleSided menu action with the toolbar.
			pb_EditorToolbarLoader.RegisterMenuItem(InitCustomAction);
		}

		// Helper function to load a new menu action object.
		static pb_MenuAction InitCustomAction()
		{
			return new MakeFacesDoubleSided();
		}

		// Usually you'll want to add a menu item entry for your action.
		// https://docs.unity3d.com/ScriptReference/MenuItem.html
		[MenuItem("Tools/ProBuilder/Geometry/Make Faces Double-Sided", true)]
		static bool MenuVerifyDoSomethingWithPbObject()
		{
			// Using pb_EditorToolbarLoader.GetInstance keeps MakeFacesDoubleSided as a singleton.
			MakeFacesDoubleSided instance = pb_EditorToolbarLoader.GetInstance<MakeFacesDoubleSided>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Make Faces Double-Sided", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoDoSomethingWithPbObject()
		{
			MakeFacesDoubleSided instance = pb_EditorToolbarLoader.GetInstance<MakeFacesDoubleSided>();

			if(instance != null)
				SceneView.lastActiveSceneView.ShowNotification(new GUIContent(instance.DoAction().notification));
		}
	}

	/// <summary>
	/// This is the actual action that will be executed.
	/// </summary>
	public class MakeFacesDoubleSided : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		/// <summary>
		/// What to show in the hover tooltip window.
		/// pb_TooltipContent is similar to GUIContent, with the exception that it also includes an optional params[]
		/// char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
		/// </summary>
		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Double-Sided",
			"Adds another face to the back of the selected faces."
		);

		/// <summary>
		/// Determines if the action should be enabled or grayed out.
		/// </summary>
		/// <returns></returns>
		public override bool IsEnabled()
		{
			// `selection` is a helper property on pb_MenuAction that returns a pb_Object[] array from the current selection.
			return 	selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedFaceCount > 0);
		}

		/// <summary>
		/// Determines if the action should be loaded in the menu (ex, face actions shouldn't be shown when in vertex editing mode).
		/// </summary>
		/// <returns></returns>
		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Face;
		}

		/// <summary>
		/// Return a pb_ActionResult indicating the success/failure of action.
		/// </summary>
		/// <returns></returns>
		public override pb_ActionResult DoAction()
		{
			Undo.RecordObjects(selection, "Make Double-Sided Faces");

			foreach(pb_Object pb in selection)
			{
				pb_AppendDelete.DuplicateAndFlip(pb, pb.SelectedFaces);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			// Rebuild the pb_Editor caches
			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Make Faces Double-Sided");
		}
	}
}
