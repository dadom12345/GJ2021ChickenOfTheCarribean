using Assets.AI;
using System.Collections;
using UnityEngine;

public class HorizontalVerticalScrollCam : MonoBehaviour
{

    public int m_CameraEdgeScrollSpeed = 10;

    public int m_CameraDistanceZ;
    public int m_CameraDistanceY;
    public int m_CameraDistanceX;
    private IEnumerator smoothMoveCoroutine;
    private bool m_CameraMoveRequestPending;
    private bool m_CameraIsMoving;
    private float m_CameraMovementCurrenTime;
    private Vector3 m_CameraMoveRequestTargetStartPosition;
    private Vector3 m_CameraMoveRequestTargetEndPosition;
    private float m_CameraMovementEndTime;
    private Vector3 m_CameraMoveRequestForwardStartPosition;
    private Vector3 m_CameraMoveRequestForwardEndPosition;


    // Use this for initialization
    void Start()
    {

     

        StartCoroutine("SetInitialPosition", .2f);
    }


    public IEnumerator SetInitialPosition(float delay)
    {

        // Get Team of Player
        SimulationTeam lPlayerTeam = m_GameMap.GetTeam(m_GameMap.teamOfUser);

        while (lPlayerTeam == null || lPlayerTeam.SpawnPosition.Equals(Rect.zero))
        {
            lPlayerTeam = m_GameMap.GetTeam(m_GameMap.teamOfUser);
            yield return null;

        }


        Vector3 lInitialPosition = lPlayerTeam.transform.position;
        Vector3 lCameraPosition = CreateCameraPositionByTarget(lInitialPosition);

        transform.position = lCameraPosition;
        transform.LookAt(lInitialPosition);

        //FocusOnGameObject(lPlayerTeam.transform);



    }


    void Update()
    {

        if (m_CameraIsMoving)
        {
            SmoothMoveToTarget();
        }
        else
        {
            CheckScreenEdgeMovement();
        }

    }



    public void FocusOnGameObject(Transform pTransform)
    {


        Vector3 lTargetPosition = pTransform.position;
        lTargetPosition.y = 0;

        m_CameraIsMoving = true;

        m_CameraMovementCurrenTime = 0f;

        Camera lThisCamera = this.GetComponent<Camera>();

        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(Camera.main.transform.forward);


        // Vector3 lCurrentTarget = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));

        Ray lRay = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

        if (Physics.Raycast(lRay, out hit, 100, 1 << LayerMask.NameToLayer("Terrain")))
        {

            Vector3 lHittedPoint = hit.point;
            lHittedPoint.y = 0;
            float lDistance = GameLogicHelper.GetDistanceWithoutYCoordinate(lHittedPoint, lTargetPosition);

            float lTime = lDistance / 5;

            m_CameraMoveRequestTargetStartPosition = lHittedPoint;
            m_CameraMoveRequestTargetEndPosition = lTargetPosition;
            m_CameraMovementEndTime = lTime;

            m_CameraMoveRequestForwardStartPosition = transform.forward;

            Vector3 dirToTarget = (m_CameraMoveRequestTargetEndPosition - transform.position);
            m_CameraMoveRequestForwardEndPosition = dirToTarget.normalized;



        }





















    }


    void SmoothMoveToTarget()
    {


        m_CameraMovementCurrenTime = m_CameraMovementCurrenTime + Time.deltaTime;

        if (m_CameraMovementCurrenTime > m_CameraMovementEndTime)
        {
            m_CameraMovementCurrenTime = m_CameraMovementEndTime;
        }

        float lInterpolation = m_CameraMovementCurrenTime / m_CameraMovementEndTime;

        Vector3 lInterpolatedTarget = Vector3.Lerp(m_CameraMoveRequestForwardStartPosition, m_CameraMoveRequestForwardEndPosition, lInterpolation);

        Vector3 lCameraStartPosition = Camera.main.transform.position;
        Vector3 lCameraEndPosition = CreateCameraPositionByTarget(m_CameraMoveRequestTargetEndPosition);

        Vector3 lInterpolatedCameraMove = Vector3.Lerp(lCameraStartPosition, lCameraEndPosition, lInterpolation);
        // Vector3 lInterpolatedCameraMove = Vector3.Lerp(lCameraStartPosition, lCameraEndPosition, lInterpolation);

        transform.position = lInterpolatedCameraMove;


        // Debug.DrawRay(transform.position, newDir, Color.red);

        // Move our position a step closer to the target.
        //float step = speed * Time.deltaTime;

        // Vector3 newDir = Vector3.RotateTowards(transform.forward, lInterpolatedTarget, step,0.0f );

        //  transform.rotation = Quaternion.LookRotation(lInterpolatedTarget);


        transform.forward = lInterpolatedTarget;


        m_CameraMoveRequestForwardStartPosition = lInterpolatedTarget;
        Vector3 dirToTarget = (m_CameraMoveRequestTargetEndPosition - transform.position);
        m_CameraMoveRequestForwardEndPosition = dirToTarget.normalized;

        if (lInterpolation >= 1)
        {
            m_CameraIsMoving = false;
            m_CameraMovementEndTime = 0f;
            m_CameraMoveRequestTargetEndPosition = Vector3.zero;
            m_CameraMoveRequestTargetStartPosition = Vector3.zero;
        }

    }

