using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KotORVR
{
	public enum Game
	{
		KotOR = 1,
		TSL = 2
	}    

    public enum ModulesTSL
    {
        M_303NAR,
        M_421DXN,
        M_901MAL,
        M_403DXN,
        M_104PER,
        M_903MAL,
        M_501OND,
        M_004EBO,
        M_231TEL,
        M_102PER,
        M_203TEL,
        M_401DXN,
        M_209TEL,
        M_233TEL,
        M_003EBO,
        M_512OND,
        M_506OND,
        M_304NAR,
        M_404DXN,
        M_906MAL,
        M_208TEL,
        M_604DAN,
        M_502OND,
        M_211TEL,
        M_007EBO,
        M_103PER,
        M_505OND,
        M_511OND,
        M_352NAR,
        M_201TEL,
        M_602DAN,
        M_904MAL,
        M_152HAR,
        M_005EBO,
        M_650DAN,
        M_371NAR,
        M_232TEL,
        M_510OND,
        M_601DAN,
        M_503OND,
        M_262TEL,
        M_261TEL,
        M_402DXN,
        M_006EBO,
        M_411DXN,
        M_301NAR,
        M_410DXN,
        M_107PER,
        M_851NIH,
        M_220TEL,
        M_305NAR,
        M_853NIH,
        M_202TEL,
        M_702KOR,
        M_852NIH,
        M_302NAR,
        M_905MAL,
        M_154HAR,
        M_204TEL,
        M_105PER,
        M_907MAL,
        M_221TEL,
        M_002EBO,
        M_101PER,
        M_153HAR,
        M_603DAN,
        M_902MAL,
        M_222TEL,
        M_205TEL,
        M_711KOR,
        M_710KOR,
        M_351NAR,
        M_001EBO,
        M_306NAR,
        M_950COR,
        M_151HAR,
        M_701KOR,
        M_610DAN,
        M_207TEL,
        M_106PER,
        M_605DAN,
        M_504OND
    }

    public enum ModulesKotor
    {

    }

	public enum ResourceType
	{
		BMP = 0001,
		TGA = 0003,
		WAV = 0004,
		TXT = 0010,
		MDL = 2002,
		NSS = 2009,
		NCS = 2010,
		ARE = 2012,
		IFO = 2014,
		WOK = 2016,
		TDA = 2017,
		TXI = 2022,
		GIT = 2023,
		UTI = 2025,
		UTC = 2027,
		DLG = 2029,
		UTT = 2032,
		GFF = 2037,
		UTD = 2042,
		UTP = 2044,
		GUI = 2047,
		UTM = 2051,
		JRL = 2056,
		UTW = 2058,
		LYT = 3000,
		TPC = 3007,
		MDX = 3008,
	}

	public static partial class Resources
	{
		public static string RootDirectory { get; private set; }
		public static string ModuleDirectory { get; private set; }

		private static Game targetGame;

		private static KEYObject keyObject;
		private static BIFObject[] bifObjects;
		private static ERFObject textures, guiTextures;
		private static List<Module> activeModules;

		private static Dictionary<string, _2DAObject> loaded2das;

		private static Dictionary<(string, Type), object> resourceCache;

		public static void Init(string rootDir, Game game)
		{
			targetGame = game;

			RootDirectory = rootDir;
			ModuleDirectory = rootDir + "\\modules";

			activeModules = new List<Module>();
			loaded2das = new Dictionary<string, _2DAObject>();

			//File.Open(@"‪D:\\Program Files\\Star Wars - KotOR2\\chitin.key", FileMode.Open);

			keyObject = new KEYObject(rootDir + "\\chitin.key");

			KEYObject.BIFStream[] bifs = keyObject.GetBIFs();
			bifObjects = new BIFObject[bifs.Length];
			for (int i = 0; i < bifs.Length; i++) {
				bifObjects[i] = new BIFObject(rootDir + "\\" + bifs[i].Filename);
			}

			textures = new ERFObject(rootDir + "\\TexturePacks\\swpc_tex_tpa.erf");
			guiTextures = new ERFObject(rootDir + "\\TexturePacks\\swpc_tex_gui.erf");
		}

		private static Stream GetStream(string resref, ResourceType type)
		{
			Stream resourceStream;
			uint id;

			//Look in unity's resources first
			//Then in the override folder
			//Then check the active modules for local files
			for (int i = 0; i < activeModules.Count; i++) {
				if (activeModules[i].TryGetResource(resref, type, out resourceStream)) {
					return resourceStream;
				}
			}

			//Then try and load from BIFs
			if (keyObject.TryGetResourceID(resref, type, out id)) {
				uint bifIndex = id >> 20;
				BIFObject bif = bifObjects[bifIndex];

				return bif.GetResourceData(bif.GetResourceById(id));
			}
			//And finally, try and load from global ERFs
			else if ((resourceStream = textures.GetResource(resref, type)) != null) { }
			else if ((resourceStream = guiTextures.GetResource(resref, type)) != null) { }

			return resourceStream;
		}

		public static void AddModule(Module module)
		{
			activeModules.Add(module);
		}

		public static Texture2D LoadTexture2D(string resref)
		{
			Stream stream;
			if (null != (stream = GetStream(resref, ResourceType.TPC))) {
				TPCObject tpc = new TPCObject(stream);
				Texture2D tex = new Texture2D(tpc.Width, tpc.Height, tpc.Format, false);

				tex.LoadRawTextureData(tpc.RawData);
				tex.Apply();

				return tex;
			}
			else if (null != (stream = GetStream(resref, ResourceType.TGA))) {
				return TGALoader.LoadTGA(stream);
			}
			else {
				//Debug.Log("Missing texture: " + resref);
				return new Texture2D(1, 1);
			}
		}

		public static Material LoadMaterial(string diffuse, string lightmap = null)
		{
            //Material mat = UnityEngine.Resources.Load("LegacyShader", typeof(Material)) as Material;
            Material mat = new Material(Shader.Find("Legacy Shaders/Lightmapped/VertexLit"));
            //string envmap;

            Texture2D tDiffuse = LoadTexture2D(diffuse);
			if (tDiffuse) {
				mat.SetTexture("_MainTex", tDiffuse);
			}

			Texture2D tLightmap;
			if (lightmap != null && (tLightmap = LoadTexture2D(lightmap))) {
				mat.SetTexture("_LightMap", tLightmap);
			}

			return mat;
		}

		public static _2DAObject Load2DA(string resref)
		{
			_2DAObject _2da;

			if (loaded2das.TryGetValue(resref, out _2da)) {
				return _2da;
			}

			Stream stream = GetStream(resref, ResourceType.TDA);
			if (stream == null) {
				Debug.Log("Missing 2da: " + resref);
				return null;
			} else {
				_2da = new _2DAObject(stream);
				loaded2das.Add(resref, _2da);
				return _2da;
			}
		}

		public static AudioClip LoadAudio(string resref)
		{
			using (FileStream stream = File.Open(RootDirectory + "\\streammusic\\" + resref + ".wav", FileMode.Open)) {
				WAVObject wav = new WAVObject(stream);

				AudioClip clip = AudioClip.Create(resref, wav.data.Length / wav.channels, wav.channels, wav.sampleRate, false);
				clip.SetData(wav.data, 0);

				return clip;
			}
		}
	}
}