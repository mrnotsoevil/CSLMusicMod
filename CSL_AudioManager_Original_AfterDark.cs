using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

public class AudioManager : SimulationManagerBase<AudioManager, AudioProperties>, IAudibleManager
{
	//
	// Static Fields
	//
	public const int STREAM_BUFFER_LIMIT = 8192;

	private static FastList<IAudibleManager> m_audibles = new FastList<IAudibleManager>();

	public const int BYTES_PER_CHANNEL = 2;

	public const int STREAM_BUFFER_SIZE = 65536;

	//
	// Fields
	//
	private Stream m_currentMusicStream;

	private Stream m_previousMusicStream;

	private object m_streamLock;

	private Thread m_streamThread;

	private float[] m_streamBuffer;

	private volatile string m_musicFile;

	private byte[] m_tempBuffer2;

	private byte[] m_tempBuffer1;

	private string[] m_musicFilesNight;

	private volatile int m_bufferWritePos;

	private SavedFloat m_musicAudioVolume;

	private SavedFloat m_mainAudioVolume;

	private volatile float m_masterVolume;

	private volatile bool m_muteAll;

	private volatile float m_musicVolume;

	private volatile bool m_needMoreData;

	private volatile bool m_terminated;

	private volatile int m_bufferReadPos;

	private string[] m_musicFiles;

	private CameraController m_cameraController;

	[NonSerialized]
	public AudioManager.AudioPlayer m_sourcePool;

	[NonSerialized]
	public Transform m_sourcePoolRoot;

	public int m_streamCrossFade;

	public string m_previousMusicFile;

	public string m_currentMusicFile;

	private AudioListener m_audioListener;

	private string m_audioLocation;

	private FastList<AudioManager.SimulationEvent> m_eventBuffer;

	private float[] m_subServiceProximity;

	private float[] m_serviceProximity;

	private AudioManager.ListenerInfo m_listenerInfo;

	private AudioGroup m_ambientGroup;

	private AudioGroup m_defaultGroup;

	//
	// Properties
	//
	public AudioGroup DefaultGroup
	{
		get
		{
			return this.m_defaultGroup;
		}
	}

	public bool isAfterDarkMenu
	{
		get;
		set;
	}

	public bool isShowingCredits
	{
		get;
		set;
	}

	public float MasterVolume
	{
		get
		{
			return this.m_masterVolume;
		}
	}

	public bool MuteAll
	{
		get
		{
			return this.m_muteAll;
		}
		set
		{
			this.m_muteAll = value;
		}
	}

	//
	// Static Methods
	//
	private static Transform GetSourcePoolRoot()
	{
		GameObject gameObject = GameObject.Find("Audio Pool");
		if (gameObject == null)
		{
			gameObject = new GameObject("Audio Pool");
			Object.DontDestroyOnLoad(gameObject);
		}
		return gameObject.transform;
	}

	public static void RegisterAudibleManager(IAudibleManager manager)
	{
		if (manager != null)
		{
			AudioManager.m_audibles.Add(manager);
		}
	}

