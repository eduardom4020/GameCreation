using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI_General : MonoBehaviour {

    private Text text;

	// Use this for initialization
	void Start ()
    {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if((GameController.init_game) && (!GameController.game_loop))
        {
            if (text.tag != "instructions")
                text.text = "Vamos começar o jogo!";

            StartCoroutine(waitAndDestroyText(2));
        }
    }

    public IEnumerator waitAndDestroyText(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(text.gameObject);
        GameController.game_loop = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            text.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            text.color = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        }
    }
}
