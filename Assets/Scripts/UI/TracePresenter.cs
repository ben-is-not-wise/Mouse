using UnityEngine;

namespace HackedDesign.UI
{
    public class TracePresenter : AbstractPresenter
    {
        [SerializeField] private UnityEngine.UI.Text traceLabel;
        [SerializeField] private UnityEngine.UI.Slider traceSlider;

        public void Repaint(CountdownTimer timer)
        {
            traceLabel.text = timer.Time.ToString("N0");
            traceSlider.maxValue = timer.InitialTime;
            traceSlider.value = timer.Time;
        }
    }
}
