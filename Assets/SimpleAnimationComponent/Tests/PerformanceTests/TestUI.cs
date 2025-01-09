using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour {

    public GameObject[] Categories;
    public float fps; // 积累的平均帧率
    public int fpssamples; // 积累的样本数量
    public int numInstances;
    private List<GameObject> m_Instances;
    // Use this for initialization
    void Start ()
    {
        m_Instances = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        if (Categories == null)
            return;

        GUILayout.BeginHorizontal();
        foreach(var category in Categories)
        {
            if (GUILayout.Button(category.name))
            {
                fpssamples = 0;
                foreach (var instance in m_Instances)
                {
                    Destroy(instance);
                }

                for (int i = 0; i < numInstances; i++)
                {
                    m_Instances.Add(Instantiate(category) as GameObject);
                }
            }
        }
        GUILayout.EndHorizontal();

        var fpsCounter = new Rect(new Vector2(0.9F * Screen.width, 0.1f * Screen.height), new Vector2(0.1F * Screen.width, 0.1f * Screen.height));
        
        if (Time.deltaTime > 0)
        {
            // 瞬时帧率
            float framerate = 1 / Time.deltaTime;
            // 加权平均算法，用以平滑帧率的显示
            // 1 * framerate：当前帧的帧率，权重为 1
            // fpssamples * fps：之前累积的平均帧率，权重为 fpssamples
            // 1 + fpssamples：总权重，即当前帧和之前所有帧的权重之和
            fps = (1 * framerate + fpssamples * fps) / (1 + fpssamples);
        }
        GUI.Label(fpsCounter, $"FPS: {fps:F2}");
        fpssamples = Mathf.Min(1000, fpssamples + 1); // 采样数不超过1000
    }
}
