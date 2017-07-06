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
            double TimeSinceLastUpdate = 5;
            UpdateUI(TimeSinceLastUpdate);

            
        }

        public async Task UpdateUI(double secondsSinceLastUpdate)
        {
            //grab value from text box. 
            string tickerAbbreviation = txt_tickerValue.Text;

            //todo: eventually add a time delayed auto refreshing call to GetTicker()
            //api call(s) entry point to get ticker info
            GetTicker(tickerAbbreviation);
            
            //do calculations
            CalculateBuyPrices();
            CalculateSellPrices();
            CalculatePriceOfBTC();
            CalculateProfitPerCoin();
            CalculateROI();

            

            //update "Last update: __ secs ago" label controls
            UpdateLastPriceUpdateTimer(secondsSinceLastUpdate);
        }

        public async Task UpdateLastPriceUpdateTimer(double secondsSinceLastUpdate)
            {

            //really hacky way of updating once per second...dat infinite recursion doe.
            
            double TimeSinceLastUpdate = secondsSinceLastUpdate;
            lbl_LastUpdateTime.Content = Math.Round(TimeSinceLastUpdate, 2, MidpointRounding.AwayFromZero) + " secs ago.";
            if (TimeSinceLastUpdate <= 3.1)
                {
                await Task.Delay(100);
                secondsSinceLastUpdate = secondsSinceLastUpdate + .1;
                UpdateLastPriceUpdateTimer(secondsSinceLastUpdate);
                }
            else
                {
                secondsSinceLastUpdate = 0;
                UpdateLastPriceUpdateTimer(secondsSinceLastUpdate);
                }

            }


        public async Task GetTicker(string coinToCheck)
        {
            var exc = new Exchange();
            var context = new ExchangeContext();

            context.ApiKey = txt_ApiKey.Text;
            context.Secret = txt_SecretKey.Text;
            context.Simulate = true;
            context.QuoteCurrency = "BTC";
            exc.Initialise(context);

            
            if (!string.IsNullOrEmpty(coinToCheck))
                //only update the prices if there's more than one char typed in.
                //hopefully this prevents the API from banning our IP address.
            {
                var coinTickerInfo = exc.GetTicker(coinToCheck);

                var updatedPrice_Last = coinTickerInfo.Last;
                var updatedPrice_Bid = coinTickerInfo.Bid;
                var updatedPrice_Ask = coinTickerInfo.Ask;

                lbl_CurrentBuyPriceAmountInBTC.Content = updatedPrice_Bid;
                lbl_CurrentSalePriceAmountInBTC.Content = updatedPrice_Ask;
            }
        }


        public async Task CalculateBuyPrices()
            {

            // 1. Find out how much BTC you have available in your account.
            // 2. find the current buy price (in btc/satoshis)
            // 3. divide step 1 by step 2. 
            // 4. Take the answer and convert to $USD and update some local variable
            // 5. update the UI

            }

        public async Task CalculateSellPrices()
            {

            // 1. Find out how much of the selected coin you have available in your account.
            // 2. find the current sale price (in btc/satoshis)
            // 3. divide step 1 by step 2. 
            // 4. Take the answer and convert to $USD and update some local variable
            // 5. update the UI

            }

        public async Task CalculatePriceOfBTC()
            {

            // Should be a simple api call to get the current price of the BTC-USD ticker.  Does bittrex even have this?  hmm...

            }

        public async Task CalculateProfitPerCoin()
            {
            // figure out how much the transaction fee will be.
            // convert the transaction fee to USD.
            // ((Buy price - Saleprice) - transactionfees/taxes/w.e else) = profit
            //update UI with profit

            }

        public async Task CalculateROI()
            {
            // Profit divided by the sum of the amount of coins you have. converted to a percentage.
            }


        private void btn_ManuallyUpdateTickerInfo_Click(object sender, RoutedEventArgs e)
        {
            //fire off one instance of the "refresh" functionality once it's all fleshed out.

            //grab value from text box. 
            string tickerAbbreviation = txt_tickerValue.Text;

            //do calculations
            CalculateBuyPrices();
            CalculateSellPrices();
            CalculatePriceOfBTC();
            CalculateProfitPerCoin();
            CalculateROI();

            //api call(s) entry point to get ticker info
            GetTicker(tickerAbbreviation);


            }

        private void txt_tickerValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            //grab value from text box. 
            string tickerAbbreviation = txt_tickerValue.Text;
            var tickerNameLength = tickerAbbreviation.Length;

            if (tickerNameLength > 1)
            {
                GetTicker(tickerAbbreviation);
            }
        }
    }
}
