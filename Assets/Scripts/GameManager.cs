using System.IO;
using UnityEditor;
using UnityEngine;

namespace KotORVR
{
	public class GameManager : MonoBehaviour
	{
        public string kotorDir = "E:\\Games\\SteamLibrary\\steamapps\\common\\Knights of the Old Republic II";

		public Game targetGame = Game.TSL;

        public ModulesTSL entryModuleID = ModulesTSL.M_001EBO;        

        private string[] ModuleTSLStr =
        {
            "303NAR","421DXN","901MAL","403DXN","104PER","903MAL","501OND","004EBO","231TEL","102PER","203TEL","401DXN","209TEL","233TEL","003EBO",
            "512OND","506OND","304NAR","404DXN","906MAL","208TEL","604DAN","502OND","211TEL","007EBO","103PER","505OND","511OND","352NAR","201TEL",
            "602DAN","904MAL","152HAR","005EBO","650DAN","371NAR","232TEL","510OND","601DAN","503OND","262TEL","261TEL","402DXN","006EBO","411DXN",
            "301NAR","410DXN","107PER","851NIH","220TEL","305NAR","853NIH","202TEL","702KOR","852NIH","302NAR","905MAL","154HAR","204TEL","105PER",
            "907MAL","221TEL","002EBO","101PER","153HAR","603DAN","902MAL","222TEL","205TEL","711KOR","710KOR","351NAR","001EBO","306NAR","950COR",
            "151HAR","701KOR","610DAN","207TEL","106PER","605DAN","504OND"
        };

        public void Awake()
		{
			Resources.Init(kotorDir, targetGame);
		}

		public void Start()
		{
            string entryModule = ModuleTSLStr[(int)entryModuleID];

            string[] files = Directory.GetFiles(kotorDir);

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
