
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpeechWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StartRecording();
        }

        private static SpeechRecognitionMode m_SpeechMode = SpeechRecognitionMode.ShortPhrase;
        private static string m_InputLanguage = "en-us";
        private static string m_SpeechToTextKey = "9e419d59f36045c4b4b44efe2e8d5014";

        public MicrophoneRecognitionClient m_MicrophoneRecording = SpeechRecognitionServiceFactory.CreateMicrophoneClient(m_SpeechMode, m_InputLanguage, m_SpeechToTextKey);
        public RecordedText m_RecordedText { get; set; } = new RecordedText();

        private void StartRecording()
        {
            m_MicrophoneRecording.OnResponseReceived += OnResponseReceived;
            m_MicrophoneRecording.OnPartialResponseReceived += OnPartialResponseReceived;

            m_MicrophoneRecording.AudioStart();
            m_MicrophoneRecording.StartMicAndRecognition();
        }

        private void StopRecording()
        {
            m_MicrophoneRecording.EndMicAndRecognition();
            m_MicrophoneRecording.AudioStop();
        }

        private void OnPartialResponseReceived(object sender, PartialSpeechResponseEventArgs e)
        {
            m_RecordedText.Text = e.PartialResult;
        }

        private void OnResponseReceived(object sender, SpeechResponseEventArgs e)
        {
            m_RecordedText.Text = e.PhraseResponse.Results.FirstOrDefault()?.DisplayText ?? "Not recognized!";
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            StopRecording();
        }

        private void m_AnalizeButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
            AnalizeFeelings();
        }

        private static ITextAnalyticsAPI client = new TextAnalyticsAPI();
        private static string m_SentimentKey = "a476d2169aa948b287902f3f265a3f74";

        private void AnalizeFeelings()
        {
            string text = m_InputText.Text;
            m_FeelingsText.Text = "Phrase: " +  text + '\n';

            client.AzureRegion = AzureRegions.Westcentralus;
            client.SubscriptionKey = m_SentimentKey;

            SentimentBatchResult m_SentimentList = client.Sentiment(
                   new MultiLanguageBatchInput(
                       new List<MultiLanguageInput>()
                       {
                           // The text is always in english
                          new MultiLanguageInput("en", "0", text)
                       }));

            foreach (var document in m_SentimentList.Documents)
            {
                m_FeelingsText.Text += "Sentiment Score: " + document.Score + '\n';
                double score = (double)document.Score;
                if (score < 0.2)
                    m_FeelingsText.Text += "Sentiment: Bad";
                else if (score < 0.8)
                    m_FeelingsText.Text += "Sentiment: Neutral";
                else
                    m_FeelingsText.Text += "Sentiment: Good";
            }
        }
    }
}
