using System;
using UnityEngine;

   
    [RequireComponent(typeof(AudioSource))]

    public class PlayerSoundManager : MonoBehaviour
    {
        #region Variables
        public AudioClip c_Idle, c_Movement, borderCollision;        
        private AudioSource[] soundSources;
        public bool isMoving;

        private Vector3 curPosition;
        private Vector3 lastPosition;

        private HoverController hoverController;    




        private void FixedUpdate()
        {
      

            PlayPlayerSound();
        }

        #endregion


        private void Start()
        {
            curPosition = transform.position;
            lastPosition = transform.position;
                   
            soundSources = GetComponents<AudioSource>();
            hoverController = GameObject.FindObjectOfType<HoverController>();
            hoverController.onBorderCollision += PlayBorderCollisionSound;
        }

        private void PlayBorderCollisionSound()
        {
            soundSources[1].PlayOneShot(borderCollision);

            Invoke("SetBorderCollidedBool", borderCollision.length*2);
        }

        private void SetBorderCollidedBool() 
        {
            hoverController.IsCollidedwithBorder = false;
        }

        private void PlayPlayerSound()
        {
            if (GameManager._instance.CurrentGameState != GameManager.gameState.game)
                soundSources[0].Stop();

        curPosition = transform.position;

            if (Vector3.Distance(curPosition, lastPosition) > 0.05f)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }

            lastPosition = curPosition;

            if (isMoving && soundSources[0].clip != c_Movement)
            {
                soundSources[0].clip = c_Movement;
                soundSources[0].Play();
            }

            if (!isMoving && soundSources[0].clip != c_Idle)
            {
                soundSources[0].clip = c_Idle;
                soundSources[0].Play();
            }
        }


        private void OnDestroy()
        {
            hoverController.onBorderCollision -= PlayBorderCollisionSound;
        }

}

