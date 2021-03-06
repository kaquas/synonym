﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class alpacaManager : MonoBehaviour
{
    [SerializeField]
    GameObject kubiprefab = default;
    [SerializeField]
    GameObject kubinearhead = default;
    [SerializeField]
    GameObject atama = default;
    [SerializeField]
    GameObject tamaprefab = default;

    Vector3 defaultpos_atama;
    Vector3 defaultpos_kubi;

    const float kubiupspeed = 0.01f;
    const float kubidownspeed = 0.01f;

    int addkubinum = 0;
    float kubisizey;
    List<GameObject> kubis = new List<GameObject>();

    float pushreturnkeyframes = 0f;

    float spilframe = 0f;
    const float spilthreshold = 15f;

    public static bool isulting = false;

    public static float ultpoint = 0f;
    public const float ultthreshold = 500f;

    private void Start()
    {
        defaultpos_atama = atama.transform.position;
        defaultpos_kubi = kubinearhead.transform.position;

        kubis.Add(kubinearhead);

        kubisizey = kubiprefab.GetComponent<SpriteRenderer>().bounds.size.y;
    }

    private void Update()
    {
        if (!isulting) //ult中は入力を受け付けない
        {
            if (Input.GetKey(KeyCode.Space)) //首伸ばす
            {
                if (atama.transform.position.y <= Utility.getScreenHeight() / 2)
                {
                    foreach (var kubi in kubis)
                    {
                        kubi.transform.position += new Vector3(0, kubiupspeed);
                    }
                    atama.transform.position += new Vector3(0, kubiupspeed);
                }
            }
            else //首縮める
            {
                if (defaultpos_atama.y < atama.transform.position.y)
                {
                    foreach (var kubi in kubis)
                    {
                        kubi.transform.position -= new Vector3(0, kubidownspeed);
                    }
                    atama.transform.position -= new Vector3(0, kubidownspeed);
                }
            }

            //首減らす
            if (kubinearhead.transform.position.y - defaultpos_kubi.y <= kubisizey / 2 * (addkubinum - 1))
            {
                addkubinum--;
                Destroy(kubis[kubis.Count - 1]);
                kubis.RemoveAt(kubis.Count - 1);
            }

            //首増やす
            if (kubinearhead.transform.position.y - defaultpos_kubi.y > kubisizey / 2 * addkubinum)
            {
                addkubinum++;
                kubis.Add(Instantiate(kubiprefab, kubinearhead.transform.position - new Vector3(0, kubisizey / 2 * addkubinum), Quaternion.identity));
            }

            //弾発射
            if (Input.GetKeyUp(KeyCode.Return))
            {
                if (spilframe >= spilthreshold)
                {
                    GameObject obj = Instantiate(tamaprefab, atama.transform.position, Quaternion.identity);
                    obj.GetComponent<spitController>().spit_initialize(pushreturnkeyframes);

                    ultpoint = Mathf.Min(ultpoint + pushreturnkeyframes, ultthreshold);

                    pushreturnkeyframes = 0f;
                    spilframe = 0f;
                }
            }

            //ult
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (ultpoint >= ultthreshold)
                {
                    ultpoint = 0;
                    isulting = true;
                    StartCoroutine(ult());
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isulting)
        {
            //弾発射用のフレーム数計測
            if (Input.GetKey(KeyCode.Return))
            {
                pushreturnkeyframes++;
            }
        }

        //発射間隔用のフレーム数
        spilframe++;
    }

    IEnumerator ult()
    {
        while (true)
        {
            //てっぺんの倍まで伸ばす
            if (atama.transform.position.y <= Utility.getScreenHeight())
            {
                foreach (var kubi in kubis)
                {
                    kubi.transform.position += new Vector3(0, kubiupspeed * 3f);
                }
                atama.transform.position += new Vector3(0, kubiupspeed * 3f);

                //首増やす
                if (kubinearhead.transform.position.y - defaultpos_kubi.y > kubisizey / 2 * addkubinum)
                {
                    addkubinum++;
                    kubis.Add(Instantiate(kubiprefab, kubinearhead.transform.position - new Vector3(0, kubisizey / 2 * addkubinum), Quaternion.identity));
                }

                yield return null;
            }
            else
            {
                break;
            }
        }

        float theta = Mathf.PI / 2f;
        while (true)
        {
            //てっぺんから右に回す
            if (theta >= 0)
            {
                theta -= 0.004f;
                Vector3 pos;
                foreach (var kubi in kubis)
                {
                    pos = kubi.transform.position - defaultpos_kubi + new Vector3(0, kubisizey / 2f);
                    kubi.transform.position = defaultpos_kubi + new Vector3(pos.magnitude * Mathf.Cos(theta), pos.magnitude * Mathf.Sin(theta)) - new Vector3(0, kubisizey / 2f);
                    kubi.transform.rotation = Quaternion.Euler(0f, 0f, theta * 180f / Mathf.PI - 90f);
                }
                pos = atama.transform.position - defaultpos_kubi + new Vector3(0, kubisizey / 2f);
                atama.transform.position = defaultpos_kubi + new Vector3(pos.magnitude * Mathf.Cos(theta), pos.magnitude * Mathf.Sin(theta)) - new Vector3(0, kubisizey / 2f);
                atama.transform.rotation = Quaternion.Euler(0f, 0f, theta * 180f / Mathf.PI - 90f);

                yield return null;
            }
            else
            {
                //色々元に戻す
                addkubinum = 0;

                for(int i=kubis.Count-1; i>0; i--)
                {
                    Destroy(kubis[i]);
                }
                kubis.Clear();
                kubis.Add(kubinearhead);
                
                atama.transform.position = defaultpos_atama;
                kubinearhead.transform.position = defaultpos_kubi;
                atama.transform.rotation = Quaternion.identity;
                kubinearhead.transform.rotation = Quaternion.identity;

                isulting = false;
                break;
            }
        }
    }

}
