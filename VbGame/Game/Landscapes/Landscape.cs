// Project: SpeedyRacer, File: Landscape.cs
// Namespace: SpeedyRacer.Landscapes, Class: LandscapeObject
// Path: C:\code\SpeedyRacer\Landscapes, Author: Abi
// Code lines: 1566, Size of file: 49,28 KB
// Creation date: 13.09.2006 06:20
// Last modified: 04.11.2006 18:11
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpeedyRacer.Graphics;
using SpeedyRacer.Helpers;
using SpeedyRacer.Shaders;
using SpeedyRacer.Tracks;
using Model = SpeedyRacer.Graphics.Model;
using SpeedyRacer.GameLogic;
using SpeedyRacer.GameScreens;
#endregion

namespace SpeedyRacer.Landscapes
{
	/// <summary>
	/// Landscape
	/// </summary>
	public class Landscape : IDisposable
	{
		#region Constants
		/// <summary>
		/// Helper data file, which stores all our landscape height points.
		/// See GenerateLandscapeHeightData below. Usually we would just load the
		/// LandscapeGridHeights.png file and use the data directly. But sadly
		/// the Xbox360 does not support the Texture.GetData method.
		/// Since there is no System.Drawing or System.Windows.Forms namespace,
		/// there is also no bitmap class and no easy way to just load in bitmaps
		/// and get the bitmap data somehow to construct the landscape heights.
		/// I had to rewrite the whole landscape generation code and it uses now
		/// a	binary file instead of a texture (which is bad if the user wants to
		/// change it).
		/// </summary>
		const string LandscapeHeightsDataFilename = "LandscapeHeights.data";

		/// <summary>
		/// Grid width and height
		/// </summary>
		const int GridWidth = 257,
			GridHeight = 257;

		const float MapWidthFactor = 10,//5,//10.0f,
			MapHeightFactor = 10,//5,//10.0f,
			MapZScale = 300.0f;//125.0f;//300.0f;
		#endregion

		#region Objects to render on this landscape
		/// <summary>
		/// Landscape object
		/// </summary>
		public class LandscapeObject
		{
			/// <summary>
			/// Model
			/// </summary>
			Model model;
			/// <summary>
			/// Matrix
			/// </summary>
			Matrix matrix;
			/// <summary>
			/// Is banner, sign or building?
			/// Shadows are only generated for these objects, not received.
			/// </summary>
			bool isBanner = false;

			/// <summary>
			/// Change model
			/// </summary>
			/// <param name="setNewModel">Set new model</param>
			public void ChangeModel(Model setNewModel)
			{
				model = setNewModel;
			} // ChangeModel(setNewModel)

			/// <summary>
			/// Is big building
			/// </summary>
			/// <returns>Bool</returns>
			public bool IsBigBuilding
			{
				get
				{
					return StringHelper.Contains(model.Name,
						new string[] { "Hotel", "Building" });
				} // get
			} // IsBigBuilding

			/// <summary>
			/// Is banner
			/// </summary>
			public bool IsBanner
			{
				get
				{
					return isBanner;
				} // get
			} // IsBanner

			/// <summary>
			/// Position
			/// </summary>
			/// <returns>Vector 3</returns>
			public Vector3 Position
			{
				get
				{
					return matrix.Translation;
				} // get
			} // Position

			/// <summary>
			/// Size
			/// </summary>
			/// <returns>Float</returns>
			public float Size
			{
				get
				{
					return model.Size;
				} // get
			} // Size

			/// <summary>
			/// Create landscape object
			/// </summary>
			/// <param name="setModel">Set model</param>
			/// <param name="setMatrix">Set matrix</param>
			public LandscapeObject(Model setModel, Matrix setMatrix)
			{
				if (setModel == null)
					throw new ArgumentNullException("setModel");

				model = setModel;
				matrix = setMatrix;
				isBanner = StringHelper.Contains(model.Name,
					// Also include signs no reason to receive shadows for them!
					// Faster and looks better!
					new string[] { "banner", "sign" });
			} // LandscapeObject(setModel, setMatrix, setNearTrack)

			/// <summary>
			/// Render
			/// </summary>
			public void Render()
			{
				model.Render(matrix);
			} // Render()

			/// <summary>
			/// Generate shadows
			/// </summary>
			public void GenerateShadows()
			{
				model.GenerateShadow(matrix);
			} // GenerateShadows()
			
			/// <summary>
			/// Use shadows
			/// </summary>
			public void UseShadows()
			{
				model.UseShadow(matrix);
			} // GenerateShadows()
		} // class LandscapeObject

		/// <summary>
		/// List of landscape objects.
		/// </summary>
		List<LandscapeObject> landscapeObjects = new List<LandscapeObject>();

		/// <summary>
		/// Extra list for objects that are near the track, all the objects
		/// in this list are also in the landscapeObjects list. Usually this
		/// list is a lot smaller and it is used for the shadow mapping
		/// generation in GenerateShadow and UseShadow methods below.
		/// </summary>
		List<LandscapeObject> nearTrackObjects = new List<LandscapeObject>();

		/// <summary>
		/// Kill all loaded objects
		/// </summary>
		public void KillAllLoadedObjects()
		{
			landscapeObjects.Clear();
			nearTrackObjects.Clear();
		} // KillAllLoadedObjects()

