using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//obs: falta adicionar o movimento. Temos apenas a animacao

public class Adventurer : MonoBehaviour
{
    public int maxSpeed;
    public int jumpPower;
    public float atenuation;
    public int slideMaxSpeed;
    public Vector2 wallJumpForce;
    public int speed;
    private int limitSpeed;    
    private bool canJump;
    private bool canDash;
    private bool canJumpDash;
    private bool horizontal_stop;
    private Animator animator;
    private Rigidbody2D body;
    private SpriteRenderer sprite;
    public AudioClip jump;
    public AudioClip dash;
    private AudioSource audio_player;

    private BoxCollider2D hit_box;

    public int dash_power;
    private int dash_speed;

    private bool max_dash_jump_actions;

    // Use this for initialization
    void Start ()
    {
        canJump = true;
        canDash = true;
        horizontal_stop = false;
        canJumpDash = false;
        max_dash_jump_actions = false;

        speed = 0;
        limitSpeed = maxSpeed;

        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        audio_player = GetComponent<AudioSource>();
        hit_box = GetComponent<BoxCollider2D>();
    }
	
	// Update is called once per frame
	void Update ()
    {

    //Comando de andar na horizontal
        if(Input.GetButton("Horizontal"))
        {
            horizontal_stop = false;

            //ganho de velocidade normal
            if ((Input.GetAxis("Horizontal") > 0) && (speed < limitSpeed))
                speed += maxSpeed / 15;

            else if ((Input.GetAxis("Horizontal") < 0) && (speed > -limitSpeed))
                speed -= maxSpeed / 15;

        //ganho de velocidade durante o pulo
            else if (animator.GetBool("isInAir"))
            {
                if ((Input.GetAxis("Horizontal") > 0) && (speed < limitSpeed))
                    speed += limitSpeed / 15;

                else if ((Input.GetAxis("Horizontal") < 0) && (speed > -limitSpeed))
                    speed -= limitSpeed / 15;
            }


            animator.SetInteger("speed", speed);
            body.AddForce(Vector2.right * speed * 300 * Time.deltaTime, ForceMode2D.Force);
        }

        if (Input.GetButtonUp("Horizontal"))
            horizontal_stop = true;

    //movimento parado com retardo
        if((horizontal_stop) && (speed != 0))
        {
            speed -= speed/15;
            animator.SetInteger("speed", speed);
            body.AddForce(Vector2.right * speed * 300 * Time.deltaTime);
            animator.SetInteger("speed", 0);

            if ((speed / 15) == 0)
            {
                horizontal_stop = false;
                speed = 0;
            }
        }
        

    //comando de pular
        if (Input.GetButtonDown("Jump"))
        {
            if (((!animator.GetBool("isInAir")) || (animator.GetBool("isDashing") && canJumpDash && (!max_dash_jump_actions))) && canJump)
            {
                audio_player.PlayOneShot(jump);
                canJump = false;
                body.AddForce(Vector2.up * jumpPower * 300 * Time.deltaTime);
            }

            //comando de pulo caso esteja deslizando
            if ((animator.GetBool("slide")) && canJump)
            {
                audio_player.PlayOneShot(jump);
                canJump = false;
                if (Input.GetAxis("Horizontal") > 0)
                {
                    body.AddForce(new Vector2(-wallJumpForce.x, wallJumpForce.y) * jumpPower * 300 * Time.deltaTime);
                    sprite.flipX = true;
                }
                if (Input.GetAxis("Horizontal") < 0)
                {
                    body.AddForce(new Vector2(wallJumpForce.x, wallJumpForce.y) * jumpPower * 300 * Time.deltaTime);
                    sprite.flipX = false;
                }
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            canJump = true;
        }

    //comando de dash
        if (Input.GetButtonDown("Dash"))
        {
            if((canDash) && (!animator.GetBool("isDashing")) && (!animator.GetBool("slide")) && (!canJumpDash) && (!max_dash_jump_actions))
            {
                audio_player.PlayOneShot(dash);
                canDash = false;
                canJumpDash = true;
                animator.SetBool("isDashing", true);
                dash_speed = dash_power;

                hit_box.size = new Vector2(hit_box.size.x, hit_box.size.y / 2);
                hit_box.offset = new Vector2(hit_box.offset.x, hit_box.offset.y - hit_box.size.y / 2);
                //OBS: Dash libera a opcao do personagem pular mais uma vez, isso estah implementado acima, na condicao jump 
            }
        }

        if (animator.GetBool("isDashing"))
        {
            //executa acao de dash
            if(!sprite.flipX)
                body.AddForce(Vector2.right * dash_speed * 300 * Time.deltaTime);
            else
                body.AddForce(Vector2.right * -dash_speed * 300 * Time.deltaTime);

            dash_speed -= dash_speed / 15;

            if ((dash_speed / 15) == 0)
            {
                max_dash_jump_actions = true;
                hit_box.offset = new Vector2(hit_box.offset.x, hit_box.offset.y + hit_box.size.y / 2);
                hit_box.size = new Vector2(hit_box.size.x, hit_box.size.y * 2);
                dash_speed = 0;
                animator.SetBool("isDashing", false);
                canJumpDash = false;
            }
        }

        if (Input.GetButtonUp("Dash"))
        {
            canDash = true;
        }

        //ao final do update atualiza a direcao do sprite
        if (speed > 0)
            sprite.flipX = false;
        if (speed < 0)
            sprite.flipX = true;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //confere se o sprite esta em contato com o chao
        if(collision.gameObject.tag.Equals("floor"))
        {
            animator.SetBool("isInAir", false);
            animator.SetBool("slide", false);
            limitSpeed = maxSpeed;
            max_dash_jump_actions = false;

            //"cola" o personagem ao chao em que ele esta pisando
            body.transform.parent = collision.gameObject.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("floor"))
        {
            animator.SetBool("isInAir", true);
            body.transform.parent = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
    //desliza na parede
        if (((collision.gameObject.tag.Equals("lateral_right_trigger")) && (Input.GetAxis("Horizontal") < 0))    //se estamos em uma parede a direita e estamos pressionando o botao esquerdo
            || ((collision.gameObject.tag.Equals("lateral_left_trigger")) && (Input.GetAxis("Horizontal") > 0))) //analogo
        {
            if (animator.GetBool("isInAir"))
            {
                animator.SetBool("slide", true);
                limitSpeed = slideMaxSpeed;

                if (speed > limitSpeed) 
                    speed = limitSpeed;
                if (speed < -limitSpeed)
                    speed = -limitSpeed;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.gameObject.tag.Equals("lateral_left_trigger")) 
            || (collision.gameObject.tag.Equals("lateral_right_trigger")))
        {
            animator.SetBool("slide", false);
            limitSpeed = maxSpeed;
        }
    }
}
