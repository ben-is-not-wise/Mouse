using CsvHelper;
using CsvHelper.Configuration;
using HackedDesign.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace HackedDesign
{
    public interface IDialogManager
    {
        List<DialogLine> CurrentDialog { get; set; }

        Sprite GetSpeakerSprite(DialogLine page);
        Sprite GetSpeakerSprite(string name, string emotion);
        void HideDialog();
        void ShowDialog(string name);
        void ShowDialog(string name, UnityAction dialogOverAction);
    }

    public class DialogManager: AutoSingleton<DialogManager>, IDialogManager
    {
        [SerializeField] private DialogPresenter presenter;
        [SerializeField] private PausePresenter messagePresenter;
        [SerializeField] private List<Speaker> speakers;
        private Dictionary<string, List<DialogLine>> dialog = new();

        public List<DialogLine> CurrentDialog { get; set; }
        public List<DialogLine> CurrentMessages { get; set; }
        public List<Speaker> Speakers { get => speakers; set => speakers = value; }

        private UnityAction dialogOverCallback;

        new void Awake()
        {
            base.Awake();
            LoadDialog();
        }

        void Start() => presenter.finishedEvent.AddListener(new UnityAction(DialogFinished));

        public Sprite GetSpeakerSprite(DialogLine page) => GetSpeakerSprite(page.Speaker, page.Emotion);

        public Sprite GetSpeakerSprite(string name, string emotion) => speakers.FirstOrDefault(x => x.name == name).GetEmotion(emotion);

        public void SetMessages(string name)
        { 
            Debug.Log("Set Messages", this);
            SetMessagesByName(name);
        }

        public void ShowDialog(string name) => ShowDialog(name, null);

        public void ShowDialog(string name, UnityAction dialogOverAction)
        {

            Debug.Log("Show Dialog", this);
            this.dialogOverCallback = dialogOverAction;
            SetDialogByName(name);
            presenter.Show();
            presenter.Repaint();
        }

        public void HideDialog() => presenter.Hide();

        #region Load
        private void LoadDialog()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "dialog.en.csv");
            StartCoroutine(LoadDialogCSV(path));
        }

        private IEnumerator LoadDialogCSV(string path)
        {
            var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string csvText = request.downloadHandler.text;
                ParseCSV(csvText);
            }
            else
            {
                Debug.LogError("Failed to load dialog CSV: " + request.error);
            }
        }

        private void ParseCSV(string csvText)
        {
            using var reader = new StringReader(csvText);
            using var csv = new CsvReader(reader,new CsvConfiguration(CultureInfo.InvariantCulture) { PrepareHeaderForMatch = args => args.Header.ToLower()});
            var records = csv.GetRecords<DialogLine>();

            foreach (DialogLine record in records)
            {
                record.Text = ExpandColorMarkup(record.Text);

                if(!dialog.ContainsKey(record.Sequence))
                {
                    dialog.Add(record.Sequence, new List<DialogLine>());
                }

                dialog[record.Sequence].Add(record);
            }
        }

        private static string ExpandColorMarkup(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var sb = new StringBuilder();
            bool open = false;
            foreach (char c in text)
            {
                if (c == '*')
                {
                    sb.Append(open ? "</color>" : "<color=\"#999999\">");
                    open = !open;
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        #endregion Load

        private void GetDialogTitlesByName(string name) => dialog.Select(d => d.Key == name);

        private void SetMessagesByName(string name) => CurrentMessages = dialog[name];

        private void SetDialogByName(string name) => CurrentDialog = dialog[name];

        private void DialogFinished()
        {
            presenter.Hide();
            this.dialogOverCallback?.Invoke();
        }
    }
}
