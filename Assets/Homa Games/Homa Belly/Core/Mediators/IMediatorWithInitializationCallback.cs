using System;

namespace HomaGames.HomaBelly
{
    public interface IMediatorWithInitializationCallback : IMediator
    {
        void Initialize(Action onInitialized = null);
    }
}