	//
	// Methods
	//
	public void AddEvent(AudioGroup group, AudioInfo info, Vector3 position, Vector3 velocity, float maxDistance, float volume, float pitch)
	{
		AudioManager.SimulationEvent item;
		item.m_group = group;
		item.m_info = info;
		item.m_position = position;
		item.m_velocity = velocity;
		item.m_maxDistance = maxDistance;
		item.m_volume = volume;
		item.m_pitch = pitch;
		while (!Monitor.TryEnter(this.m_eventBuffer, SimulationManager.SYNCHRONIZE_TIMEOUT))
		{
		}
		try
		{
			this.m_eventBuffer.Add(item);
		}
		finally
		{
			Monitor.Exit(this.m_eventBuffer);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_audioLocation = Path.Combine(DataLocation.gameContentPath, "Audio");
		this.m_currentMusicFile = null;
		this.m_previousMusicFile = null;
		this.m_streamCrossFade = 0;
		this.m_listenerInfo = new AudioManager.ListenerInfo();
		this.m_defaultGroup = new AudioGroup(3, new SavedFloat(Settings.uiAudioVolume, Settings.gameSettingsFile, DefaultSettings.uiAudioVolume, true));
		this.m_ambientGroup = new AudioGroup(3, new SavedFloat(Settings.ambientAudioVolume, Settings.gameSettingsFile, DefaultSettings.ambientAudioVolume, true));
		this.m_serviceProximity = new float[21];
		this.m_subServiceProximity = new float[20];
		this.m_eventBuffer = new FastList<AudioManager.SimulationEvent>();
		this.m_mainAudioVolume = new SavedFloat(Settings.mainAudioVolume, Settings.gameSettingsFile, DefaultSettings.mainAudioVolume, true);
		this.m_musicAudioVolume = new SavedFloat(Settings.musicAudioVolume, Settings.gameSettingsFile, DefaultSettings.musicAudioVolume, true);
		this.m_tempBuffer1 = new byte[16384];
		this.m_tempBuffer2 = new byte[16384];
		this.m_streamBuffer = new float[65536];
		this.m_streamLock = new object();
		this.m_streamThread = new Thread(new ThreadStart(this.StreamThread));
		this.m_streamThread.Name = "Music Stream";
		this.m_streamThread.Priority = ThreadPriority.AboveNormal;
		this.m_streamThread.Start();
		if (!this.m_streamThread.IsAlive)
		{
			CODebugBase<LogChannel>.Error(LogChannel.Core, "Audio stream thread failed to start!");
		}
		GameObject gameObject = new GameObject("Audio Listener");
		Object.DontDestroyOnLoad(gameObject);
		this.m_audioListener = gameObject.AddComponent<AudioListener>();
		this.m_audioListener.enabled = false;
		gameObject.AddComponent<MusicFilter>();
	}

	public override void DestroyProperties(AudioProperties properties)
	{
		if (this.m_properties == properties)
		{
			if (this.m_defaultGroup != null)
			{
				this.m_defaultGroup.Reset();
			}
			if (this.m_ambientGroup != null)
			{
				this.m_ambientGroup.Reset();
			}
			this.m_cameraController = null;
			this.m_musicFiles = null;
			this.m_musicFilesNight = null;
		}
		base.DestroyProperties(properties);
	}

	private void FillStreamBuffer()
	{
		string musicFile = this.m_musicFile;
		string text = musicFile;
		int num = 1;
		if (this.m_muteAll)
		{
			text = null;
			num = 4;
		}
		if (text != this.m_currentMusicFile)
		{
			if (text == this.m_previousMusicFile)
			{
				this.m_previousMusicFile = this.m_currentMusicFile;
				this.m_currentMusicFile = text;
				Stream currentMusicStream = this.m_currentMusicStream;
				this.m_currentMusicStream = this.m_previousMusicStream;
				this.m_previousMusicStream = currentMusicStream;
				this.m_streamCrossFade = 65536 - this.m_streamCrossFade;
			}
			else
			{
				if (this.m_previousMusicStream == null)
				{
					this.m_previousMusicFile = this.m_currentMusicFile;
					this.m_previousMusicStream = this.m_currentMusicStream;
					this.m_currentMusicFile = text;
					if (text == null || text == string.Empty)
					{
						this.m_currentMusicStream = null;
					}
					else
					{
						try
						{
							this.m_currentMusicStream = new FileStream(text, FileMode.Open, FileAccess.Read);
							if (this.m_previousMusicStream != null)
							{
								long length = this.m_previousMusicStream.Length;
								long length2 = this.m_currentMusicStream.Length;
								if (Mathf.Abs((float)(length - length2)) < 1024)
								{
									long position = this.m_previousMusicStream.Position;
									if (position < length2)
									{
										this.m_currentMusicStream.Position = position;
									}
								}
							}
						}
						catch (Exception ex)
						{
							this.m_currentMusicStream = null;
							CODebugBase<LogChannel>.Error(LogChannel.Core, "Failed to open audio stream: " + ex.Message + "
" + ex.StackTrace);
						}
					}
					this.m_streamCrossFade = 65536 - this.m_streamCrossFade;
				}
			}
		}
		while ((this.m_bufferReadPos - this.m_bufferWritePos & 65535) >= 8192)
		{
			int num2 = this.m_bufferWritePos;
			this.FillTempBuffer(this.m_currentMusicStream, this.m_tempBuffer1);
			if (this.m_streamCrossFade == 65536)
			{
				int num3 = 0;
				for (int i = 0; i < 8192; i++)
				{
					short num4 = (short)this.m_tempBuffer1[num3++];
					num4 |= (short)(this.m_tempBuffer1[num3++] << 8);
					this.m_streamBuffer[num2++] = (float)num4 * 3,051758E-05;
					num2 &= 65535;
				}
			}
			else
			{
				this.FillTempBuffer(this.m_previousMusicStream, this.m_tempBuffer2);
				int num5 = 0;
				int num6 = 0;
				int num7 = 2;
				for (int j = 0; j < 8192; j += num7)
				{
					for (int k = 0; k < num7; k++)
					{
						short num8 = (short)this.m_tempBuffer1[num5++];
						num8 |= (short)(this.m_tempBuffer1[num5++] << 8);
						short num9 = (short)this.m_tempBuffer2[num6++];
						num9 |= (short)(this.m_tempBuffer2[num6++] << 8);
						int num10 = ((int)num9 << 16) + (int)(num8 - num9) * this.m_streamCrossFade;
						this.m_streamBuffer[num2++] = (float)num10 * 4,656613E-10;
						num2 &= 65535;
					}
					int num11 = this.m_streamCrossFade + num;
					if (num11 > 65536)
					{
						this.m_streamCrossFade = 65536;
					}
					else
					{
						this.m_streamCrossFade = num11;
					}
				}
			}
			this.m_bufferWritePos = num2;
		}
		if (this.m_streamCrossFade == 65536 && this.m_previousMusicStream != null && (this.m_currentMusicStream != null || musicFile == null))
		{
			this.m_previousMusicStream.Dispose();
			this.m_previousMusicStream = null;
			this.m_previousMusicFile = null;
		}
	}

	private void FillTempBuffer(Stream stream, byte[] buffer)
	{
		int num = buffer.Length;
		int i = 0;
		do
		{
			int num2 = num - i;
			if (stream != null)
			{
				long num3 = stream.Length - stream.Position;
				if ((long)num2 >= num3)
				{
					num2 = stream.Read(buffer, i, (int)num3);
					stream.Position = 0;
				}
				else
				{
					num2 = stream.Read(buffer, i, num2);
				}
			}
			else
			{
				num2 = 0;
			}
			i += num2;
			if (num2 == 0)
			{
				while (i < num)
				{
					buffer[i++] = 0;
				}
			}
		}
		while (i < num);
	}

	public override void InitializeProperties(AudioProperties properties)
	{
		base.InitializeProperties(properties);
		GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
		if (gameObject != null)
		{
			this.m_cameraController = gameObject.GetComponent<CameraController>();
		}
		if (properties.m_musicFiles != null)
		{
			this.m_musicFiles = new string[properties.m_musicFiles.Length];
			for (int i = 0; i < properties.m_musicFiles.Length; i++)
			{
				this.m_musicFiles[i] = Path.Combine(this.m_audioLocation, properties.m_musicFiles[i]);
			}
		}
		if (properties.m_musicFilesNight != null)
		{
			this.m_musicFilesNight = new string[properties.m_musicFilesNight.Length];
			for (int j = 0; j < properties.m_musicFilesNight.Length; j++)
			{
				this.m_musicFilesNight[j] = Path.Combine(this.m_audioLocation, properties.m_musicFilesNight[j]);
			}
		}
	}

	private void LateUpdate()
	{
		this.m_masterVolume = this.m_mainAudioVolume;
		this.m_musicVolume = this.m_masterVolume * this.m_musicAudioVolume;
		if (this.m_cameraController != null)
		{
			Vector3 position;
			if (this.m_properties != null)
			{
				position = this.m_properties.m_listenerOffset;
			}
			else
			{
				position = Vector3.zero;
			}
			Transform transform = this.m_cameraController.transform;
			Transform transform2 = this.m_audioListener.transform;
			transform2.position = transform.TransformPoint(position);
			transform2.rotation = transform.rotation;
			this.m_audioListener.enabled = false;
			this.m_audioListener.enabled = true;
		}
		else
		{
			if (!this.m_audioListener.enabled)
			{
				this.m_audioListener.enabled = true;
			}
		}
		if (!Singleton<LoadingManager>.instance.m_currentlyLoading)
		{
			this.m_listenerInfo.m_position = this.m_audioListener.transform.position;
			this.m_listenerInfo.m_dopplerLevel = 0,5 / (float)Mathf.Max(1, Singleton<SimulationManager>.instance.FinalSimulationSpeed);
		}
		int size = this.m_eventBuffer.m_size;
		for (int i = 0; i < size; i++)
		{
			AudioManager.SimulationEvent simulationEvent = this.m_eventBuffer.m_buffer[i];
			if (Vector3.SqrMagnitude(this.m_listenerInfo.m_position - simulationEvent.m_position) < simulationEvent.m_maxDistance * simulationEvent.m_maxDistance)
			{
				simulationEvent.m_group.AddPlayer(this.m_listenerInfo, 0, simulationEvent.m_info, simulationEvent.m_position, simulationEvent.m_velocity, simulationEvent.m_maxDistance, simulationEvent.m_volume, simulationEvent.m_pitch);
			}
			this.m_eventBuffer.m_buffer[i] = default(AudioManager.SimulationEvent);
		}
		while (!Monitor.TryEnter(this.m_eventBuffer, SimulationManager.SYNCHRONIZE_TIMEOUT))
		{
		}
		try
		{
			for (int j = size; j < this.m_eventBuffer.m_size; j++)
			{
				this.m_eventBuffer.m_buffer[j - size] = this.m_eventBuffer.m_buffer[j];
				this.m_eventBuffer.m_buffer[j] = default(AudioManager.SimulationEvent);
			}
			this.m_eventBuffer.m_size -= size;
		}
		finally
		{
			Monitor.Exit(this.m_eventBuffer);
		}
		try
		{
			for (int k = 0; k < AudioManager.m_audibles.m_size; k++)
			{
				AudioManager.m_audibles.m_buffer[k].PlayAudio(this.m_listenerInfo);
			}
		}
		finally
		{
		}
	}

	public AudioManager.AudioPlayer ObtainPlayer(AudioInfo info)
	{
		if (this.m_sourcePool != null)
		{
			AudioManager.AudioPlayer sourcePool = this.m_sourcePool;
			this.m_sourcePool = sourcePool.m_nextPlayer;
			sourcePool.m_nextPlayer = null;
			sourcePool.m_info = info;
			sourcePool.m_source.gameObject.SetActive(true);
			sourcePool.m_source.clip = info.ObtainClip();
			return sourcePool;
		}
		if (this.m_sourcePoolRoot == null)
		{
			this.m_sourcePoolRoot = AudioManager.GetSourcePoolRoot();
		}
		GameObject gameObject = new GameObject("Audio Source");
		gameObject.transform.parent = this.m_sourcePoolRoot;
		return new AudioManager.AudioPlayer {
			m_source = gameObject.AddComponent<AudioSource>(),
			m_transform = gameObject.transform,
			m_info = info,
			m_source =  {
				playOnAwake = false,
				clip = info.ObtainClip()
			}
		};
	}

	public AudioManager.AudioPlayer ObtainPlayer(AudioClip clip)
	{
		if (this.m_sourcePool != null)
		{
			AudioManager.AudioPlayer sourcePool = this.m_sourcePool;
			this.m_sourcePool = sourcePool.m_nextPlayer;
			sourcePool.m_nextPlayer = null;
			sourcePool.m_info = null;
			sourcePool.m_source.gameObject.SetActive(true);
			sourcePool.m_source.clip = clip;
			return sourcePool;
		}
		if (this.m_sourcePoolRoot == null)
		{
			this.m_sourcePoolRoot = AudioManager.GetSourcePoolRoot();
		}
		GameObject gameObject = new GameObject("Audio Source");
		gameObject.transform.parent = this.m_sourcePoolRoot;
		return new AudioManager.AudioPlayer {
			m_source = gameObject.AddComponent<AudioSource>(),
			m_transform = gameObject.transform,
			m_info = null,
			m_source =  {
				playOnAwake = false,
				clip = clip
			}
		};
	}

	private void OnDestroy()
	{
		while (!Monitor.TryEnter(this.m_streamLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
		{
		}
		try
		{
			this.m_terminated = true;
			Monitor.PulseAll(this.m_streamLock);
		}
		finally
		{
			Monitor.Exit(this.m_streamLock);
		}
	}

	virtual void PlayAudio(AudioManager.ListenerInfo listenerInfo)
	{
		base.PlayAudio(listenerInfo);
	}

	protected override void PlayAudioImpl(AudioManager.ListenerInfo listenerInfo)
	{
		this.m_defaultGroup.UpdatePlayers(listenerInfo, this.m_masterVolume);
		float listenerHeight;
		if (this.m_properties != null && Singleton<LoadingManager>.instance.m_loadingComplete)
		{
			listenerHeight = Mathf.Max(0, listenerInfo.m_position.y - Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(listenerInfo.m_position, true, 0));
		}
		else
		{
			listenerHeight = 0;
		}
		this.UpdateAmbient(listenerInfo, listenerHeight);
		this.UpdateMusic(listenerInfo, listenerHeight);
	}

	public void PlaySound(AudioInfo info, float volume)
	{
		this.m_defaultGroup.AddPlayer(0, info, volume);
	}

	public void PlaySound(AudioInfo info)
	{
		this.m_defaultGroup.AddPlayer(0, info, 1);
	}

	public void PlaySound(AudioClip clip, float volume, bool loop)
	{
		this.m_defaultGroup.AddPlayer(0, clip, volume, loop);
	}

	public void PlaySound(AudioClip clip, float volume)
	{
		this.PlaySound(clip, volume, false);
	}

	public void PlaySound(AudioClip clip)
	{
		this.m_defaultGroup.AddPlayer(0, clip, 1, false);
	}

	public void ReadStreamBuffer(float[] target, int channels)
	{
		float musicVolume = this.m_musicVolume;
		int num = this.m_bufferReadPos;
		int num2 = target.Length;
		int num3 = this.m_bufferWritePos - this.m_bufferReadPos & 65535;
		if (num3 == 0)
		{
			num3 = 65534;
		}
		else
		{
			num3 = Mathf.Max(0, num3 - 2);
		}
		if (num3 * channels < num2 * 2)
		{
			num2 = num3 * channels / 2;
		}
		if (channels == 1)
		{
			for (int i = 0; i < num2; i++)
			{
				float num4 = this.m_streamBuffer[num++];
				float num5 = this.m_streamBuffer[num++];
				target[i] += (num4 + num5) * 0,5 * musicVolume;
				num &= 65535;
			}
		}
		else
		{
			if (channels >= 2)
			{
				for (int j = 0; j < num2; j += channels)
				{
					target[j] += this.m_streamBuffer[num++] * musicVolume;
					target[j + 1] += this.m_streamBuffer[num++] * musicVolume;
					num &= 65535;
				}
			}
		}
		this.m_bufferReadPos = num;
		num3 = (this.m_bufferWritePos - this.m_bufferReadPos & 65535);
		if (num3 < 57344)
		{
			while (!Monitor.TryEnter(this.m_streamLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
			{
			}
			try
			{
				this.m_needMoreData = true;
				Monitor.Pulse(this.m_streamLock);
			}
			finally
			{
				Monitor.Exit(this.m_streamLock);
			}
		}
	}

	public void ReleasePlayer(AudioManager.AudioPlayer player)
	{
		if (player.m_source != null)
		{
			player.m_source.Stop();
			player.m_source.clip = null;
			player.m_source.gameObject.SetActive(false);
		}
		if (player.m_info != null)
		{
			player.m_info.ReleaseClip();
			player.m_info = null;
		}
		player.m_nextPlayer = this.m_sourcePool;
		this.m_sourcePool = player;
	}

	private void StreamThread()
	{
		while (true)
		{
			while (!Monitor.TryEnter(this.m_streamLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
			{
			}
			try
			{
				while (!this.m_needMoreData && !this.m_terminated)
				{
					Monitor.Wait(this.m_streamLock);
				}
				if (this.m_terminated)
				{
					if (this.m_currentMusicStream != null)
					{
						this.m_currentMusicStream.Dispose();
						this.m_currentMusicStream = null;
					}
					if (this.m_previousMusicStream != null)
					{
						this.m_previousMusicStream.Dispose();
						this.m_previousMusicStream = null;
					}
					break;
				}
				this.m_needMoreData = false;
			}
			finally
			{
				Monitor.Exit(this.m_streamLock);
			}
			try
			{
				this.FillStreamBuffer();
			}
			catch (Exception ex)
			{
				UIView.ForwardException(ex);
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Audio stream error: " + ex.Message + "
" + ex.StackTrace);
			}
		}
	}

	private void UpdateAmbient(AudioManager.ListenerInfo listenerInfo, float listenerHeight)
	{
		if (!Singleton<LoadingManager>.instance.m_currentlyLoading && this.m_properties != null && this.m_properties.m_ambients != null && this.m_properties.m_ambients.Length != 0 && this.m_ambientGroup.m_totalVolume >= 0,01)
		{
			float num = Mathf.Clamp01(listenerHeight * 0,001) - 1;
			num *= num;
			float num2 = Singleton<NaturalResourceManager>.instance.CalculateForestProximity(listenerInfo.m_position, 500 - num * 400);
			float num3 = 0;
			float num4 = Singleton<TerrainManager>.instance.CalculateWaterProximity(listenerInfo.m_position, 400 - num * 300, out num3);
			Singleton<BuildingManager>.instance.CalculateServiceProximity(listenerInfo.m_position, 400 - num * 300, this.m_serviceProximity, this.m_subServiceProximity);
			float volume = 1 - num;
			float volume2 = (num2 + this.m_subServiceProximity[6]) / (1 + listenerHeight * 0,002);
			float volume3 = num4 * (1 - num3) / (1 + listenerHeight * 0,003);
			float volume4 = num4 * num3 / (1 + listenerHeight * 0,003);
			float volume5 = (this.m_subServiceProximity[5] + this.m_subServiceProximity[8] + this.m_subServiceProximity[9]) / (1 + listenerHeight * 0,004);
			float volume6 = (this.m_serviceProximity[17] + this.m_serviceProximity[12]) / (1 + listenerHeight * 0,004);
			float volume7 = (this.m_subServiceProximity[1] + this.m_subServiceProximity[3]) / (1 + listenerHeight * 0,004);
			float volume8 = (this.m_subServiceProximity[2] + this.m_subServiceProximity[4] + this.m_serviceProximity[8]) / (1 + listenerHeight * 0,004);
			float volume9 = this.m_subServiceProximity[7] / (1 + listenerHeight * 0,004);
			float volume10 = this.m_subServiceProximity[18] / (1 + listenerHeight * 0,004);
			float volume11 = this.m_subServiceProximity[19] / (1 + listenerHeight * 0,004);
			AudioInfo[] array;
			if (Singleton<SimulationManager>.instance.m_isNightTime && this.m_properties.m_ambientsNight != null && this.m_properties.m_ambientsNight.Length != 0)
			{
				array = this.m_properties.m_ambientsNight;
			}
			else
			{
				array = this.m_properties.m_ambients;
			}
			this.m_ambientGroup.AddPlayer(0, array[0], volume);
			this.m_ambientGroup.AddPlayer(1, array[1], volume2);
			this.m_ambientGroup.AddPlayer(2, array[2], volume3);
			this.m_ambientGroup.AddPlayer(3, array[3], volume4);
			this.m_ambientGroup.AddPlayer(4, array[4], volume5);
			this.m_ambientGroup.AddPlayer(5, array[5], volume6);
			this.m_ambientGroup.AddPlayer(6, array[6], volume7);
			this.m_ambientGroup.AddPlayer(7, array[7], volume8);
			this.m_ambientGroup.AddPlayer(8, array[8], volume9);
			this.m_ambientGroup.AddPlayer(9, array[9], volume10);
			this.m_ambientGroup.AddPlayer(10, array[10], volume11);
		}
		this.m_ambientGroup.UpdatePlayers(listenerInfo, (!this.m_muteAll) ? this.m_masterVolume : 0);
	}

	private void UpdateMusic(AudioManager.ListenerInfo listenerInfo, float listenerHeight)
	{
		if (!Singleton<LoadingManager>.instance.m_currentlyLoading)
		{
			if (this.m_musicVolume < 0,01)
			{
				this.m_musicFile = null;
			}
			else
			{
				if (this.m_musicFiles != null && this.m_musicFiles.Length != 0)
				{
					string[] array = this.m_musicFiles;
					AudioManager.MusicType musicType = AudioManager.MusicType.Worst;
					if (this.isAfterDarkMenu)
					{
						musicType = AudioManager.MusicType.Normal;
					}
					if (this.isShowingCredits)
					{
						musicType = AudioManager.MusicType.Bad;
					}
					if (Singleton<LoadingManager>.instance.m_loadingComplete)
					{
						if (Singleton<SimulationManager>.instance.m_isNightTime && this.m_musicFilesNight != null && this.m_musicFilesNight.Length != 0)
						{
							array = this.m_musicFilesNight;
						}
						int finalHappiness = (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_finalHappiness;
						if (finalHappiness < 25)
						{
							musicType = AudioManager.MusicType.Worst;
						}
						else
						{
							if (finalHappiness < 40)
							{
								musicType = AudioManager.MusicType.Bad;
							}
							else
							{
								if (finalHappiness < 55)
								{
									musicType = AudioManager.MusicType.Normal;
								}
								else
								{
									if (finalHappiness < 70)
									{
										musicType = AudioManager.MusicType.Good;
									}
									else
									{
										if (finalHappiness < 85)
										{
											musicType = AudioManager.MusicType.VeryWell;
										}
										else
										{
											if (finalHappiness < 95)
											{
												musicType = AudioManager.MusicType.NearWonder;
											}
											else
											{
												musicType = AudioManager.MusicType.AlmostWonder;
											}
										}
									}
								}
							}
						}
						if (listenerHeight > 1400)
						{
							musicType += 7;
						}
					}
					int num = Mathf.Min((int)musicType, array.Length - 1);
					this.m_musicFile = array[num];
				}
			}
		}
	}

	//
	// Nested Types
	//
	public enum AmbientType
	{
		None = -1,
		World,
		Forest,
		Sea,
		Stream,
		Industrial,
		Plaza,
		Suburban,
		City,
		Agricultural,
		Leisure,
		Tourist
	}

	public class AudioPlayer
	{
		public AudioSource m_source;

		public Transform m_transform;

		public AudioInfo m_info;

		public AudioManager.AudioPlayer m_nextPlayer;

		public Vector3 m_velocity;

		public float m_fadeSpeed;

		public int m_id;

		public bool m_is3d;

		public bool m_notReady;
	}

	public class ListenerInfo
	{
		public Vector3 m_position;

		public float m_dopplerLevel;
	}

	public enum MusicType
	{
		None = -1,
		Worst,
		Bad,
		Normal,
		Good,
		VeryWell,
		NearWonder,
		AlmostWonder,
		Sky_Worst,
		Sky_Bad,
		Sky_Normal,
		Sky_Good,
		Sky_VeryWell,
		Sky_NearWonder,
		Sky_AlmostWonder
	}

	public struct SimulationEvent
	{
		public AudioGroup m_group;

		public AudioInfo m_info;

		public Vector3 m_position;

		public Vector3 m_velocity;

		public float m_maxDistance;

		public float m_volume;

		public float m_pitch;
	}
}
