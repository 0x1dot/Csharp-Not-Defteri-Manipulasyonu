using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CW_Manipüle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        User32.WinEventDelegate procDelegate;
        IntPtr hhook;

        const uint EVENT_OBJECT_INVOKED = 0x8013;
        const uint WINEVENT_OUTOFCONTEXT = 0;
        private void BtnBaslat_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\notepad.exe";
                Process process = Process.Start(path);
            trying:
                IntPtr handle = process.MainWindowHandle;
                if (handle == IntPtr.Zero) goto trying;

                IntPtr editpoint = User32.FindWindowEx(handle, IntPtr.Zero, "Edit", IntPtr.Zero);

                var hMainMenu = User32.CreateMenu();
                var hPopupMenu = User32.CreateMenu();

                StringBuilder sb = new StringBuilder();
                User32.GetWindowText(handle, sb, 200);
                var windowTitle = sb.ToString();

                var sysMenu = User32.GetMenu(handle);
                if (User32.GetMenuItemCount(sysMenu) != 6)
                {trying2:
                    bool check = User32.InsertMenu(sysMenu, 4, User32.MenuFlags.MF_BYPOSITION | User32.MenuFlags.MF_POPUP, (int)hMainMenu, "Ekstra Menü");
                    if (!check) goto trying2;

                    User32.InsertMenu(hMainMenu, 0, User32.MenuFlags.MF_STRING | User32.MenuFlags.MF_POPUP, (int)hPopupMenu, "C# Derleyici(Konsol)");
                    User32.InsertMenu(hPopupMenu, 0, User32.MenuFlags.MF_STRING, (int)102, "Örnek Konsol Uygulaması");
                    User32.InsertMenu(hPopupMenu, 0, User32.MenuFlags.MF_STRING, (int)103, "Örnek Konsol Uygulaması(Hesap Makinesi)");
                    User32.InsertMenu(hPopupMenu, 0, User32.MenuFlags.MF_SEPARATOR, (int)104, "");
                    User32.InsertMenu(hPopupMenu, 0, User32.MenuFlags.MF_STRING, (int)105, "Derle");
                    User32.InsertMenu(hMainMenu, 0, User32.MenuFlags.MF_STRING, (int)106, "Html Önizle");
                    User32.InsertMenu(hMainMenu, 0, User32.MenuFlags.MF_SEPARATOR, (int)107, "");
                    User32.InsertMenu(hMainMenu, 0, User32.MenuFlags.MF_STRING, (int)108, "Hakkında");

                    User32.SetMenu(handle, sysMenu);
                    User32.DrawMenuBar(handle);
                }
            this.FormClosing += new FormClosingEventHandler(delegate
            {
                try
                {
                    User32.UnhookWinEvent(hhook);
                    User32.RemoveMenu(sysMenu, 4, (uint)0x00000400L);
                    User32.DrawMenuBar(sysMenu);
                }
                catch
                {
                }
             
            });

            procDelegate = new User32.WinEventDelegate(delegate (IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
            {
                switch (idChild)
                {
                    case 102:
                    case 103:
                        OrnekUygulama(editpoint,handle,idChild);
                        break;
                    case 105:
                        DebugKonsol(editpoint);
                        break;
                    case 106://Html önizle
                        HtmlPreview(editpoint);//Not defterimizin texteditörün bellek adresini gönderiyoruz
                        break;
                    case 108:
                            User32.MessageBoxEx(handle, "Test Projesi 0x1dot tarafından hazırlanmıştır.", "Cyber-Warrior.Org", (uint)User32.MessageUType.MB_OK | (uint)User32.MessageIconType.MB_ICONINFORMATION, 0);
                        break;
                }
            });
            hhook = User32.SetWinEventHook(EVENT_OBJECT_INVOKED, EVENT_OBJECT_INVOKED, IntPtr.Zero,
                    procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Hata",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        private void OrnekUygulama(IntPtr editpoint,IntPtr handle,int idChild)
        {
            if (string.IsNullOrEmpty(User32.GetWindowText(editpoint)))
            {
                switch (idChild)
                {
                    case 102:
                        User32.SendMessage(editpoint, 0x000C, IntPtr.Zero, konsol);
                        break;
                    case 103:
                        User32.SendMessage(editpoint, 0x000C, IntPtr.Zero, konsol2);
                        break;
                }
             
            }
            else User32.MessageBoxEx(handle, "Metin dosyasını lütfen temizleyiniz!", "Cyber-Warrior.Org", (uint)User32.MessageUType.MB_OK | (uint)User32.MessageIconType.MB_ICONINFORMATION, 0);
        }
        public void DebugKonsol(IntPtr editpoint)
        {
            CSharpCodeProvider csc = new CSharpCodeProvider(new Dictionary<string, string>
            {
                {
                    "CompilerVersion",
                    "v4.0"
                }
            });
            CompilerParameters parametres = new CompilerParameters(new string[]
            {
        "System.Core.dll",
        "mscorlib.dll",
        "System.Core.dll",
        "System.dll",
        "System.Windows.Forms.dll",
        "System.Drawing.dll",
        "System.Data.dll",
        "System.Xml.dll"
            }, "KonsolUygulaması.exe", true);

            parametres.GenerateExecutable = true;
            CompilerResults results = null;
        trying:
            var code = User32.GetWindowText(editpoint);
            if (string.IsNullOrEmpty(code)) goto trying;
            results = csc.CompileAssemblyFromSource(parametres, new string[]
            {
            code
            });
            bool hasErrors = results.Errors.HasErrors;
            if (hasErrors)
            { string err="";
                results.Errors.Cast<CompilerError>().ToList<CompilerError>().ForEach(delegate (CompilerError error)
                {
                    err = err + " Satır: " + error.Line +" Hata: "+ error.ErrorText+"\n";
                });
                User32.MessageBoxEx(editpoint, err, "Hata", (uint)User32.MessageUType.MB_OK | (uint)User32.MessageIconType.MB_ICONEXCLAMATION, 0);
                err = "";
            }
            else
            {
                Process.Start(Application.StartupPath + "/KonsolUygulaması.exe");
            }
        }
        public void HtmlPreview(IntPtr editpoint)
        {
            var markdownText = User32.GetWindowText(editpoint);
            var htmlOutput = htmlBody.Replace("{html_body}", markdownText);

            Form fWeb = new Form();
            fWeb.Size = new System.Drawing.Size(800, 600);
            fWeb.StartPosition = FormStartPosition.CenterScreen;
            fWeb.Text = "Html Önizleme";

            WebBrowser wb = new WebBrowser();
            wb.Dock = DockStyle.Fill;
            wb.ScriptErrorsSuppressed = true;
            wb.DocumentText = htmlOutput;

            RichTextBox rtHtml = new RichTextBox();
            rtHtml.Dock = DockStyle.Fill;
            rtHtml.Text = htmlOutput;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 1;
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp.RowCount = 2;
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlp.Controls.AddRange(new Control[] { wb, rtHtml });
            fWeb.Controls.Add(tlp);

            fWeb.Show();
            User32.SetForegroundWindow(fWeb.Handle);
        }
        #region Markdown / CSS-Code
        string htmlBody = @"
         
          <html>
          <head>
              <style type=""text/css"">;
                  body {
                  font-family: Helvetica, arial, sans-serif;
                  font-size: 14px;
                  line-height: 1.6;
                  padding-top: 10px;
                  padding-bottom: 10px;
                  background-color: white;
                  padding: 30px; }
 
                body > *:first-child {
                  margin-top: 0 !important; }
                body > *:last-child {
                  margin-bottom: 0 !important; }
 
                a {
                  color: #4183C4; }
                a.absent {
                  color: #cc0000; }
                a.anchor {
                  display: block;
                  padding-left: 30px;
                  margin-left: -30px;
                  cursor: pointer;
                  position: absolute;
                  top: 0;
                  left: 0;
                  bottom: 0; }
 
                h1, h2, h3, h4, h5, h6 {
                  margin: 20px 0 10px;
                  padding: 0;
                  font-weight: bold;
                  -webkit-font-smoothing: antialiased;
                  cursor: text;
                  position: relative; }
 
                h1:hover a.anchor, h2:hover a.anchor, h3:hover a.anchor, h4:hover a.anchor, h5:hover a.anchor, h6:hover a.anchor {
                  background: url('../../images/modules/styleguide/para.pn') no-repeat 10px center;
                  text-decoration: none; }
 
                h1 tt, h1 code {
                  font-size: inherit; }
 
                h2 tt, h2 code {
                  font-size: inherit; }
 
                h3 tt, h3 code {
                  font-size: inherit; }
 
                h4 tt, h4 code {
                  font-size: inherit; }
 
                h5 tt, h5 code {
                  font-size: inherit; }
 
                h6 tt, h6 code {
                  font-size: inherit; }
 
                h1 {
                  font-size: 28px;
                  color: black; }
 
                h2 {
                  font-size: 24px;
                  border-bottom: 1px solid #cccccc;
                  color: black; }
 
                h3 {
                  font-size: 18px; }
 
                h4 {
                  font-size: 16px; }
 
                h5 {
                  font-size: 14px; }
 
                h6 {
                  color: #777777;
                  font-size: 14px; }
 
                p, blockquote, ul, ol, dl, li, table, pre {
                  margin: 15px 0; }
 
                hr {
                  background: transparent url('../../images/modules/pulls/dirty-shade.png') repeat-x 0 0;
                  border: 0 none;
                  color: #cccccc;
                  height: 4px;
                  padding: 0; }
 
                body > h2:first-child {
                  margin-top: 0;
                  padding-top: 0; }
                body > h1:first-child {
                  margin-top: 0;
                  padding-top: 0; }
                  body > h1:first-child + h2 {
                    margin-top: 0;
                    padding-top: 0; }
                body > h3:first-child, body > h4:first-child, body > h5:first-child, body > h6:first-child {
                  margin-top: 0;
                  padding-top: 0; }
 
                a:first-child h1, a:first-child h2, a:first-child h3, a:first-child h4, a:first-child h5, a:first-child h6 {
                  margin-top: 0;
                  padding-top: 0; }
 
                h1 p, h2 p, h3 p, h4 p, h5 p, h6 p {
                  margin-top: 0; }
 
                li p.first {
                  display: inline-block; }
 
                ul, ol {
                  padding-left: 30px; }
 
                ul :first-child, ol :first-child {
                  margin-top: 0; }
 
                ul :last-child, ol :last-child {
                  margin-bottom: 0; }
 
                dl {
                  padding: 0; }
                  dl dt {
                    font-size: 14px;
                    font-weight: bold;
                    font-style: italic;
                    padding: 0;
                    margin: 15px 0 5px; }
                    dl dt:first-child {
                      padding: 0; }
                    dl dt > :first-child {
                      margin-top: 0; }
                    dl dt > :last-child {
                      margin-bottom: 0; }
                  dl dd {
                    margin: 0 0 15px;
                    padding: 0 15px; }
                    dl dd > :first-child {
                      margin-top: 0; }
                    dl dd > :last-child {
                      margin-bottom: 0; }
 
                blockquote {
                  border-left: 4px solid #dddddd;
                  padding: 0 15px;
                  color: #777777; }
                  blockquote > :first-child {
                    margin-top: 0; }
                  blockquote > :last-child {
                    margin-bottom: 0; }
 
                table {
                  padding: 0; }
                  table tr {
                    border-top: 1px solid #cccccc;
                    background-color: white;
                    margin: 0;
                    padding: 0; }
                    table tr:nth-child(2n) {
                      background-color: #f8f8f8; }
                    table tr th {
                      font-weight: bold;
                      border: 1px solid #cccccc;
                      text-align: left;
                      margin: 0;
                      padding: 6px 13px; }
                    table tr td {
                      border: 1px solid #cccccc;
                      text-align: left;
                      margin: 0;
                      padding: 6px 13px; }
                    table tr th :first-child, table tr td :first-child {
                      margin-top: 0; }
                    table tr th :last-child, table tr td :last-child {
                      margin-bottom: 0; }
 
                img {
                  max-width: 100%; }
 
                span.frame {
                  display: block;
                  overflow: hidden; }
                  span.frame > span {
                    border: 1px solid #dddddd;
                    display: block;
                    float: left;
                    overflow: hidden;
                    margin: 13px 0 0;
                    padding: 7px;
                    width: auto; }
                  span.frame span img {
                    display: block;
                    float: left; }
                  span.frame span span {
                    clear: both;
                    color: #333333;
                    display: block;
                    padding: 5px 0 0; }
                span.align-center {
                  display: block;
                  overflow: hidden;
                  clear: both; }
                  span.align-center > span {
                    display: block;
                    overflow: hidden;
                    margin: 13px auto 0;
                    text-align: center; }
                  span.align-center span img {
                    margin: 0 auto;
                    text-align: center; }
                span.align-right {
                  display: block;
                  overflow: hidden;
                  clear: both; }
                  span.align-right > span {
                    display: block;
                    overflow: hidden;
                    margin: 13px 0 0;
                    text-align: right; }
                  span.align-right span img {
                    margin: 0;
                    text-align: right; }
                span.float-left {
                  display: block;
                  margin-right: 13px;
                  overflow: hidden;
                  float: left; }
                  span.float-left span {
                    margin: 13px 0 0; }
                span.float-right {
                  display: block;
                  margin-left: 13px;
                  overflow: hidden;
                  float: right; }
                  span.float-right > span {
                    display: block;
                    overflow: hidden;
                    margin: 13px auto 0;
                    text-align: right; }
 
                code, tt {
                  margin: 0 2px;
                  padding: 0 5px;
                  white-space: nowrap;
                  border: 1px solid #eaeaea;
                  background-color: #f8f8f8;
                  border-radius: 3px; }
 
                pre code {
                  margin: 0;
                  padding: 0;
                  white-space: pre;
                  border: none;
                  background: transparent; }
 
                .highlight pre {
                  background-color: #f8f8f8;
                  border: 1px solid #cccccc;
                  font-size: 13px;
                  line-height: 19px;
                  overflow: auto;
                  padding: 6px 10px;
                  border-radius: 3px; }
 
                pre {
                  background-color: #f8f8f8;
                  border: 1px solid #cccccc;
                  font-size: 13px;
                  line-height: 19px;
                  overflow: auto;
                  padding: 6px 10px;
                  border-radius: 3px; }
                  pre code, pre tt {
                    background-color: transparent;
                    border: none; }
            </style>        
        <title>$_</title>
        <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
        </head>
        <body>
        {html_body}
        </body>
	</html>
            ";
        #endregion
        #region Konsol Uygulaması
        string konsol = @"using System;
namespace TestKonsolUygulaması
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write(""Hoşgeldiniz, isminizi öğrenebilir miyim?"");
            var isim = Console.ReadLine();
            Console.WriteLine(""Tekrardan hoşgeldin,""+{isim});
            Console.ReadKey();
        }
    }
}
";
        #endregion
        #region Konsol Uygulaması2
        string konsol2 = @"using System;
namespace TestKonsolUygulaması
{
    class Program
    {
        static void Main(string[] args)
        {
        string sayi1, sayi2,islem;
        int s1, s2;
        double sonuc=0;

        Console.WriteLine(""İlk Sayısı Giriniz !"");
        sayi1 = Console.ReadLine();
        s1 = Int32.Parse(sayi1);
        Console.WriteLine(""İkinci Sayısı Giriniz !"");
        sayi2 = Console.ReadLine();
        s2 = Int32.Parse(sayi2);
        Console.WriteLine(""İşlemi Giriniz [Toplama: + , Çıkarma: - , Çarpma: * , Bölme: /]"");
        islem = Console.ReadLine();

        switch (islem)
        {
            case ""+"": sonuc = s1 + s2; break;
            case ""-"": sonuc = s1 - s2; break;
            case ""/"": sonuc = (double) s1 / (double) s2; break;
            case ""*"": sonuc = s1* s2; break;
        }
        Console.WriteLine(""İşlem Sonucu : ""+sonuc.ToString());
        Console.WriteLine(""Çıkmak İçin Bir Tuşa Basın !"");
        Console.ReadKey();
        }
    }
}
";
        #endregion
        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.cyber-warrior.org/Forum/pop_up_profile.asp?profile=228097");
        }
    }
}
