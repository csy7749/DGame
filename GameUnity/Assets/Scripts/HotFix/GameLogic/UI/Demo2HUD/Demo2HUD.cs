using System;
using UnityEngine;

namespace GameLogic
{
    public class Demo2HUD : Demo2HUDAuto
    {
        protected override UILayer windowLayer => UILayer.UI;

        protected override ModelType GetModelType() => ModelType.NoneType;

        protected override void RegisterEvent()
        {
            AddUIEvent<SurvivorHudData>(ISurvivorUI_Event.OnHudDataChanged, OnHudDataChanged);
        }

        protected override void OnSliderExpChange(float value)
        {
        }

        protected override void OnSliderHpChange(float value)
        {
        }

        private void OnHudDataChanged(SurvivorHudData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            m_sliderExp.SetValueWithoutNotify(data.ExperienceProgress);
            m_textLevel.text = $"Lv.{data.Level:0}";
            m_textKill.text = data.KillCount.ToString("0");
            m_textTimer.text = FormatTime(data.RemainingTime);
            m_sliderHp.SetValueWithoutNotify(data.HealthProgress);
        }

        private static string FormatTime(float time)
        {
            int seconds = Mathf.FloorToInt(time);
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }
    }
}
