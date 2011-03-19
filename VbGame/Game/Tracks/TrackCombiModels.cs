// Project: SpeedyRacer, File: TrackCombiModels.cs
// Namespace: SpeedyRacer.Tracks, Class: TrackCombiModels
// Path: C:\code\SpeedyRacer\Tracks, Author: Abi
// Code lines: 163, Size of file: 4,21 KB
// Creation date: 04.11.2006 05:16
// Last modified: 04.11.2006 06:29
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SpeedyRacer.Helpers;
using SpeedyRacer.Landscapes;
#endregion

namespace SpeedyRacer.Tracks
{
	/// <summary>
	/// Little helper class to load combinations of objects, which simplifies
	/// our level creation process. Also used for randomly generated objects
	/// near the track.
	/// </summary>
	public class TrackCombiModels
	{
		#region Constants
		/// <summary>
		/// Directory for loading combi models.
		/// </summary>
		public const string Directory = "Content";
		/// <summary>
		/// Extension we use for combi models.
		/// </summary>
		public const string Extension = "CombiModel";
		#endregion

		#region Variables
		/// <summary>
		/// CombiObject for every object in this combi model
		/// </summary>
		[Serializable]
		public class CombiObject
		{
			/// <summary>
			/// Model name
			/// </summary>
			public string modelName;
			/// <summary>
			/// Matrix
			/// </summary>
			public Matrix matrix;

			/// <summary>
			/// Create combi object
			/// </summary>
			public CombiObject()
			{
			} // CombiObject()

			/// <summary>
			/// Create CombiObject
			/// </summary>
			/// <param name="setModelName">Set model name</param>
			/// <param name="setMatrix">Set matrix</param>
			public CombiObject(string setModelName, Matrix setMatrix)
			{
				modelName = setModelName;
				matrix = setMatrix;
			} // CombiObject(setModelName, setMatrix)
		} // class CombiObject

		/// <summary>
		/// List of combi objects used in this file.
		/// </summary>
		private List<CombiObject> objects = new List<CombiObject>();

		/// <summary>
		/// Name of this combi model (extracted from filename).
		/// </summary>
		private string name = "";

		/// <summary>
		/// Size of this combi model.
		/// </summary>
		private float size = 10;
		#endregion

		#region Properties
		/// <summary>
		/// Name
		/// </summary>
		/// <returns>String</returns>
		public string Name
		{
			get
			{
				return name;
			} // get
		} // Name

		/// <summary>
		/// Size
		/// </summary>
		public float Size
		{
			get
			{
				// Return size 10 for palms, stones and ruins, rest gets size = 50
				return size;
			} // get
		} // Size
		#endregion

		#region Load
		/// <summary>
		/// Load track combi models
		/// </summary>
		/// <param name="filename">Filename</param>
		public TrackCombiModels(string filename)
		{
			StreamReader file = new StreamReader(FileHelper.LoadGameContentFile(
				Directory+"\\"+filename+"."+Extension));

			// Load everything into this class with help of the XmlSerializer.
			objects = (List<TrackCombiModels.CombiObject>)
				new XmlSerializer(typeof(List<TrackCombiModels.CombiObject>)).
				Deserialize(file.BaseStream);

			// Close the file
			file.Close();

			name = StringHelper.ExtractFilename(filename, true);

			// Return size 10 for palms, stones and ruins, rest gets size = 50
			size = StringHelper.Compare(Name,
				new string[] { "CombiPalms", "CombiPalms2", "CombiRuins",
					"CombiRuins2", "CombiStones", "CombiStones2" }) ?
					10 : 50;
		} // TrackCombiModels(filename)
		#endregion

		#region Add all models
		/// <summary>
		/// Add all models
		/// </summary>
		/// <param name="landscape">Landscape</param>
		/// <param name="parentMatrix">Parent matrix</param>
		public void AddAllModels(Landscape landscape, Matrix parentMatrix)
		{
			// Just add all models in our combi
			foreach (CombiObject obj in objects)
				landscape.AddObjectToRender(obj.modelName,
					obj.matrix * parentMatrix, false);
		} // AddAllModels(landscape, parentMatrix)
		#endregion
	} // class TrackCombiModels
} // namespace SpeedyRacer.Tracks
