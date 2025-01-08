using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaysUI : MonoBehaviour
{
    private Ways way = null;

    private LineRenderer line;
    private GameObject dot1;
    private GameObject dot2;

    private bool isConnected;
    private int connectTime;

    void Start()
    {
        isConnected = false;
        connectTime = 0;
    }

    public void setWayModel(Ways p_ways)
    {
        way = p_ways;
    }

    public void createUI()
    {
        connectTime = 0;

        line = GetComponent<LineRenderer>();

        dot1 = transform.GetChild(0).gameObject;
        dot2 = transform.GetChild(1).gameObject;

        dot1.GetComponent<SpriteRenderer>().color = ThemeChanger.current.dotColor;
        dot2.GetComponent<SpriteRenderer>().color = ThemeChanger.current.dotColor;

        Color c = new Color(0.886f, 0.886f, 0.886f);

        if (way.pathTag > 1)
        {
            c = Color.red;
            c.a = 0.6f;
        }

        line.startColor = c;
        line.endColor = c;

        line.positionCount = 2;

        Vector3 posStart = GridManager.GetGridManger().GetPosForGrid(way.startingGridPosition);
        Vector3 posEnd = GridManager.GetGridManger().GetPosForGrid(way.endGridPositon);

        dot1.transform.position = posStart;
        dot2.transform.position = posEnd;

        line.SetPosition(0, posStart);
        line.SetPosition(1, posEnd);
    }

    public Vector3 pointOnLine()
    {
        Vector3 posStart = line.GetPosition(0);
        Vector3 posEnd = line.GetPosition(1);

        float xDiff = (posEnd.x - posStart.x) * (posEnd.x - posStart.x);
        float yDiff = (posEnd.y - posStart.y) * (posEnd.y - posStart.y);

        float finalVar = xDiff + yDiff;
        float dis = Mathf.Sqrt(finalVar);

        float dt = dis / 5;

        float t = dt / dis;
        float xt = ((1 - t) * posStart.x) + (t * posEnd.x);
        float yt = ((1 - t) * posStart.y) + (t * posEnd.y);

        return new Vector3(xt, yt, 0);
    }

    public void chageColor(Object child)
    {
        if (!isConnected)
        {
            connectTime++;

            if (connectTime >= way.pathTag)
            {
                isConnected = true;

                Color c = ThemeChanger.current.lineColor;

                line.startColor = c;
                line.endColor = c;

                Sound.instance.Play(Sound.Others.Line);
            }
            else
            {
                Color c = line.startColor;
                c.a += 0.2f;

                line.startColor = c;
                line.endColor = c;
            }

            GameObject.FindObjectOfType<PathReader>().pushConnectedPath(gameObject);
            GameObject childGo = (GameObject)child;

            GameObject.FindObjectOfType<DotAnimation>().setEnableAtPosition(true, childGo.transform.position);
        }

        if (CheckForWins())
        {
            GameObject.FindObjectOfType<UIControllerForGame>().ShowAnimationOnAllNodes();
        }
    }

    public Ways getWayModel()
    {
        return way;
    }

    public void childCount(Object obj)
    {
        if (isConnected)
            return;

        GameObject go = (GameObject)obj;

        int count = transform.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);

            if (!child.gameObject.Equals(go))
            {
                if (startingToEnd(i))
                {
                    GameObject.FindObjectOfType<PathReader>().readOtherChild(child.gameObject);
                }
            }
        }
    }

    // check wheter you are allowed to move from one point to another
    public bool startingToEnd(int indexOfTarget)
    {
        if (way.direction > 1)
        { // greate then 1 mean it directive
            if (indexOfTarget == 0)
            {
                return false;
            }
        }

        return true;
    }

    public void moveBack()
    {
        if (isConnected)
        {
            isConnected = false;
        }
        if (connectTime > 0)
        {
            connectTime--;
        }

        if (way.pathTag > 1)
        {
            Color c = line.startColor;

            c.a -= 0.2f;

            line.startColor = c;
            line.endColor = c;
        }
        else
        {
            Color c = new Color(0.886f, 0.886f, 0.886f);

            line.startColor = c;
            line.endColor = c;
        }

        int count = transform.childCount;

        GameObject.FindObjectOfType<DotAnimation>().revertPrvState();
        Vector3 dotAnimationPos = GameObject.FindObjectOfType<DotAnimation>().transform.position;

        for (int i = 0; i < count; i++)
        {
            Transform childGo = transform.GetChild(i);

            if (!childGo.position.Equals(dotAnimationPos))
            {
                GameObject.FindObjectOfType<DotAnimation>().transform.position = childGo.position;
                break;
            }
        }
    }

    public static void LoadSceneAagin()
    {
        SceneManager.LoadScene(1);
    }

    public static bool CheckForWins()
    {
        WaysUI[] allWays = GameObject.FindObjectsOfType<WaysUI>();

        int count = allWays.Length;
        for (int i = 0; i < count; i++)
        {
            if (!allWays[i].isConnected)
            {
                return false;
            }
        }

        return true;
    }

    public Vector3 childPos(int indexChild)
    {
        return transform.GetChild(indexChild).position;
    }
}
