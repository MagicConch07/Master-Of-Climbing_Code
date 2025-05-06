using System;
using MKDir;
using Unity.Cinemachine;

public class GameManager : MonoSingleton<GameManager>
{
    public event Action GameResetEvent;
    public PlayerMovement Player;

    private void Start() {
        SoundManager.Instance.Init();
        SoundManager.Instance.Play("Bgm/TitleBGM", Sound.Bgm);
    }
}
