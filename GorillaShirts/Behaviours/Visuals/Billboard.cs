using GorillaExtensions;
using UnityEngine;

namespace GorillaShirts.Behaviours.Visuals
{
    public class Billboard : MonoBehaviour
    {
        public string Mode = "0";

        public void Start()
        {
            Mode = transform.GetChild(transform.childCount - 1).name[^1].ToString();
        }

        public void Update()
        {
            if (Mode == "0")
            {
                Vector3 forward = Camera.main.transform.position - transform.position;
                Vector3 eulerAngles = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.WithZ(0);
                transform.rotation = Quaternion.Euler(eulerAngles);
            }
            else
            {
                Vector3 forward = Camera.main.transform.position - transform.position;
                Vector3 eulerAngles = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.WithZ(0);
                Quaternion quaternion = Quaternion.Euler(eulerAngles);
                transform.rotation = new Quaternion(0f, quaternion.y, 0f, quaternion.w);
            }
        }
    }
}
