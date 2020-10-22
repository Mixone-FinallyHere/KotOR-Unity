using System.IO;
using UnityEditor;
using EasyButtons;
using UnityEngine;

namespace KotORVR
{
	public class GameManager : MonoBehaviour
	{
        public string kotorDir = "E:\\Games\\SteamLibrary\\steamapps\\common\\Knights of the Old Republic II";

		public Game targetGame = Game.TSL;

        public ModulesTSL entryModuleID = ModulesTSL.EBO001;        

        private string[] ModuleTSLStr =
        {
            "950COR","601DAN","602DAN","603DAN","604DAN","605DAN","610DAN","650DAN","401DXN","402DXN","403DXN","404DXN","410DXN","411DXN","421DXN",
            "001EBO","002EBO","003EBO","004EBO","005EBO","006EBO","007EBO","151HAR","152HAR","153HAR","154HAR","701KOR","702KOR","710KOR","711KOR",
            "901MAL","902MAL","903MAL","904MAL","905MAL","906MAL","907MAL","301NAR","302NAR","303NAR","304NAR","305NAR","306NAR","351NAR","352NAR",
            "371NAR","851NIH","852NIH","853NIH","501OND","502OND","503OND","504OND","505OND","506OND","510OND","511OND","512OND","101PER","102PER",
            "103PER","104PER","105PER","106PER","107PER","201TEL","202TEL","203TEL","204TEL","205TEL","207TEL","208TEL","209TEL","211TEL","220TEL",
            "221TEL","222TEL","231TEL","232TEL","233TEL","261TEL","262TEL"
        };

        public void Awake()
		{
			Resources.Init(kotorDir, targetGame);
		}

        [Button]
        public void LoadMap()
        {
            Resources.Init(kotorDir, targetGame);
            string entryModule = ModuleTSLStr[(int)entryModuleID];
            Module mod = Module.Load(entryModule);
        }

        public void Start()
		{
            string entryModule = ModuleTSLStr[(int)entryModuleID];
            Module mod = Module.Load(entryModule);

			GameObject.FindGameObjectWithTag("Player").transform.position = mod.entryPosition;

			AudioSource source = GetComponent<AudioSource>();
			if (mod.ambientMusic) {
				source.clip = mod.ambientMusic;
				source.Play();
			}
		}
	}
}
