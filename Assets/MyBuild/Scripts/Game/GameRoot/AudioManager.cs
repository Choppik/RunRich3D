using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace MyBuild.Scripts.Game.GameRoot
{

    /// <summary>
    /// Менеджер всех аудио в игре.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public enum SoundType { None, Music, SFX, UI, Voice }

        [Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            public SoundType type;
            public bool loop;
            public bool playOneShot;
            public bool playAwake;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [SerializeField] private Sound[] sounds;
        [SerializeField] private AudioMixer audioMixer;

        private Dictionary<string, Sound> soundDictionary;

        private void Awake()
        {
            soundDictionary = new Dictionary<string, Sound>();
            foreach (var sound in sounds)
            {
                soundDictionary[sound.name] = sound;

                var source = gameObject.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.volume = sound.volume;
                source.loop = sound.loop;
                source.playOnAwake = sound.playAwake;

                switch (sound.type)
                {
                    case SoundType.Music:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
                        break;
                    case SoundType.SFX:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                        break;
                    case SoundType.UI:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("UI")[0];
                        break;
                    case SoundType.Voice:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Voice")[0];
                        break;
                }
            }
        }

        /// <summary>
        /// Воспроизведение звука.
        /// </summary>
        /// <param name="soundName">Название аудио.</param>
        public void Play(string soundName)
        {
            if (soundDictionary.TryGetValue(soundName, out Sound s))
            {
                var source = GetComponents<AudioSource>().FirstOrDefault(a => a.clip == s.clip);

                if (null != source.clip && source.enabled)
                {
                    if (s.playOneShot)
                    {
                        source.PlayOneShot(s.clip);
                    }
                    else
                    {
                        source.Play();
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Аудио: {soundName} не найдено!");
            }
        }

        /// <summary>
        /// Остановка звука.
        /// </summary>
        /// <param name="soundName">Название аудио.</param>
        public void Stop(string soundName)
        {
            if (soundDictionary.TryGetValue(soundName, out Sound s))
            {
                var source = GetComponents<AudioSource>().FirstOrDefault(a => a.clip == s.clip);

                if (null != source.clip && source.enabled)
                {
                    source.Stop();
                }
            }
            else
            {
                Debug.LogWarning($"Аудио: {soundName} не найдено!");
            }
        }

        /// <summary>
        /// Установка активности для конкретной группы.
        /// </summary>
        /// <param name="type">Тип звуков.</param>
        /// <param name="enable">Устанавливаемая активность звуков.</param>
        public void SetEnable(SoundType type, bool enable)
        {
            foreach (var kvp in soundDictionary)
            {
                if (kvp.Value.type == type)
                {
                    var source = GetComponents<AudioSource>().FirstOrDefault(a => a.clip == kvp.Value.clip);

                    source.enabled = enable;
                }
            }
        }

        /// <summary>
        /// Получение активности для конкретной группы.
        /// </summary>
        /// <param name="type">Тип звуков.</param>
        public bool GetEnable(SoundType type)
        {
            foreach (var kvp in soundDictionary)
            {
                if (kvp.Value.type == type)
                {
                    var source = GetComponents<AudioSource>().FirstOrDefault(a => a.clip == kvp.Value.clip);

                    return source.enabled;
                }
            }

            return false;
        }

        /// <summary>
        /// Изменение звука для группы миксера.
        /// </summary>
        /// <param name="type">Тип звука.</param>
        /// <param name="volume">Громкость.</param>
        public void SetVolume(float volume, SoundType type = SoundType.None)
        {
            float db = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
            switch (type)
            {
                case SoundType.Music:
                    audioMixer.SetFloat("MusicVolume", db);
                    break;
                case SoundType.SFX:
                    audioMixer.SetFloat("SFXVolume", db);
                    break;
                case SoundType.UI:
                    audioMixer.SetFloat("UIVolume", db);
                    break;
                case SoundType.Voice:
                    audioMixer.SetFloat("VoiceVolume", db);
                    break;
                case SoundType.None:
                default:
                    audioMixer.SetFloat("Master", db);
                    break;
            }
        }
    }
}
