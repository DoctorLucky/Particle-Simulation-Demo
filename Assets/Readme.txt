This project was written and developed by Marshall Nystrom on August 9th, 2021 as a sample for the HaptX interview process.

There's only one necessary script for this simulation: ParticleSimulatorManager. It handles (through the UI) creation and destruction of particles, toggling of boundaries
in which the particles can interact, and manipulation of timescale.

The UI buttons are as follows:
"Clear and Create Set" deletes all active particles and creates a new random set of 10 particles; the position, charge, and masses of these particles is random, as
described by "Add Random Particle".

"Regenerate Particles" resets all velocities of active particles to 0 and randomizes their positions inside the boundary cube, whether or not the boundaries are active.

"Toggle Boundaries" toggles the boundary cube (100-meter cube, centered around 0,0,0) on and off. When toggled on, calls ResetParticles automatically.

"Add Random Particle" adds a new particle to the scene. The particle's charge will be between 6 and 20 mili-Coulombs, and its mass will be between 5 and 15 kilograms.
When creating a particle with this button, it updates the Charge and Mass fields to show the values chosen.

"Add Particle" adds a new particle to the scene. The particle's charge will be the value read from the Charge input field, in Coulombs, and the particle's mass will
be the value read from the Mass input field, in kilograms. This can be used after creating a random particle to create multiple copies of the same particle.

"Destroy Particles" removes all active particles from the scene.

"Halve Timescale" halves Unity physics timescale.

"Double Timescale" doubles Unity physics timescale. Warning: at high timescales, force calculations can greatly exceed Unity's ability to simulate the forces accurately, 
and particles may be ejected and experience odd behavior.

"Toggle Time Flow" sets the Unity timestep to 0; if time is already stopped, it resets it to the last value.



Note: Particle size is based on mass; particle color is red for positive charges, blue for negative charges, with no indicator of amount of charge apart from physical
interactions observed.

Warning: Extremely high-charge, low-mass systems (> 0.01 C, with masses less than 20 kg, for example) can easily exceed Unity's ability to simulate forces accurately
at the standard time scale, and particles may be ejected and experience odd behavior. To simulate such systems, reducing the time scale is highly recommended.



Recommended sample sets: 
A 0.01-Coulomb charge with a mass of 100 kg, and six -0.001-Coulomb charges with masses of 10 kg
Five 0.001-Coulomb charges with masses of 5 kg each, and five equal-mass negative charges
