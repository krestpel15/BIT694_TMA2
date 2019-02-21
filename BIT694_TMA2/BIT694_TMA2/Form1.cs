using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BIT694_TMA2
{
    public partial class Form1 : Form
    {
        Hashtable wf = new Hashtable();
        private Dictionary<object, object> mostFrequentTermsDictionary = new Dictionary<object, object>();
        Hashtable newTable = new Hashtable();


        public Form1()
        {
            InitializeComponent();

        }

        private void stopWordsBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.stopWordsBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this._Assessment_2___StopWordsDataSet);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the '_Assessment_2___StopWordsDataSet.StopWords' table. You can move, or remove it, as needed.
            this.stopWordsTableAdapter.Fill(this._Assessment_2___StopWordsDataSet.StopWords);

        }


        //Adds new words to the database of StopWords

        private void AddWord_Click(object sender, EventArgs e)
        {
            String addedWord = addWord.Text;

            try
            {
                DataRow newRow = _Assessment_2___StopWordsDataSet.Tables["StopWords"].NewRow();
                newRow["word"] = addWord.Text;
                newRow["Frequency"] = DBNull.Value;
                addWord.Text = "";

                _Assessment_2___StopWordsDataSet.Tables["StopWords"].Rows.Add(newRow);
                stopWordsTableAdapter.Update(_Assessment_2___StopWordsDataSet.StopWords);
                MessageBox.Show("Added '" + addedWord + "' to database");
            }
            catch (Exception error)
            {
                MessageBox.Show("An error has occured: " + error);
            }
        }


        //Removes a word from the StopWords database

        private void RemoveWord_Click(object sender, EventArgs e)
        {
            String removedWord = removeWord.Text;

            try
            {
                _Assessment_2___StopWordsDataSet.StopWordsRow StopwordsRow = _Assessment_2___StopWordsDataSet.StopWords.FindByword(removeWord.Text);
                StopwordsRow.Delete();
                MessageBox.Show("Removed '" + removedWord + "' to database");
            }
            catch (Exception error)
            {
                MessageBox.Show("An error occured: " + error);
            }
        }


        //Searches StopWords database if a word already exists

        private void QueryWord_Click(object sender, EventArgs e)
        {
            String queriedWord = queryWord.Text;

            _Assessment_2___StopWordsDataSet.StopWordsRow wordsRow = _Assessment_2___StopWordsDataSet.StopWords.FindByword(queryWord.Text);
            if (wordsRow != null)
            {
                MessageBox.Show("'" + queriedWord + "' was found in database");
            }
            else
            {
                MessageBox.Show("'" + queriedWord + "' was not found in database");
            }
        }


        //Updates the frequecy of a specified StopWord

        private void UpdateFrequency_Click(object sender, EventArgs e)
        {
            try
            {
                _Assessment_2___StopWordsDataSet.StopWordsRow wordsRow = _Assessment_2___StopWordsDataSet.StopWords.FindByword(updateWord.Text);
                wordsRow["Frequency"] = frequencyVal.Text;
                frequencyVal.Text = "";
            }
            catch (Exception error)
            {
                MessageBox.Show("An error occured: " + error);
            }
        }


        //Browse the computer to add a folder in the Browse textBox

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browsedFolder = new FolderBrowserDialog();
            if (browsedFolder.ShowDialog() == DialogResult.OK)
            {
                selectedFolder.Text = browsedFolder.SelectedPath;
            }
        }


        // Method created to scan files

        public void ScanFiles()
        {
            //Hashtable wf = new Hashtable(); //Hashtable that holds scanned file

            List<string> stopWord = new List<string>(); //create a list of words in StopWords database
            foreach (DataRow row in _Assessment_2___StopWordsDataSet.Tables["StopWords"].Rows)
            {
                stopWord.Add(row["word"].ToString()); //add Stopwords to the list
            }


            //try and catch when directory is not found
            try
            {
                if (!Directory.Exists(selectedFolder.Text))
                    throw new DirectoryNotFoundException();
            }
            catch (DirectoryNotFoundException err)
            {
                MessageBox.Show("An error occured " + err);
            }

            //Read through 200 files and add to the wf hashtable
            foreach (string file in Directory.GetFiles(selectedFolder.Text)) //Scans the files in selected directory
            {

                String myLine;
                String[] words;

                TextReader tr = new StreamReader(file);

                while ((myLine = tr.ReadLine()) != null) //read the 200 files
                {
                    String aLine = Regex.Replace(myLine, "[^a-zA-Z\\s+]", ""); //replace punctuations and numbers with blank space
                    words = aLine.Split(' ');
                    Regex regex = new Regex("[a-zA-Z]{2,20}$"); //sets a regex to test matches
                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] != "")
                        {
                            words[i] = words[i].ToLower().Trim();
                            if (stopWord.Contains(words[i]))
                            {
                                //do nothing here
                            }
                            
                            else
                            {
                                if (wf.ContainsKey(words[i]) && regex.IsMatch(words[i]))
                                {
                                    try
                                    {
                                        wf[words[i]] = double.Parse(wf[words[i]].ToString()) + 1; //increment the value if words[i] already exists
                                    }
                                    catch (Exception error)
                                    {
                                        MessageBox.Show("An error occured: " + error);
                                    }
                                }
                                else
                                {
                                    wf.Remove(words[i]);
                                    wf.Add(words[i].Trim(), 1.0); // add the word key and value to the hashtable
                                }
                            }
                        }
                    }
                }
            }
        }


        //Scan the files in the directory selected from the browse button,
        //sort and add 50 most frequent terms in the textBox

        private void Scan_files_Click(object sender, EventArgs e)
        {
            
            ScanFiles();

            var result = new List<DictionaryEntry>(wf.Count); //store a list to be sorted

            int hashWordCount = wf.Count; //holds the number of word in the array


            //a new Hashtable that will hold the words with their corresponding frequency 
            //the newTable was created as changing of a value inside for each loop is not allowed
            foreach (DictionaryEntry entry in wf)
            {
                var key = entry.Key;
                var value = entry.Value;
                newTable.Add(key, Math.Round((Convert.ToDouble(value) / Convert.ToDouble(hashWordCount)), 2)); //compute the frequency
            }


            //this directory sorts in an ascending order

            foreach (DictionaryEntry entry in newTable)
            {
                result.Add(entry);
            }

            result.Sort(
                (x, y) =>
                {
                    IComparable comparable = x.Value as IComparable;
                    if (comparable != null)
                    {
                        return (comparable.CompareTo(y.Value));
                    }
                    return 0;
                });


            // Sorts the mostFrequentTerms dictionary to descending order

            var mostFrequentTerms = (from entry in result orderby entry.Value descending select entry)
               .ToDictionary(pair => pair.Key, pair => pair.Value).Take(50);

            foreach (var entry in mostFrequentTerms)
            {
                mostFrequentTermsDictionary.Add(entry.Key, entry.Value);
            }

            PopulateTextBox();
        }


        //A method that popultes the textBox

        public void PopulateTextBox()
        {
            arrayList.Text = "";
            foreach (var entry in mostFrequentTermsDictionary)
            {
                if (arrayList.Text == "")
                {
                    arrayList.Text = entry.Key.ToString() + "     " + entry.Value.ToString();
                }
                else
                {
                    arrayList.Text += "\r\n" + entry.Key.ToString() + "    " + entry.Value.ToString();
                }
            }
        }


        // Adds a selected word to the bottom of the textBox

        private void add_array_Click(object sender, EventArgs e)
        {
            if (mostFrequentTermsDictionary.ContainsKey(addArray.Text))
            {
                MessageBox.Show("Word already exists");
            }
            else
            {
                mostFrequentTermsDictionary.Add(addArray.Text, 0);
                PopulateTextBox();
            }
            //arrayList.AppendText("\r\n" + addArray.Text);
        }


        //Removes a selected word from the 50 most frequent words list in the textBox.

        private void remove_array_Click(object sender, EventArgs e)
        {
            if (mostFrequentTermsDictionary.ContainsKey(removeArray.Text))
            {
                mostFrequentTermsDictionary.Remove(removeArray.Text);
                PopulateTextBox();
            }
            else
            {
                MessageBox.Show("Word does not exist");
            }
        }


        // Displays the frequency of selected word to a textBox from the hashtable of read files

        private void display_frequency_Click(object sender, EventArgs e)
        {
            var word = displayArray.Text;
            try
            {
                frequencyArray.Text = newTable[word].ToString();
            }
            catch
            {
                MessageBox.Show("Word does not Exist");
            }
        }
    }
}

