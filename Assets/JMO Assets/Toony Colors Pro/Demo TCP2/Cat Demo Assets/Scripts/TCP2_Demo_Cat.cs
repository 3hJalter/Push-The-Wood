﻿// Toony Colors Pro+Mobile 2
// (c) 2014-2023 Jean Moreno

using System;
using UnityEngine;
using UnityEngine.UI;

namespace ToonyColorsPro
{
    namespace Demo
    {
        public class TCP2_Demo_Cat : MonoBehaviour
        {
            public Ambience[] ambiences;
            public int amb;

            [Space] public ShaderStyle[] styles;

            public int style;

            [Space] public GameObject shadedGroup;

            public GameObject flatGroup;

            [Space] public Animation[] catAnimation;

            public Animation[] unityChanAnimation;

            [Space] public GameObject[] cats;

            public GameObject[] unityChans;
            public GameObject unityChanCopyright;

            [Space] public Light catDirLight;

            public Light unityChanDirLight;

            [Space] public AnimationClip[] catAnimations;

            public AnimationClip[] unityChanAnimations;

            [Space] public Light[] dirLights;

            public Light[] otherLights;
            public Transform rotatingPointLights;

            [Space] public Button[] ambiencesButtons;

            public Button[] stylesButtons;
            public Button[] characterButtons;
            public Button[] textureButtons;
            public Button[] animationButtons;

            [Space] public Canvas canvas;

            private bool animationPaused;
            private int catAnim;
            private float playingSpeed = 1;
            private int uchanAnim;
            public bool rotateLights { get; set; }
            public bool rotatePointLights { get; set; }

            //------------------------------------------------------------------------------------------------------------------------

            private void Awake()
            {
                rotatePointLights = true;
                rotateLights = false;
                SetAmbience(0);
                SetStyle(0);
                SetCat(true);
                SetFlat(false);
                SetAnimation(0);
            }

            private void Update()
            {
                if (rotateLights)
                    foreach (Light l in dirLights)
                        l.transform.Rotate(Vector3.up * Time.deltaTime * -30f, Space.World);

                if (rotatePointLights)
                    rotatingPointLights.transform.Rotate(Vector3.up * 50f * Time.deltaTime, Space.World);

                //Keyboard shortcuts
                //Switch style
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        SetStyle(--style);
                    else
                        SetStyle(++style);
                }

