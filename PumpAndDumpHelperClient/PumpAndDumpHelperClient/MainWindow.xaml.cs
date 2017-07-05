using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bittrex;
using Newtonsoft.Json;


namespace PumpAndDumpHelperClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            int TimeSinceLastUpdate = 5;
            UpdateLastPriceUpdateTimer(TimeSinceLastUpdate);
            
        }

        public async Task UpdateLastPriceUpdateTimer(int secondsSinceLastUpdate)
        {
            await Task.Delay(1000);
            int TimeSinceLastUpdate = secondsSinceLastUpdate;
            lbl_LastUpdateTime.Content = TimeSinceLastUpdate + " secs ago.";
            if (TimeSinceLastUpdate < 4)
            {
                secondsSinceLastUpdate = secondsSinceLastUpdate + 1;
                UpdateLastPriceUpdateTimer(secondsSinceLastUpdate);
            }
            else
            {
                secondsSinceLastUpdate = 0;
                UpdateLastPriceUpdateTimer(secondsSinceLastUpdate);
            }

        }


        public async Task GetTicker()
        {
            var exc = new Exchange();
            var context = new ExchangeContext();

            context.ApiKey = txt_ApiKey.Text;
            context.Secret = txt_SecretKey.Text;
            context.Simulate = false;
            context.QuoteCurrency = "BTC";
            exc.Initialise(context);

            var coinToCheck = txt_tickerValue.Text;
            if (!string.IsNullOrEmpty(coinToCheck))
            {

                var coinTickerInfo = exc.GetTicker(txt_tickerValue.Text);

                var updatedPrice_Last = coinTickerInfo.Last;
                var updatedPrice_Bid = coinTickerInfo.Bid;
                var updatedPrice_Ask = coinTickerInfo.Ask;

                lbl_CurrentBuyPriceAmountInBTC.Content = updatedPrice_Bid;
                lbl_CurrentSalePriceAmountInBTC.Content = updatedPrice_Ask;
            }
        }


        public async Task CalculateBuyPrices()
            {

            }

        public async Task CalculateSellPrices()
            {

            }

        public async Task CalculatePriceOfBTC()
            {

            }

        public async Task CalculateProfitPerCoin()
            {

            }

        public async Task CalculateROI()
            {

            }


        private void btn_ManuallyUpdateTickerInfo_Click(object sender, RoutedEventArgs e)
        {
            //fire off one instance of the "refresh" functionality once it's all fleshed out.
        }
    }
}
