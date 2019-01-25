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
    public double scaleX;
    public double scaleY;
    public double newScaleX;
    public double newScaleY;
    public float stretchX;
    public float stretchY;
    public float pointScale;
    public float lineScale;
    public Vector2 initialOffset;
    public Vector2 offset;

    //Intastance variables
    List<Vector2> svgPositions = new List<Vector2>();
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
        foreach (string s in cordsSplit)
        {
            //check if need to skip a letter (filling edge)
            int letterIdx = 0;
            if (s[letterIdx] == 'z' || s[letterIdx] == 'Z')
                letterIdx++;
            //check if reached end of string
            if (s.Length == 1)
                continue;
            //check if start of new shape (new origin)
            if (s[letterIdx] == 'm' || s[letterIdx] == 'M')
                isOrigin.Add(true);
            else
                isOrigin.Add(false);
            //add coordinates for cur shape
            string[] xyCords = s.Substring(letterIdx + 1, s.Length - (letterIdx + 1)).Split(',');
            double curX = (float)Convert.ToDouble(xyCords[0]) * stretchX;
            double curY = -(float)Convert.ToDouble(xyCords[1]) * stretchY;
            totalX += curX;
            totalY += curY;
            svgPositions.Add(new Vector2((float)curX, (float)curY));
        }
        //Find + apply initial offset before making connections
        initialOffset = new Vector2((float)totalX / (float)svgPositions.Count, (float)totalY / (float)svgPositions.Count);
        for (int i = 0; i < svgPositions.Count; i++)
        {
            svgPositions[i] -= initialOffset + offset;
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
                    for (int p = 0; p < lr.positionCount; p++)
                    {
                        lr.SetPosition(p, svgPositions[p + curOriginIdx]);
                    }
                    lr.startWidth = lineScale;
                    lr.endWidth = lineScale;
                    lr.startColor = Color.white;
                    lr.endColor = Color.white;
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                }
                curOriginIdx = i;
                curOrigin = Instantiate(svgPoint, svgPositions[i], new Quaternion(), transform);
                curOrigin.transform.localScale *= pointScale;
                curOrigin.name = "svgORIGIN";
            }
            else
            {
                GameObject curPoint = Instantiate(svgPoint, svgPositions[i], new Quaternion(), transform);
                curPoint.transform.localScale *= pointScale;
            }
        }
        if (curOrigin != null)
        {
            LineRenderer lr = curOrigin.AddComponent<LineRenderer>();
            lr.positionCount = svgPositions.Count - curOriginIdx;
            for (int p = curOriginIdx; p < svgPositions.Count; p++)
            {
                lr.SetPosition(p - curOriginIdx, svgPositions[p]);
            }
            lr.startWidth = lineScale;
            lr.endWidth = lineScale;
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
}
