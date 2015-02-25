/*
The MIT License(MIT)
Copyright(c) 2015 fred
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

namespace GenerateProductKeys
{
  public partial class FormMain : Form
  {
    public FormMain()
    {
      InitializeComponent();
    }

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
      labelDuration.Text = "Duration (hour: minute:second: millisecond): 0";
      Application.DoEvents();
      progressBarGeneration.Visible = true;
      int NumberOfkeys;
      try
      {
        Int32.TryParse(textBoxNumberOfKeys.Text, out NumberOfkeys);
        if (NumberOfkeys == 0)
        {
          DisplayMessageBox("The value is a number greater than int32.MaxValue", "Value too big", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return; 
        }
      }
      catch (Exception)
      {                
      }

      if (!Int32.TryParse(textBoxNumberOfKeys.Text, out NumberOfkeys))
      {
        MessageBox.Show("The number of keys to be generated must be a number");
        return;
      }

      if (NumberOfkeys < 0)
      {
        DisplayMessageBox("The number of keys to be generated must greater than 0\n\nPlease type in a positive integer.",
          "Number of keys greater than zero", MessageBoxButtons.OK, MessageBoxIcon.Error);
        textBoxNumberOfKeys.Text = string.Empty;
        return;
      }

      progressBarGeneration.Minimum = 1;
      progressBarGeneration.Maximum = NumberOfkeys;
      progressBarGeneration.Value = progressBarGeneration.Minimum;
      Stopwatch chrono = new Stopwatch();
      chrono.Start();
      for (int i = 1; i <= NumberOfkeys; i++)
      {
        listBoxKeys.Items.Add(GenerateNewKey());
        progressBarGeneration.Value = i;
      }

      chrono.Stop();
      labelCountKeys.Text = "Count: " + listBoxKeys.Items.Count + " items";
      labelDuration.Text = "Duration (hour: minute:second: millisecond): " + chrono.Elapsed;
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
        numbers.Add(((int)letter) - 64);
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
        for (int i = 0; i < 5; i++)
        {
          newString += sourceCharacters[GenerateRandomNumberUsingCrypto(0, 26 + 9)];
        }

        newString += "-";
      }

      newString = newString.Substring(0, newString.Length - 1);
      return newString;
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
      } while (result <= min || result >= max);
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

    private void DisplayMessageBox(string message, string title = "", MessageBoxButtons button = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
      MessageBox.Show(message, title, button, icon);
    }
  }
}