using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
 
    public Vector3 _Position;
     void FixedUpdate()
    {
        player_Controller();

    }
    private void player_Controller()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, 0, 5 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += new Vector3(0, 5*Time.deltaTime, 0 );
        }
      
        


        _Position = transform.position;

        
        ClientSend.PlayerMoveMent(_Position);
    }
    
}
