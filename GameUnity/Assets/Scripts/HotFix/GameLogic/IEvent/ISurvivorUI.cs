using DGame;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface ISurvivorUI
    {
        void OnHudChanged(string title, string status);

        void OnResultChanged(string result);
    }
}
