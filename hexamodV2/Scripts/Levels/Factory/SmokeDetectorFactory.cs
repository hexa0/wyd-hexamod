using UnityEngine;

namespace HexaMapAssemblies
{
    public class SmokeDetectorFactory : MonoBehaviour
    {
        void Start()
        {
            var smokeDetector = gameObject.AddComponent<SmokeDectector>();
            smokeDetector.lightOnMat = lightOnMat;
            smokeDetector.lightOffMat = lightOffMat;
            smokeDetector.rend = rend;
            smokeDetector.gameStateController = GameObject.Find("GameStateController");
            smokeDetector.beepSound = beepSound;
            smokeDetector.tag = "Use";
        }

        public Material lightOnMat;
        public Material lightOffMat;
        public Renderer rend;
        public GameObject beepSound;
    }
}
