﻿using System;
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
        COR950,
        DAN601,
        DAN602,
        DAN603,
        DAN604,
        DAN605,
        DAN610,
        DAN650,
        DXN401,
        DXN402,
        DXN403,
        DXN404,
        DXN410,
        DXN411,
        DXN421,
        EBO001,
        EBO002,
        EBO003,
        EBO004,
        EBO005,
        EBO006,
        EBO007,
        HAR151,
        HAR152,
        HAR153,
        HAR154,
        KOR701,
        KOR702,
        KOR710,
        KOR711,
        MAL901,
        MAL902,
        MAL903,
        MAL904,
        MAL905,
        MAL906,
        MAL907,
        NAR301,
        NAR302,
        NAR303,
        NAR304,
        NAR305,
        NAR306,
        NAR351,
        NAR352,
        NAR371,
        NIH851,
        NIH852,
        NIH853,
        OND501,
        OND502,
        OND503,
        OND504,
        OND505,
        OND506,
        OND510,
        OND511,
        OND512,
        PER101,
        PER102,
        PER103,
        PER104,
        PER105,
        PER106,
        PER107,
        TEL201,
        TEL202,
        TEL203,
        TEL204,
        TEL205,
        TEL207,
        TEL208,
        TEL209,
        TEL211,
        TEL220,
        TEL221,
        TEL222,
        TEL231,
        TEL232,
        TEL233,
        TEL261,
        TEL262
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
                Debug.Log("2DA contents of " + resref);
				_2da = new _2DAObject(stream, resref);
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