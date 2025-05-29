**Unity Auto-Test Tool Task Definition**

### Overview

Create an automated testing tool for Unity 2D slot games. The tool should simulate random button presses to verify UI interactions efficiently and identify potential issues in game flows.

---

### Core Requirements

#### 1. Button Detection

* Automatically detect all pressable buttons in the scene that inherit from the class `Collider2DButton`.
* Cache detected buttons for quicker access during testing.

#### 2. Pressable Logic

* Only interact with buttons physically reachable (fully visible and not blocked by UI overlays or untouchable elements).
* Determine pressability dynamically based on physics and raycast checks.

#### 3. Random Interaction

* Randomly select buttons to press, without structured ordering.
* Track and log button press counts.
* Display button press statistics in real-time, updating every 5 seconds.
* Show logs unobtrusively (small text at the top-left corner).

#### 4. Adjustable Settings

The testing behavior should be customizable with the following adjustable settings available via an in-game canvas:

* **Pressing Speed:** Frequency of button presses.
* **Press Duration:** How long each press is held.
* **Delay Between Presses:** Adjustable pause between presses.
* **Randomness Factor:** Optional variance in pressing intervals and durations.

#### 5. Dynamic UI Detection

* Automatically detect newly opened windows or pop-ups.
* Dynamically update the set of buttons available for interaction based on window visibility.
* Exclude any buttons no longer physically reachable due to new overlays or windows.

#### 6. User Interface and Control

* Activation of the testing tool via pressing the Enter key three times quickly.
* Upon activation, display a configuration canvas with adjustable settings and explicit Start/Stop buttons.
* When testing is active, hide all UI elements except the Stop button.
* The Stop button is clearly visible and always available to end the automated testing.

#### 7. Technical Considerations

* The tool must run in development builds on actual testing devices.
* Avoid testing any buttons placed within canvases (used exclusively for the tool's UI itself).

---

### Deliverables

* A Unity Editor-compatible C# script.
* Clearly commented and documented source code.
* User-friendly UI for managing and configuring testing sessions.

### Additional Notes

* Ensure robustness in detecting button visibility and reachability.
* Maintain lightweight operation suitable for extended testing sessions without significant performance overhead.