		/// <summary>
		/// All landscape models are preloaded and then used in AddObjectToRender.
		/// </summary>
		Model[] landscapeModels = new Model[]
			{
				new Model("StartLight"),
				new Model("Blockade"),
				new Model("Blockade2"),
				new Model("Hydrant"),
				new Model("Kaktus"),
				new Model("Kaktus2"),
				new Model("KaktusBenny"),
				new Model("KaktusSeg"),
				new Model("AlphaDeadTree"),
				new Model("Laterne"),
				new Model("Roadsign"),
				new Model("Roadsign2"),
				new Model("Building"),
				new Model("Building2"),
				new Model("Building3"),
				new Model("Building4"),
				new Model("Building5"),
				new Model("OilPump"),
				new Model("OilTanks"),
				new Model("Windmill"),
				new Model("Ruin"),
				new Model("RuinHouse"),
				new Model("Banner"),
				new Model("Banner2"),
				new Model("Banner3"),
				new Model("Banner4"),
				new Model("Banner5"),
				new Model("Banner6"),
				new Model("Sign"),
				new Model("Sign2"),
				new Model("SignWarning"),
				new Model("SignCurveLeft"),
				new Model("SignCurveRight"),
				new Model("SharpRock"),
				new Model("SharpRock2"),
				new Model("GuardRailHolder"),
			};

		/// <summary>
		/// Combos, which are used in the level file and for the automatic
		/// object generation below. Very useful. Each combo contains between
		/// 5 and 15 landscape model objects.
		/// </summary>
		TrackCombiModels[] combos = new TrackCombiModels[]
			{
				new TrackCombiModels("CombiPalms"),
				new TrackCombiModels("CombiPalms2"),
				new TrackCombiModels("CombiRuins"),
				new TrackCombiModels("CombiStones"),
				new TrackCombiModels("CombiOilTanks"),
				new TrackCombiModels("CombiBuildings"),
			};

		/// <summary>
		/// Names for autogenerating stuff near the road to fill the level up.
		/// First 6 entries are used with more propability (fit better).
		/// </summary>
		internal string[] autoGenerationNames = new string[]
			{
				"CombiPalms",
				"CombiPalms2",
				"CombiRuins",
				"CombiStones",
				"Kaktus",
				"Kaktus2",
				"KaktusBenny",
				"KaktusSeg",
				"AlphaDeadTree",
				"Laterne2Sides",
				"Trashcan",
				"OilPump",
				"OilTanks",
				"RoadColumnSegment",
				"Windmill",
				"Ruin",
				"RuinHouse",
				"Sign",
				"Sign2",
				"SharpRock",
				"SharpRock2",
				"Stone4",
				"Stone5",
			};

		/// <summary>
		/// Add object to render
		/// </summary>
		/// <param name="modelName">Model name</param>
		/// <param name="renderMatrix">Render matrix</param>
		/// <param name="isNearTrack">Is near track</param>
		public void AddObjectToRender(string modelName, Matrix renderMatrix,
			bool isNearTrackForShadowGeneration)
		{
			// Ignore train, we don't got it here
			if (modelName == "AlphaTrain" ||
				modelName == "RoadColumnSegment" ||
				modelName == "Laterne2Sides" ||
				modelName == "Trashcan")
				return;

			// Fix wrong model names
			if (modelName == "OilWell")
				modelName = "OilPump";
			else if (modelName == "PalmSmall" ||
				modelName == "AlphaPalmSmall" ||
				modelName == "AlphaPalm" ||
				modelName == "AlphaPalm2" ||
				modelName == "AlphaPalm3" ||
				modelName == "AlphaPalm4" ||
				modelName == "Palm" ||
				modelName == "Combi" ||
				modelName == "CombiPalms" ||
				modelName == "Hotel01" ||
				modelName == "Hotel02" ||
				modelName == "Casino01")
				modelName = "AlphaDeadTree";
			else if (modelName == "CombiSandCastle" ||
				modelName == "CombiStones2" ||
				modelName == "CombiRuins2")
				modelName = "CombiStones";
			else if (modelName == "Stone4" ||
				modelName == "Stone5")
				modelName = "SharpRock2";
			else if (modelName == "StartLight2" ||
				modelName == "StartLight3")
				modelName = "StartLight";

			// Always include windmills and buildings for shadow generation
			if (modelName == "Windmill" ||
				StringHelper.Contains(modelName,
				new string[] { "Hotel", "Building" }))
				isNearTrackForShadowGeneration = true;

			// Search for combos
			for (int num = 0; num < combos.Length; num++)
			{
				TrackCombiModels combi = combos[num];
				//slower: if (StringHelper.Compare(combi.Name, modelName))
				if (combi.Name == modelName)
				{
					// Add all combi objects (calls this method for each model)
					combi.AddAllModels(this, renderMatrix);
					// Thats it.
					return;
				} // if (combi.Name)
			} // for (num)

			Model foundModel = null;
			// Search model by name!
			for (int num = 0; num < landscapeModels.Length; num++)
			{
				Model model = landscapeModels[num];
				//slower: if (StringHelper.Compare(model.Name, modelName))
				if (model.Name == modelName)
				{
					foundModel = model;
					break;
				} // if (model.Name)
			} // for (num)

			// Only add if we found the model
			if (foundModel != null)
			{
				// Fix z position to be always ABOVE the landscape
				Vector3 modelPos = renderMatrix.Translation;

				// Get landscape height here
				float landscapeHeight = GetMapHeight(modelPos.X, modelPos.Y);
				// And make sure we are always above it!
				if (modelPos.Z < landscapeHeight)
				{
					modelPos.Z = landscapeHeight;
					// Fix render matrix
					renderMatrix.Translation = modelPos;
				} // if (modelPos.Z)

				// Check if another object is nearby, then skip this one!
				// Don't skip signs or banners!
				if (modelName.StartsWith("Banner") == false &&
					modelName.StartsWith("Sign") == false &&
					modelName.StartsWith("StartLight") == false)
				{
					for (int num = 0; num < landscapeObjects.Count; num++)
						if (Vector3.DistanceSquared(
							landscapeObjects[num].Position, modelPos) <
							foundModel.Size*foundModel.Size / 4)
						{
							// Don't add
							return;					
						} // for if (Vector3.DistanceSquared)
				} // if (modelName.StartsWith)

				LandscapeObject newObject =
					new LandscapeObject(foundModel,
					// Scale all objects up a little (else world is not filled enough)
					Matrix.CreateScale(1.2f) *
					renderMatrix);

				// Add
				landscapeObjects.Add(newObject);

				// Add again to the nearTrackObjects list if near the track
				if (isNearTrackForShadowGeneration)
					nearTrackObjects.Add(newObject);
			} // if (foundModel)
#if DEBUG
			else if (StringHelper.Contains(modelName, "Track") == false)
				// Add warning in log file
				Log.Write("Landscape model "+modelName+" is not supported and "+
					"can't be added for rendering!");
#endif
		} // AddObjectToRender(modelName, renderMatrix, isNearTrack)

