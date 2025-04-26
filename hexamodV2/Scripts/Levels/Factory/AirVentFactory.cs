using System.Collections;
using UnityEngine;

namespace HexaMapAssemblies
{
    public class AirVentFactory : MonoBehaviour
    {
        void Start()
        {
            var vent = gameObject.AddComponent<AirVent>();
            vent.exitPos = exitPos;
            vent.broken = broken;
            vent.correspondingVent = vent;
            vent.brokenVent = brokenVent;
            vent.tag = "Enter";
            vent.gameObject.layer = 8;
            StartCoroutine(AssignCorrespondingVent());
        }

        IEnumerator AssignCorrespondingVent()
        {
            // done with a delay so the other factories are done adding the AirVent components
            yield return new WaitForSeconds(1f);
            gameObject.GetComponent<AirVent>().correspondingVent = correspondingVent.GetComponent<AirVent>();
            gameObject.GetComponent<AirVent>().netView = gameObject.GetComponent<PhotonView>();
        }

        public Transform exitPos;
        public bool broken;
        public GameObject correspondingVent;
        public GameObject brokenVent;
    }
}
