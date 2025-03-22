using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BuffManager : MonoBehaviour
{
    public static BuffManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public GameObject BuffIconPrefab;
    Dictionary<string, Sprite> ImageList = new Dictionary<string, Sprite>();
    public Skill[] Skills;
    public Sprite[] BuffImages;
    public List<GameObject> BuffList;

    private void Start()
    {
        BuffList = new List<GameObject>();
        for (int i = 0; i < BuffImages.Length; i++)
        {
            ImageList[Skills[i].skillName] = BuffImages[i];
        }
    }

    public void AddBuff(string skillName)
    {
        GameObject go = Instantiate(BuffIconPrefab, transform);
        go.GetComponent<BuffIconAnimation>().skillName = skillName;
        go.GetComponent<BuffIconAnimation>().duration = Skills.First(x => x.skillName == skillName).duration;
        go.GetComponent<Image>().sprite = ImageList[skillName];
        BuffList.Add(go);
    }

    public void RemoveBuff(string skillName)
    {
        GameObject target = null;
        foreach(GameObject go in BuffList)
        {
            if (go.GetComponent<BuffIconAnimation>().skillName == skillName)
            {
                target = go;
                break;
            }
        }
        if (target != null)
        {
            Destroy(target);
        }
    }
}
