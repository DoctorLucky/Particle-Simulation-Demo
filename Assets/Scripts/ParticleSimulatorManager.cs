/* Written by Marshall Nystrom on August 9th, 2021 as a sample for an interview.
 * This script manages the particle force interaction, as well as the maintenance of 
 * boundaries, creation and destruction of particles, and flow of time.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleSimulatorManager : MonoBehaviour
{
    /// <summary>
    /// Data structure for holding information about a charged particle; holds variables for charge, mass,
    /// and references to the GameObject and Rigidbody components of the particle for easy reference and
    /// manipulation.
    /// </summary>
    public struct ChargedParticle
    {
        public float charge; // in Coulombs
        public float mass; // in Kilograms
        public GameObject go;
        public Rigidbody rb;
        
        public ChargedParticle(float charge, float mass, GameObject go, Rigidbody rb)
        {
            this.charge = charge;
            this.mass = mass;
            this.go = go;
            this.rb = rb;
        }
    }

    private const double k = 8987551792.314; // Coulomb's constant, in Newton-Meters^2 / Coulomb^2

    /// Random generator values
    private const float MAX_POS = 40f; // Max displacement of particles from 0,0,0, in x, y, and z, positive or negative
    private const float MIN_CHARGE = 6f; // in miliCoulombs
    private const float MAX_CHARGE = 20f; // in miliCoulombs
    private const float MIN_MASS = 5f; // in kg
    private const float MAX_MASS = 15f; // in kg

    [Tooltip("The number of particles to create by default, and when the scene is reset.")]
    [SerializeField]
    private int startingParticles = 10;

    private Dictionary<int, ChargedParticle> particles; // integer-keyed iterable dictionary of all particles in the scene.

    private float savedTimeMultiplier; // This is used when pausing and resuming time, so that the previous timescale can be restored.
    private float fixedDeltaTime; // Used to keep physics operations flowing at a constant rate, regardless of in-game timescale.

    private float chargeOfNewParticle; // Used when creating a new particle, modified by the chargeField input field or random generator.
    private float massOfNewParticle; // Used when creating a new particle, modified by the massField input field or random generator.

    [Tooltip("Put the boundaries for the particle systems here. Can be a parent object, or each boundary individually.")]
    [SerializeField]
    private GameObject[] boundaries; // The premade boundary objects that contain the simulation, if desired.

    [Tooltip("Put the InputField for particle charge here.")]
    [SerializeField]
    private InputField chargeField; // UI input field for charge
    [Tooltip("Put the InputField for particle mass here.")]
    [SerializeField]
    private InputField massField; // UI input field for mass
    [Tooltip("Put the UI text that shows time scale here.")]
    [SerializeField]
    private Text timeScaleText;

    // Awake is called before any Start() functions once per scene initialization
    private void Awake()
    {
        particles = new Dictionary<int, ChargedParticle> { };
        fixedDeltaTime = Time.fixedDeltaTime; // save the initial fixedDeltaTime so we can modify it to be at a constant rate when changing time scale
    }

    // FixedUpdate is called once per physics update, base rate of 50 calls/second.
    private void FixedUpdate()
    {
        for(int i = 0; i < particles.Count; i++)
        {
            for(int j = 0; j < particles.Count; j++)
            {
                if (j > i) // Since each force is equal and opposite, each force calculation only has to be done once per pair. This cuts the number of calculations in half.
                {
                    /// Force magnitude is determined via Coulomb's Law: F = (k * q1 * q2 / r^2).
                    float forceMag = (float)(k * particles[i].charge * particles[j].charge / Mathf.Pow(Vector3.Distance(particles[i].go.transform.position, particles[j].go.transform.position), 2));

                    /// Vector from particle j to particle i is position of i minus position of j; that is the force direction for i if the charges are the same sign.
                    Vector3 forceDir = Vector3.Normalize(particles[i].go.transform.position - particles[j].go.transform.position); 

                    particles[i].rb.AddForce(forceMag * forceDir);
                    particles[j].rb.AddForce(forceMag * -forceDir); // apply equal and opposite force to the second particle, j
                }
            }
        }
    }

    /// <summary>
    /// Check to see if boundaries are currently enabled. If they are, disable them. If they are not,
    /// move all active particles inside the 100m cube centered around 0,0,0 and instantiate boundaries.
    /// </summary>
    public void ToggleBoundaries()
    {
        if(boundaries[0].activeInHierarchy)
        {
            for(int i = 0; i < boundaries.Length; i++)
            {
                boundaries[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < boundaries.Length; i++)
            {
                boundaries[i].SetActive(true);
            }
            RegenerateParticles();
        }
    }

    /// <summary>
    /// Set the public charge of new particles randomly; this is used when the UI button calls for a random new particle.
    /// </summary>
    private void AssignRandomCharge()
    {
        /// Set the sign of the charge randomly
        int sign = Random.Range(0, 2);
        if (sign == 0)
            sign = -1;

        chargeOfNewParticle = Mathf.Pow(Random.Range(MIN_CHARGE, MAX_CHARGE), -3) * sign; // Reduce random charge to -3rd power since charges are multiplied together in force calculation and k is 10^9 power
        
        chargeField.text = chargeOfNewParticle.ToString();
    }

    /// <summary>
    /// Set the public mass of the next new particle; this is used when the UI button calls for a new random particle.
    /// </summary>
    private void AssignRandomMass()
    {
        massOfNewParticle = Random.Range(MIN_MASS, MAX_MASS);

        massField.text = massOfNewParticle.ToString();
    }

    /// <summary>
    /// Set a random charge and mass and then call the standard AddParticle function.
    /// </summary>
    public void AddRandomParticle()
    {
        AssignRandomCharge();
        AssignRandomMass();

        AddParticle(chargeOfNewParticle, massOfNewParticle);
    }

    /// <summary>
    /// Create a particle of the UI-specified charge and mass, assigning them randomly if not set.
    /// </summary>
    public void AddUIParticle()
    {
        try
        {
            chargeOfNewParticle = float.Parse(chargeField.text);
        }
        catch
        {
            AssignRandomCharge();
        }
        try
        {
            massOfNewParticle = float.Parse(massField.text);
        }
        catch
        {
            AssignRandomMass();
        }
        AddParticle(chargeOfNewParticle, massOfNewParticle);
    }

    /// <summary>
    /// Create, name, randomly position, and scale a new Sphere primitive.
    /// Color it based on charge (blue for negative charges, red for positive)
    /// Finally, add and set up the RigidBody component and add the new particle to the 'particles' dictionary.
    /// </summary>
    /// <param name="charge"></param>
    /// <param name="mass"></param>
    public void AddParticle(float charge, float mass)
    {
        GameObject newParticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newParticle.name = charge.ToString() + "C, " + mass.ToString() + "kg particle"; // Name the new particle "(charge) C, (mass) kg particle"
        newParticle.transform.position = new Vector3(Random.Range(-MAX_POS, MAX_POS), Random.Range(-MAX_POS, MAX_POS), Random.Range(-MAX_POS, MAX_POS));
        newParticle.transform.localScale = Vector3.one * mass / 5f; // Set the scale of the particle based on its mass
        if(charge < 0)
            newParticle.GetComponent<Renderer>().material.color = Color.blue;
        else
            newParticle.GetComponent<Renderer>().material.color = Color.red;

        /// Set up rigidbody for the new particle: enable continuous dynamic collision checking for the particles,
        /// as they may interact at high speeds, disable gravity, and set mass as called.
        Rigidbody rb = newParticle.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; 

        particles.Add(particles.Count, new ChargedParticle(charge, mass, newParticle, rb));
    }

    /// <summary>
    /// Reset and randomize the positions of all particles in the scene.
    /// </summary>
    public void RegenerateParticles()
    {
        for(int i = 0; i < particles.Count; i++)
        {
            particles[i].rb.velocity = Vector3.zero;
            particles[i].go.transform.position = new Vector3(Random.Range(-MAX_POS, MAX_POS), Random.Range(-MAX_POS, MAX_POS), Random.Range(-MAX_POS, MAX_POS));
        }
    }

    /// <summary>
    /// Create a system of particles equal to startingParticles in the scene.
    /// </summary>
    public void SetupRandomParticles()
    {
        DestroyParticles();
        for (int i = 0; i < startingParticles; i++)
        {
            AddRandomParticle();
        }
    }

    /// <summary>
    /// Destroys all active particles;
    /// </summary>
    public void DestroyParticles()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            Destroy(particles[i].go, 0);
        }
        particles = new Dictionary<int, ChargedParticle>();
    }

    /// <summary>
    /// Halve the simulation timescale, multiplying the forces and velocity by 0.5
    /// </summary>
    public void HalveTimescale()
    {
        Time.timeScale *= 0.5f;
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale; // Keep the physics timestep the same number of real-world seconds to allow for more precise simulation
        timeScaleText.text = "Time Scale: " + Time.timeScale.ToString();
    }

    /// <summary>
    /// Double the simulation timescale, multiplying forces and velocity by 2
    /// </summary>
    public void DoubleTimescale()
    {
        Time.timeScale *= 2f;
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale; // Keep the physics timestep the same number of real-world seconds to allow for more precise simulation
        timeScaleText.text = "Time Scale: " + Time.timeScale.ToString();
    }

    /// <summary>
    /// If time is flowing, save the current scale and stop time. If it's stopped, return timescale to previous value.
    /// </summary>
    public void ToggleTime()
    {
        if(savedTimeMultiplier == 0)
        {
            savedTimeMultiplier = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = savedTimeMultiplier;
            savedTimeMultiplier = 0f;
        }

        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale; // Keep the physics timestep the same number of real-world seconds to allow for more precise simulation
        timeScaleText.text = "Time Scale: " + Time.timeScale.ToString();
    }
}