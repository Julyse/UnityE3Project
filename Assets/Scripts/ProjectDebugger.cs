using UnityEngine;
using System.IO;
using KinematicCharacterController;

public class ProjectDebugger : MonoBehaviour
{
    void Start()
    {
        string logPath = Path.Combine(Application.dataPath, "../unity_debug_log.txt");
        string log = "--- Unity Project Debugger ---\n";
        log += "Time: " + System.DateTime.Now.ToString() + "\n\n";

        // Check Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            log += "Player Found: " + player.name + "\n";
            
            // Rigidbody Check
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                log += "[Rigidbody]\n";
                log += "  - IsKinematic: " + rb.isKinematic + "\n";
                log += "  - Interpolation: " + rb.interpolation + "\n";
                log += "  - Constraints: " + rb.constraints + "\n";
            }
            else
            {
                log += "[Rigidbody] NOT FOUND on Player\n";
            }

            // KCC Check
            KinematicCharacterMotor motor = player.GetComponent<KinematicCharacterMotor>();
            if (motor != null)
            {
                log += "[KCC Motor]\n";
                log += "  - IsActive: " + motor.enabled + "\n";
            }
            else
            {
                log += "[KCC Motor] NOT FOUND on Player\n";
            }

            // Transform Check
            log += "[Transforms]\n";
            log += "  - Root Scale: " + player.transform.localScale + "\n";
            
            // Children
            foreach (Transform child in player.transform)
            {
                log += "  - Child: " + child.name + " (LocalPos: " + child.localPosition + ", LocalRot: " + child.localRotation.eulerAngles + ")\n";
            }
        }
        else
        {
            log += "ERROR: Player with tag 'Player' not found!\n";
        }

        // Rigging Check
        Component rigBuilder = player.GetComponent("RigBuilder");
        if (rigBuilder != null)
        {
            log += "\n[Animation Rigging]\n";
            log += "  - RigBuilder: FOUND (Enabled: " + ((Behaviour)rigBuilder).enabled + ")\n";
        }
        
        Component[] iks = player.GetComponentsInChildren<Component>();
        foreach (var c in iks)
        {
            if (c.GetType().Name.Contains("IK") || c.GetType().Name.Contains("Constraint"))
            {
                log += "  - IK/Constraint: " + c.GetType().Name + " on " + c.gameObject.name + "\n";
            }
        }

        // Camera Check - find ALL cameras
        Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        log += "\n[Cameras Found: " + allCams.Length + "]\n";
        foreach (Camera cam in allCams)
        {
            log += "  - Name: " + cam.name + " (Tag: " + cam.tag + ")\n";
            
            // Try to find CinemachineBrain
            Component brain = cam.GetComponent("CinemachineBrain");
            if (brain != null)
            {
                log += "    * CinemachineBrain: FOUND\n";
                try {
                    var updateProp = brain.GetType().GetProperty("m_UpdateMethod");
                    if (updateProp != null)
                        log += "    * Update Method: " + updateProp.GetValue(brain) + "\n";
                } catch { }
            }
        }

        // Find ALL FreeLook Cameras in the scene
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            Component freelook = obj.GetComponent("CinemachineFreeLook");
            if (freelook != null)
            {
                log += "\n[FreeLook Camera: " + obj.name + "]\n";
                try {
                    var type = freelook.GetType();
                    
                    // Basic Targets
                    var follow = type.GetProperty("Follow")?.GetValue(freelook) as Transform;
                    var lookAt = type.GetProperty("LookAt")?.GetValue(freelook) as Transform;
                    log += "  - Following: " + (follow ? follow.name : "NULL") + "\n";
                    log += "  - Looking At: " + (lookAt ? lookAt.name : "NULL") + "\n";

                    // Axis Settings
                    var xAxis = type.GetField("m_XAxis", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(freelook);
                    var yAxis = type.GetField("m_YAxis", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(freelook);
                    
                    if (xAxis != null) {
                        var speed = xAxis.GetType().GetField("m_MaxSpeed")?.GetValue(xAxis);
                        log += "  - X Axis MaxSpeed: " + speed + "\n";
                    }
                    if (yAxis != null) {
                        var speed = yAxis.GetType().GetField("m_MaxSpeed")?.GetValue(yAxis);
                        log += "  - Y Axis MaxSpeed: " + speed + "\n";
                    }

                    // Orbits and Damping
                    var orbits = type.GetField("m_Orbits", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(freelook) as System.Array;
                    if (orbits != null && orbits.Length >= 3) {
                        log += "  - Orbit Heights: ";
                        for (int i = 0; i < orbits.Length; i++) {
                            var h = orbits.GetValue(i).GetType().GetField("m_Height")?.GetValue(orbits.GetValue(i));
                            var r = orbits.GetValue(i).GetType().GetField("m_Radius")?.GetValue(orbits.GetValue(i));
                            log += "[" + i + ": H=" + h + ", R=" + r + "] ";
                        }
                        log += "\n";
                    }

                    // Check for Lookahead and Noise
                    log += "  - Time.fixedDeltaTime: " + Time.fixedDeltaTime + "\n";
                    
                    // Probing for Noise (usually on the individual orbits)
                    log += "  - Noise/Lookahead: Probing...\n";
                    try {
                        var rigField = type.GetField("m_Rigs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (rigField != null) {
                            var rigs = rigField.GetValue(freelook) as Object[];
                            if (rigs != null) {
                                foreach(var rig in rigs) {
                                    if (rig == null) continue;
                                    var noise = rig.GetType().GetMethod("GetCinemachineComponent")?.Invoke(rig, new object[] { 4 }); // 4 = Noise
                                    if (noise != null) log += "    * Noise found on " + rig.name + "\n";
                                }
                            }
                        }
                    } catch { }

                } catch (System.Exception e) {
                    log += "  - Error probing FreeLook: " + e.Message + "\n";
                }
            }
        }

        File.WriteAllText(logPath, log);
        Debug.Log("Project Debugger: Information saved to " + logPath);
    }
}