		/// <summary>
		/// Add object to render
		/// </summary>
		/// <param name="modelName">Model name</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="trackPos">Track position</param>
		/// <param name="trackRight">Track right</param>
		/// <param name="distance">Distance</param>
		public void AddObjectToRender(string modelName,
			float rotation, Vector3 trackPos, Vector3 trackRight,
			float distance)
		{
			// Find out size
			float objSize = 1;

			// Search for combos
			for (int num = 0; num < combos.Length; num++)
			{
				TrackCombiModels combi = combos[num];
				//slower: if (StringHelper.Compare(combi.Name, modelName))
				if (combi.Name == modelName)
				{
					objSize = combi.Size;
					break;
				} // if (combi.Name)
			} // for (num)

			// Search model by name!
			for (int num = 0; num < landscapeModels.Length; num++)
			{
				Model model = landscapeModels[num];
				//slower: if (StringHelper.Compare(model.Name, modelName))
				if (model.Name == modelName)
				{
					objSize = model.Size;
					break;
				} // if (model.Name)
			} // for (num)

			// Make sure it is away from the road.
			if (distance > 0 &&
				distance-10 < objSize)
				distance += objSize;
			if (distance < 0 &&
				distance+10 > -objSize)
				distance -= objSize;

			AddObjectToRender(modelName,
				Matrix.CreateRotationZ(rotation) *
				Matrix.CreateTranslation(
				trackPos+trackRight*distance+new Vector3(0, 0, -100)), false);
		} // AddObjectToRender(modelName, rotation, trackPos)

		/// <summary>
		/// Add object to render
		/// </summary>
		/// <param name="modelName">Model name</param>
		/// <param name="renderPos">Render position</param>
		public void AddObjectToRender(string modelName, Vector3 renderPos)
		{
			AddObjectToRender(modelName, Matrix.CreateTranslation(renderPos), false);
		} // AddObjectToRender(modelName, renderPos)
		#endregion

		#region Variables
		/// <summary>
		/// Vertices
		/// </summary>
		TangentVertex[] vertices = new TangentVertex[GridWidth*GridHeight];
		/// <summary>
		/// Matrix
		/// </summary>
		Material mat = new Material(
			//new Color(62, 62, 62), // ambient
			//new Color(240, 240, 240), // diffuse
			//new Color(24, 24, 24), // specular
			new Color(88, 88, 88), // ambient (bright day)
			new Color(234, 234, 234), // diffuse (also bright)
			new Color(33, 33, 33), // specular (unused anyway)
			"Landscape",
			"LandscapeNormal",
			"",
			"LandscapeDetail");

		/// <summary>
		/// City material for displaying an extra material whereever the ground
		/// is flat. This makes the ground look much better at such locations,
		/// especially where the city is at.
		/// </summary>
		Material cityMat = new Material(
			new Color(32, 32, 32),
			new Color(200, 200, 200),
			new Color(128, 128, 128),
			"CityGround",
			"CityGroundNormal", "", "");

		/// <summary>
		/// City material
		/// </summary>
		/// <returns>Material</returns>
		public Material CityMaterial
		{
			get
			{
				return cityMat;
			} // get
		} // CityMaterial

		/// <summary>
		/// City planes we render additionally to the landscape.
		/// Each city plane is just 2 triangles and the cityMat material, very
		/// fast and easy stuff.
		/// </summary>
		PlaneRenderer cityPlane = null;

		/// <summary>
		/// Vertex buffer for our landscape
		/// </summary>
		VertexBuffer vertexBuffer;
		/// <summary>
		/// Index buffer for our landscape
		/// </summary>
		IndexBuffer indexBuffer;

		/// <summary>
		/// Map heights
		/// </summary>
		float[,] mapHeights = null;

