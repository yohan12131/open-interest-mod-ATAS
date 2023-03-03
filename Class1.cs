using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ATAS.Indicators.Technical.Properties;
using System.Globalization;

namespace ATAS.Indicators.Technical
{
    [DisplayName("Open Interest")]

    public class OpenInterest : Indicator
    {
        #region Nested types

        public enum OpenInterestMode
        {
            [Display(ResourceType = typeof(Resources), Name = "ByBar")]
            ByBar,

            [Display(ResourceType = typeof(Resources), Name = "Session")]
            Session,

            [Display(ResourceType = typeof(Resources), Name = "Cumulative")]
            Cumulative
        }

        #endregion

        #region Fields

        private readonly CandleDataSeries _oi = new("Open interest");
        private readonly ValueDataSeries _diff = new("OI NETO");
        private const decimal Million = 1000000m;
        private OpenInterestMode _mode = OpenInterestMode.ByBar;
      
        #endregion

        #region Properties

        [Display(ResourceType = typeof(Resources), Name = "Mode")]
        public OpenInterestMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                RecalculateValues();
            }
        }
       
        #endregion

        #region ctor

        public OpenInterest()
            : base(true)
        {
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.OnlyValueOnAxis;
           // DataSeries[0].Name = "Value";
            DataSeries.Add(_oi);
            DataSeries.Add(_diff);     
            Panel = IndicatorDataProvider.NewPanel;        
        }

        #endregion

        #region Protected methods

        protected override void OnCalculate(int bar, decimal value)
        {
            if (bar == 0)
                return;
            var currentCandle = GetCandle(bar);
            if (currentCandle.OI == 0)
                return;
            var prevCandle = GetCandle(bar - 1);
            var currentOpen = prevCandle.OI;
            var candle = _oi[bar];
                 switch (_mode)
            {
                case OpenInterestMode.ByBar:
                    candle.Open = 0;
                    candle.Close = Math.Round((currentCandle.OI - currentOpen) / Million, 2);
                    candle.High = (currentCandle.MaxOI - currentOpen) / Million;
                    candle.Low = (currentCandle.MinOI - currentOpen) / Million;
                    _diff[bar] = Math.Round((candle.Close - candle.Open) * Million, 2);
                    this[bar] = decimal.Parse(candle.Close.ToString("0.00"), CultureInfo.InvariantCulture);                 
                    break;
                case OpenInterestMode.Cumulative:
                    candle.Open = decimal.Round(currentOpen / Million, 2);
                    candle.Close = decimal.Round(currentCandle.OI / Million, 2);
                    candle.High = decimal.Round(currentCandle.MaxOI / Million, 2);
                    candle.Low = decimal.Round(currentCandle.MinOI / Million, 2);
                    _diff[bar] = Math.Round((candle.Close - candle.Open) * Million, 2);
                  break;
                default:
                    var prevvalue = _oi[bar - 1].Close;
                    var dOi = currentOpen - prevvalue;

                    if (IsNewSession(bar))
                    dOi = currentOpen;
                    candle.Open = (currentOpen - dOi) / Million;
                    candle.Close = (currentCandle.OI - dOi) / Million;
                    candle.High = (currentCandle.MaxOI - dOi) / Million;
                    candle.Low = (currentCandle.MinOI - dOi) / Million;
                    _diff[bar] = Math.Round((candle.Close - candle.Open) * Million, 2);
                    this[bar] = decimal.Parse(candle.Close.ToString("0.00"), CultureInfo.InvariantCulture);
                    break;
            }


            _diff[bar] = candle.Close - candle.Open;
            this[bar] = candle.Close;
        }
    }
    #endregion
}
