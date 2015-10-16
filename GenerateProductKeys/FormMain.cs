/*
The MIT License(MIT)
Copyright(c) 2015 Freddy Juhel
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using GenerateProductKeys.Properties;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GenerateProductKeys
{
  public partial class FormMain : Form
  {
    public FormMain()
    {
      InitializeComponent();
    }

    readonly Dictionary<string, string> _languageDicoEn = new Dictionary<string, string>();
    readonly Dictionary<string, string> _languageDicoFr = new Dictionary<string, string>();
    private string _currentLanguage = "english";

    private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      AboutBoxApplication aboutBoxApplication = new AboutBoxApplication();
      aboutBoxApplication.ShowDialog();
    }

    private void DisplayTitle()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
      Text += string.Format(" V{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
      DisplayTitle();
      GetWindowValue();
      progressBarGeneration.Visible = false;
      labelCountKeys.Text += " 0";
      DisplayLabelduration();
      LoadLanguages();
      SetLanguage(Settings.Default.LastLanguageUsed);
    }

    private void DisplayLabelduration()
    {
      labelDuration.Text = Translate("Duration") + " : 0";
      labelDuration2.Text = "(hour: minute: second: millisecond)";
    }

    private string Translate(string index) // could add (string index, string _currentLanguage = "english")
    {
      string result = string.Empty;
      switch (_currentLanguage.ToLower())
      {
        case "english":
          result = _languageDicoEn.ContainsKey(index) ? _languageDicoEn[index] :
           "the term: \"" + index + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
        case "french":
          result = _languageDicoFr.ContainsKey(index) ? _languageDicoFr[index] :
            "the term: \"" + index + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
      }

      return result;
    }

    private void GetWindowValue()
    {
      Width = Settings.Default.WindowWidth;
      Height = Settings.Default.WindowHeight;
      Top = Settings.Default.WindowTop < 0 ? 0 : Settings.Default.WindowTop;
      Left = Settings.Default.WindowLeft < 0 ? 0 : Settings.Default.WindowLeft;
    }

    private void SaveWindowValue()
    {
      Settings.Default.WindowHeight = Height;
      Settings.Default.WindowWidth = Width;
      Settings.Default.WindowLeft = Left;
      Settings.Default.WindowTop = Top;
      Settings.Default.LastLanguageUsed = frenchToolStripMenuItem.Checked ? "French" : "English";
      Settings.Default.Save();
    }

    private void FormMainFormClosing(object sender, FormClosingEventArgs e)
    {
      SaveWindowValue();
    }

    private void buttonGenerateKeys_Click(object sender, EventArgs e)
    {
      // Format key generated AZAZ1-AZAZ9-AZAZA-AZAZ9-12346
      listBoxKeys.Items.Clear();
      labelCountKeys.Text = "Count: " + listBoxKeys.Items.Count + " items";
      DisplayLabelduration();
      Application.DoEvents();
      progressBarGeneration.Visible = true;
      int numberOfkeys;
      try
      {
        Int32.TryParse(textBoxNumberOfKeys.Text, out numberOfkeys);
        if (numberOfkeys == 0)
        {
          string message = string.Format("The value is a number greater than {0:n0}", Int32.MaxValue);
          DisplayMessageBox(message , "Value too big", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return; 
        }
      }
      catch (Exception)
      {                
      }

      if (!Int32.TryParse(textBoxNumberOfKeys.Text, out numberOfkeys))
      {
        MessageBox.Show("The number of keys to be generated must be a number");
        return;
      }

      if (numberOfkeys < 0)
      {
        DisplayMessageBox("The number of keys to be generated must greater than 0\n\nPlease type in a positive integer.",
          "Number of keys greater than zero", MessageBoxButtons.OK, MessageBoxIcon.Error);
        textBoxNumberOfKeys.Text = string.Empty;
        return;
      }

      progressBarGeneration.Minimum = 1;
      progressBarGeneration.Maximum = numberOfkeys;
      progressBarGeneration.Value = progressBarGeneration.Minimum;
      Stopwatch chrono = new Stopwatch();
      chrono.Start();
      for (int i = 1; i <= numberOfkeys; i++)
      {
        listBoxKeys.Items.Add(GenerateNewKey());
        progressBarGeneration.Value = i;
      }

      chrono.Stop();
      labelCountKeys.Text = "Count: " + listBoxKeys.Items.Count + " items";
      labelDuration.Text = "Duration: " + chrono.Elapsed;
      progressBarGeneration.Value = 1;
      progressBarGeneration.Visible = false;
    }

    public static string GenerateNewKey()
    {
      List<string> alphabet = new List<string>();
      List<int> numbers = new List<int>();
      for (char letter = 'A'; letter <= 'Z'; letter++)
      {
        alphabet.Add(letter.ToString());
      }

      for (int number = 1; number <= 9; number++)
      {
        numbers.Add(number);
      }

      List<string> sourceCharacters = new List<string>();
      sourceCharacters = alphabet;
      foreach (var item in numbers)
      {
        sourceCharacters.Add(item.ToString());
      }

      string newString = string.Empty;
      for (int j = 0; j < 5; j++)
      {
        newString += GenerateCharacters(sourceCharacters, 5);
        newString += "-";
      }

      newString = newString.Substring(0, newString.Length - 1);
      return newString;
    }

    public static string GenerateCharacters(List<string> source, int numberOfCharacters = 1)
    {
      if (source == null || source.Count == 0)
      {
        return string.Empty;
      }

      string result = string.Empty;
      for (int i = 0; i < numberOfCharacters; i++)
      {
        result += source[GenerateRandomNumberUsingCrypto(0, source.Count - 1)];
      }

      return result;
    }

    public static int GenerateRandomNumberUsingCrypto(int min, int max)
    {
      if (max >= 255)
      {
        return 0;
      }

      int result;
      RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
      byte[] randomNumber = new byte[1];
      do
      {
        crypto.GetBytes(randomNumber);
        result = randomNumber[0];
      } while (result < min || result > max); // while it is not true meaning outside the wanted range

      return result;
    }

    private void buttonCopyKeyToClipBoard_Click(object sender, EventArgs e)
    {
      if (listBoxKeys.Items.Count == 0)
      {
        DisplayMessageBox("The result list is empty.\n\nYou must run the key generation first.", "List is empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      
      if (listBoxKeys.SelectedIndex == -1)
      {
        DisplayMessageBox("You haven't selected at least one key to copy.\n\nPlease select one key first.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      if (listBoxKeys.SelectedIndex > 0)
      {
        Clipboard.SetText(listBoxKeys.SelectedItem.ToString());
        DisplayMessageBox("The selected key has been copied to the clipboard.", "Copy OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private static void DisplayMessageBox(string message, string title = "", MessageBoxButtons button = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
      MessageBox.Show(message, title, button, icon);
    }

    private void frenchToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetLanguage(Language.French.ToString());
    }

    private void englishToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetLanguage(Language.English.ToString());
    }

    private void SetLanguage(string myLanguage)
    {
      switch (myLanguage)
      {
        case "English":
          frenchToolStripMenuItem.Checked = false;
          englishToolStripMenuItem.Checked = true;
          fileToolStripMenuItem.Text = _languageDicoEn["MenuFile"];
          newToolStripMenuItem.Text = _languageDicoEn["MenuFileNew"];
          openToolStripMenuItem.Text = _languageDicoEn["MenuFileOpen"];
          saveToolStripMenuItem.Text = _languageDicoEn["MenuFileSave"];
          saveasToolStripMenuItem.Text = _languageDicoEn["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = _languageDicoEn["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = _languageDicoEn["MenufilePageSetup"];
          quitToolStripMenuItem.Text = _languageDicoEn["MenufileQuit"];
          editToolStripMenuItem.Text = _languageDicoEn["MenuEdit"];
          cancelToolStripMenuItem.Text = _languageDicoEn["MenuEditCancel"];
          redoToolStripMenuItem.Text = _languageDicoEn["MenuEditRedo"];
          cutToolStripMenuItem.Text = _languageDicoEn["MenuEditCut"];
          copyToolStripMenuItem.Text = _languageDicoEn["MenuEditCopy"];
          pasteToolStripMenuItem.Text = _languageDicoEn["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = _languageDicoEn["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = _languageDicoEn["MenuTools"];
          personalizeToolStripMenuItem.Text = _languageDicoEn["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = _languageDicoEn["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = _languageDicoEn["MenuLanguage"];
          englishToolStripMenuItem.Text = _languageDicoEn["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = _languageDicoEn["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = _languageDicoEn["MenuHelp"];
          summaryToolStripMenuItem.Text = _languageDicoEn["MenuHelpSummary"];
          indexToolStripMenuItem.Text = _languageDicoEn["MenuHelpIndex"];
          searchToolStripMenuItem.Text = _languageDicoEn["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = _languageDicoEn["MenuHelpAbout"];

          break;
        case "French":
          frenchToolStripMenuItem.Checked = true;
          englishToolStripMenuItem.Checked = false;
          fileToolStripMenuItem.Text = _languageDicoFr["MenuFile"];
          newToolStripMenuItem.Text = _languageDicoFr["MenuFileNew"];
          openToolStripMenuItem.Text = _languageDicoFr["MenuFileOpen"];
          saveToolStripMenuItem.Text = _languageDicoFr["MenuFileSave"];
          saveasToolStripMenuItem.Text = _languageDicoFr["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = _languageDicoFr["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = _languageDicoFr["MenufilePageSetup"];
          quitToolStripMenuItem.Text = _languageDicoFr["MenufileQuit"];
          editToolStripMenuItem.Text = _languageDicoFr["MenuEdit"];
          cancelToolStripMenuItem.Text = _languageDicoFr["MenuEditCancel"];
          redoToolStripMenuItem.Text = _languageDicoFr["MenuEditRedo"];
          cutToolStripMenuItem.Text = _languageDicoFr["MenuEditCut"];
          copyToolStripMenuItem.Text = _languageDicoFr["MenuEditCopy"];
          pasteToolStripMenuItem.Text = _languageDicoFr["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = _languageDicoFr["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = _languageDicoFr["MenuTools"];
          personalizeToolStripMenuItem.Text = _languageDicoFr["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = _languageDicoFr["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = _languageDicoFr["MenuLanguage"];
          englishToolStripMenuItem.Text = _languageDicoFr["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = _languageDicoFr["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = _languageDicoFr["MenuHelp"];
          summaryToolStripMenuItem.Text = _languageDicoFr["MenuHelpSummary"];
          indexToolStripMenuItem.Text = _languageDicoFr["MenuHelpIndex"];
          searchToolStripMenuItem.Text = _languageDicoFr["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = _languageDicoFr["MenuHelpAbout"];

          break;

      }
    }

    public enum Language
    {
      French,
      English
    }

    private void LoadLanguages()
    {
      if (!File.Exists(Settings.Default.LanguageFileName))
      {
        CreateLanguageFile();
      }

      // read the translation file and feed the language
      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(Settings.Default.LanguageFileName);
      }
      catch (Exception exception)
      {
        MessageBox.Show("Error while loading xml file " + exception);
        CreateLanguageFile();
        return;
      }

      var result = from node in xDoc.Descendants("term")
                   where node.HasElements
                   let xElementName = node.Element("name")
                   where xElementName != null
                   let xElementEnglish = node.Element("englishValue")
                   where xElementEnglish != null
                   let xElementFrench = node.Element("frenchValue")
                   where xElementFrench != null
                   select new
                   {
                     name = xElementName.Value,
                     englishValue = xElementEnglish.Value,
                     frenchValue = xElementFrench.Value
                   };
      foreach (var i in result)
      {
        if (!_languageDicoEn.ContainsKey(i.name))
        {
          _languageDicoEn.Add(i.name, i.englishValue);
        }
        else
        {
          MessageBox.Show("Your xml file has duplicate like: " + i.name);
        }

        if (!_languageDicoFr.ContainsKey(i.name))
        {
          _languageDicoFr.Add(i.name, i.frenchValue);
        }
        else
        {
          MessageBox.Show("Your xml file has duplicate like: " + i.name);
        }
      }
    }

    private static void CreateLanguageFile()
    {
      List<string> minimumVersion = new List<string>
      {
        "<?xml version=\"1.0\" encoding=\"utf - 8\" ?>",
        "<Document>",
        "<DocumentVersion>",
        "<version> 1.0 </version>",
        "</DocumentVersion>",
        "<terms>",
         "<term>",
        "<name>MenuFile</name>",
        "<englishValue>File</englishValue>",
        "<frenchValue>Fichier</frenchValue>",
        "</term>",
        "  </terms>",
        "</Document>"
      };
      StreamWriter sw = new StreamWriter(Settings.Default.LanguageFileName);
      foreach (string item in minimumVersion)
      {
        sw.WriteLine(item);
      }

      sw.Close();
    }
  }
}