		/// <summary>
		/// Track for our landscape, is just TrackSimple, can't be changed yet.
		/// </summary>
		Track track = null;

		/// <summary>
		/// Best replay for the best lap time showing the player driving.
		/// And a new replay which is recorded in case we archive a better
		/// time this time when we drive :)
		/// </summary>
		Replay bestReplay = null,
			newReplay = null;

		/// <summary>
		/// Compare checkpoint time to the bestReplay times.
		/// </summary>
		/// <param name="checkpointNum">Checkpoint num</param>
		/// <returns>Time in milliseconds we improved</returns>
		public int CompareCheckpointTime(int checkpointNum)
		{
			// Invalid data?
			if (bestReplay == null ||
				checkpointNum >= bestReplay.CheckpointTimes.Count)
				// Then we can't return anything
				return 0;

			// Else just return difference
			float differenceMs =
				SpeedyRacerManager.Player.GameTimeMilliseconds -
				bestReplay.CheckpointTimes[checkpointNum] * 1000.0f;

			return (int)differenceMs;
		} // CompareCheckpointTime(checkpointNum)

		/// <summary>
		/// Start new lap, checks if the newReplay is good and
		/// can be stored as best replay :)
		/// </summary>
		public void StartNewLap()
		{
			float thisLapTime =
				SpeedyRacerManager.Player.GameTimeMilliseconds / 1000.0f;

			// Upload new highscore (as we currently are in game,
			// no bonus or anything will be added, this score is low!)
			Highscores.SetHighscore(
				(int)SpeedyRacerManager.Player.GameTimeMilliseconds);

			SpeedyRacerManager.Player.AddLapTime(thisLapTime);

			if (thisLapTime < bestReplay.LapTime)
			{
				// Add final checkpoint
				SpeedyRacerManager.Landscape.NewReplay.CheckpointTimes.Add(
					thisLapTime);

				// Save this replay to load it everytime in the future
				newReplay.Save(thisLapTime);

				// Set it as the current best replay
				bestReplay = newReplay;
			} // if

			// And start a new replay for this round
			newReplay = new Replay(true, track);
		} // StartNewLap()

		/// <summary>
		/// New replay
		/// </summary>
		public Replay NewReplay
		{
			get
			{
				return newReplay;
			} // get
		} // NewReplay

		/// <summary>
		/// Remember a list of brack tracks, which will be generated if we brake.
		/// </summary>
		List<TangentVertex> brakeTracksVertices = new List<TangentVertex>();
		
		/// <summary>
		/// Little helper to avoid creating a new array each frame for rendering
		/// </summary>
		TangentVertex[] brakeTracksVerticesArray = null;
		#endregion

		#region Properties
		/// <summary>
		/// Track length
		/// </summary>
		/// <returns>Float</returns>
		public float TrackLength
		{
			get
			{
				return track.Length;
			} // get
		} // TrackLength

		/// <summary>
		/// Remember checkpoint segment positions for easier checkpoint checking.
		/// </summary>
		public List<int> CheckpointSegmentPositions
		{
			get
			{
				return track.CheckpointSegmentPositions;
			} // get
		} // CheckpointSegmentPositions

		/// <summary>
		/// Best replay for the best lap time showing the player driving.
		/// </summary>
		public Replay BestReplay
		{
			get
			{
				return bestReplay;
			} // get
		} // BestReplay
		#endregion

		#region Get map height
		/// <summary>
		/// Get map height at a specific point, int based and not as percise as
		/// the float version, which interpolates between our grid points.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <returns>Float</returns>
		public float GetMapHeight(int x, int y)
		{
			if (x < 0)
				x = 0;
			if (y < 0)
				y = 0;
			if (x >= GridWidth)
				x = GridWidth - 1;
			if (y >= GridHeight)
				y = GridHeight - 1;

			return mapHeights[x, y];
		} // GetMapHeight(x, y)

		/// <summary>
		/// This functions keeps sure we keep in 0-max range,
		/// simple modulate (%) will do this only correctly for positiv values!
		/// </summary>
		private static int ModulateValueInRange(float val, int max)
		{
			if (val < 0.0f)
				return (max - 1) - ((int)(-val) % max);
			else
				return (int)val % max;
		} // ModulateValueInRange(val, max)

		/// <summary>
		/// Get map height at a specific point
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <returns>Float</returns>
		public float GetMapHeight(float x, float y)
		{
			// Rescale to our current dimensions
			x /= MapWidthFactor;
			y /= MapHeightFactor;

			// Interpolate the current position
			int
				// size-1 is because we need +1 for interpolating
				ix = ModulateValueInRange(x, GridWidth - 1),
				iy = ModulateValueInRange(y, GridHeight - 1);

			// Get the position ON the current tile (0.0-1.0)!!!
			float
				fX = x - ((float)((int)x)),
				fY = y - ((float)((int)y));

			int ix2 = (ix + 1) % (GridWidth - 1);
			int iy2 = (iy + 1) % (GridHeight - 1);

			if (fX + fY < 1) // opt. version
			{
				// we are on triangle 1 !!
				//     ------- (f_tile_width-mx)/f_tile_width
				//  0__v___1
				//  |     /
				//  |    /
				//  |---/--- (f_tile_height-my)/f_tile_height
				//  |  /
				//  | /
				//  3/
				return
					mapHeights[ix, iy] + // 0
					fX * (mapHeights[ix2, iy] - mapHeights[ix, iy]) + // 1
					fY * (mapHeights[ix, iy2] - mapHeights[ix, iy]); // 3
			} // if (fX)
			// we are on triangle 1 !!
			// calc height (as above, but a bit more difficult for triangle 1)
			//        1
			//       /|
			//      / |
			//     /  |  my/f_tile_height (fX)
			//    /   |
			//   /    |
			//  3_____2
			//     ^---  mx/f_tile_width  (fY)
			return
				mapHeights[ix2, iy2] + // 2
				(1.0f - fY) * (mapHeights[ix2, iy] - mapHeights[ix2, iy2]) +	// 1
				(1.0f - fX) * (mapHeights[ix, iy2] - mapHeights[ix2, iy2]); // 3
		} // GetMapHeight(x, y)
		#endregion

