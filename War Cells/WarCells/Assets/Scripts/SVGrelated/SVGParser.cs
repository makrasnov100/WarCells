using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml.Linq;
using UnityEngine;

public class SVGParser : MonoBehaviour
{
    //Reference to svg assets used
    public TextAsset SVGFile; //TODO: implememt drag and drop ability
    public GameObject svgPoint;
    string svgContent;

    //Image Properties
    // - sizes
    private double scaleX;
    private double scaleY;
    public double newScaleX;
    public double newScaleY;
    public float stretchX;
    public float stretchY;
    // - points
    public float pointScale;
    public float lineScale;
    public float varienceRadius;
    // - svg positioning
    public Vector2 initialOffset;
    public Vector2 offset;
    public int lineSortingOrder;
    public int pointSortingOrder;
    // - color
    public Color color;

    //Intastance variables
    List<Vector2> svgPositions = new List<Vector2>();
    List<GameObject> svgPoints = new List<GameObject>();
    List<bool> isOrigin = new List<bool>();


    // Start is called before the first frame update
    void Start()
    {
        if (IsSVGReadable())
            ParseSVGVectors();
    }

    //IsSVGReadable: Checks precconditions required for script to get needed info
    bool IsSVGReadable()
    {
        // Check if file exists
        if (SVGFile == null)
            return false;

        svgContent = SVGFile.text;

        if (!svgContent.Contains("<path d="))
            return false;

        return true;
    }

    //ParseSVGVectors: parses svg and creates vector points of shapes
    private void ParseSVGVectors()
    {
        //Get Current Scale of SVG
        // - horizontal
        int startScaleXIdx = svgContent.IndexOf("width=") + 7;
        int scaleXIdxLength = svgContent.Substring(startScaleXIdx, 9).IndexOf("\"");
        scaleX = Convert.ToDouble(svgContent.Substring(startScaleXIdx, scaleXIdxLength));
        // - vertical
        int startScaleYIdx = svgContent.IndexOf("height=") + 8;
        int scaleYLength = svgContent.Substring(startScaleYIdx, 9).IndexOf("\"");
        scaleY = Convert.ToDouble(svgContent.Substring(startScaleYIdx, scaleYLength));
        // - debug
        //print("SVG scale at start - x: " + scaleX + " y: " + scaleY);


        int startPathIdx = svgContent.IndexOf("path d=") + 8;
        int endPathIdx = svgContent.Substring(startPathIdx, svgContent.Length - startPathIdx).IndexOf("\"");
        string cords = svgContent.Substring(startPathIdx, endPathIdx);
        string[] cordsSplit = cords.Split(' ');


        stretchX = (float)newScaleX / (float)scaleX;
        stretchY = (float)newScaleY / (float)scaleY;
        double totalX = 0;
        double totalY = 0;
        string output = "";
        for(int i = 0; i < cordsSplit.Length; i++)
        {
            output += cordsSplit[i] + Environment.NewLine;

            //check if need to skip a letter (filling edge)
            int letterIdx = 0;
            if (cordsSplit[i][letterIdx] == 'z' || cordsSplit[i][letterIdx] == 'Z')
                letterIdx++;
            //check if reached end of string
            if (cordsSplit[i].Length == 1)
                continue;
            //check if start of new shape (new origin)
            if (cordsSplit[i][letterIdx] == 'm' || cordsSplit[i][letterIdx] == 'M')
                isOrigin.Add(true);
            else
                isOrigin.Add(false);
            //add coordinates for cur shape

            if (cordsSplit[i][letterIdx] == 'c' || cordsSplit[i][letterIdx] == 'C')
            {
                i += 2;
                letterIdx = 0;
            }
            else
            {
                letterIdx++;
            }
            string[] xyCords = cordsSplit[i].Substring(letterIdx, cordsSplit[i].Length - letterIdx).Split(',');
            double curX = (float)Convert.ToDouble(xyCords[0]) * stretchX;
            double curY = -(float)Convert.ToDouble(xyCords[1]) * stretchY;
            totalX += curX;
            totalY += curY;
            svgPositions.Add(new Vector2((float)curX, (float)curY));
        }
        //print(output);

        //Find + apply initial offset before making connections
        initialOffset = new Vector2((float)totalX / (float)svgPositions.Count, (float)totalY / (float)svgPositions.Count);
        for (int i = 0; i < svgPositions.Count; i++)
        {
            svgPositions[i] -= initialOffset - offset;
        }

        GameObject curOrigin = null;
        int curOriginIdx = 0;
        for (int i = 0; i < svgPositions.Count; i++)
        {
            if (isOrigin[i])
            {
                if (curOrigin != null)
                {
                    LineRenderer lr = curOrigin.AddComponent<LineRenderer>();
                    lr.positionCount = i - curOriginIdx;
                    UpdateLinePoints ulp = curOrigin.AddComponent<UpdateLinePoints>();
                    for (int p = 0; p < lr.positionCount; p++)
                    {
                        lr.SetPosition(p, svgPositions[p + curOriginIdx]);
                        ulp.AddPoint(svgPoints[p + curOriginIdx]);
                    }
                    lr.startWidth = lineScale;
                    lr.endWidth = lineScale;
                    lr.startColor = color;
                    lr.endColor = color;
                    lr.sortingOrder = lineSortingOrder;
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                }
                curOriginIdx = i;
                curOrigin = Instantiate(svgPoint, svgPositions[i], new Quaternion(), transform);
                curOrigin.GetComponent<SpriteRenderer>().color = color;
                curOrigin.GetComponent<SpriteRenderer>().sortingOrder = pointSortingOrder;
                svgPoints.Add(curOrigin);
                LerpInRadius lir = curOrigin.AddComponent<LerpInRadius>();
                lir.Construct(varienceRadius, 4f, 5f);
                curOrigin.transform.localScale *= pointScale;
                curOrigin.name = "svgORIGIN";
            }
            else
            {
                GameObject curPoint = Instantiate(svgPoint, svgPositions[i], new Quaternion(), transform);
                curPoint.GetComponent<SpriteRenderer>().color = color;
                curPoint.GetComponent<SpriteRenderer>().sortingOrder = pointSortingOrder;
                svgPoints.Add(curPoint);
                LerpInRadius lir = curPoint.AddComponent<LerpInRadius>();
                lir.Construct(varienceRadius, 4f, 5f);
                curPoint.transform.localScale *= pointScale;
            }
        }
        if (curOrigin != null)
        {
            LineRenderer lr = curOrigin.AddComponent<LineRenderer>();
            lr.positionCount = svgPositions.Count - curOriginIdx;
            UpdateLinePoints ulp = curOrigin.AddComponent<UpdateLinePoints>();
            for (int p = curOriginIdx; p < svgPositions.Count; p++)
            {
                lr.SetPosition(p - curOriginIdx, svgPositions[p]);
                ulp.AddPoint(svgPoints[p]);
            }
            lr.startWidth = lineScale;
            lr.endWidth = lineScale;
            lr.startColor = color;
            lr.endColor = color;
            lr.sortingOrder = lineSortingOrder;
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
}
