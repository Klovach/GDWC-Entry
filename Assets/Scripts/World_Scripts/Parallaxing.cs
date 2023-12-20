using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class Parallaxing : MonoBehaviour
{
    public Transform[] backgrounds; // array(list) of all the back and forgrounds to be parallaxed
    private float[] parallaxscales; // the proportion of the camera’s movement to move the backgrounds by
    public float smoothing = 1; // how smooth the parallax is going to be. make sure to set this above 0

    private Transform cam; // reference to the main camera’s transform
    private Vector3 previouscampos; // the position of the camera in the previous frame

    // is called before start(). great for references.
    void Awake()
    {
        // set up the camera reference
        cam = Camera.main.transform;
    }

    // Use this for initialization
    void Start()
    {
        // the previous frame had the current frames camera position
        previouscampos = cam.position;
        // assigning corresponding parallaxscales
        parallaxscales = new float[backgrounds.Length];

        for (int i = 0; i < backgrounds.Length; i++)
        {
            parallaxscales[i] = backgrounds[i].position.z * -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // for each background
        for (int i = 0; i < backgrounds.Length; i++)
        {
            // the parallax is the opposite of the camera because the previous frame multiplied by the scale
            float parallax = (previouscampos.x - cam.position.x) * parallaxscales[i];

            // set a target x position which is the current position plus the parallax
            float backgroundtargetposX = backgrounds[i].position.x + parallax;

            // create a target position which is the backgrounds current position with its target x position
            Vector3 backgroundtargetpos = new Vector3(backgroundtargetposX, backgrounds[i].position.y, backgrounds[i].position.z);

            // fade between current position and the target position using lerp
            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundtargetpos, smoothing * Time.deltaTime);
        }

        // set the previous cam position to the camera’s position at the end of the frame
        previouscampos = cam.position;
    }
}