		#region Constructor
		/// <summary>
		/// Create landscape.
		/// This constructor should only be called
		/// from the SpeedyRacer main class!
		/// </summary>
		/// <param name="setLevel">Level we want to load</param>
		internal Landscape()
		{
			#region Load map height data
#if OBS_DOESNT_WORK_ON_XBOX360
			// Ok, load map grid heights. We can't load this as a Bitmap
			// because we don't have the System.Drawing namespace in XNA.
			// We also can't use BitmapContent or PixelBitmapContent<Color> from
			// the Microsoft.XNA.Framework.Content.Pipeline namespace because
			// our content is not compatible with that (its just a texture).
			Texture2D tex =
				BaseGame.Content.Load<Texture2D>(
				Path.Combine(Directories.ContentDirectory, "LandscapeGridHeights"));
			/*old
				Texture2D.FromFile(BaseGame.Device,
				"Textures\\LandscapeGridHeights.png");
			 */

			if (tex.Width != GridWidth ||
				tex.Height != GridHeight)
				throw new Exception(tex.Name + " has the resolution of " +
					tex.Width + "x" + tex.Height + ", but for the landscape we need " +
					GridWidth + "x" + GridHeight + "!");

			// With help of GetData we can get to the data.
			Color[] texData = new Color[GridWidth * GridHeight];
			tex.GetData<Color>(texData, 0, GridWidth * GridHeight);
			//tst: Log.Write("Pixel 0, 0=" + texData[0]);
#endif
			FileStream file = FileHelper.LoadGameContentFile(
				"Content\\"+LandscapeHeightsDataFilename);
			byte[] heights = new byte[GridWidth * GridHeight];
			file.Read(heights, 0, GridWidth*GridHeight);
			file.Close();

			mapHeights = new float[GridWidth, GridHeight];
			#endregion

			#region Build tangent vertices
			// Build our tangent vertices
			for (int x = 0; x < GridWidth; x++)
				for (int y = 0; y < GridHeight; y++)
				{
					// Step 1: Calculate position
					int index = x + y * GridWidth;
					Vector3 pos = CalcLandscapePos(x, y, heights);//texData);
					mapHeights[x, y] = pos.Z;
					vertices[index].pos = pos;

					//if (x == 0)
					//	Log.Write("vertices " + y + ": " + pos);

					// Step 2: Calculate all edge vectors (for normals and tangents)
					// This involves quite complicated optimizations and mathematics,
					// hard to explain with just a comment. Read my book :D
					Vector3 edge1 = pos - CalcLandscapePos(x, y + 1, heights);//texData);
					Vector3 edge2 = pos - CalcLandscapePos(x + 1, y, heights);//texData);
					Vector3 edge3 = pos - CalcLandscapePos(x - 1, y + 1, heights);//texData);
					Vector3 edge4 = pos - CalcLandscapePos(x + 1, y + 1, heights);//texData);
					Vector3 edge5 = pos - CalcLandscapePos(x - 1, y - 1, heights);//texData);

					// Step 3: Calculate normal based on the edges (interpolate
					// from 3 cross products we build from our edges).
					vertices[index].normal = Vector3.Normalize(
						Vector3.Cross(edge2, edge1) +
						Vector3.Cross(edge4, edge3) +
						Vector3.Cross(edge3, edge5));

					// Step 4: Set tangent data, just use edge1
					vertices[index].tangent = Vector3.Normalize(edge1);

					// Step 5: Set texture coordinates, use full 0.0f to 1.0f range!
					vertices[index].uv = new Vector2(
						//x / (float)(GridWidth - 1),
						//y / (float)(GridHeight - 1));
						y / (float)(GridHeight - 1),
						x / (float)(GridWidth - 1));
				} // for for (int)
			#endregion

			#region Smooth normals
			// Smooth all normals, first copy them over, then smooth everything
			Vector3[,] normalsForSmoothing = new Vector3[GridWidth, GridHeight];
			for (int x = 0; x < GridWidth; x++)
				for (int y = 0; y < GridHeight; y++)
				{
					int index = x + y * GridWidth;
					normalsForSmoothing[x, y] = vertices[index].normal;
				} // for for (int)

			// Time to smooth to normals we just saved
			for (int x = 1; x < GridWidth - 1; x++)
				for (int y = 1; y < GridHeight - 1; y++)
				{
					int index = x + y * GridWidth;

					// Smooth 3x3 normals, but still use old normal to 40% (5 of 13)
					Vector3 normal = vertices[index].normal * 4;
					for (int xAdd = -1; xAdd <= 1; xAdd++)
						for (int yAdd = -1; yAdd <= 1; yAdd++)
							normal += normalsForSmoothing[x+xAdd, y+yAdd];
					vertices[index].normal = Vector3.Normalize(normal);

					// Also recalculate tangent to let it stay 90 degrees on the normal
					Vector3 helperVector = Vector3.Cross(
						vertices[index].normal,
						vertices[index].tangent);
					vertices[index].tangent = Vector3.Cross(
						helperVector,
						vertices[index].normal);
				} // for for (int)
			#endregion

			#region Set vertex buffer
			// Set vertex buffer
			vertexBuffer = new VertexBuffer(
				BaseGame.Device,
				typeof(TangentVertex),
				vertices.Length,
				ResourceUsage.WriteOnly,
				ResourceManagementMode.Automatic);
			vertexBuffer.SetData(vertices);
			#endregion

			#region Calc index buffer
			// Calc index buffer (Note: have to use uint, ushort is not sufficiant
			// in our case because we have MANY vertices ^^)
			uint[] indices = new uint[(GridWidth - 1) * (GridHeight - 1) * 6];
			int currentIndex = 0;
			for (int x = 0; x < GridWidth - 1; x++)
				for (int y = 0; y < GridHeight - 1; y++)
				{
					// Set landscape data (Note: Right handed)
					indices[currentIndex + 0] = (uint)(x * GridHeight + y);
					indices[currentIndex + 2] =
						(uint)((x + 1) * GridHeight + (y + 1));
					indices[currentIndex + 1] = (uint)((x + 1) * GridHeight + y);
					indices[currentIndex + 3] =
						(uint)((x + 1) * GridHeight + (y + 1));
					indices[currentIndex + 5] = (uint)(x * GridHeight + y);
					indices[currentIndex + 4] = (uint)(x * GridHeight + (y + 1));

					// Add indices
					currentIndex += 6;
				} // for for (int)
			#endregion

			#region Set index buffer
			indexBuffer = new IndexBuffer(
				BaseGame.Device,
				typeof(uint),
				(GridWidth - 1) * (GridHeight - 1) * 6,
				ResourceUsage.WriteOnly,
				ResourceManagementMode.Automatic);
			indexBuffer.SetData(indices);
			#endregion

			#region Load track (and replay inside ReloadLevel method)
			ReloadLevel();
			#endregion

			#region Add city planes
			// Just set one giant plane for the whole city!
			foreach (LandscapeObject obj in landscapeObjects)
				if (obj.IsBigBuilding)
				{
					cityPlane = new PlaneRenderer(
						obj.Position,
						new Plane(new Vector3(0, 0, 1), 0.1f),
						cityMat, Math.Min(obj.Position.X, obj.Position.Y));//);
					break;
				} // foreach if (obj.IsBigBuilding)
			#endregion
		} // Landscape(game, level)

