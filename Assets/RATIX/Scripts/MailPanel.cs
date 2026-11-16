using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Mail popup: email rows with a View button that opens the detail window.
    /// The text content is authored in the scene; the panel just swaps it.
    /// </summary>
    public class MailPanel : MonoBehaviour
    {
        [SerializeField] private GameObject detailsWindow;
        [SerializeField] private Vector2 detailsHomePosition = new Vector2(0f, -5f);
        [SerializeField] private TMP_Text detailsTitle;
        [SerializeField] private TMP_Text detailsBody;
        [SerializeField] private string[] subjects;
        [SerializeField] private string[] senders;
        [SerializeField] private string[] bodies;

        private int selected;
        private bool _wired;

        public void Init(GameObject details, TMP_Text title, TMP_Text body,
            string[] mailSubjects, string[] mailSenders, string[] mailBodies)
        {
            detailsWindow = details;
            detailsTitle = title;
            detailsBody = body;
            subjects = mailSubjects;
            senders = mailSenders;
            bodies = mailBodies;
        }

        private void Awake()
        {
            Wire();
        }

        // Every time the mail opens it starts on the list, not on a detail.
        private void OnEnable()
        {
            if (detailsWindow != null) detailsWindow.SetActive(false);
        }

        private void Wire()
        {
            if (_wired) return;
            _wired = true;

            var x = transform.Find("Window/XButton");
            if (x != null)
            {
                var b = x.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(CloseMail);
            }

            if (detailsWindow != null)
            {
                var close = detailsWindow.transform.Find("Window/ActionBar/Close");
                if (close != null)
                {
                    var b = close.GetComponent<Button>();
                    if (b != null) b.onClick.AddListener(CloseDetails);
                }

                var open = detailsWindow.transform.Find("Window/ActionBar/Open");
                if (open != null)
                {
                    var b = open.GetComponent<Button>();
                    if (b != null) b.onClick.AddListener(Claim);
                }
            }

            var clear = transform.Find("Window/ActionBar/Clear all");
            if (clear != null)
            {
                var b = clear.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(ClearAll);
            }

            var openAll = transform.Find("Window/ActionBar/Open All");
            if (openAll != null)
            {
                var b = openAll.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(OpenAll);
            }
        }

        public void Select(int index)
        {
            if (subjects == null || index < 0 || index >= subjects.Length) return;
            selected = index;
            if (detailsWindow == null) return;

            // The details window is authored apart from the mail list so both can
            // be edited in the scene; it snaps over the list when opened.
            if (detailsWindow.transform is RectTransform detailsRect)
                detailsRect.anchoredPosition = detailsHomePosition;

            detailsWindow.SetActive(true);
            detailsWindow.transform.SetAsLastSibling();
            if (detailsTitle != null) detailsTitle.text = subjects[index];
            if (detailsBody != null) detailsBody.text = bodies != null && index < bodies.Length ? bodies[index] : string.Empty;
        }

        public void CloseDetails()
        {
            if (detailsWindow != null) detailsWindow.SetActive(false);
        }

        public void Claim()
        {
            Debug.Log($"[RATIX] Mail claimed: {(subjects != null && selected < subjects.Length ? subjects[selected] : "?")}");
            CloseDetails();
        }

        public void ClearAll() => Debug.Log("[RATIX] Clear all mail");
        public void OpenAll() => Debug.Log("[RATIX] Open all mail");
        public void CloseMail() => gameObject.SetActive(false);
    }
}
