using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public static bool init_game;
    public static bool game_loop;

    public float camera_top;
    public float camera_side;

    public int platforms_num;
    private List<GameObject> platforms;

    public int ground_5_num;
    private List<GameObject> grounds_5;

    public int spawn_time;
    public int spawn_ground_5_time;
    private int time_count;
    private int time_count_g5;

    public GameObject[] obj_instances;
    public GameObject[] spawns;

    public GameObject player;
    public Text game_over;
    public Text score;

    private int score_val;

    private AudioSource audio_source;
    public AudioClip game_over_clip;


    // Use this for initialization
    void Start()
    {
        init_game = false;
        game_loop = false;

        time_count = 0;
        time_count_g5 = 0;

        score_val = 0;

        if (platforms_num < 3)
            platforms_num = 3;

        platforms = new List<GameObject>();
        platforms.Capacity = platforms_num;

        grounds_5 = new List<GameObject>();
        grounds_5.Capacity = ground_5_num;

        //adiciona os dois primeiros blocos a lista platforms
        platforms.AddRange(GameObject.FindGameObjectsWithTag("platform"));

        audio_source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (game_loop)
        {
            time_count++;
            time_count_g5++;
            instantiatePlatform();
            instantiateGround5();
            movePlatforms(platforms);
            movePlatforms(grounds_5);

            if (player.transform.position.y < -camera_top - 2)
                gameOver();
        }
    }

    private void movePlatforms(List<GameObject> platforms)
    {
        for(int i=0; i<platforms.Count; i++)
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

            platforms[i].transform.Translate(0, UnityEngine.Random.Range(-0.009f, -0.025f), 0);

            if (platforms[i].transform.position.y < -camera_top - 2.5)
            {
                GameObject remove = platforms[i];

                platforms.RemoveAt(i);

                player.transform.parent = null;
                Destroy(remove.gameObject);

                score_val++;
                score.text = score_val.ToString();
            }
        }
    }

    private void instantiatePlatform()
    {
        //a condicao abaixo faz aparecer plataformas a partir do topo da camera
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        if ((platforms.Count < platforms_num) && (time_count >= spawn_time))
        {
            time_count = 0;

            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

            platforms.Add(Instantiate(obj_instances[UnityEngine.Random.Range(0, 12)],
                new Vector3(spawn_x(5), camera_top+1, -1),
                new Quaternion()));
        }
    }

    private void instantiateGround5()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        if ((grounds_5.Count < ground_5_num) && (time_count_g5 >= spawn_ground_5_time))
        {
            time_count_g5 = 0;

            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

            grounds_5.Add(Instantiate(obj_instances[12],
                new Vector3(spawn_x(5), camera_top, -1),
                new Quaternion()));
        }
    }

    private float spawn_x(int amt)
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        int it;

        while (true)
        {
            it = UnityEngine.Random.Range(0, amt);

            if (spawns[it].tag == "Respawn")
            {
                spawns[it].tag = "Untagged";
                return spawns[it].transform.position.x;
            }
            else if (amt > 0) 
            {
                amt--;            
            }
            else
            {
                foreach (GameObject obj in spawns)
                {
                    obj.tag = "Respawn";
                }
            }
        }
    }

    private void gameOver()
    {
        audio_source.clip = game_over_clip;
        audio_source.Play();

        game_over.color = new Color(game_over.color.r, game_over.color.g, game_over.color.b, 1.0f);
        game_loop = false;
        Destroy(player.gameObject);
    }
}