		#region Reload level
		/// <summary>
		/// Reload level before starting a game
		/// </summary>
		public void ReloadLevel()
		{
			// Load track
			if (track == null)
				track = new Track("TrackSimple", this);
			else
				track.Reload("TrackSimple", this);

			// Load replay for this track to show best player
			bestReplay = new Replay(false, track);
			newReplay = new Replay(true, track);

			// Kill brake tracks
			brakeTracksVertices.Clear();
			brakeTracksVerticesArray = null;

			// Set car at start pos
			SetCarToStartPosition();
		} // ReloadLevel
		#endregion

		#region Calc landscape position
		/// <summary>
		/// Calc landscape position
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <returns>Vector 3</returns>
		private static Vector3 CalcLandscapePos(int x, int y, byte[] heights)
		{
			// Make sure we stay on the valid map data
			int mapX = x < 0 ? 0 : x >= GridWidth ? GridWidth - 1 : x;
			int mapY = y < 0 ? 0 : y >= GridHeight ? GridHeight - 1 : y;

			// Get height
			float heightPercent = heights[mapX+mapY*GridWidth] / 255.0f;

			// Build landscape position vector
			return new Vector3(
				x * MapWidthFactor,
				y * MapHeightFactor,
				heightPercent * MapZScale);
		} // CalcLandscapePos(x, y, texData)
		#endregion
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		} // Dispose()

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing">Disposing</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				for (int num = 0; num < landscapeModels.Length; num++)
					landscapeModels[num].Dispose();

