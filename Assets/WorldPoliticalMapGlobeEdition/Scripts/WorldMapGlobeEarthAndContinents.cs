// World Political Map - Globe Edition for Unity - Main Script
// Copyright 2015-2017 Kronnect
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPM {

	public enum EARTH_STYLE {
		Natural = 0,
		Alternate1= 1,
		Alternate2= 2,
		Alternate3= 3,
		SolidColor = 4,
		NaturalHighRes = 5,
		Scenic = 6,
		NaturalHighResScenic = 7,
		NaturalHighResScenicScatter = 8,
		NaturalHighResScenicScatterCityLights = 9,
		Custom = 10,
		NaturalHighResScenicCityLights = 11,
		ScenicCityLights = 12,
		NaturalHighRes16K = 13,
		NaturalHighRes16KScenic = 14,
		NaturalHighRes16KScenicCityLights = 15,
		NaturalHighRes16KScenicScatter = 16,
		NaturalHighRes16KScenicScatterCityLights = 17,
		StandardShader2K = 18,
		StandardShader8K = 19,
		Tiled = 20
	}

	public static class EarthStyleEnumExtensions {

		public static bool isScenic(this EARTH_STYLE style) {
			return style == EARTH_STYLE.NaturalHighResScenic || style == EARTH_STYLE.NaturalHighResScenicScatter || style == EARTH_STYLE.Scenic 
				|| style == EARTH_STYLE.NaturalHighResScenicScatterCityLights || style == EARTH_STYLE.NaturalHighResScenicCityLights || style == EARTH_STYLE.ScenicCityLights ||
					style == EARTH_STYLE.NaturalHighRes16KScenic || style == EARTH_STYLE.NaturalHighRes16KScenicCityLights || style == EARTH_STYLE.NaturalHighRes16KScenicScatter || 
					style == EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights;
		}

		public static bool isScatter(this EARTH_STYLE style) {
			return style == EARTH_STYLE.NaturalHighRes16KScenicScatter || style == EARTH_STYLE.NaturalHighResScenicScatter || style == EARTH_STYLE.NaturalHighResScenicScatterCityLights
				|| style == EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights;
		}

		public static bool isTiled(this EARTH_STYLE style) {
			return style == EARTH_STYLE.Tiled;
		}

	}

	/* Public WPM Class */
	public partial class WorldMapGlobe : MonoBehaviour {

		
		[SerializeField]
		float
			_contrast = 1.02f;
		
		public float contrast {
			get { return _contrast; }
			set {
				if (_contrast != value) {
					_contrast = value;
					isDirty = true;
					UpdateMaterialBrightness ();
				}
			}
		}

		
		[SerializeField]
		float
			_brightness = 1.05f;
		
		public float brightness {
			get { return _brightness; }
			set {
				if (_brightness != value) {
					_brightness = value;
					isDirty = true;
					UpdateMaterialBrightness ();
				}
			}
		}


		[SerializeField]
		bool
			_showInlandFrontiers = false;
		
		/// <summary>
		/// Toggle frontiers visibility.
		/// </summary>
		public bool showInlandFrontiers { 
			get {
				return _showInlandFrontiers; 
			}
			set {
				if (value != _showInlandFrontiers) {
					_showInlandFrontiers = value;
					isDirty = true;

					OptimizeFrontiers();
					DrawFrontiers();
					DrawInlandFrontiers();
//					if (inlandFrontiersLayer != null) {
//						inlandFrontiersLayer.SetActive (_showInlandFrontiers);
//					} else if (_showInlandFrontiers) {
//						DrawInlandFrontiers ();
//					}
				}
			}
		}

		/// <summary>
		/// Global color for inland frontiers.
		/// </summary>
		public Color inlandFrontiersColor {
			get {
				if (inlandFrontiersMatCurrent != null) {
					return inlandFrontiersMatCurrent.color;
				} else {
					return _inlandFrontiersColor;
				}
			}
			set {
				if (value != _inlandFrontiersColor) {
					_inlandFrontiersColor = value;
					isDirty = true;
					UpdateInlandFrontiersMat();
				}
			}
		}


		[SerializeField]
		bool
			_showWorld = true;
		/// <summary>
		/// Toggle Earth visibility.
		/// </summary>
		public bool showEarth { 
			get {
				return _showWorld; 
			}
			set {
				if (value != _showWorld) {
					_showWorld = value;
					isDirty = true;
					earthRenderer.enabled = _showWorld;
					DrawAtmosphere();
				}
			}
		}

		[SerializeField]
		[Range(-2f, 2f)]
		float
			_autoRotationSpeed = 0.02f;
		
		public float autoRotationSpeed {
			get { return _autoRotationSpeed; }
			set {
				if (value != _autoRotationSpeed) {
					_autoRotationSpeed = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		Color
			_inlandFrontiersColor = new Color(0.1f, 0.5f, 0.1f, 1);

		[SerializeField]
		EARTH_STYLE
			_earthStyle = EARTH_STYLE.Natural;
		
		public EARTH_STYLE earthStyle {
			get {
				return _earthStyle;
			}
			set {
				if (value != _earthStyle) {
					_earthStyle = value;
					isDirty = true;
					if (_earthStyle.isTiled()) {
						_earthScenicLightDirection = Vector3.back;
					}
					RestyleEarth ();
				}
			}
		}

		[SerializeField]
		Vector3 _earthScenicLightDirection = new Vector4(-0.5f, 0.5f, -1f);
		public Vector3 earthScenicLightDirection {
			get { return _earthScenicLightDirection; }
			set { if (value!=_earthScenicLightDirection) { _earthScenicLightDirection = value; isDirty = true; RestyleEarth(); } }
		}

		[SerializeField]
		float _earthScenicAtmosphereIntensity = 1.0f;
		public float earthScenicAtmosphereIntensity {
			get { return _earthScenicAtmosphereIntensity; }
			set { if (value!=_earthScenicAtmosphereIntensity) { _earthScenicAtmosphereIntensity = value; isDirty = true; RestyleEarth(); } }
		}

		[SerializeField]
		float _earthScenicGlowIntensity = 1.0f;
		public float earthScenicGlowIntensity {
			get { return _earthScenicGlowIntensity; }
			set { if (value!=_earthScenicGlowIntensity) { _earthScenicGlowIntensity = value; isDirty = true; RestyleEarth(); } }
		}

		[SerializeField]
		bool _earthGlowScatter = true;

		/// <summary>
		/// Uses the atmospheric scattering glow. Not compatible with mobile.
		/// </summary>
		public bool earthGlowScatter {
			get { return _earthGlowScatter;	}
			set { if (value!=_earthGlowScatter) { _earthGlowScatter = value; isDirty = true; RestyleEarth(); } }
		}


		[SerializeField]
		bool _earthInvertedMode;

		/// <summary>
		/// Enables Inverted Mode (sits you at the center of the globe). Useful for VR applications.
		/// </summary>
		public bool earthInvertedMode {
			get {
				return _earthInvertedMode;
			}
			set {
				if (value!=_earthInvertedMode) {
					_earthInvertedMode = value;
					isDirty = true;
					DestroyOverlay();
					DestroySurfacesLayer();
					Redraw();
					if (_earthInvertedMode) {
						Camera.main.transform.position = transform.position;
						Camera.main.transform.rotation = Quaternion.Euler(Misc.Vector3zero);
						Camera.main.fieldOfView = MAX_FIELD_OF_VIEW;
					} else {
						Camera.main.transform.position = transform.position + Vector3.back * lastGlobeScaleCheck.z * 1.2f;
						Camera.main.transform.LookAt(transform.position);
						Camera.main.fieldOfView = 60;
					}
				}
			}
		}
		
		[SerializeField]
		Color
			_earthColor = Color.black;
		
		/// <summary>
		/// Color for Earth (for SolidColor style)
		/// </summary>
		public Color earthColor {
			get {
				return _earthColor;
			}
			set {
				if (value != _earthColor) {
					_earthColor = value;
					isDirty = true;
					
					if (_earthStyle == EARTH_STYLE.SolidColor && earthRenderer!=null) {
						earthRenderer.sharedMaterial.color = _earthColor;
					}
				}
			}
		}

		/// <summary>
		/// Instantiates current Earth material and returns a reference.
		/// </summary>
		public Material earthMaterial { 
			get { 
				string matName = earthRenderer.sharedMaterial.name;
				Material mat = earthRenderer.material;
				mat.name = matName;
				earthRenderer.sharedMaterial = mat;
				return mat;
			}
		}

		/// <summary>
		/// Instantiates current Earth texture and returns a reference.
		/// </summary>
		public Texture2D earthTexture {
			get {
				string name = earthRenderer.sharedMaterial.mainTexture.name;
				Texture2D tex = Instantiate(earthRenderer.sharedMaterial.mainTexture) as Texture2D;
				tex.name = name;
				earthRenderer.sharedMaterial.mainTexture = tex;
				return tex;
			}
		}



		[SerializeField]
		bool _earthHighDensityMesh = true;

		/// <summary>
		/// Specifies the mesh asset to load and render as Earth mesh
		/// </summary>
		public bool earthHighDensityMesh {
			get { return _earthHighDensityMesh; }
			set { if (value != _earthHighDensityMesh) {
					_earthHighDensityMesh = value;
					isDirty = true;
					RestyleEarth();
				}
			}
		}


	#region Public API area

		/// <summary>
		/// Makes the globe's north points upwards.
		/// </summary>
		public void StraightenGlobe () {
			StraightenGlobe(0, true);
		}

		/// <summary>
		/// Makes the globe's north points upwards smoothly
		/// </summary>
		public void StraightenGlobe (float duration) {
			StraightenGlobe(duration, false);
		}

		/// <summary>
		/// Makes the globe's north points upwards smoothly and optionally retains current location on the center of globe
		/// </summary>
		public void StraightenGlobe (float duration, bool keepLocationOnCenter)
		{
			if (_earthInvertedMode) {
				if (keepLocationOnCenter) {
					Quaternion oldRotation = transform.localRotation;
					Vector3 v2 = Camera.main.transform.forward;
					Vector3 v3  = Vector3.ProjectOnPlane(transform.up, v2);
					float angle2 = SignedAngleBetween (Camera.main.transform.up, v3, v2);
					if (duration == SMOOTH_STRAIGHTEN_ON_POLES) {
						if (Mathf.Abs(Vector3.Dot (transform.up, v2))<0.96f) {	// avoid crazy rotation on poles
							angle2 = Mathf.Clamp(angle2, -2, 2);
						} else {
							angle2 = 0;
						}
					}
					transform.Rotate (v2, -angle2, Space.World);
					flyToEndQuaternion = transform.localRotation;
					transform.localRotation = oldRotation;
				} else {
					flyToEndQuaternion = Quaternion.Euler (Misc.Vector3zero);
				}
			} else {
				Quaternion oldRotation = transform.localRotation;
				if (keepLocationOnCenter) {
					Vector3 v2 = -Camera.main.transform.forward; // Camera.main.transform.position - transform.position;
					Vector3 v3  = Vector3.ProjectOnPlane(transform.up, v2);
					float angle2 = SignedAngleBetween (Camera.main.transform.up, v3, v2);
					if (duration == SMOOTH_STRAIGHTEN_ON_POLES) {
						if (Mathf.Abs(Vector3.Dot (transform.up, v2.normalized))<0.96f) {	// avoid crazy rotation on poles
							angle2 = Mathf.Clamp(angle2, -2, 2);
						} else {
							angle2 = 0;
						}
					}
					transform.Rotate (v2, -angle2, Space.World);
					Vector3 currentDestination = transform.InverseTransformVector(Camera.main.transform.position - transform.position);
					flyToEndDestination = currentDestination;
				} else {
					Vector3 v1 = Camera.main.transform.position - transform.position;
					float angleY = SignedAngleBetween (v1, transform.right, transform.up) + 90.0f;
					transform.localRotation = Camera.main.transform.localRotation;
					transform.Rotate (Vector3.up * angleY, Space.Self);
				}
				flyToEndQuaternion = transform.localRotation;
				transform.localRotation = oldRotation;
			}
			if (!Application.isPlaying || duration == SMOOTH_STRAIGHTEN_ON_POLES) duration = 0;
			flyToDuration = duration;
			flyToStartQuaternion = transform.localRotation;
			flyToStartTime = Time.time;
			flyToActive = true;
			flyToCameraStartPosition = flyToCameraEndPosition = _cursorLocation;
			flyToMode = NAVIGATION_MODE.EARTH_ROTATES;
			if (flyToDuration == 0)
				RotateToDestination ();
		}

		/// <summary>
		/// Set Earth rotations and moves smoothly.
		/// </summary>
		public void TiltGlobe (Vector3 angles, float duration)
		{
			if (_earthInvertedMode) {
				flyToEndQuaternion = Quaternion.Euler (angles);
			} else {
				Vector3 v1 = Camera.main.transform.position - transform.position;
				float angleY = SignedAngleBetween (v1, transform.right, transform.up) + 90.0f;
				flyToEndQuaternion = Quaternion.Euler (angles) * Quaternion.Euler (Vector3.up * angleY);
			}
			if (!Application.isPlaying) duration = 0;
			flyToDuration = duration;
			flyToStartQuaternion = transform.localRotation;
			flyToStartTime = Time.time;
			flyToActive = true;
			flyToMode = NAVIGATION_MODE.EARTH_ROTATES;
			if (flyToDuration == 0)
				RotateToDestination ();
		}

		/// <summary>
		/// Iterates for the countries list and colorizes those belonging to specified continent name.
		/// </summary>
		public void ToggleContinentSurface(string continentName, bool visible, Color color) {
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					ToggleCountrySurface (countries [colorizeIndex].name, visible, color);
			}

		}
		}

		
		/// <summary>
		/// Uncolorize/hide specified countries beloning to a continent.
		/// </summary>
		public void HideContinentSurface (string continentName) {
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					HideCountrySurface(colorizeIndex);
				}
			}
		}


		/// <summary>
		/// Discards current material and reloads it based on earthStyle
		/// </summary>
		public void ReloadEarthTexture() {
			if (earthRenderer!=null) earthRenderer.sharedMaterial = null;
			RestyleEarth();
		}


		#endregion


	}

}