# AI-Tag
**Overview:**
AI Tag Arena is an interactive, physics-based 3D environment built in Unity where autonomous agents learn complex evasion and pursuit strategies using Reinforcement Learning.

Originally conceived as an AI-driven soccer simulation, the project evolved into an intense, adversarial game of "Tag." This transition allowed for a highly focused exploration of Proximal Policy Optimization (PPO), reward shaping, and multi-agent training. The final game features a complete gameplay loop where a human player can choose to be the Runner or the Seeker, competing against a highly trained neural network in a custom-built parkour arena.

**Features:**
Human vs. AI Gameplay: Players can choose their role (Blue Runner or Orange Seeker) via an interactive UI menu. The game automatically swaps the opposing agent to a trained AI inference model (.onnx).

Asymmetrical Balancing: To account for human unpredictability, AI agents operate at slightly higher movement speeds (50) compared to the human player (35), ensuring intense and challenging chases.

Custom Parkour Physics: Agents utilize finely tuned Rigidbody physics, complete with custom gravity, reduced friction physical materials, and "air control" to allow for precise jumping and vertical traversal over ramps and pillars.

Dynamic Camera System: Players can seamlessly toggle between First-Person, Third-Person, and an Overhead Arena View using custom keybinds.

Fully Featured UI Loop: Includes role selection with smooth CanvasGroup fade-ins, a 3-2-1 countdown sequence, a 30-second survival timer, and dynamic Win/Loss end screens.

**Machine Learning Architecture:**
The AI in this project was trained using the Unity ML-Agents Toolkit and the Proximal Policy Optimization (PPO) algorithm.

Training the agents required solving several complex Reinforcement Learning challenges:

Observation Space: Agents process the world through a 19-vector observation space (tracking velocities, vertical height differences, and facing directions) alongside dual Ray Perception 3D Sensors. A specific "HighSensor" was added to cure "horizontal blindness," allowing agents to see platform ledges and jump paths.

Overcoming Reward Hacking: Early training iterations saw the Runner exploiting the environment by loop-jumping on safe platforms. This was solved by implementing custom C# reward shaping, including height-camping penalties, jump-spam penalties, and rewarding the Seeker for vertical pursuit.

Shattering Local Optima (Premature Convergence): To prevent the Runner from memorizing a single "one-trick" escape route, the neural network was forced to generalize using:

Entropy Regularization: Increased Beta values (0.05) to force action exploration.

Intrinsic Curiosity: Rewarding the Runner for discovering unseen parts of the map.

Fictitious Self-Play: Adjusted the sliding window to 30 and the latest-model ratio to 0.3, forcing agents to constantly play against older, highly varied "ghosts" of past iterations.

**Technologies Used:**
Engine: Unity 3D

Language: C#

Machine Learning: Unity ML-Agents Toolkit, Python, TensorBoard

Algorithms: Proximal Policy Optimization (PPO), Self-Play, Curiosity Module

**How to Play:**
Launch the game to view the Role Selection Menu.

Click the Blue Button to play as the Runner (Goal: Survive for 30 seconds).

Click the Orange Button to play as the Seeker (Goal: Catch the Runner before time runs out).

Wait for the 3-2-1 countdown.

**Controls:**

W/A/S/D: Move 

J / L: Turn Left/Right

Spacebar: Jump

V / K: Toggle Camera Perspectives# AI-Tag
# AI-Tag