				mat.Dispose();
				cityMat.Dispose();
				vertexBuffer.Dispose();
				indexBuffer.Dispose();
				track.Dispose();
			} // if
		} // Dispose(disposing)
		#endregion

		#region Set car to start pos
		/// <summary>
		/// Set car to start pos
		/// </summary>
		public void SetCarToStartPosition()
		{
			SpeedyRacerManager.Player.SetCarPosition(
				track.StartPosition, track.StartDirection, track.StartUpVector);
			// Camera is set in zooming in method of the Player class.
		} // SetCarToStartPosition()
		#endregion

		#region Render
		/// <summary>
		/// Render landscape (just at the origin)
		/// </summary>
		public void Render()
		{
			// Make sure z buffer is on
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;

			BaseGame.WorldMatrix = Matrix.Identity;

			// Render landscape (pretty easy with all the data we got here)
			ShaderEffect.landscapeNormalMapping.Render(
				mat, "DiffuseWithDetail20",
				new BaseGame.RenderHandler(RenderLandscapeVertices));

			cityPlane.Render();

			// Render track
			track.Render();

			// Render all landscape objects
			for (int num = 0; num < landscapeObjects.Count; num++)
			{
				landscapeObjects[num].Render();
			} // for (num)

			// Render all brake tracks
			RenderBrakeTracks();
		} // Render()

		#region RenderLandscapeVertices
		/// <summary>
		/// Render landscape vertices
		/// </summary>
		private void RenderLandscapeVertices()
		{
			BaseGame.Device.VertexDeclaration = TangentVertex.VertexDeclaration;
			BaseGame.Device.Vertices[0].SetSource(vertexBuffer, 0,
				TangentVertex.SizeInBytes);
			BaseGame.Device.Indices = indexBuffer;
			BaseGame.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
				0, 0, GridWidth * GridHeight,
				0, (GridWidth - 1) * (GridHeight - 1) * 2);
		} // RenderLandscapeVertices()
		#endregion
		#endregion

		#region Generate and use shadow for the landscape
		/// <summary>
		/// Generate shadow
		/// </summary>
		public void GenerateShadow()
		{
			// Don't generate shadow for the landscape, it only receives shadow!

			// Just generate shadows for the road.
			track.GenerateShadow();
			
			// Render shadow all landscape objects that near our road
			for (int num = 0; num < nearTrackObjects.Count; num++)
			{
				nearTrackObjects[num].GenerateShadows();
			} // for (num)
		} // GenerateShadow()

		/// <summary>
		/// Use shadow
		/// </summary>
		public void UseShadow()
		{
			// Receive shadow on the landscape, just render it out.
			ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(Matrix.Identity);

			// Render shadows for palms and other objects near the road.
			// But don't receive shadow in ps1.1, this causes farplane problems!
			if (BaseGame.CanUsePS20)
			{
				RenderLandscapeVertices();
				
				// Also receive shadows for all landscape objects that near our road.
				// This is not really required (still looks good without it), but
				// sometimes objects may have lookthrough-shadows or windmills
				// are usually a problem. This fixes this or makes it at least less
				// noticable.
				if (BaseGame.CanUsePS30)
				{
					for (int num = 0; num < nearTrackObjects.Count; num++)
						// Don't receive shadows on signs, looks weird.
						if (nearTrackObjects[num].IsBanner == false)
						{
							nearTrackObjects[num].UseShadows();
						} // for (num)
				} // if (BaseGame.CanUsePS30)
			} // if (BaseGame.CanUsePS20)

			// And the track receives shadow too
			track.UseShadow();
		} // UseShadow()
		#endregion

		#region GetTrackPositionMatrix and UpdateCarTrackPosition
		/// <summary>
		/// Get track position matrix, used for the game background and unit tests.
		/// </summary>
		/// <param name="carTrackPos">Car track position</param>
		/// <param name="roadWidth">Road width</param>
		/// <param name="nextRoadWidth">Next road width</param>
		/// <returns>Matrix</returns>
		public Matrix GetTrackPositionMatrix(float carTrackPos,
			out float roadWidth, out float nextRoadWidth)
		{
			return track.GetTrackPositionMatrix(carTrackPos,
				out roadWidth, out nextRoadWidth);
		} // GetTrackPositionMatrix(carTrackPos, roadWidth, nextRoadWidth)

		/// <summary>
		/// Get track position matrix
		/// </summary>
		/// <param name="trackSegmentNum">Track segment number</param>
		/// <param name="trackSegmentPercent">Track segment percent</param>
		/// <param name="roadWidth">Road width</param>
		/// <param name="nextRoadWidth">Next road width</param>
		/// <returns>Matrix</returns>
		public Matrix GetTrackPositionMatrix(
			int trackSegmentNum, float trackSegmentPercent,
			out float roadWidth, out float nextRoadWidth)
		{
			return track.GetTrackPositionMatrix(
				trackSegmentNum, trackSegmentPercent,
				out roadWidth, out nextRoadWidth);
		} // GetTrackPositionMatrix(trackSegmentNum, trackSegmentPercent, roadWi)

		/// <summary>
		/// Update car track position
		/// </summary>
		/// <param name="carPos">Car position</param>
		/// <param name="trackSegmentNumber">Track segment number</param>
		/// <param name="trackPositionPercent">Track position percent</param>
		public void UpdateCarTrackPosition(
			Vector3 carPos, //unused: Vector3 carDir, Vector3 carUp,
			ref int trackSegmentNumber, ref float trackPositionPercent)
		{
			track.UpdateCarTrackPosition(carPos, //unused: carDir, carUp,
				ref trackSegmentNumber, ref trackPositionPercent);
		} // UpdateCarTrackPosition(carPos, ..)
		#endregion

		#region Add and render brake tracks
		/// <summary>
		/// Helper to skip track generation if it is near the last generated pos.
		/// </summary>
		Vector3 lastAddedTrackPos = new Vector3(-1000, -1000, -1000);
		/// <summary>
		/// Render a maximum of 140 brake tracks.
		/// </summary>
		const int MaxBrakeTrackVertices = 6*140;

		/// <summary>
		/// Add brake track
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="dir">Dir vector</param>
		/// <param name="right">Right vector</param>
		public void AddBrakeTrack(Vector3 position, Vector3 dir, Vector3 right)
		{
			// Just skip if we setting to a similar location again.
			// This check is much faster and accurate for tracks on top of each
			// other than the foreach loop below, which is only useful to
			// put multiple tracks correctly behind each other!
			if (Vector3.DistanceSquared(position, lastAddedTrackPos) < 0.024f ||
				// Limit number of tracks to keep rendering fast.
				brakeTracksVertices.Count > MaxBrakeTrackVertices)
				return;

			lastAddedTrackPos = position;

			const float width = 2.4f; // car is 2.6m width, we use 2.4m for tires
			const float length = 4.5f; // Make brake track 6.5m long
			float maxDist =
				(float)Math.Sqrt(width * width + length * length) / 2 - 0.1f;

			// Check if there is any track already set here or nearby?
			for (int num = 0; num < brakeTracksVertices.Count; num++)
				if (Vector3.DistanceSquared(brakeTracksVertices[num].pos, position) <
					maxDist * maxDist)
					// Then skip this brake track, don't put that much stuff on
					// top of each other.
					return;

			// Move position a little bit up (above the road)
			position += new Vector3(0, 0, 0.05f + //0.025f +
				0.001f * (brakeTracksVertices.Count % 100));
			Vector3 upVector = new Vector3(0, 0, 1);

			// Just add 6 new vertices to render (2 triangles)
			TangentVertex[] newVertices = new TangentVertex[]
			{
				// First triangle
				new TangentVertex(
				position -right*width/2 -dir*length/2, 0, 0, upVector, right),
				new TangentVertex(
				position -right*width/2 +dir*length/2, 0, 5, upVector, right),
				new TangentVertex(
				position +right*width/2 +dir*length/2, 1, 5, upVector, right),
				// Second triangle
				new TangentVertex(
				position -right*width/2 -dir*length/2, 0, 0, upVector, right),
				new TangentVertex(
				position +right*width/2 +dir*length/2, 1, 5, upVector, right),
				new TangentVertex(
				position +right*width/2 -dir*length/2, 1, 0, upVector, right),
			};

			brakeTracksVertices.AddRange(newVertices);
			brakeTracksVerticesArray = brakeTracksVertices.ToArray();
		} // AddBrakeTrack(position, dirVector, rightVector)

		/// <summary>
		/// Render brake tracks
		/// </summary>
		public void RenderBrakeTracks()
		{
			// Nothing to render?
			if (brakeTracksVerticesArray == null)
				return;

			BaseGame.AlphaBlending = true;
			BaseGame.WorldMatrix = Matrix.Identity;
			BaseGame.Device.VertexDeclaration = TangentVertex.VertexDeclaration;
			ShaderEffect.simple.Render(
				SpeedyRacerManager.BrakeTrackMaterial,
				"Diffuse20",
				delegate
				{
					// Draw the vertices
					BaseGame.Device.DrawUserPrimitives(
						PrimitiveType.TriangleList,
						brakeTracksVerticesArray, 0, brakeTracksVerticesArray.Length / 3);
				});
		} // RenderBrakeTracks()
		#endregion

		#region Unit Testing
