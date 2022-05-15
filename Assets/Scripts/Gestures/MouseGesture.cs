using System;
using System.Collections.Generic;
using UnityEngine;

public class MouseGesture : MonoBehaviour
{
    //public Text drawText;
    //public Text CostText;

    [Range(0, 32)]
    public int DEFAULT_NB_SECTORS = 32;   // Number of sectors
    public const float DEFAULT_PRECISION = 0;    // Precision of catpure in pixels
    public const uint DEFAULT_FIABILITY = 30;   // Default fiability level

    public List<float> MoveString = new List<float>();     // Mouse gestures
    private Vector2 lastPoint = Vector2.zero;              // Last mouse point

    protected float sectorRad;                             // Angle of one sector		
    protected List<float> anglesMap = new List<float>();

    public List<Vector2> DrawPoints = new List<Vector2>();

    private void Start()
    {
        //StartCapture();
        BuildAnglesMap();
    }

    //public string SearchGest(List<float> moves)
    //{
    //    string m = null;
    //    int sCost = 1000;
    //    string bestMatch = null;
    //    foreach (float f in moves)
    //    {
    //        m += f.ToString();
    //    }

    //    if (Gestures.list.Count > 0)
    //    {
    //        foreach (var g in Gestures.list)
    //        {
    //            for (int i = 0; i < g.Value.Count; i++)
    //            {
    //                int cost = LevenshteinDistance(m, g.Value[i]);
    //                if (cost <= DEFAULT_FIABILITY)
    //                {
    //                    if (cost < sCost)
    //                    {
    //                        sCost = cost;
    //                        bestMatch = g.Key;
    //                    }
    //                }
    //            }
    //        }

    //        if (drawText != null && CostText != null)
    //        {
    //            drawText.text = string.Concat("Desenho: ", bestMatch);
    //            CostText.text = string.Concat("Custo: ", sCost.ToString());
    //        }

    //        //print("Desenho: " + bestMatch);
    //        return bestMatch;
    //    }
    //    else
    //    {
    //        print("Sem Objetos Cadastrados!");
    //        return null;
    //    }
    //}

    protected void BuildAnglesMap()
    {
        // Angle of one sector
        /*
         * PI * 2 = Complete Circle
         */
        sectorRad = (Mathf.PI * 2) / DEFAULT_NB_SECTORS;

        // Map containing sectors no from 0 to PI*2
        anglesMap.Clear();

        // The precision is Mathf.PI*2/100
        float step = Mathf.PI * 2 / 100;

        // Memorize sectors
        float sector;
        for (float i = -sectorRad / 2; i <= Math.PI * 2 - sectorRad / 2; i += step)
        {
            sector = Mathf.Floor((i + sectorRad / 2) / sectorRad);
            anglesMap.Add(sector);
        }
    }

    public string CaptureHandler(Vector2 mousePos)
    {
        // calcul dif 
        float difx = (mousePos.x - lastPoint.x);
        float dify = (mousePos.y - lastPoint.y);

        float sqDist = difx * difx + dify * dify;
        float sqPrec = DEFAULT_PRECISION * DEFAULT_PRECISION;

        string retVal = null;
        if (sqDist > sqPrec)
        {
            retVal = AddMove(difx, dify);
            lastPoint.x = mousePos.x;
            lastPoint.y = mousePos.y;
        }

        return retVal;
    }

    protected string AddMove(float dx, float dy)
    {
        float angle = Mathf.Atan2(dy, dx) + sectorRad / 2;
        if (angle < 0)
            angle += Mathf.PI * 2;
        int no = Mathf.FloorToInt(angle / (Mathf.PI * 2) * 100);
        MoveString.Add(anglesMap[no]);
        //print(anglesMap[no]);
        return anglesMap[no].ToString();
    }

    public void StartCapture()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward * 5;
        // Moves
        MoveString.Clear();
        // Last point
        lastPoint = new Vector2(mousePos.x, mousePos.y);
    }

    public void ResetMoveString(Vector2 lastP)
    {
        MoveString.Clear();
        lastPoint = lastP;
    }

    public void ResetMoveString(Vector2 lastP, int index)
    {
        MoveString.RemoveAt(index);
        lastPoint = lastP;
    }

    protected int DifAngle(int a, int b)
    {
        int dif = Mathf.Abs(a - b);
        if (dif > DEFAULT_NB_SECTORS / 2)
        {
            dif = (int)DEFAULT_NB_SECTORS - dif;
        }
        return dif;
    }

    protected int[][] Fill2DTable(int w, int h, int f)
    {
        int[][] o = new int[w][];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                o[x] = new int[h];
            }
        }
        return o;
    }

    public int CostLeven(string a, string b)
    {
        // point
        if (a[0] == -1)
        {
            return b.Length == 0 ? 0 : 100000;
        }

        // precalc difangles
        int[][] d = Fill2DTable(a.Length + 1, b.Length + 1, 0);
        int[][] w = new int[d.Length][];
        Array.Copy(d, w, d.Length);

        int x;
        int y;
        for (x = 1; x <= a.Length; x++)
        {
            for (y = 1; y < b.Length; y++)
            {
                d[x][y] = DifAngle(a[x - 1], b[y - 1]);
            }
        }

        // max cost
        for (y = 1; y <= b.Length; y++) w[0][y] = 100000;
        for (x = 1; x <= a.Length; x++) w[x][0] = 100000;
        w[0][0] = 0;

        // levensthein application
        int cost = 0;
        int pa;
        int pb;
        int pc;

        for (x = 1; x <= a.Length; x++)
        {
            for (y = 1; y < b.Length; y++)
            {
                cost = d[x][y];
                pa = w[x - 1][y] + cost;
                pb = w[x][y - 1] + cost;
                pc = w[x - 1][y - 1] + cost;
                w[x][y] = Mathf.Min(Mathf.Min(pa, pb), pc);
            }
        }

        return w[x - 1][y - 1];
    }

    public int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Mathf.Min(Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }

    public string ListToString()
    {
        string value = null;
        for (int i = 0; i < MoveString.Count; i++)
        {
            if (i != MoveString.Count - 1)
                value = value + MoveString[i] + "-";
            else
                value = value + MoveString[i];
        }

        return value;
    }
}
