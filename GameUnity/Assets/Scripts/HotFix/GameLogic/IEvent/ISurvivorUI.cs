using DGame;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface ISurvivorUI
    {
        void OnHudDataChanged(SurvivorHudData data);

        void OnResultChanged(string result);
    }
}
