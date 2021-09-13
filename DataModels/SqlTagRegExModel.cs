﻿using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AI_Note_Review
{
    public class SqlTagRegEx
    {
        // Declare the event
        public int TagRegExID { get; set; }
        public int TargetTag { get; set; }

        public int TargetSection { get; set; }

        public string RegExText { get; set; }
        public int TagRegExType { get; set; }

        public double MinAge { get; set; }
        public double MaxAge { get; set; }
        public bool Male { get; set; }
        public bool Female { get; set; }

        public SqlTagRegEx()
        {
        }

        public SqlTagRegEx(int intTargetTag, string strRegExText, int intTargetSection, int TagRegExType = 1, double dMinAge = 0, double dMaxAge = 99, bool bMale = true, bool bFemale = true)
        {
            strRegExText = strRegExText.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO TagRegEx (TargetTag, RegExText, TargetSection, TagRegExType, MinAge, MaxAge, Male, Female) VALUES ({intTargetTag}, '{strRegExText}', {intTargetSection}, {TagRegExType},{dMinAge},{dMaxAge},{bMale},{bFemale});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
        }

        public void SaveToDB()
        {
            string sql = "UPDATE TagRegEx SET " +
                    "TagRegExID=@TagRegExID, " +
                    "TargetTag=@TargetTag, " +
                    "RegExText=@RegExText, " +
                    "TagRegExType=@TagRegExType, " +
                    "TargetSection=@TargetSection, " +
                    "MinAge=@MinAge," +
                    "MaxAge=@MaxAge," +
                    "Male=@Male," +
                    "Female=@Female " +
                    "WHERE TagRegExID=@TagRegExID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        public bool DeleteFromDB()
        {
            string sql = "Delete from TagRegEx WHERE TagRegExID=@TagRegExID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;
        }
    }
}