    private Vector3 CreateCameraPositionByTarget(Vector3 pTargetStartPos)
    {
        Vector3 lCameraPosition = pTargetStartPos;
        lCameraPosition.y = lCameraPosition.y + m_CameraDistanceY;
        lCameraPosition.x = lCameraPosition.x + m_CameraDistanceX;
        lCameraPosition.z = lCameraPosition.z + m_CameraDistanceZ;

        return lCameraPosition;

    }




    // Update is called once per frame



    void CheckScreenEdgeMovement()
    {
        Vector3 camPos = this.GetComponent<Camera>().transform.position;

        Vector3 lStartNodePosition = m_TileMap.GetWorldPositionOfNode(m_TileMap.StartNode);
        Vector3 lEndNodePosition = m_TileMap.GetWorldPositionOfNode(m_TileMap.EndNode);


        if (Input.mousePosition.x > Screen.width - 30)
        {
            // Mouse is on Right End

            // X will be fully scrolled. 

            camPos.x = Mathf.Clamp(camPos.x + m_CameraEdgeScrollSpeed * Time.deltaTime, lStartNodePosition.x + (m_CameraDistanceX * 4), lEndNodePosition.x);

            // Y depends on difference between middle and end

            float lYScrollFactor = (Input.mousePosition.y / ((float)Screen.height / 2f)) - 1;

            if (lYScrollFactor >= -0.25 && lYScrollFactor <= 0.25)
            {
                lYScrollFactor = 0;
            }
            // When screenwidth is 1000 and y is 750 it should be 50% on 500 it should be 0%
            camPos.z = Mathf.Clamp(camPos.z + (m_CameraEdgeScrollSpeed * lYScrollFactor) * Time.deltaTime, lStartNodePosition.z + (m_CameraDistanceZ * 4), lEndNodePosition.z);


        }
        else if (Input.mousePosition.x < 30)
        {
            // Mouse is on Left End


            camPos.x = Mathf.Clamp(camPos.x - m_CameraEdgeScrollSpeed * Time.deltaTime, lStartNodePosition.x + (m_CameraDistanceX * 4), lEndNodePosition.x);

            float lYScrollFactor = (Input.mousePosition.y / ((float)Screen.height / 2f)) - 1;

            if (lYScrollFactor >= -0.25 && lYScrollFactor <= 0.25)
            {
                lYScrollFactor = 0;
            }
            // When screenwidth is 1000 and y is 750 it should be 50% on 500 it should be 0%
            camPos.z = Mathf.Clamp(camPos.z + (m_CameraEdgeScrollSpeed * lYScrollFactor) * Time.deltaTime, lStartNodePosition.z + (m_CameraDistanceZ * 4), lEndNodePosition.z);


        }

        else if (Input.mousePosition.y > Screen.height - 30)
        {
            // Mouse is on Top

            camPos.z = Mathf.Clamp(camPos.z + m_CameraEdgeScrollSpeed * Time.deltaTime, lStartNodePosition.z + (m_CameraDistanceZ * 4), lEndNodePosition.z);


            float lXScrollFactor = (Input.mousePosition.x / ((float)Screen.width / 2f)) - 1;
            // When screenwidth is 1000 and y is 750 it should be 50% on 500 it should be 0%
            if (lXScrollFactor >= -0.25 && lXScrollFactor <= 0.25)
            {
                lXScrollFactor = 0;
            }
            camPos.x = Mathf.Clamp(camPos.x + ((m_CameraEdgeScrollSpeed * lXScrollFactor) * Time.deltaTime), lStartNodePosition.x + (m_CameraDistanceX * 4), lEndNodePosition.x);


        }
        else if (Input.mousePosition.y < 30)
        {

            // BOTTOM
            camPos.z = Mathf.Clamp(camPos.z - m_CameraEdgeScrollSpeed * Time.deltaTime, lStartNodePosition.z + (m_CameraDistanceZ * 4), lEndNodePosition.z);


            float lXScrollFactor = (Input.mousePosition.x / ((float)Screen.width / 2f)) - 1;
            // When screenwidth is 1000 and y is 750 it should be 50% on 500 it should be 0%
            if (lXScrollFactor >= -0.25 && lXScrollFactor <= 0.25)
            {
                lXScrollFactor = 0;
            }
            camPos.x = Mathf.Clamp(camPos.x + (m_CameraEdgeScrollSpeed * lXScrollFactor) * Time.deltaTime, lStartNodePosition.x + (m_CameraDistanceX * 4), lEndNodePosition.x);



        }
        this.GetComponent<Camera>().transform.position = camPos;

    }
}
