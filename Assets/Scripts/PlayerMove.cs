using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float speed; // Movement speed
    public Rigidbody2D body; // Reference to Rigidbody2D component for movement
    public Animator anima; // Reference to Animator component for animations
    public bool grounded; // Check if the player is grounded
    public float fallThreshold = -10f;

    public int starsCollected = 0; // Counter for collected stars
    public int totalStars = 5; // Total number of stars required to end the game
    public GameObject darkOverlay; // Reference to the dark overlay GameObject
    public Text timerText; // Reference to the TimerText UI element

    public bool canMove = true; // Control player movement
    public float timeRemaining = 30f; // Default 30-second timer for SampleScene

    // Audio variables
    public AudioSource audioSource; // Reference to Audio Source component
    public AudioClip jumpSound; // Jump sound clip
    public AudioClip collectStarSound; // Star collection sound clip
    public AudioClip hazardHitSound; // Hazard collision sound clip

    void Start()
    {
        // Set timer based on current scene
        if (SceneManager.GetActiveScene().name == "Level 2")
        {
            timeRemaining = 20f; // Set to 20 seconds for Level 2
        }
        else
        {
            timeRemaining = 30f; // Default to 30 seconds for SampleScene
        }

        // Get the Rigidbody2D and Animator components from the player object
        body = GetComponent<Rigidbody2D>();
        anima = GetComponent<Animator>();

        // Make sure the overlay is initially disabled
        if (darkOverlay != null)
        {
            darkOverlay.SetActive(false);
        }

        // Initialize the timer display
        UpdateTimerDisplay();
    }

    void Update() 
    {
        if (!canMove) return; // Exit Update if player can't move

        // Countdown timer
        timeRemaining -= Time.deltaTime;
        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            if (SceneManager.GetActiveScene().name == "Level 2")
            {
                SceneManager.LoadScene("SampleScene"); // Reset to SampleScene if time runs out in Level 2
            }
            else
            {
                RestartLevel(); // Reset the game if time runs out in SampleScene
            }
        }

        // Get horizontal input (A/D keys or Left/Right arrow keys)
        float horizontalInput = Input.GetAxis("Horizontal");

        // Apply movement based on input
        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

        // Flip the player sprite when moving left or right
        if (horizontalInput > 0.01f) 
            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        else if (horizontalInput < -0.01f) 
            transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);

        // Check for jump input (Spacebar) and if the player is grounded
        if (Input.GetKey(KeyCode.Space) && grounded)   
            Jump();

        // Update animator to trigger the running and grounded animations
        anima.SetBool("run", horizontalInput != 0);
        anima.SetBool("grounded", grounded);

        // Check if player falls below the threshold
        if (transform.position.y < fallThreshold) 
        {
            RestartLevel();
        }
    }

    // Method to update the timer display on screen
    void UpdateTimerDisplay()
    {
        if (timerText != null) // Ensure we have a TimerText object to update
        {
            int displayTime = Mathf.CeilToInt(timeRemaining); // Round up to show whole seconds
            timerText.text = "Time: " + displayTime.ToString();
        }
    }

    // Jump function
    public void Jump() 
    {
        body.velocity = new Vector2(body.velocity.x, speed); // Apply vertical velocity for jumping
        anima.SetTrigger("jump");
        grounded = false; // 

        // Play jump sound
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // Check for collisions to detect if the player is grounded
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ground") 
        {
            grounded = true; 
        }
    }

    // Detect trigger collisions, specifically with saws, spikes, and stars
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Saw") || other.CompareTag("Spike")) // If colliding with a hazard
        {
            PlaySound(hazardHitSound); // Play hazard sound
            StartCoroutine(HandleHazardCollision()); // Start hazard coroutine
        }
        else if (other.CompareTag("Star")) 
        {
            PlaySound(collectStarSound); // Play star collection sound
            CollectStar(other.gameObject);
        }
    }

    // Coroutine to handle hazard collision with a delay before restarting
    IEnumerator HandleHazardCollision()
    {
        canMove = false; // Disable player movement
        body.velocity = Vector2.zero; 
        yield return new WaitForSeconds(0.15f); // Wait for 0.15 a second
        RestartLevel(); 
    }

    // Method to handle star collection
    void CollectStar(GameObject star)
    {
        starsCollected++; 
        Destroy(star); 

        // Ensure darkOverlay is active for the effect
        if (darkOverlay != null)
        {
            darkOverlay.SetActive(true);

            // Calculate the new alpha based on the number of stars collected
            float alpha = Mathf.Clamp01(starsCollected / (float)totalStars); 
            Color overlayColor = darkOverlay.GetComponent<Image>().color; 
            overlayColor.a = alpha; 
            darkOverlay.GetComponent<Image>().color = overlayColor; 
        }

        // Check if all stars have been collected
        if (starsCollected >= totalStars)
        {
            StartCoroutine(EndGame()); 
        }
    }

    // Coroutine to transition to the next level or back to SampleScene after collecting all stars
    IEnumerator EndGame()
    {
        Debug.Log("All stars collected!");

        canMove = false; // Disable player movement
        body.velocity = Vector2.zero; 

        yield return new WaitForSeconds(5f); // Wait for 5 seconds

        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            SceneManager.LoadScene("Level 2"); // Transition from SampleScene to Level 2
        }
        else
        {
            SceneManager.LoadScene("SampleScene"); // Transition from Level 2 back to SampleScene
        }
    }

    // Method to restart the current level
    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }

    // Helper method to play sounds
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}