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
using PumpAndDumpHelperClient.Properties;
using HtmlAgilityPack;

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
            GetBalanceOfACoin("BTC");
            UpdateUI(TimeSinceLastUpdate);
            


        }

        public async Task UpdateUI(double secondsSinceLastUpdate)
        {
            //grab value from text box. 
            string tickerAbbreviation = txt_tickerValue.Text;

            //todo: eventually add a time delayed auto refreshing call to GetTicker()
            
            //api call(s) entry point to get market summary
            GetMarketSummary(tickerAbbreviation);
            
            
            //do calculations
            CalculateBuyPrices();
            var profitGoal = Convert.ToInt32(txt_ProfitGoal.Text);
            CalculateSellPrices(profitGoal);
            CalculatePriceOfBTC();
            CalculateProfitPerCoin();
            CalculateROI();

            

            //update "Last update: __ secs ago" label controls
            UpdateLastPriceUpdateTimer(secondsSinceLastUpdate);
        }

        public async Task UpdateLastPriceUpdateTimer(double secondsSinceLastUpdate)
            {
            string tickerAbbreviation = txt_tickerValue.Text;
            //really hacky way of updating once per 3seconds...dat infinite recursion doe.
            
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
                GetMarketSummary(tickerAbbreviation);
                UpdateLastPriceUpdateTimer(secondsSinceLastUpdate);
                }

            }


        public async Task GetTicker(string coinToCheck)
        {
            var exc = new Exchange();
            var context = new ExchangeContext();

            context.ApiKey = Settings.Default["APIKey"].ToString();
            context.Secret = Settings.Default["SecretKey"].ToString();
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
                lbl_CurrentTickerBuyPriceAmountInBTC.Content = updatedPrice_Bid;
                lbl_CurrentTickerSalePriceAmountInBTC.Content = updatedPrice_Ask;
                lbl_SellPriceInBTCAmount.Content = updatedPrice_Ask;
            }
        }

        public async Task GetMarketSummary(string coinToCheck)
        {

            var exc = new Exchange();
            var context = new ExchangeContext();

            context.ApiKey = Settings.Default["APIKey"].ToString();
            context.Secret = Settings.Default["SecretKey"].ToString();
            context.Simulate = true;
            context.QuoteCurrency = "BTC";
            exc.Initialise(context);

            if (!string.IsNullOrEmpty(coinToCheck))
            //only update the prices if there's more than one char typed in.
            //hopefully this prevents the API from banning our IP address.
            {
                var coinTickerInfo = exc.GetMarketSummary(coinToCheck);

                var updatedPrice_Last = coinTickerInfo.Last;
                var updatedPrice_Bid = coinTickerInfo.Bid;
                var updatedPrice_Ask = coinTickerInfo.Ask;

                lbl_CurrentBuyPriceAmountInBTC.Content = updatedPrice_Bid;
                lbl_CurrentTickerBuyPriceAmountInBTC.Content = updatedPrice_Bid;
                lbl_CurrentTickerSalePriceAmountInBTC.Content = updatedPrice_Ask;
                lbl_SellPriceInBTCAmount.Content = updatedPrice_Ask;
            }
        }

        public async Task CalculateBuyPrices()
            {

            // 1. Find out how much BTC you have available in your account.
            // 2. find the current buy price (in btc/satoshis)
            // 3. divide step 1 by step 2. 
            // 4. Take the answer and convert to $USD and update some local variable
            // 5. update the UI

            //GetBalanceOfBTC()
            //GetMarketSummary(bid)
            var currentBuyPriceOfCoin = lbl_CurrentBuyPriceAmountInBTC.Content;
            var buyPriceInBTC = lbl_BuyPriceInBTCAmount.Content;
            //var priceInUSD = lbl_BuyPriceInUSDAmount.Content;

            decimal coinsPurchasable = decimal.Parse(lbl_BTCBalanceAmount.Content.ToString()) / decimal.Parse(lbl_CurrentBuyPriceAmountInBTC.Content.ToString());

            //double coinsPurchasable = (double)lbl_BTCBalanceAmount.Content * (double)lbl_CurrentBuyPriceAmountInBTC.Content;
            decimal BTCTotalPriceOfBuyOrder = decimal.Parse(currentBuyPriceOfCoin.ToString()) * decimal.Parse(coinsPurchasable.ToString());
            //double BTCTotalPriceOfBuyOrder = (double)currentBuyPriceOfCoin * coinsPurchasable;

            lbl_CoinsPurchasableAmount.Content = coinsPurchasable.ToString();
            lbl_BuyPriceInBTCAmount.Content = BTCTotalPriceOfBuyOrder.ToString();

            var priceInUSD = decimal.Parse(lbl_BTCCurrentValueAmount.Content.ToString()) * decimal.Parse(BTCTotalPriceOfBuyOrder.ToString());
            lbl_BuyPriceInUSDAmount.Content = priceInUSD;
        }

        public async Task CalculateSellPrices(int incomingProfitGoal)
            {

            // 1. Find out how much of the selected coin you have available in your account.
            // 2. find the current sale price (in btc/satoshis)
            // 3. divide step 1 by step 2. 
            // 4. Take the answer and convert to $USD and update some local variable
            // 5. update the UI

            var profitGoal = txt_ProfitGoal.Text;
            var projectedSalePriceperCoin = double.Parse(lbl_CurrentBuyPriceAmountInBTC.Content.ToString()) * double.Parse(profitGoal) ;
            var projectedTotalSalePriceInBTC = double.Parse(lbl_BuyPriceInBTCAmount.Content.ToString()) * double.Parse(profitGoal);
            var projectedTotalSalePriceInUSD = double.Parse(lbl_BuyPriceInUSDAmount.Content.ToString()) * double.Parse(profitGoal);

            lbl_SalePricePerCoinIinBTCAmount.Content = projectedSalePriceperCoin;
            lbl_SellPriceInBTCAmount.Content = projectedTotalSalePriceInBTC;
            lbl_SellPriceInUSDAmount.Content = projectedTotalSalePriceInUSD;
            var projectedTotalProfit = decimal.Subtract(decimal.Parse(lbl_SellPriceInUSDAmount.Content.ToString()), decimal.Parse(lbl_BuyPriceInUSDAmount.Content.ToString()));
            lbl_TotalProfitAmount.Content = projectedTotalProfit;


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
            var profitGoal = Convert.ToInt32(txt_ProfitGoal.Text);
            CalculateSellPrices(profitGoal);
            CalculatePriceOfBTC();
            CalculateProfitPerCoin();
            CalculateROI();

            //api call(s) entry point to get ticker info
            GetTicker(tickerAbbreviation);


            }

        public async Task GetBalanceOfACoin(string coinToCheck)
            {

            var exc = new Exchange();
            var context = new ExchangeContext();

            context.ApiKey = Settings.Default["APIKey"].ToString();
            context.Secret = Settings.Default["SecretKey"].ToString();
            context.Simulate = true;
            context.QuoteCurrency = "BTC";
            exc.Initialise(context);

            if (!string.IsNullOrEmpty(coinToCheck))        
            {
                var CoinBalanceCall = exc.GetBalance(coinToCheck);
                var CoinCurrentBalance = CoinBalanceCall.Balance;

                if (coinToCheck.ToUpper() == "BTC" ) {
                    lbl_BTCBalanceAmount.Content = CoinCurrentBalance.ToString() ;
                }else {
                    lbl_CoinsOwnedAmount.Content = CoinCurrentBalance.ToString();
                }

            }

        }

        public async Task BuyCoinTest(string coinToBuy)
            {

            var exc = new Exchange();
            var context = new ExchangeContext();

            context.ApiKey = Settings.Default["APIKey"].ToString();
            context.Secret = Settings.Default["SecretKey"].ToString();
            context.Simulate = false;
            context.QuoteCurrency = "BTC";
            exc.Initialise(context);

            GetMarketSummary(coinToBuy);

            var buyingprice = decimal.Parse(lbl_CurrentTickerSalePriceAmountInBTC.Content.ToString());

            var minimumQuantity = exc.CalculateMinimumOrderQuantity(coinToBuy, buyingprice);

            var askprice = decimal.Parse(lbl_CurrentTickerBuyPriceAmountInBTC.Content.ToString());

            if (!string.IsNullOrEmpty(coinToBuy))        
            {


                //exc.PlaceBuyOrder("ANS", decimal.Parse("0.2"), buyingprice);
                exc.PlaceBuyOrder(coinToBuy, minimumQuantity, buyingprice);

                GetMarketSummary(coinToBuy);

                askprice = decimal.Parse(lbl_CurrentTickerBuyPriceAmountInBTC.Content.ToString());
                var sellingprice = askprice * 2;

                //await Task.Delay(1000);
                SellCoinTest(coinToBuy, minimumQuantity, sellingprice);


            }

        }

        public async Task SellCoinTest(string coinToSell, decimal amountToSell, decimal priceToSellAt)
        {
            var exc = new Exchange();
            var context = new ExchangeContext();

            context.ApiKey = Settings.Default["APIKey"].ToString();
            context.Secret = Settings.Default["SecretKey"].ToString();
            context.Simulate = false;
            context.QuoteCurrency = "BTC";
            exc.Initialise(context);

            //var readyToSale = isOrderCompleted();


                //await Task.Delay(1000);
                exc.PlaceSellOrder(coinToSell, amountToSell, priceToSellAt);

            
        }

        private bool isOrderCompleted()
        {
            return true;
        }

        private void txt_tickerValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            //grab value from text box. 
            string tickerAbbreviation = txt_tickerValue.Text;
            var tickerNameLength = tickerAbbreviation.Length;
            var currentBTCBalance = lbl_BTCBalanceAmount.Content;
            var totalBTCValueofTheSelectedCoin = lbl_BTCBalanceAmount.Content;
            var totalUSDValueofTheSelectedCoin = lbl_PositionValueInUSDAmount.Content;
            var priceofCoinInBTC = lbl_BuyPriceInBTCAmount.Content;
            var profitGoal = Convert.ToInt32(txt_ProfitGoal.Text);
            

            if (tickerNameLength > 1 && tickerNameLength < 5)
            {
                GetMarketSummary(tickerAbbreviation);
                GetBalanceOfACoin(tickerAbbreviation);

                CalculateBuyPrices();
                CalculateSellPrices(profitGoal);

                var portfolioValueInBTC = decimal.Parse(lbl_CoinsOwnedAmount.Content.ToString()) * decimal.Parse(lbl_CurrentTickerSalePriceAmountInBTC.Content.ToString());
                var portfolioValueInUSD = decimal.Parse(lbl_BTCCurrentValueAmount.Content.ToString()) * portfolioValueInBTC;
                lbl_PositionValueInBTCAmount.Content = portfolioValueInBTC;
                lbl_PositionValueInUSDAmount.Content = portfolioValueInUSD;
            }
        }

        private void btn_ManuallyUpdateBTCBalance_Click(object sender, RoutedEventArgs e)
        {
            GetBalanceOfACoin("BTC");
        }

        private void txt_ProfitGoal_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt_ProfitGoal.Text.Length > 0)
            {
                var profitGoal = Convert.ToInt32(txt_ProfitGoal.Text);
                CalculateSellPrices(profitGoal);
            }
        }

        private void txt_ProfitGoal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void btn_JoinPump_Click(object sender, RoutedEventArgs e)
        {
            //todo: the brains of the operation.  join the pump
            // calculate prices, shoot off api call to place order
            //start loop to try and place sell order once buy order has been filled.
            //
            string tickerAbbreviation = txt_tickerValue.Text;

            //BuyCoinTest("ANS");
            if (!String.IsNullOrEmpty(tickerAbbreviation))
            {
                BuyCoinTest(tickerAbbreviation);
            }
                


        }
    }
}
