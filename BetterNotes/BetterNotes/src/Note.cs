﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BetterNotes {
    public class Note {
        //private vars and get/sets
        public string FilePath { get; set; } //set only in case you want to move the and change property while open (renaming notes, temp directories, etc)
        public string Name { get; set; } 
        public User CreateUser { get; set; } 
        public DateTime CreatedDateTime { get; } //RONLY value, should not change after first creation
        public DateTime LastModifiedDateTime { get; set; }
        public bool IsReminder { get; set; }
        public DateTime TimeToRemind { get; set; }
        public bool RemindToast { get; set; }
        public string RemindPhone { get; set; }
        public string RemindEmail { get; set; }

        //make a note object (new file)
        public Note(string name, User createUser) {
            this.FilePath = GlobalVars.BnotWorkDir + "\\" + name;
            this.Name = name;
            this.CreateUser = createUser;
            this.CreatedDateTime = DateTime.Now;
            this.LastModifiedDateTime = DateTime.Now;
            if (!Directory.Exists(FilePath + "\\img")) Directory.CreateDirectory(FilePath + "\\img");
            if (!Directory.Exists(FilePath + "\\note")) Directory.CreateDirectory(FilePath + "\\note");
            if (!Directory.Exists(FilePath + "\\speech")) Directory.CreateDirectory(FilePath + "\\speech");
            if (!File.Exists(FilePath + "\\NoteMetadata.properties")) File.Create(FilePath + "\\NoteMetadata.properties").Dispose();
            SaveNoteMetadata();
        }

        //make a reminder object
        public Note(string name, User createUser, DateTime timeToRemind, bool remindToast, string remindEmail, string remindPhone) {
            this.FilePath = GlobalVars.BnotWorkDir + "\\" + name;
            this.Name = name;
            this.CreateUser = createUser;
            this.CreatedDateTime = DateTime.Now;
            this.LastModifiedDateTime = DateTime.Now;
            this.IsReminder = true;
            this.TimeToRemind = timeToRemind;
            this.RemindToast = remindToast;
            this.RemindPhone = remindPhone;
            this.RemindEmail = remindEmail;
            if (!Directory.Exists(FilePath + "\\img")) Directory.CreateDirectory(FilePath + "\\img");
            if (!Directory.Exists(FilePath + "\\note")) Directory.CreateDirectory(FilePath + "\\note");
            if (!Directory.Exists(FilePath + "\\speech")) Directory.CreateDirectory(FilePath + "\\speech");
            if (!File.Exists(FilePath + "\\NoteMetadata.properties")) File.Create(FilePath + "\\NoteMetadata.properties").Dispose();
            SaveNoteMetadata();
            AddReminderMetadata();
        }

        //make a note/reminder object (existing file)
        public Note(string currentBnotDir) {
            Regex testRegex = new Regex(@"([^\\]*[.]*\.bnot)$", RegexOptions.Multiline);
            string nameAndExt = testRegex.Match(currentBnotDir).Value;
            this.Name = nameAndExt.Substring(0, nameAndExt.Length - 5);
            this.FilePath = GlobalVars.BnotWorkDir + "\\" + this.Name;
            Archive.UnarchiveFile(GlobalVars.BnotWorkDir + "\\" + this.Name, currentBnotDir, this.FilePath);
            List<string> csvIn = new List<string>();
            using (var reader = new StreamReader(this.FilePath + "\\NoteMetaData.properties")) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    foreach (string element in line.Split(',')) csvIn.Add(element);
                }
            }
            foreach (User user in UserHandler.UserList) if (user.Name.Equals(csvIn[1])) this.CreateUser = user;
            DateTime.TryParse(csvIn[2], out DateTime tryCreateDate);
            this.CreatedDateTime = tryCreateDate;
            this.LastModifiedDateTime = DateTime.Now;
            Boolean.TryParse(csvIn[4], out bool tryIsReminder);
            this.IsReminder = tryIsReminder;
            DateTime.TryParse(csvIn[5], out DateTime tryRemindDate);
            this.TimeToRemind = tryRemindDate;
            Boolean.TryParse(csvIn[6], out bool tryRemindToast);
            this.RemindToast = tryRemindToast;
            this.RemindPhone = csvIn[7];
            this.RemindEmail = csvIn[8];
        }

        //Save note based on all values, then archive
        public void SaveNote(RichTextBox noteContent, string savePath) {
            SaveNoteMetadata();
            if (this.IsReminder) SaveReminderMetadata();
            SaveRichTexBox(noteContent);
            SaveRecentMetadata(savePath);
            Archive.ArchiveFile(this.FilePath, savePath);
        }

        //Delete Note
        public void DeleteNote() {
            DeleteReminderMetadata();
            DeleteRecentMetadata();
            Directory.Delete(this.FilePath, true);
        }

        //Save only the current note's metadata
        public void SaveNoteMetadata() {
            string noteMetadata =
                this.Name + "," +
                this.CreateUser.Name + "," +
                this.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.IsReminder.ToString() + "," +
                this.TimeToRemind.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.RemindToast.ToString() + "," +
                this.RemindPhone + "," +
                this.RemindEmail;
            File.WriteAllText(this.FilePath + "\\NoteMetadata.properties", noteMetadata);
        }

        //Add current reminder
        public void AddReminderMetadata() {
            string remindMetadata =
                this.TimeToRemind.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.Name + "," +
                this.RemindPhone + "," +
                this.RemindEmail + "," +
                this.RemindToast.ToString() +
                Environment.NewLine;
            File.AppendAllText(GlobalVars.BnotReminderCsv, remindMetadata);
            NotificationHandler.RefreshList();
        }

        //Edit only the current reminder information to bnot metadata
        public void SaveReminderMetadata() {
            string remindMetadata =
                this.TimeToRemind.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.Name + "," +
                this.RemindPhone + "," +
                this.RemindEmail + "," +
                this.RemindToast.ToString();
            string remindCsv = "";
            int count = 0;
            using (var reader = new StreamReader(GlobalVars.BnotReminderCsv)) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    if (line.Split(',')[1].Equals(this.Name)) {
                        remindCsv += remindMetadata + Environment.NewLine;
                        count++;
                    }
                    else { remindCsv += line + Environment.NewLine; }
                }
                reader.Close();
            }
            File.WriteAllText(GlobalVars.BnotReminderCsv, remindCsv);
            if (count == 0) AddReminderMetadata();
            else { NotificationHandler.RefreshList(); }
        }

        //Delete note from reminder
        public void DeleteReminderMetadata() {
            string remindCsv = "";
            using (var reader = new StreamReader(GlobalVars.BnotReminderCsv)) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    if (!line.Split(',')[1].Equals(this.Name)) remindCsv += line + Environment.NewLine;
                }
                reader.Close();
            }
            File.WriteAllText(GlobalVars.BnotReminderCsv, remindCsv);
            NotificationHandler.RefreshList();
        }

        //Add to Recent Note
        public void AddRecentMetadata(string bnotPath) {
            string recentMetadata =
                this.LastModifiedDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.Name + "," +
                bnotPath +
            Environment.NewLine;
            File.AppendAllText(GlobalVars.BnotRecentNoteCsv, recentMetadata);
        }

        //Edit Recent Note metadata to contain current note information
        public void SaveRecentMetadata(string bnotPath) {
            string recentMetadata =
                this.LastModifiedDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                this.Name + "," +
                bnotPath;
            string recentCsv = "";
            int count = 0;
            using (var reader = new StreamReader(GlobalVars.BnotRecentNoteCsv)) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    if (line.Split(',')[2].Equals(this.Name)) {
                        recentCsv += recentMetadata + Environment.NewLine;
                        count++;
                    }
                    else { recentCsv += line + Environment.NewLine; }
                }
                reader.Close();
            }
            File.WriteAllText(GlobalVars.BnotRecentNoteCsv, recentCsv);
            if (count == 0) AddRecentMetadata(bnotPath);
        }

        //Delete Note from recent
        public void DeleteRecentMetadata() {
            string recentCsv = "";
            using (var reader = new StreamReader(GlobalVars.BnotRecentNoteCsv)) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    if (!line.Split(',')[2].Equals(this.Name)) recentCsv += line + Environment.NewLine; ;
                }
                reader.Close();
            }
            File.WriteAllText(GlobalVars.BnotRecentNoteCsv, recentCsv);
        }

        //save only the current rtb to xaml
        public void SaveRichTexBox(RichTextBox noteContent) {
            TextRange range;
            FileStream fStream;
            range = new TextRange(noteContent.Document.ContentStart, noteContent.Document.ContentEnd);
            fStream = new FileStream(this.FilePath + "\\note\\note", FileMode.Create);
            range.Save(fStream, DataFormats.XamlPackage);
            fStream.Close();
        }
    }
}