                //Keypad -> style
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SetStyle(0);
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SetStyle(1);
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SetStyle(2);
                if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) SetStyle(3);
                if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) SetStyle(4);
                if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) SetStyle(5);

                //Show/hide UI
                if (Input.GetKeyDown(KeyCode.H)) canvas.enabled = !canvas.enabled;
            }

            //------------------------------------------------------------------------------------------------------------------------
            // UI Callbacks

            public void SetAmbience(int index)
            {
                foreach (Ambience a in ambiences)
                foreach (GameObject g in a.activate)
                    g.SetActive(false);

                amb = index % ambiences.Length;
                Ambience current = ambiences[amb];
                foreach (GameObject g in current.activate)
                    g.SetActive(true);

                RenderSettings.skybox = current.skybox;
                DynamicGI.UpdateEnvironment();

                for (int i = 0; i < ambiencesButtons.Length; i++)
                {
                    ColorBlock colors = ambiencesButtons[i].colors;
                    colors.colorMultiplier = i == index ? 0.96f : 0.6f;
                    ambiencesButtons[i].colors = colors;
                }
            }

            public void SetStyle(int index)
            {
                if (index < 0)
                    index = styles.Length - 1;
                if (index >= styles.Length)
                    index = 0;
                style = index;

                ShaderStyle s = styles[style];

                foreach (ShaderStyle.CharacterSettings setting in s.settings)
                foreach (Renderer r in setting.renderers)
                    r.sharedMaterial = setting.material;

                for (int i = 0; i < stylesButtons.Length; i++)
                {
                    ColorBlock colors = stylesButtons[i].colors;
                    colors.colorMultiplier = i == index ? 0.96f : 0.6f;
                    stylesButtons[i].colors = colors;
                }
            }

            public void SetFlat(bool flat)
            {
                bool isCat = !unityChanCopyright.activeInHierarchy;
                float currentTime;
                if (isCat)
                {
                    Animation anim = catAnimation[flat ? 0 : 1];
                    currentTime = anim[anim.clip.name].normalizedTime;
                }
                else
                {
                    Animation anim = unityChanAnimation[flat ? 0 : 1];
                    currentTime = anim[anim.clip.name].normalizedTime;
                }

                shadedGroup.SetActive(!flat);
                flatGroup.SetActive(flat);

                PlayCurrentAnimation(currentTime);

                for (int i = 0; i < textureButtons.Length; i++)
                {
                    ColorBlock colors = textureButtons[i].colors;
                    colors.colorMultiplier = (i == 1 && flat) || (i == 0 && !flat) ? 0.96f : 0.6f;
                    textureButtons[i].colors = colors;
                }
            }

            public void SetCat(bool cat)
            {
                foreach (GameObject c in cats)
                    c.SetActive(cat);
                foreach (GameObject u in unityChans)
                    u.SetActive(!cat);

                if (unityChanDirLight != null) unityChanDirLight.gameObject.SetActive(!cat);

                if (catDirLight != null) catDirLight.gameObject.SetActive(cat);

                unityChanCopyright.SetActive(!cat);

                PlayCurrentAnimation();

                for (int i = 0; i < characterButtons.Length; i++)
                {
                    ColorBlock colors = characterButtons[i].colors;
                    colors.colorMultiplier = (i == 0 && cat) || (i == 1 && !cat) ? 0.96f : 0.6f;
                    characterButtons[i].colors = colors;
                }
            }

            public void SetLightShadows(bool on)
            {
                foreach (Light l in dirLights)
                    l.shadows = on ? LightShadows.Soft : LightShadows.None;

                foreach (Light l in otherLights)
                    l.shadows = on ? LightShadows.Soft : LightShadows.None;
            }

            public void SetAnimation(int index)
            {
                catAnim = index;
                if (catAnim >= catAnimations.Length)
                    catAnim = 0;
                if (catAnim < 0)
                    catAnim = catAnimations.Length - 1;

                foreach (Animation anim in catAnimation) anim.clip = catAnimations[index];

                uchanAnim = index;
                if (uchanAnim >= unityChanAnimations.Length)
                    uchanAnim = 0;
                if (uchanAnim < 0)
                    uchanAnim = unityChanAnimations.Length - 1;

                foreach (Animation anim in unityChanAnimation) anim.clip = unityChanAnimations[index];

                PlayCurrentAnimation();

                for (int i = 0; i < animationButtons.Length; i++)
                {
                    ColorBlock colors = animationButtons[i].colors;
                    colors.colorMultiplier = i == index ? 0.96f : 0.6f;
                    animationButtons[i].colors = colors;
                }
            }

            public void SetAnimationSpeed(float speed)
            {
                playingSpeed = speed;
                UpdateAnimSpeed();
            }

            public void PauseUnpauseAnimation(bool play)
            {
                animationPaused = !play;
                UpdateAnimSpeed();
            }

            private void UpdateAnimSpeed()
            {
                foreach (Animation anim in catAnimation)
                foreach (AnimationState state in anim)
                    state.speed = animationPaused ? 0 : playingSpeed;

                foreach (Animation anim in unityChanAnimation)
                foreach (AnimationState state in anim)
                    state.speed = animationPaused ? 0 : playingSpeed;
            }

            private void PlayCurrentAnimation(float time = -1)
            {
                bool isCat = !unityChanCopyright.activeInHierarchy;
                bool isFlat = flatGroup.activeSelf;
                if (isCat)
                {
                    Animation anim = catAnimation[isFlat ? 1 : 0];
                    anim.Play();
                    if (time >= 0) anim[anim.clip.name].normalizedTime = time;
                }
                else
                {
                    Animation anim = unityChanAnimation[isFlat ? 1 : 0];
                    anim.Play();
                    if (time >= 0) anim[anim.clip.name].normalizedTime = time;

                    // shadows
                    anim = unityChanAnimation[2];
                    anim.Play();
                    if (time >= 0) anim[anim.clip.name].normalizedTime = time;
                }
            }

            [Serializable]
            public class Ambience
            {
                public string name;
                public GameObject[] activate;
                public Material skybox;
            }

            [Serializable]
            public class ShaderStyle
            {
                public string name;
                public CharacterSettings[] settings;

                [Serializable]
                public class CharacterSettings
                {
                    public Material material;
                    public Renderer[] renderers;
                }
            }
        }
    }
}