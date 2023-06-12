using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaneGenerator : MonoBehaviour {
    public float maxVariance = 0f;
    public int count = 16;
    public GameObject InitialColumn;
    public Transform ball;
    public GameObject targetPrefab;
    public List<Color32> colorList;

    public List<GameObject> m_ColumnList;
    private int m_OldestColumnIndex;
    private int m_NewestColumnIndex;
    private float m_LastAppliedBallZ;
    private float m_Hue;
    private float saturation;
    private int targetCounter;
    private float m_LaneSize;
    int targetFrequency = 20;
    int currentFogColor;
    Color32 currentColor;
    int currentColorIndex;

    // Use this for initialization
    void Start () {
        currentColorIndex = 0;
        currentColor = colorList[0];
        // Record lane width, which allows accurate positioning
        m_LaneSize = InitialColumn.transform.localScale.z;
        // Creat list of columns
        m_ColumnList = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            // First one exists, just add it and set it's color
            if (i == 0)
            {
                m_ColumnList.Add(InitialColumn);
                SetColor(0);
            }
            // Others are created & added. Color is set by PlaceColumn
            else
            {
                GameObject newColumn = Instantiate(InitialColumn);
                newColumn.transform.parent = InitialColumn.transform.parent;
                m_ColumnList.Add(newColumn);
                PlaceColumn(m_ColumnList.Count - 1, m_ColumnList[m_ColumnList.Count - 2].transform.position, maxVariance);
            }
        }
        // Start hue value, increment for each new row
        m_Hue = 0.5f;

        // Reference indexes for the newest and oldest columns. We move the oldest, and put it next to the newest.
        m_OldestColumnIndex = 0;
        m_NewestColumnIndex = m_ColumnList.Count - 1;
        currentFogColor = count / 2;
        StartCoroutine(delayChangeColor());
        StartCoroutine(delayChangeFogColor());
    }
	
	// Update is called once per frame
	void Update () {
        // Check if the ball position has move a full column unit. If so, move the oldest to the next available position. Happens off screen.
		if (ball.position.z > m_LastAppliedBallZ + m_LaneSize*60)
        {
            PlaceColumn(m_OldestColumnIndex, m_ColumnList[m_NewestColumnIndex].transform.position, maxVariance);
            
            // Update newest and oldest rows.
            m_NewestColumnIndex = m_OldestColumnIndex;
            m_OldestColumnIndex = (m_OldestColumnIndex + 1) % m_ColumnList.Count;
            m_LastAppliedBallZ += m_LaneSize;
        }
	}

    private void PlaceColumn(int columnIndex, Vector3 prevColumnPos, float maximumVariance )
    {
        // Get the desired position, one column width downstream from the latest column
        float offsetY = 0;
        //if (columnIndex % 2 == 0)
        //{
        //    offsetY = prevColumnPos.y + maximumVariance;          
        //}
        //else
        //{
        //    offsetY = prevColumnPos.y - maximumVariance;
        //}
        offsetY = Random.Range(prevColumnPos.y - maximumVariance, prevColumnPos.y + maximumVariance);
        if(offsetY > 2 || offsetY < -2)
        {
            offsetY = 0;
        }
        Vector3 movePos = new Vector3(prevColumnPos.x, offsetY, prevColumnPos.z + m_LaneSize);
        // Move to that new position
        m_ColumnList[columnIndex].transform.position = movePos;
        // Set new color (with increment hue)
        SetColor(columnIndex);
        // Keep track of how many rows are generated since the last target.
        targetCounter++;
        // If the row contains a target, remove it.
        if (m_ColumnList[columnIndex].transform.Find(columnIndex.ToString()))
        {
            Destroy(m_ColumnList[columnIndex].transform.Find(columnIndex.ToString()).gameObject);
        }
        // If it's time to add a target, do so.
        targetFrequency = Random.Range(2, 10);
        if (targetCounter >= targetFrequency)
        {
            var target = Instantiate(targetPrefab);
            target.transform.parent = m_ColumnList[columnIndex].transform;
            // Name it specifically so we can detect it for removal later.
            target.name = columnIndex.ToString();
            target.transform.localPosition = new Vector3(0, 16f, 0);
            target.transform.localScale = new Vector3(0.06f, 0.3f, 0.06f);
            //Reset counter
            targetCounter = 0;
        }

        var backgroundObject = m_ColumnList[columnIndex].transform.GetChild(2);
        backgroundObject.transform.localPosition = new Vector3(Random.Range(-100, -20), Random.Range(-50, 10), 0);
        backgroundObject.transform.localScale = new Vector3(Random.Range(1, 3), Random.Range(5, 15), Random.Range(1, 3));
        //backgroundObject.transform.DORotate(new Vector3(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)), 5);
        currentFogColor++;
        if(currentFogColor > m_ColumnList.Count - 1)
        {
            currentFogColor = 0;
        }
    }

    private void SetColor(int columnIndex)
    {
        // increment the hue, looping back to 0 if necessary
        //m_Hue += (1f / 250f);
        //if (m_Hue > 1f)
        //    m_Hue = 0;
        // Default saturation
        //saturation = .8f;
        // Alternative saturation for even numbers
        //if (columnIndex % 2 == 0)
        //{
        //    saturation = .5f;
        //}
        // Set color for each child's renderer
        foreach (MeshRenderer renderer in m_ColumnList[columnIndex].GetComponentsInChildren<MeshRenderer>())
        {
            //renderer.material.color = Color.HSVToRGB(m_Hue, saturation, .7f);
            renderer.material.color = currentColor;
        }
        //RenderSettings.fogColor = Color.HSVToRGB(m_Hue, saturation, .7f);
    }

    IEnumerator delayChangeFogColor()
    {
        yield return new WaitForSeconds(2);
        var renderer = m_ColumnList[currentFogColor].GetComponentInChildren<MeshRenderer>();
        var color = renderer.material.color;
        DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, color/*Color.HSVToRGB(m_Hue, saturation, .7f)*/, 3f);
        StartCoroutine(delayChangeFogColor());
    }

    IEnumerator delayChangeColor()
    {
        yield return new WaitForSeconds(10);
        currentColorIndex++;
        if(currentColorIndex > colorList.Count - 1)
        {
            currentColorIndex = 0;
        }
        currentColor = colorList[currentColorIndex];
    }
}
