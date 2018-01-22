using UnityEngine;
using System.Collections;

public abstract class StimulusPlayer: MonoBehaviour{

		abstract public void PlaySound(GameObject soundLocation);
		abstract public void CreateSound(GameObject soundLocation);
		abstract public void LoadHRTF(GameObject soundLocation);
		abstract public void MuteSource(GameObject soundLocation);
		abstract public void SwitchHRTF(GameObject soundLocation);
		abstract public void StartRendering(GameObject soundLocation);
		abstract public void AddASIOSource(GameObject soundLocation);
		abstract public void AddNoiseSource(GameObject soundLocation);
		abstract public void AddWAVSource(GameObject soundLocation);
		abstract public void AdjustSourceLevel(GameObject soundLocation);
		abstract public void AdjustSourcePosition(GameObject soundLocation);
	

	}

