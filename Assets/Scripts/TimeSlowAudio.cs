using UnityEngine;
using UnityEngine.Audio;

public class TimeSlowAudio : MonoBehaviour
{
    public AudioMixer mixer; // Drag your Audio Mixer here
    public string pitchParameter = "Pitch"; // The exposed pitch parameter name

    void Update()
    {
        float targetPitch = Time.timeScale; // Use your current timescale
        mixer.SetFloat(pitchParameter, targetPitch); // Directly set the mixer parameter
    }
}
