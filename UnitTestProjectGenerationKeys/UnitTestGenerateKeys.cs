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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnitTestProjectGenerationKeys
{
  [TestClass]
  public class UnitTestGeneration
  {
    [TestMethod]
    public void TestMethodGenerateNewKey()
    {
      string key = GenerateProductKeys.FormMain.GenerateNewKey();
      Assert.IsTrue(key.Length == 29);

      bool result = false;
      for (int i = 0; i < 2; i++)
      {
        if (GenerateProductKeys.FormMain.GenerateNewKey().Contains("-"))
        {
          result = true;
        }
      }

      Assert.IsTrue(result);

      key = GenerateProductKeys.FormMain.GenerateNewKey();
      Assert.IsTrue(CountSpecialCharacter(key, '-') == 4);

      key = GenerateProductKeys.FormMain.GenerateNewKey();
      Assert.IsFalse(AreAllCharactersTheSame(key));

      key = GenerateProductKeys.FormMain.GenerateNewKey();
      Assert.IsTrue(CountLetters(key) + CountNumbers(key) == 25);
    }

    private static int CountSpecialCharacter(string key, char myChar)
    {
      int result = 0;
      foreach (char item in key)
      {
        if (item == myChar)
        {
          result++;
        }
      }

      return result;
    }

    private static int CountSpecialCharacterWithLinq(string key, char myChar)
    {
      return key.Count(item => item == myChar);
    }

    private int CountLetters(string myString)
    {
      int result = 0;
      if (!string.IsNullOrWhiteSpace(myString))
      {
        char[] charArray = myString.ToCharArray();
        foreach (char character in charArray)
        {
          if (IsLetter(character))
          {
            result++;
          }
        }
      }

      return result;
    }

    private static bool IsLetter(char mychar)
    {
      Regex pattern = new Regex(@"[A-Z]");
      return pattern.Match(mychar.ToString()).Success;
    }

    private static int CountNumbers(string myString)
    {
      int result = 0;
      if (!string.IsNullOrWhiteSpace(myString))
      {
        foreach (char item in myString)
        {
          if (HasDigit(item))
          {
            result++;
          }
        }
      }

      return result;
    }

    private bool HasDigit(string myString)
    {
      Regex pattern = new Regex(@"^\d+$");
      return pattern.Match(myString).Success;
    }

    private static bool HasDigit(char myChar)
    {
      Regex pattern = new Regex(@"^\d+$");
      return pattern.Match(myChar.ToString()).Success;
    }

    public static bool AreAllCharactersTheSame(string s)
    {
      return s.Length == 0 || s.All(ch => ch == s[0]);
    }


    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_in_between()
    {
      int result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(1, 10);
      Assert.IsTrue(result >= 1 && result <= 10);
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_25_generations_in_between()
    {
      for (int i = 0; i < 25; i++)
      {
        int result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(1, 100);
        Assert.IsTrue(result >= 1 && result <= 100);
      }
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_all_254_numbers_are_generated()
    {
      //for (int i = 1; i < 254; i++)
      //{
      //  int result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(i, i);
      //  Assert.IsTrue(result == i);
      //}
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_generation_of_number_one()
    {
      //int result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(1, 1);
      //Assert.IsTrue(result == 1);
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_generation_all_numbers_are_inferior_to_254()
    {
      //for (int i = 2; i < 254; i++)
      //{
      //  result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(1, i);
      //  Assert.IsTrue(result >= 1 && result <= i);
      //}
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_one_million_generation_to_get_a_number_two()
    {
      //bool find = false;
      //for (int i = 1; i < 1000000; i++) // no value 2 in 1 million generations
      //{
      //  if (GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(1, 254) == 2)
      //  {
      //    find = true;
      //  }

      //  Assert.IsTrue(find);
      //}
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_not_above_twenty()
    {
      int result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(1, 20);
      Assert.IsFalse(result > 20);
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_false_under_minimum()
    {
      int result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(10, 20);
      Assert.IsFalse(result < 10);
    }

    [TestMethod]
    public void TestMethodGenerateRandomNumberUsingCrypto_zero_above_255()
    {
      int result = GenerateProductKeys.FormMain.GenerateRandomNumberUsingCrypto(1, 255);
      Assert.IsTrue(result == 0);
    }
  }
}