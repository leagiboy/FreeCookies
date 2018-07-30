﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreeCookies
{
    public delegate void ListViewChangeEventHandler(object sender, ChangeCookieItemEventArgs e);
    public partial class CookiesControl : UserControl
    {
        public CookiesControl()
        {
            InitializeComponent();
        }

        CookiesInjectInfo cookiesInjectInfo = null;
        List<KeyValuePair<string, string>> myCookiesList = null;
        Timer myTimer = new Timer();
        int flushCookieTextTick = -1;
        const int flushFormatTime = 3;
        const int flushColorTime = 20;
        List<KeyValuePair<ListViewItem,int>> editedItemList=null;
        Graphics graphicsForRtbCookies=null;
        Pen warnPen = null;

        public delegate void cookiesControlButtonEventHandler(object sender);
        public event cookiesControlButtonEventHandler OnGetCookies;

        private void CookiesControl_Load(object sender, EventArgs e)
        {
            cookiesInjectInfo = new CookiesInjectInfo();
            cookiesInjectInfo.ContainUrl = tb_urlFilter.Text;
            cookiesInjectInfo.IsInject = ck_isInjeckCookies.Checked;
            myCookiesList=new List<KeyValuePair<string,string>>();
            editedItemList = new List<KeyValuePair<ListViewItem, int>>();
            editCookieControl.SetListView(lv_cookie);
            editCookieControl.OnListViewChange += editCookieControl_OnListViewChange;

            graphicsForRtbCookies = rtb_cookies.CreateGraphics();
            warnPen = new Pen(Color.Yellow, 2f);

            myTimer.Interval = 200;
            myTimer.Tick += myTimer_Tick;
            myTimer.Start();

        }

        
        public CookiesInjectInfo InjectInfo
        {
            get { return cookiesInjectInfo; }
        }

        public string InjectCookies
        {
            get { return rtb_cookies.Text; }
        }

        public List<KeyValuePair<string, string>> CookiesList
        {
            get { return myCookiesList; }
        }

        public string GetControlCookies()
        {
            return rtb_cookies.Text;
        }

        public void SetControlCookies(string yourCookies)
        {
            if (yourCookies=="")
            {
                PaintWarnInfo();
            }
            rtb_cookies.Text = yourCookies;
            ReFlushCookiesView(true);
        }

        public void AddMessageInfo(string sender , string yourMessage)
        {
            switch (sender)
            {
                case null:
                    if (yourMessage != null)
                    {
                        rtb_info.AppendText("\r\n");
                        rtb_info.AppendText(yourMessage);
                    }
                    break;
                case "OnGetCookies_Error":
                    if (yourMessage != null)
                    {
                        PutError(yourMessage);
                    }
                    break;
                case "OnGetCookies":
                    if (yourMessage != null)
                    {
                        PutWarn(yourMessage);
                    }
                    break;
                default:
                    rtb_info.AppendText(yourMessage);
                    break;
            }
           
        }

        public void FiddlerFreeCookiesSetCookieded(string yourUri,string yourHeads)
        {
            PutWarn(string.Format( "Set Cookie with 【{0}】",yourUri));
            PutInfo(yourHeads);
            if(!cb_injectAlways.Checked)
            {
                ck_isInjeckCookies.Checked = false;
                PutWarn("Set InjeckCookies unable");
            }
        }

        #region Inner Event
        void myTimer_Tick(object sender, EventArgs e)
        {
            if (flushCookieTextTick == 0)
            {
                ReFlushCookiesView(true);
                flushCookieTextTick = -1;
            }
            else if (flushCookieTextTick > 0)
            {
                flushCookieTextTick--;
            }
            else
            {
                //nothing to do
            }

            if(editedItemList.Count>0)
            {
                for(int i =editedItemList.Count-1 ;i>=0;i--)
                {
                    if(editedItemList[i].Value>0)
                    {
                        editedItemList[i] = new KeyValuePair<ListViewItem, int>(editedItemList[i].Key, editedItemList[i].Value-1);
                    }
                    else
                    {
                        editedItemList[i].Key.BackColor = SystemColors.Window;
                        editedItemList.RemoveAt(i);
                    }
                }

            }
        }
        private void bt_getCookies_Click(object sender, EventArgs e)
        {
            
            if (OnGetCookies != null)
            {
                this.OnGetCookies(null);
            }
        }


        private void lv_cookie_SelectedIndexChanged(object sender, EventArgs e)
        {
            editCookieControl.ReflushEditItem();
        }

        internal void editCookieControl_OnListViewChange(object sender, ChangeCookieItemEventArgs e)
        {
            ReFlushCookiesView(false);
            ListViewItem tempItem = e.EditItem;
            if(tempItem!=null)
            {
                bool isInList = false;
                for (int i = editedItemList.Count - 1; i >= 0; i--)
                {
                    if (editedItemList[i].Key == tempItem)
                    {
                        editedItemList[i] = new KeyValuePair<ListViewItem, int>(tempItem, flushColorTime);
                        isInList = true;
                        break;
                    }
                }

                if(!isInList)
                {
                    tempItem.BackColor = Color.PowderBlue;
                    editedItemList.Add(new KeyValuePair<ListViewItem, int>(tempItem, flushColorTime));
                }

            }
        }


        private void rtb_cookies_KeyUp(object sender, KeyEventArgs e)
        {
            flushCookieTextTick = flushFormatTime;
        }

        private void tb_urlFilter_TextChanged(object sender, EventArgs e)
        {
            cookiesInjectInfo.ContainUrl = tb_urlFilter.Text;
        }

        private void ck_isInjeckCookies_CheckedChanged(object sender, EventArgs e)
        {
            cookiesInjectInfo.IsInject = ck_isInjeckCookies.Checked;
            groupBox_urlFilter.Enabled = !ck_isInjeckCookies.Checked;
            if(cb_injectAlways.Checked && !ck_isInjeckCookies.Checked)
            {
                cb_injectAlways.Checked = false;
            }
        }

        private void cb_injectAlways_CheckedChanged(object sender, EventArgs e)
        {
            if(cb_injectAlways.Checked && !ck_isInjeckCookies.Checked)
            {
                ck_isInjeckCookies.Checked = true;
            }
        }

        private void lv_cookie_DoubleClick(object sender, EventArgs e)
        {
            EditCookieForm f = new EditCookieForm(lv_cookie.SelectedItems[0]);
            f.OnListViewChange += editCookieControl_OnListViewChange;
            f.ShowDialog();
            //f.ShowDialog(this);
        }


        //pictureBox change for all
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            ((PictureBox)sender).BackColor = Color.Honeydew;
        }

        //pictureBox change for all
        private void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).BackColor = Color.Transparent;
        }

        private void CookiesControl_Resize(object sender, EventArgs e)
        {
            //rtb_info.Width = this.Width - 364;
            //rtb_info.Height = this.Height - 285;
            //splitContainer_info.Location = new Point(0, 262);
            splitContainer_info.Height = this.Height - 280;
            tb_urlFilter.Width = groupBox_urlFilter.Width - 60;
            groupBox_editResponse.Height = splitContainer_info.Height - 80;
        }


        #endregion

        #region Function
        private List<KeyValuePair<string, string>> GetCookieList(string yourCookieString)
        {
            List<KeyValuePair<string, string>> tempCL=null;
            if(yourCookieString!=null)
            {
                string[] tempCS = yourCookieString.Split(';');
                if(tempCS.Length>0)
                {
                    tempCL = new List<KeyValuePair<string, string>>();
                    foreach(string eachCookies in tempCS)
                    {
                        string cookieKey = null;
                        string cookieVaule = null;
                        int splitIndex = eachCookies.IndexOf('=');
                        if(splitIndex<0)
                        {
                            return null;
                        }
                        cookieKey = eachCookies.Remove(splitIndex);
                        cookieKey = cookieKey.Trim();
                        cookieVaule = eachCookies.Substring(splitIndex + 1);
                        tempCL.Add(new KeyValuePair<string, string>(cookieKey, cookieVaule));
                    }
                }
            }
            return tempCL;
        }
        
        private void ReFlushCookiesView(bool isFormRawData)
        {
            if(isFormRawData)
            {
                 var tempCookiesList = GetCookieList(rtb_cookies.Text);
                 if (tempCookiesList != null)
                {
                    myCookiesList = tempCookiesList;
                    rtb_cookies.ForeColor = Color.Black;
                    lv_cookie.SuspendLayout();
                    lv_cookie.Items.Clear();
                    foreach (var kvCookiein in myCookiesList)
                    {
                        lv_cookie.Items.Add(new ListViewItem(new string[] { kvCookiein.Key, kvCookiein.Value ,""}));
                    }
                    lv_cookie.ResumeLayout();
                }
                else
                {
                    myCookiesList.Clear();
                    lv_cookie.Items.Clear();
                    editedItemList.Clear();
                    if (rtb_cookies.Text != "")
                    {
                        rtb_cookies.ForeColor = Color.Red;
                    }
                }
                
                editCookieControl.ReflushEditItem();
            }
            else
            {
                myCookiesList.Clear();
                if(lv_cookie.Items.Count>0)
                {
                    StringBuilder sbCookies = new StringBuilder(lv_cookie.Items.Count * 20);
                    foreach(ListViewItem eachItem in lv_cookie.Items)
                    {
                        sbCookies.Append(string.Format(" {0}={1};",eachItem.SubItems[0].Text,eachItem.SubItems[1].Text));
                        myCookiesList.Add(new KeyValuePair<string, string>(eachItem.SubItems[0].Text, eachItem.SubItems[2].Text == "" ? eachItem.SubItems[1].Text : String.Format("{0}; {1}", eachItem.SubItems[1].Text, eachItem.SubItems[2].Text)));
                    }
                    if (sbCookies[sbCookies.Length-1]==';')
                    {
                        sbCookies.Remove(sbCookies.Length - 1, 1);
                    }
                    rtb_cookies.Text = sbCookies.ToString();
                }
                else
                {
                    rtb_cookies.Text = "";
                }
            }
        }

        private void PaintWarnInfo()
        {
            graphicsForRtbCookies.DrawRectangle(warnPen, new Rectangle(Point.Empty, rtb_cookies.Size));
        }

        private void PutInfo(string info)
        {
            rtb_info.SelectionColor = Color.Black;
            rtb_info.AppendText(info);
            rtb_info.AppendText("\r\n");
            rtb_info.SelectionColor = Color.Black;
        }

        private void PutWarn(string info)
        {
            rtb_info.SelectionColor = Color.Indigo;
            rtb_info.AppendText(info);
            rtb_info.AppendText("\r\n");
            rtb_info.SelectionColor = Color.Black;
        }

        private void PutError(string info)
        {
            rtb_info.SelectionColor = Color.Red;
            rtb_info.AppendText(info);
            rtb_info.AppendText("\r\n");
            rtb_info.SelectionColor = Color.Black;
        }

        #endregion
    }



    public class CookiesInjectInfo
    {
        public bool IsInject { get; set; }
        public string ContainUrl { get; set; }

    }

    public class ChangeCookieItemEventArgs : EventArgs
    {
        public ListViewItem EditItem { get; set; }
        public ChangeCookieItemEventArgs(ListViewItem yourItem)
        {
            EditItem = yourItem;
        }
    }

}