#if DEBUG
		#region Test render landscape
		/// <summary>
		/// Test render landscape
		/// </summary>
		public static void TestRenderLandscape()
		{
			TestGame.Start(
				"TestRenderLandscape",
				delegate
				{
					SpeedyRacerManager.Landscape.SetCarToStartPosition();
				},
				delegate
				{
					if (BaseGame.AllowShadowMapping)
					{
						// Generate shadows
						ShaderEffect.shadowMapping.GenerateShadows(
							delegate
							{
								SpeedyRacerManager.Landscape.GenerateShadow();
								SpeedyRacerManager.CarModel.GenerateShadow(
									SpeedyRacerManager.Player.CarRenderMatrix);
							});

						// Render shadows
						ShaderEffect.shadowMapping.RenderShadows(
							delegate
							{
								SpeedyRacerManager.Landscape.UseShadow();
								SpeedyRacerManager.CarModel.UseShadow(
									SpeedyRacerManager.Player.CarRenderMatrix);
							});
					} // if (BaseGame.AllowShadowMapping)
					
					BaseGame.UI.PostScreenGlowShader.Start();

					BaseGame.UI.RenderGameBackground();

					SpeedyRacerManager.Landscape.Render();
					SpeedyRacerManager.CarModel.RenderCar(
						false, SpeedyRacerManager.Player.CarRenderMatrix);

					// And flush render manager to draw all objects
					BaseGame.MeshRenderManager.Render();
					
					if (BaseGame.AllowShadowMapping)
					{
						ShaderEffect.shadowMapping.ShowShadows();
					} // if (BaseGame.AllowShadowMapping)

					BaseGame.UI.PostScreenGlowShader.Show();

					TestGame.UI.RenderGameUI(
						495049, 344941, 2, 148, 4,// 3, 234, 5,
						0.64f,//Input.MousePos.X / (float)BaseGame.Width,
						"Beginner",
						new int[] { 344941, 375841, 395485, 418954, 420591 });
					
					TextureFont.WriteText(
						2, 50, "Number of objects: "+
						SpeedyRacerManager.Landscape.landscapeObjects.Count);
				});
		} // TestRenderLandscape()
		#endregion
#endif
		#endregion
	} // class Landscape
} // namespace SpeedyRacer.Landscapes
